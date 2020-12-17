
using SlimDX;
using System.IO;
using System.Collections.Generic;

using static System.Math;
using static System.Diagnostics.Debug;
using System;
using System.Runtime.Remoting.Channels;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Drawing;
using Microsoft.VisualBasic.CompilerServices;
using SlimDX.Direct3D10;
using SlimDX.DirectInput;

namespace SlimFrame.Models
{
    /// <summary>
    /// Manipulates 3DS Model for our model system.
    /// </summary>
    public class Manipulate3DS
    {
        #region Constants

        const ushort MainID = 0x4d4d;
        const ushort M3DVersionID = 0x0002;
        const ushort EditorID = 0x3d3d;
        const ushort EditorVersionID = 0x3d3e;
        const ushort ObjectBlockID = 0x4000;
        const ushort TriangularMeshID = 0x4100;
        const ushort VerticesListID = 0x4110;
        const ushort FacesDescriptionID = 0x4120;
        const ushort FacesMaterialID = 0x4130;
        const ushort SmoothingGroupListID = 0x4150;
        const ushort MappingCoordinatesListID = 0x4140;
        const ushort LocalCoordinatesSystemID = 0x4160;
        const ushort LightID = 0x4600;
        const ushort SpotlightID = 0x4610;
        const ushort CameraID = 0x4700;
        const ushort MaterialBlockID = 0xAFFF;
        const ushort MaterialNameID = 0xA000;
        const ushort AmbientColorID = 0xA010;
        const ushort DiffuseColorID = 0xA020;
        const ushort SpecularColorID = 0xA030;
        const ushort TextureMapID = 0xA200;
        const ushort BumpMapID = 0xA230;
        const ushort ReflectionMapID = 0xA220;
        const ushort MappingFilenameID = 0xA300;
        const ushort MappingParaemtersID = 0xA351;
        const ushort KeyframerID = 0xB000;
        const ushort MeshInformationBlockID = 0xB002;
        const ushort SpotLightInformationBlockID = 0xB007;
        const ushort FramesID = 0xB008;
        const ushort ObjectNameID = 0xB010;
        const ushort ObjectPivotPointID = 0xB013;
        const ushort PositionTrackID = 0xB020;
        const ushort RotationTrackID = 0xB021;
        const ushort ScaleTrackID = 0xB022;
        const ushort HierarchyPositionID = 0xB030;

        const int ColorRGB = 0x10;
        const int ColorTrue = 0x11;


        #endregion

        #region Structures

        struct Chunk
        {
            public ushort ID;
            public long length;
            public long start;
            public long end;

            public static Chunk Parse(BinaryReader reader)
            {
                Chunk chunk = new Chunk();
                chunk.start = reader.BaseStream.Position;
                chunk.ID = reader.ReadUInt16();
                chunk.length = reader.ReadUInt32();
                chunk.end = chunk.start + chunk.length;

                return chunk;
            }

            public void SeekToEnd(BinaryReader reader)
            {
                reader.BaseStream.Seek(end, SeekOrigin.Begin);
            }
        }

        #endregion

        /// <summary>
        /// Stream to the model file to load model
        /// </summary>
        public static FileStream file;

        /// <summary>
        /// Loads a 3DS model to our model system.
        /// </summary>
        /// <param name="device">D3D 11 device</param>
        /// <param name="filepath">Path to the model</param>
        /// <param name="isRHCoordSys">Whether model is in right-handed coordinate system or not</param>
        /// <param name="computeNormals">Whether normals to be calculated or not</param>
        /// <param name="Debug">Whether the loading be done in Debug mode or not</param>
        /// <returns>A model loaded from 3DS model file</returns>
        public static Model Load(SlimDX.Direct3D11.Device device, string filepath, bool isRHCoordSys = false, bool computeNormals = true, bool Debug = false)
        {
            Model model = new Model(); // Model Object to which the file to be loaded
            string folder = filepath.Substring(0, filepath.LastIndexOf("\\") + 1); //path to folder where the model file resides

            if (Debug) WriteLine(filepath);

            file = new FileStream(filepath, FileMode.Open); //File stream to the file on which reader will work
            BinaryReader reader = new BinaryReader(file); //Binary reader for file

            Chunk chunk = new Chunk();
            ObjectBase objData = null;
            MaterialBase matData = null;

            Vector3[] positions = new Vector3[] { };
            Vector2[] texCoords = new Vector2[] { };
            uint[] indices = new uint[] { };

            List<string> materials = new List<string>();
            bool newObject = false;
            bool newMaterial = false;

            while (reader.BaseStream.Position < reader.BaseStream.Length - 6)
            {
                chunk = Chunk.Parse(reader);
                Write("ID : " + Microsoft.VisualBasic.Conversion.Hex(chunk.ID));
                switch (chunk.ID)
                {
                    case MainID:
                        break;

                    case EditorID:
                        break;

                    case M3DVersionID:
                        int version = reader.ReadInt32();
                        if (version < 3)
                            throw new NotSupportedException("The Library can read 3ds file having version grater or equal to 3!");

                        break;
                    case EditorVersionID:
                        int editorversion = reader.ReadInt32();
                        break;

                    case ObjectBlockID:
                        if (objData != null)
                        {
                            AddObject(device, model, objData, positions, texCoords, isRHCoordSys);
                            newObject = false;
                        }

                        objData = new ObjectBase();
                        objData.SubsetNames.Add(readString(reader));
                        //WriteLine("Object : " + objData.SubsetNames[0]);
                        newObject = true;
                        break;

                    case TriangularMeshID:
                        break;

                    case VerticesListID:
                        positions = readVerticesList(chunk, reader);
                        //WriteLine("Vertices : " + positions.Length);
                        break;

                    case FacesDescriptionID:
                        objData.indices = readFaceDescription(chunk, reader);
                        //WriteLine("Indices : " + objData.indices.Length);
                        break;

                    case MappingCoordinatesListID:
                        texCoords = readMappingCoordinatesList(chunk, reader);
                        //WriteLine("TextureCoords : " + texCoords.Length);
                        break;

                    case FacesMaterialID:
                        string materialName = readString(reader);
                        materials.Add(materialName);
                        break;

                    case MaterialBlockID:
                        break;

                    case MaterialNameID:

                        if (matData != null)
                        {
                            model.Materials.Add(new Material(device, matData));
                            newMaterial = false;
                        }

                        matData = new MaterialBase();
                        matData.Name = readString(reader);
                        matData.Folder = folder;
                        newMaterial = true;
                        break;

                    case AmbientColorID:
                        matData.Ambient = readColorChunk(reader);
                        //WriteLine(matData.Ambient);
                        break;

                    case DiffuseColorID:
                        matData.Diffuse = readColorChunk(reader);
                        //WriteLine(matData.Diffuse);
                        break;

                    case SpecularColorID:
                        matData.Specular = readColorChunk(reader);
                        //WriteLine(matData.Specular);
                        break;

                    case TextureMapID:
                        matData.Texture = readTexture(chunk, reader);
                        break;

                    case BumpMapID:
                        matData.BumpMap = readTexture(chunk, reader);
                        break;

                    //case ReflectionMapID: break;
                    //case LocalCoordinatesSystemID: break;
                    //case SmoothingGroupListID: break;
                    //case LightID: break;
                    //case SpotlightID: break;
                    //case CameraID: break;
                    default:
                        if (chunk.ID != 0)
                            chunk.SeekToEnd(reader);
                        Write(" unknown");
                        break;
                }
                WriteLine(", Start : " + Microsoft.VisualBasic.Conversion.Hex(chunk.start));
            }

            if (newObject)
                AddObject(device, model, objData, positions, texCoords, isRHCoordSys);

            if (newMaterial && matData != null)
                model.Materials.Add(new Material(device, matData));


            //Chunk mainchunk;
            //Chunk VersionChunk;

            //mainchunk = Chunk.Parse(reader);

            //if (mainchunk.ID != MainID)
            //{
            //    if (Debug) WriteLine(filepath + " model is corrupted!");
            //    return null;
            //}

            //VersionChunk = Chunk.Parse(reader);
            //if (reader.ReadInt32() < 3)
            //{
            //    if (Debug) WriteLine("Loader can only read 3ds file having version greater than 3!");
            //    return null;
            //}

            //do
            //{
            //   Chunk chunk = Chunk.Parse(reader);
            //    switch (chunk.ID)
            //    {
            //        case EditorID:
            //            readEditorData(device, chunk, reader, folder, computeNormals, ref model, isRHCoordSys);
            //            break;
            //        case KeyframerID:
            //            readKeyframeData(chunk, reader);
            //            break;
            //    }
            //    chunk.SeekToEnd(reader);
            //} while (reader.BaseStream.Position < mainchunk.end);

            while (materials.Count < model.Objects.Count)
            {
                materials.Add("  ");
            }

            if (materials.Count == 0)
            {
                foreach (Object3D item in model.Objects)
                {
                    item.MaterialIDs.Add(0);
                }
            }
            else
            {
                for (int i = 0; i < model.Objects.Count; i++)
                {
                    int index = model.Materials.FindIndex((Material m) => { return m.Name == materials[i]; });

                    if (index >= 0)
                    {
                        model.Objects[i].MaterialIDs.Add(index);
                    }
                    else
                    {
                        int index2 = model.Materials.IndexOf(Material.Default);
                        if (index2 >= 0)
                        {
                            model.Objects[i].MaterialIDs.Add(index2);
                        }
                        else
                        {
                            model.Materials.Add(Material.Default);
                            model.Objects[i].MaterialIDs.Add(model.Materials.Count - 1);
                        }
                    }
                }
            }

            reader.Dispose();
            file.Dispose();

            file = null;

            if (Debug) WriteLine(filepath + " model is loaded!");
            return model;
        }

        /// <summary>
        /// Adds an object to the model
        /// </summary>
        /// <param name="device">Direct3D 11 device</param>
        /// <param name="model">Our model</param>
        /// <param name="objData">Data of object</param>
        /// <param name="positions">position vectors of object</param>
        /// <param name="texCoords">texture coordinates of Object</param>
        /// <param name="isRHCoordSys">helps us to interpret that the model is whether Right handed or not</param>
        private static void AddObject(SlimDX.Direct3D11.Device device, Model model, ObjectBase objData, Vector3[] positions,Vector2[] texCoords,bool isRHCoordSys)
        {
            if (objData != null)
            {

                List<Vertex> vertices = new List<Vertex>();


                if (texCoords.Length > 0)
                {
                    for (int i = 0; i < positions.Length; i++)
                    {
                        Vector3 pos = positions[i];
                        Vector2 texCoord = texCoords[i];

                        if (isRHCoordSys)
                            pos.Z = -pos.Z;
                        if (isRHCoordSys)
                            texCoord.Y = 1.0f - texCoord.Y;

                        vertices.Add(new Vertex() { Position = pos, TexCoord = texCoord });
                        //WriteLine(vertices[i].Position);
                    }
                }
                else
                {
                    for (int i = 0; i < positions.Length; i++)
                    {
                        vertices.Add(new Vertex() { Position = positions[i], TexCoord = new Vector2() });
                        //WriteLine(vertices[i].Position);
                    }
                }
                objData.vertices = vertices.ToArray();
                model.Objects.Add(new Object3D(device, objData, true, isRHCoordSys: isRHCoordSys));
            }
        }

        //private static void readEditorData(SlimDX.Direct3D11.Device device, Chunk Editor,BinaryReader reader,string folder, bool computeNormals, ref Model model, bool isRHCoordSys = false)
        //{
        //    List<string> materials = new List<string>();

        //    do
        //    {
        //        Chunk chunk = Chunk.Parse(reader);

        //        switch (chunk.ID)
        //        {
        //            case ObjectBlockID:
        //                readObjectData(device, chunk, reader, computeNormals, ref materials, ref model, isRHCoordSys);
        //                break;
        //            case MaterialBlockID:
        //                readMaterialData(device, chunk, reader, folder,ref materials, ref model);
        //                break;
        //        }
        //        chunk.SeekToEnd(reader);
        //    } while (reader.BaseStream.Position < Editor.end);

        //}

        /// <summary>
        /// reads string from binaryreader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static string readString(BinaryReader reader)
        {
            var notacceptable = new byte[] { 34, 42, 47, 58, 60, 62, 63, 92, 124 };

            int i = 0;
            string text = null;
            char char_;
            char_ = (char) reader.ReadByte();

            while (char_ != '\0' && i < 20)
            {
                if (!notacceptable.Contains((byte)char_))
                    text += char_;
                char_ = (char) reader.ReadByte();
                i++;
            };

            return text;
        }

        //private static void readObjectData(SlimDX.Direct3D11.Device device, Chunk SpatialObject,BinaryReader reader, bool computeNormals, ref List<string> materials, ref Model model, bool isRHCoordSys = false)
        //{
        //    string name = readString(reader);
        //    //WriteLine(name);

        //    do
        //    {
        //        Chunk chunk = Chunk.Parse(reader);
        //        switch (chunk.ID)
        //        {
        //            case TriangularMeshID:
        //                //WriteLine("\n\nObject : " + name);

        //                Object3D obj = readTriangularMeshData(device, chunk,reader, computeNormals, ref materials, ref model, isRHCoordSys);
        //                obj.SubsetNames.Add(name);

        //                model.Objects.Add(obj);
        //                break;
        //            case LightID:
        //                readLightData(chunk, reader);
        //                break;
        //            case CameraID:
        //                readCameraData(chunk, reader);
        //                break;
        //        }
        //        chunk.SeekToEnd(reader);
        //    } while (reader.BaseStream.Position < SpatialObject.end);
        //}
        //private static Object3D readTriangularMeshData(SlimDX.Direct3D11.Device device, Chunk Triangularmesh,BinaryReader reader, bool computeNormals, ref List<string> materials, ref Model model, bool isRHCoordSys = false)
        //{
        //    Object3D obj;
        //    Vector3[] Positions = new Vector3[] { };
        //    Vector2[] Texcoords = new Vector2[] { };
        //    uint[] indices = new uint[] { };
        //    List<Vertex> vertices = new List<Vertex>();

        //    do
        //    {
        //        Chunk chunk = Chunk.Parse(reader);
        //        switch (chunk.ID)
        //        {
        //            case VerticesListID:
        //                Positions = readVerticesList(chunk, reader);
        //                break;
        //            case FacesDescriptionID:
        //                indices = readFaceDescription(chunk, reader);
        //                break;
        //            case MappingCoordinatesListID:
        //                Texcoords = readMappingCoordinatesList(chunk, reader);
        //                break;
        //            case LocalCoordinatesSystemID:
        //                readLocalCoordinatesSystem(chunk, reader);
        //                break;
        //        }
        //        chunk.SeekToEnd(reader);
        //    } while (reader.BaseStream.Position < Triangularmesh.end);

        //    //WriteLine("Vertices : ");
        //    if (Texcoords.Length > 0)
        //    {
        //        for (int i = 0; i < Positions.Length; i++)
        //        {
        //            Vector3 pos = Positions[i];
        //            Vector2 texCoord = Texcoords[i];

        //            if (isRHCoordSys)
        //                pos.Z = -pos.Z;
        //            if (isRHCoordSys)
        //                texCoord.Y = 1.0f - texCoord.Y;

        //            vertices.Add(new Vertex() { Position = pos, TexCoord = texCoord });
        //            //WriteLine(vertices[i].Position);
        //        }
        //    } else {

        //        for (int i = 0; i < Positions.Length; i++)
        //        {
        //            vertices.Add(new Vertex() { Position = Positions[i], TexCoord = new Vector2()});
        //            //WriteLine(vertices[i].Position);
        //        }
        //    }

        //    //WriteLine("Indices : ");
        //    //foreach (int index in indices)
        //    //{
        //    //    WriteLine(index);
        //    //}
        //    obj = new Object3D(device, vertices.ToArray(), indices.ToArray(), -1, null,computeNormal: computeNormals, isRHCoordSys: isRHCoordSys);
        //    obj.Tranformation = Matrix.Identity;
        //    return obj;
        //}
        //private static object readLocalCoordinatesSystem(Chunk chunk, BinaryReader reader)
        //{
        //    List<object> objects = new List<object>();

        //    return objects.ToArray();
        //}
        /// <summary>
        /// Reads mapping coordinates List from the chunk
        /// </summary>
        /// <param name="chunk">mapping coordinates chunk</param>
        /// <param name="reader">Binaryreader for reading the stream of model file</param>
        /// <returns></returns>
        private static Vector2[] readMappingCoordinatesList(Chunk chunk, BinaryReader reader)
        {
            List<Vector2> texCoords = new List<Vector2>();
            int numbers = reader.ReadUInt16();

            for(int i =0; i < numbers; i++)
            {
                texCoords.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
            }

            return texCoords.ToArray();
        }

        private static uint[] readFaceDescription(Chunk Face, BinaryReader reader)
        {
            //TODO: First read the face as this way: read integer, say it as faces count and read faces,
            //      which having four component in a integer list, but store only first three
            //      And then read face material chunk and smoothing group chunk
            List<uint> indices = new List<uint>();
            int facesCount = reader.ReadUInt16();

            for (int i = 0; i < facesCount; i++)
            {
                uint a, b, c, d;
                a = reader.ReadUInt16();
                b = reader.ReadUInt16();
                c = reader.ReadUInt16();
                d = reader.ReadUInt16();
                indices.AddRange(new uint[]{ a,b,c});
            }
            return indices.ToArray();
        }

        private static Vector3[] readVerticesList(Chunk chunk, BinaryReader reader)
        {
            int verticeCount = reader.ReadUInt16();
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < verticeCount; i++)
            {
                vertices.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
            }
            chunk.SeekToEnd(reader);
            return vertices.ToArray();
        }

        //private static void readLightData(Chunk Light,BinaryReader reader)
        //{
        //     //TODO: First read Light Data and then read SpotLight chunk, if any
        //}
        //private static void readCameraData(Chunk Light,BinaryReader reader)
        //{

        //}
        //private static void readMaterialData(SlimDX.Direct3D11.Device device, Chunk Material,BinaryReader reader,string folder, ref List<string> materials, ref Model model)
        //{
        //    MaterialBase mat = new MaterialBase();
        //    mat.Folder = folder;

        //    do
        //    {
        //        Chunk chunk = Chunk.Parse(reader);

        //        switch (chunk.ID)
        //        {
        //            case MaterialNameID:
        //                mat.Name = readString(reader);
        //                break;
        //            case AmbientColorID:
        //                mat.Ambient = readColorChunk(reader);
        //                WriteLine("Ambient " + mat.Ambient);
        //                break;
        //            case DiffuseColorID:
        //                mat.Diffuse = readColorChunk(reader);
        //                WriteLine("Diffuse " + mat.Diffuse);
        //                break;
        //            case SpecularColorID:
        //                mat.Specular = readColorChunk(reader);
        //                WriteLine("Specular " + mat.Specular);
        //                break;
        //            case TextureMapID:
        //                mat.Texture = readTexture(chunk,reader);
        //                mat.TextureType = TextureType.texture2D;
        //                break;
        //            case BumpMapID:
        //                mat.BumpMap = readTexture(chunk,reader);
        //                mat.BumpType = TextureType.texture2D;
        //                break;
        //            case ReflectionMapID:
        //                readTexture(chunk,reader);
        //                //mat.BumpMap = 
        //                //mat.BumpType = TextureType.texture2D;
        //                break;
        //        }
        //        chunk.SeekToEnd(reader);
        //    } while (reader.BaseStream.Position < Material.end);

        //    model.Materials.Add(new Material(device, mat));
        //}

        private static string readTexture(Chunk Texture,BinaryReader reader)
        {
            string name = null;
            do
            {
                Chunk chunk = Chunk.Parse(reader);
                switch (chunk.ID)
                {
                    case MappingFilenameID:
                        name = readString(reader);
                        break;
                    case MappingParaemtersID:
                        break;
                }

            } while (reader.BaseStream.Position < Texture.end);
            return name;
        }
        private static Color4 readColorChunk(BinaryReader reader)
        {
            Color4 color = new Color4();
            int type = reader.ReadUInt16();
            uint unknown;
            //color = readColorTrue(reader);
            //WriteLine("0x" + Microsoft.VisualBasic.Conversion.Hex(type));
            unknown = reader.ReadUInt32();

            switch (type)
            {
                case ColorRGB:
                    color = readColorRGB(reader);
                    //WriteLine(color);
                    break;
                case ColorTrue:
                    color = readColorTrue(reader);
                    //WriteLine(color);
                    break;
            }
            
            return color;
        }
        private static Color4 readColorRGB(BinaryReader reader)
        {
            return new Color4()
            {
                Red = reader.ReadSingle(),
                Green = reader.ReadSingle(),
                Blue = reader.ReadSingle(),
                Alpha = 1
            };
        }
        private static Color4 readColorTrue(BinaryReader reader)
        {
            return Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        //private static void readKeyframeData(Chunk KeyFrame,BinaryReader reader)
        //{
        //    do
        //    {
        //        Chunk chunk = Chunk.Parse(reader);
        //        switch (chunk.ID)
        //        {
        //            case MeshInformationBlockID:
        //                readMeshInformationBlock(chunk, reader);
        //                break;
        //            case SpotLightInformationBlockID:
        //                readSpotLightInformationBlock(chunk,reader);
        //                break;
        //            case FramesID:
        //                readFramesData(chunk, reader);
        //                break;
        //        }
        //        chunk.SeekToEnd(reader);
        //    } while (reader.BaseStream.Position < KeyFrame.end);
        //}
        //private static void readMeshInformationBlock(Chunk chunk, BinaryReader reader)
        //{

        //}
        //private static void readSpotLightInformationBlock(Chunk chunk, BinaryReader reader)
        //{

        //}
        //private static void readFramesData(Chunk FramesChunk, BinaryReader reader)
        //{
        //    string name;
        //    do
        //    {
        //        Chunk chunk = Chunk.Parse(reader);
        //        switch (chunk.ID)
        //        {
        //            case ObjectNameID:
        //                name = readString(reader);
        //                break;
        //            case ObjectPivotPointID:
        //                readObjectPivotPoint(chunk, reader);
        //                break;
        //            case PositionTrackID:
        //                readPositionTrackData(chunk, reader);
        //                break;
        //            case RotationTrackID:
        //                readRotationTrackData(chunk, reader);
        //                break;
        //            case ScaleTrackID:
        //                readScaleTrackData(chunk, reader);
        //                break;
        //            case HierarchyPositionID:
        //                readHierarchyPosition(chunk, reader);
        //                break;
        //        }
        //        //chunk.SeekToEnd(reader);
        //    } while (reader.BaseStream.Position + 6 < FramesChunk.end);
        //}
        //private static void readObjectPivotPoint(Chunk chunk, BinaryReader reader)
        //{

        //}
        //private static void readPositionTrackData(Chunk chunk, BinaryReader reader)
        //{

        //}
        //private static void readRotationTrackData(Chunk chunk, BinaryReader reader)
        //{

        //}
        //private static void readScaleTrackData(Chunk chunk, BinaryReader reader)
        //{

        //}
        //private static void readHierarchyPosition(Chunk chunk, BinaryReader reader)
        //{

        //}
    }
}
