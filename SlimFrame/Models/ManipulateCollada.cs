
using SlimDX;
using System.IO;
using System.Collections.Generic;

using static System.Math;
using static System.Diagnostics.Debug;
using System;

namespace SlimFrame.Models
{
    /// <summary>
    /// Manipulates Collada (.dae) Model for our model system.
    /// </summary>
    public class ManipulateCollada
    {
        #region Constants

        const short MainChunkID = 0x4d4d;
        const short EditorChunkID = 0x4d4d;
        #endregion

        /// <summary>
        /// Contains information about a tag
        /// </summary>
        struct Tag
        {
            public string Head;
            public List<string> Field;
            public List<string> Value;
            public bool Inline; // It will be true for tag having ending with "/>" otherwise false
        }

        /// <summary>
        /// Loads a Collada (.dae) model to our model system.
        /// </summary>
        /// <param name="device">D3D 11 device</param>
        /// <param name="filepath">Path to the model</param>
        /// <param name="isRHCoordSys">Whether model is in right-handed coordinate system or not</param>
        /// <param name="computeNormals">Whether normals to be calculated or not</param>
        /// <param name="Debug">Whether the loading be done in Debug mode or not</param>
        /// <returns>A model loaded from Collada (.dae) model file</returns>
        public static Model Load(SlimDX.Direct3D11.Device device, string filepath, bool isRHCoordSys = false, bool computeNormals = false, bool Debug = false)
        {
            Model model = new Model(); // Model Object to which the file to be loaded
            string folder = filepath.Substring(0, filepath.LastIndexOf("\\") + 1); //path to folder where the model file resides

            List<uint> indices = new List<uint>(); // List of indices to vertex in each triangle
            List<Vector3> vertPos = new List<Vector3>(); // List of vertex position
            List<Vector3> vertNorm = new List<Vector3>(); // List of vertex normal
            List<Vector2> vertTex = new List<Vector2>(); // List of vertex texture co-ordinate
            List<string> meshMaterials = new List<string>();// List of mesh materials
            List<MaterialBase> material = new List<MaterialBase>();// List of mesh materials

            List<int> meshSubsetMaterial = new List<int>();
            List<int> meshSubsetIndexStart = new List<int>();
            List<string> meshSubsetNames = new List<string>();


            if (Debug) System.Diagnostics.Debug.WriteLine(filepath);

            FileStream file = new FileStream(filepath, FileMode.Open); //File stream to the file on which reader will work
            StreamReader reader = new StreamReader(file); //Binary reader for file



            reader.Dispose();
            file.Dispose();

            if (Debug) System.Diagnostics.Debug.WriteLine(filepath + " model is loaded!");
            return model;
        }

        /// <summary>
        /// Reads a tag from a XML file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Tag readTag(StreamReader reader)
        {
            Tag tag = new Tag();
            string data = reader.ReadLine();
            if (data.Substring(0,4) == "<xs:")
            {
                data = data.Substring(4);
                data = data.Substring(0,data.Length-1);

                if (data.EndsWith("/"))
                {
                    tag.Inline = true;
                    data = data.Substring(0, data.Length - 1);
                }
                data = data.Trim();

                tag.Head = data.Split(' ')[0];
            }

            return tag;
        }
    }
}
