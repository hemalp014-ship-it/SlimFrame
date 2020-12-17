

using SlimDX;
using System.IO;
using System.Collections.Generic;

using static System.Math;
using static System.Diagnostics.Debug;
using System.Linq;
using System;

namespace SlimFrame.Models
{
    /// <summary>
    /// Manipulates Wavefront(.Obj) Model for our model system.
    /// </summary>
    public class ManipulateObj
    {

        /// <summary>
        /// Generates name for duplicate name as name.xxx 
        /// </summary>
        /// <param name="name">Base name</param>
        /// <param name="prefixNumber">No. of duplicate copy</param>
        /// <param name="minNumberofDigit">Number of digits in name (i.e, 3 -> 000, 4-> 0000)</param>
        /// <returns></returns>
        private static string GenerateName(string name, int prefixNumber, int minNumberofDigit)
        {
            string number = prefixNumber.ToString();
            int digits = Max(number.Length, minNumberofDigit);

            if (prefixNumber > 0)
            {
                while (number.Length < digits)
                {
                    number = "0" + number;
                }
            }
            else
            {
                return name;
            }

            return name + "." + number;
        }
        /// <summary>
        /// Stream to the model file for loading model
        /// </summary>
        public static FileStream file;
        /// <summary>
        /// Loads a wavefront (.obj) model to our model system.
        /// </summary>
        /// <param name="device">D3D 11 device</param>
        /// <param name="filepath">Path to the model</param>
        /// <param name="isRHCoordSys">Whether model is in right-handed coordinate system or not</param>
        /// <param name="computeNormals">Whether normals to be calculated or not</param>
        /// <param name="Debug">Whether the loading be done in Debug mode or not</param>
        /// <returns>A model loaded from wavefront (.obj) model file</returns>
        public static Model Load(SlimDX.Direct3D11.Device device, string filepath, bool isRHCoordSys = false, bool computeNormals = false, bool Debug = false)
        {
            Model model = new Model(); // Model Object to which the file to be loaded
            string folder = filepath.Substring(0, filepath.LastIndexOf("\\") + 1); //path to folder where the model file resides

            string meshMaterialLib = ""; //String to hold our obj material library filepath

            List<uint> indices = new List<uint>(); // List of indices to vertex in each triangle
            List<Vector3> vertPos = new List<Vector3>(); // List of vertex position
            List<Vector3> vertNorm = new List<Vector3>(); // List of vertex normal
            List<Vector2> vertTex = new List<Vector2>(); // List of vertex texture co-ordinate
            List<string> meshMaterials = new List<string>();// List of mesh materials
            List<MaterialBase> material = new List<MaterialBase>();// List of mesh materials

            List<int> meshSubsetMaterial = new List<int>();
            List<int> meshSubsetIndexStart = new List<int>();
            List<string> meshSubsetNames = new List<string>();

            int meshSubsets = 0;

            // Vertex definition indices
            List<int> vertPosIndex = new List<int>();
            List<int> vertNormIndex = new List<int>();
            List<int> vertTCIndex = new List<int>();

            //Make sure we have a default if no tex coords or normals are defined
            bool hasTexCoord = false;
            bool hasNorm = false;

            //Temp variables to store into vectors
            int vertPosIndexTemp = 0;
            int vertNormIndexTemp = 0;
            int vertTCIndexTemp = 0;

            int vIndex = 0;            //Keep track of our vertex index count
            int totalVerts = 0;

            string lastStringRead = null;

            bool vertAlreadyExists = false;

            if (Debug) System.Diagnostics.Debug.WriteLine(filepath);

            file = new FileStream(filepath, FileMode.Open); //File stream to the file on which reader will work
            //BinaryReader reader = new BinaryReader(file); //Binary reader for file
            StreamReader sr = new StreamReader(file);

            if (Debug) System.Diagnostics.Debug.WriteLine(filepath + " loaded!");

            if (sr != null)
            {
                //try
                //{
                while (!sr.EndOfStream)
                {

                    do
                    {
                        lastStringRead = sr.ReadLine().Trim();    //Get next line
                        if (lastStringRead != "")
                            break;
                    } while (!sr.EndOfStream);

                    if (sr.EndOfStream && lastStringRead == "") break;

                    string[] data = lastStringRead.Split(' '); // index 0 -> indentifier; index 1 -> data

                    switch (data[0][0])
                    {
                        case '#':
                            //It is a comment do nothing!
                            break;
                        case 'v':
                            Vector3 vec3 = new Vector3();
                            Vector2 vec2 = new Vector2();
                            switch (data[0])
                            {
                                case "v":
                                    vec3 = loadVector3(data);
                                    if (isRHCoordSys)    //If model is from an RH Coord System
                                        vec3.Z = -vec3.Z;
                                    vertPos.Add(vec3);
                                    break;
                                case "vn":
                                    vec3 = loadVector3(data);
                                    if (isRHCoordSys)    //If model is from an RH Coord System
                                        vec3.Z = -vec3.Z;

                                    vertNorm.Add(vec3);
                                    break;
                                case "vt":
                                    vec2 = loadVector2(data);
                                    if (isRHCoordSys)    //If model is from an RH Coord System
                                        vec2.Y = 1.0f - vec2.Y;

                                    vertTex.Add(vec2);

                                    hasTexCoord = true;    //We know the model uses texture coords
                                    break;
                            }
                            break;
                        case 'o':
                            break;
                        case 'g':
                            meshSubsetIndexStart.Add(vIndex);        //Start index for this subset
                            meshSubsets++;

                            if (data.Length > 1)
                                meshSubsetNames.Add(data[1]);
                            else
                            {
                                var Objects = meshSubsetNames.Find((string s) => { return s.StartsWith("Object"); });
                                var count = 0;
                                if (Objects != null)
                                    count = Objects.Length;
                                meshSubsetNames.Add(GenerateName("Object", count, 3));
                            }
                            break;
                        case 'f':
                            int numOfVerticesPerFace = data.Length - 1;
                            int firstIndex = 0;
                            int lastIndex = 0;

                            for (int i = 1; i < data.Length; i++)
                            {
                                if (i > 3)
                                {
                                    indices.Add((uint)firstIndex);
                                    vIndex++;
                                    indices.Add((uint)lastIndex);
                                    vIndex++;
                                }

                                var faceData = data[i].Split('/');
                                if (data[1] == "" && i + 1 < data.Length)
                                    faceData = data[i + 1].Split('/');

                                vertPosIndexTemp = int.Parse(faceData[0]) - 1;

                                if (faceData.Length == 1)
                                {
                                    vertNormIndexTemp = 0;
                                    vertTCIndexTemp = 0;
                                }
                                else
                                {
                                    if (faceData[1] == "")
                                        vertTCIndexTemp = 0;
                                    else
                                        vertTCIndexTemp = int.Parse(faceData[1]) - 1;

                                    if (faceData.Length == 3)
                                        vertNormIndexTemp = int.Parse(faceData[2]) - 1;
                                }
                                //Avoid duplicate vertices
                                vertAlreadyExists = false;
                                if (totalVerts >= 3)    //Make sure we at least have one triangle to check
                                {
                                    //Time(in second) taken for loadind a obj model with 61444:-
                                    //  Using for loop method to detect duplicate vertices : 
                                    //      24.6647678 24.5100255 24.4431397 24.6670047 24.689782(Avg: 24.59494394)
                                    //  Using List<T>.IndexOf method to detect duplicate vertices :  
                                    //      4.2013184 4.0985182 4.115278 4.1150706 4.139565(Avg: 4.13395004)
                                    //(This method performed well performed so it is selected now!)

                                    int index = vertPosIndex.IndexOf(vertPosIndexTemp);
                                    if (index > 0)
                                    {
                                        if (vertTCIndex[index] == vertTCIndexTemp)
                                        {
                                            indices.Add((uint)index);
                                            vertAlreadyExists = true;
                                        }
                                    }
                                }
                                //Check to make sure there is at least one subset
                                if (meshSubsets == 0)
                                {
                                    meshSubsetIndexStart.Add(vIndex);        //Start index for this subset
                                    meshSubsets++;
                                }
                                //If this vertex is not already in our vertex arrays, put it there
                                if (!vertAlreadyExists)
                                {
                                    vertPosIndex.Add(vertPosIndexTemp);
                                    vertTCIndex.Add(vertTCIndexTemp);
                                    vertNormIndex.Add(vertNormIndexTemp);
                                    totalVerts++;    //We created a new vertex
                                    indices.Add((uint)totalVerts - 1);    //Set index for this vertex
                                }

                                if (i == 1)
                                    firstIndex = (int)indices[vIndex];
                                else if (i >= 3)
                                    lastIndex = (int)indices[vIndex];

                                vIndex++;
                            }
                            break;
                        case 'm':
                            if (data[0] == "mtllib")
                                meshMaterialLib = lastStringRead.Replace("mtllib","").Trim();
                            break;
                        case 'u':
                            if (data[0] == "usemtl")
                                meshMaterials.Add(data[1]);
                            break;
                    }
                }
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show(e.ToString()); return null;
                //}
            }
            else
            {
                // TODO : error management
            }

            meshSubsetIndexStart.Add(vIndex);

            //Make sure we have a default for the tex coord and normal
            //if one or both are not specified
            if (!hasNorm)
                vertNorm.Add(new Vector3(0.0f, 0.0f, 0.0f));
            if (!hasTexCoord)
                vertTex.Add(new Vector2(0.0f, 0.0f));


            //Close the obj file, and open the mtl file
            sr.Dispose();
            file.Dispose();

            string path = Path.Combine(folder, meshMaterialLib);
            file = new FileStream(path, FileMode.Open);
            sr = new StreamReader(file);

            if (Debug) System.Diagnostics.Debug.WriteLine(path + " loaded!");

            int matCount = 0;    //total materials;

            //kdset - If our diffuse color was not set, we can use the ambient color (which is usually the same)
            //If the diffuse color WAS set, then we don't need to set our diffuse color to ambient
            bool kdset = false;

            MaterialBase tempMat = new MaterialBase();
            tempMat.Transparency = 0.0f;
            tempMat.Texture = "";
            tempMat.BumpMap = "";
            tempMat.TextureType = TextureType.texture2D;
            tempMat.BumpType = TextureType.texture2D;


            if (sr != null)
            {
                while (!sr.EndOfStream)
                {
                    do
                    {
                        lastStringRead = sr.ReadLine().Trim();    //Get next line
                        if (lastStringRead != "")
                            break;
                    } while (!sr.EndOfStream);

                    if (sr.EndOfStream && lastStringRead == "") break;

                    string[] data = lastStringRead.Split(' '); // index 0 -> indentifier; index 1 -> data

                    switch (data[0][0])
                    {
                        //Check for comment
                        case '#':
                            //do nothing
                            break;
                        case 'K':
                            switch (data[0][1])
                            {
                                case 'a':
                                    tempMat.Ambient = loadColor(data);
                                    break;
                                case 'd':
                                    tempMat.Diffuse = loadColor(data);
                                    break;
                                case 's':
                                    tempMat.Specular = loadColor(data);
                                    break;
                                case 'e':
                                    tempMat.Emissive = loadColor(data);
                                    break;
                            }
                            break;
                        case 'T':
                            switch (data[0][1])
                            {
                                case 'r':
                                    tempMat.Transparency = float.Parse(data.Last()) * 100;
                                    break;
                                case 'f':
                                    break;
                            }
                            break;
                        case 'd':
                            tempMat.Dissolve = float.Parse(data.Last());
                            break;
                        case 'm':
                            if (data[0] == "map_Kd")
                                tempMat.Texture = lastStringRead.Replace("map_Kd","").Trim();
                            //else if (data[0] == "map_Ka")
                            //    tempMat.Texture = data[1];
                            else if (data[0] == "map_bump")
                                tempMat.BumpMap = lastStringRead.Replace("map_bump","").Trim();

                            WriteLine(tempMat.Texture);
                            break;
                        case 'i':
                            if (data[0] == "illum")
                            {
                                float.Parse(data.Last());
                            }
                            break;
                        case 'N':
                            switch (data[0][1])
                            {
                                case 's':
                                    tempMat.SpecularPower = float.Parse(data.Last());
                                    break;
                                case 'i':
                                    tempMat.Refractivity = float.Parse(data.Last());
                                    break;
                            };
                            break;
                        case 'n':
                            if (data[0] == "newmtl")
                            {
                                if (matCount > 0)
                                    material.Add(tempMat);

                                //New material, set its defaults
                                tempMat = new MaterialBase();
                                tempMat.Name = data.Last();
                                tempMat.Transparency = 0.0f;
                                tempMat.Texture = "";
                                tempMat.BumpMap = "";
                                tempMat.TextureType = TextureType.texture2D;
                                tempMat.BumpType = TextureType.texture2D;
                                tempMat.Folder = folder;
                                tempMat.Refractivity = 1.0f;

                                matCount++;
                                kdset = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
                material.Add(tempMat);
            }


            //Close the mtl file
            sr.Close();
            file.Close();

            file = null;

            //Set the subsets material to the index value
            //of the its material in our material array
            for (int i = 0; i < meshSubsets; ++i)
            {
                bool hasMat = false;
                for (int j = 0; j < material.Count; ++j)
                {
                    //if (i + 1 > meshMaterials.Count)
                    //{
                    //    if (meshMaterials.First() == material[j].Name)
                    //    {
                    //        MaterialBase mat = material[j];
                    //        mat.Folder = folder;
                    //        meshSubsetMaterial.Add(j);
                    //        hasMat = true;
                    //    }
                    //}
                    //else
                    //{
                        if (meshMaterials[i] == material[j].Name)
                        {
                            MaterialBase mat = material[j];
                            mat.Folder = folder;
                            meshSubsetMaterial.Add(j);
                            hasMat = true;
                        }
                    //}
                }
                if (!hasMat)
                {
                    MaterialBase mat = material[0];
                    mat.Folder = folder;

                    meshSubsetMaterial.Add(0); //Use first material in array
                }
            }


            List<Vertex> vertices = new List<Vertex>();
            Vertex tempVert = new Vertex();

            //Create our vertices using the information we got 
            //from the file and store them in a vector
            for (int j = 0; j < totalVerts; ++j)
            {
                tempVert.Position = vertPos[vertPosIndex[j]];
                tempVert.Normal = vertNorm[vertNormIndex[j]];
                tempVert.TexCoord = vertTex[vertTCIndex[j]];

                vertices.Add(tempVert);
            }

            //////////////////////Compute Normals///////////////////////////
            //If computeNormals was set to true then we will create our own
            //normals, if it was set to false we will use the obj files normals
            Object3D obj = new Object3D(device, vertices.ToArray(), indices.ToArray(), meshSubsetMaterial.ToArray(), meshSubsetIndexStart.ToArray(), meshSubsetNames.ToArray(), computeNormal: computeNormals) { Tranformation = Matrix.Scaling(1, 1, 1) };

            model.Objects.Add(obj);

            foreach (MaterialBase mat in material)
                model.Materials.Add(new Material(device, mat));
            return model;
        }
        /// <summary>
        /// Loads color from color string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        static private Color4 loadColor(string[] data, int offset = 1)
        {
            Color4 col = new Color4();
            offset = Max(data[1] == "" ? 2 : 1, offset);

            col.Red = float.Parse(data[offset++]);
            col.Green = float.Parse(data[offset++]);
            col.Blue = float.Parse(data[offset++]);
            col.Alpha = 1.0f;
            return col;
        }
        /// <summary>
        /// Loads vector3 from string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        static private Vector3 loadVector3(string[] data, int offset = 1)
        {
            Vector3 vec = new Vector3();

            offset = Max(data[1] == "" ? 2 : 1, offset);

            vec.X = float.Parse(data[offset++]);
            vec.Y = float.Parse(data[offset++]);
            vec.Z = float.Parse(data[offset++]);
            return vec;
        }
        /// <summary>
        /// Loads Vector2 from string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        static private Vector2 loadVector2(string[] data, int offset = 1)
        {
            Vector2 vec = new Vector2();

            offset = Max(data[1] == "" ? 2 : 1, offset);
            vec.X = float.Parse(data[offset++]);
            vec.Y = float.Parse(data[offset++]);
            return vec;
        }
    }
}
