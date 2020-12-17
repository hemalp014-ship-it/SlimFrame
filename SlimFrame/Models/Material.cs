using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.IO;

using SlimDX;
using SlimDX.Direct3D11;
using Microsoft.VisualBasic;
using System.Diagnostics;
using SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using static System.Diagnostics.Debug;
using System.Drawing;

namespace SlimFrame.Models
{

    /// <summary>
    /// Material for Models
    /// </summary>
    public class Material : IDisposable
    {
        #region Member Variables

        /// <summary>
        /// Material ID (for software usage)
        /// </summary>
        public int ID;
        /// <summary>
        /// Material Name (for Human usage)
        /// </summary>
        public string Name;
        /// <summary>
        /// Diffuse Color of Material
        /// </summary>
        public Color4 Diffuse;
        /// <summary>
        /// Ambient Color of Material
        /// </summary>
        public Color4 Ambient;
        /// <summary>
        /// Specular Color of Material
        /// </summary>
        public Color4 Specular;
        /// <summary>
        /// Emissive Color of Material
        /// </summary>
        public Color4 Emissive;
        /// <summary>
        /// Texture of Material
        /// </summary>
        public ShaderResourceView Texture;
        /// <summary>
        /// Name of Texture of Material
        /// </summary>
        public string TextureName;
        /// <summary>
        /// Type of Texture of Material
        /// </summary>
        public TextureType TextureType;
        /// <summary>
        /// BumpMap of Material
        /// </summary>
        public ShaderResourceView BumpMap;
        /// <summary>
        /// Name of BumpMap of Material
        /// </summary>
        public string BumpMapName;
        /// <summary>
        /// Type of BumpMap of Material
        /// </summary>
        public TextureType BumpType;
        /// <summary>
        /// Transparency of Material
        /// </summary>
        public float Transparency;
        /// <summary>
        /// Reflectivity of Material
        /// </summary>
        public float Reflectivity;
        /// <summary>
        /// Refractivity of Material
        /// </summary>
        public float Refractivity;
        /// <summary>
        /// Specular power of Material
        /// </summary>
        public float SpecularPower;
        /// <summary>
        /// Emissive power of Material
        /// </summary>
        public float EmissivePower;
        /// <summary>
        /// Determine whether it has texture or not
        /// </summary>
        public bool hasTexture
        {
            get { return TextureType != TextureType.None; }
        }

        /// <summary>
        /// Determine whether it has bump map or not
        /// </summary>
        public bool hasBumpMap
        {
            get { return BumpType != TextureType.None; }
        }
        /// <summary>
        /// Determines how much the Object will dissolve to surround when viewed from the camera.
        /// </summary>
        public float Dissolve;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of Material
        /// </summary>
        public Material()
        {

        }

        public static Material Default
        {
            get
            {
                Material material = new Material();
                material.Diffuse = new Color4(1f, 1f, 1f, 1f);
                material.Ambient = new Color4(0.2f, 0.2f, 0f, 0f);
                material.TextureType = TextureType.None;
                material.BumpType = TextureType.None;
                material.Name = "Default";

                return material;
            }
        }

        /// <summary>
        /// Creates a new instance of Material from MaterialBase structure
        /// </summary>
        /// <param name="dev">3D rendering device</param>
        /// <param name="mat">MaterialBase structure holding material data</param>
        /// <param name="Debug">Set true for get debugging information or otherwise false</param>
        public Material(D3D.Device dev, MaterialBase mat, bool Debug = false)
        {

            ID = mat.ID;
            Name = mat.Name;
            Diffuse = mat.Diffuse;
            Ambient = mat.Ambient;
            Specular = mat.Specular;
            Emissive = mat.Emissive;
            TextureType = mat.TextureType;
            BumpType = mat.BumpType;
            Transparency = mat.Transparency;
            Reflectivity = mat.Reflectivity;
            Refractivity = mat.Refractivity;
            SpecularPower = mat.SpecularPower;
            EmissivePower = mat.EmissivePower;
            Dissolve = mat.Dissolve;
            TextureName = "";
            BumpMapName = "";


            if (mat.Texture != null && mat.Texture.Length > 0)
            {
                if (Debug) System.Diagnostics.Debug.WriteLine("Texture Loaded : " +  mat.Folder + mat.Texture);
                Texture = LoadTexture(dev, Path.Combine(mat.Folder , mat.Texture), ref TextureType);

                TextureName = mat.Texture.Substring(0, mat.Texture.LastIndexOf("."));
                //TextureResource = new ShaderResourceView(dev, Texture);
            }
            else
            {
                TextureType = TextureType.None;
            }
            if (mat.BumpMap != null && mat.BumpMap.Length > 0)
            {
                if (Debug) System.Diagnostics.Debug.WriteLine("BumpMap Loaded : " +  mat.Folder + mat.BumpType);
                BumpMap = LoadTexture(dev, Path.Combine(mat.Folder, mat.BumpMap), ref BumpType);

                if (mat.BumpMap.Length > 1)
                    BumpMapName = mat.BumpMap.Substring(0, mat.BumpMap.LastIndexOf("."));
                else
                    BumpMapName = mat.BumpMap;
                //BumpMapResource = new ShaderResourceView(dev, BumpMap);
            }
            else
            {
                BumpType = TextureType.None;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes material information to the model file and saves material textures to folder containing model file
        /// </summary>
        /// <param name="devCon">3D Rendering device context</param>
        /// <param name="writer">BinaryWriter for the model file</param>
        /// <param name="folderPath">Path of folder where model file is to be stored</param>
        /// <param name="ReplaceOldFiles">Set true for replace old files or otherwise false</param>
        public void Write(DeviceContext devCon, BinaryWriter writer, string folderPath, bool ReplaceOldFiles = false)
        {
            Write(devCon, writer, folderPath, this, ReplaceOldFiles);
        }

        /// <summary>
        /// Reads material information from model file and loads Textures using path specified in model file
        /// </summary>
        /// <param name="device">3D Rendering device</param>
        /// <param name="reader">BinaryReader for the model file</param>
        /// <param name="FolderPath">Path of folder where model file is stored</param>
        public void Read(D3D.Device device, BinaryReader reader, string FolderPath)
        {
            Read(this, device, reader, FolderPath);
        }

        #endregion

        #region Shared Methods

        /// <summary>
        /// Writes material information to the model file and saves material textures to folder containing model file
        /// </summary>
        /// <param name="devCon">3D Rendering device context</param>
        /// <param name="writer">BinaryWriter for the model file</param>
        /// <param name="folderPath">Path of folder where model file is to be stored</param>
        /// <param name="ReplaceOldFiles">Set true for replace old files or otherwise false</param>
        /// <param name="material">Material of which data is to be written to model file</param>
        public static void Write(DeviceContext devCon, BinaryWriter writer, string folderPath, Material material, bool ReplaceOldFiles = false)
        {
            int In, Out;

            writer.Write(MaterialBase.cMaterialID);
            In = (int)writer.BaseStream.Position;
            writer.Write(0);
            writer.Write(material.ID);
            writer.Write(material.Name);
            Write(writer, material.Diffuse);
            Write(writer, material.Ambient);
            Write(writer, material.Specular);
            Write(writer, material.Emissive);
            writer.Write(SaveTexture(devCon, material.Texture, material.TextureType, folderPath, material.TextureName, ReplaceOldFiles));
            writer.Write((int)material.TextureType);
            writer.Write(SaveTexture(devCon, material.BumpMap, material.TextureType, folderPath, material.BumpMapName, ReplaceOldFiles));
            writer.Write((int)material.BumpType);
            writer.Write(material.Transparency);
            writer.Write(material.Reflectivity);
            writer.Write(material.Refractivity);
            writer.Write(material.SpecularPower);
            writer.Write(material.EmissivePower);
            writer.Write(material.Dissolve);
            Out = (int)writer.BaseStream.Position;

            writer.BaseStream.Position = In;
            writer.Write(Out - In + 2);
            writer.BaseStream.Position = Out;
        }

        /// <summary>
        /// Saves texture to the folder containig model file
        /// </summary>
        /// <param name="devCon">3D Rendering device context</param>
        /// <param name="texture">ShaderResourceView containing texture resource</param>
        /// <param name="type">Type of Texture</param>
        /// <param name="folderPath">Path of folder where model file is stored</param>
        /// <param name="file">Name of file without extension</param>
        /// <param name="ReplaceOldFiles">Set true for replace old files or otherwise false</param>
        /// <returns>Name of file to which texture is stored (Because of sometimes due to lack of
        /// avilablity of given name for file the name might be changed, so it is required to give the name of file stored)
        /// </returns>
        private static string SaveTexture(DeviceContext devCon, ShaderResourceView texture, TextureType type, string folderPath, string file, bool ReplaceOldFiles = false)
        {
            if (file == null) return "";
            if (file.Length == 0) return file;

            if (texture == null) return "";

            string filepath;
            string filebase = file.Substring(file.LastIndexOf('\\') + 1);
            string extension;

            ImageFileFormat format;
            int fileNumber = 0;

            if (type == TextureType.texture2D)
            {
                extension = ".png";
                format = ImageFileFormat.Png;
            }
            else if (type == TextureType.texture3D)
            {
                extension = ".dds";
                format = ImageFileFormat.Dds;
            }
            else
            {
                return null;
            }
            recheck:


            filepath = Path.Combine(folderPath, filebase) + extension;

            if (File.Exists(filepath) && !ReplaceOldFiles)
            {
                    fileNumber++;
                    filebase = file + ((fileNumber < 10) ? ("0" + fileNumber.ToString()) : fileNumber.ToString());
                    goto recheck;
            }
            else
            {
                //Texture2D.ToFile(devCon, texture, format, filepath);
                //texture.Resource.AsSurface();
                SlimDX.Direct3D11.Resource.SaveTextureToFile(devCon, texture.Resource, format, filepath);
            }

            return filebase + extension;
        }

        /// <summary>
        /// Writes Color4 colour using BinaryWriter
        /// </summary>
        /// <param name="reader">BinaryWriter of model file</param>
        /// <param name="color">Input Color</param>
        public static void Write(BinaryWriter writer, Color4 color)
        {
            writer.Write(color.Alpha);
            writer.Write(color.Red);
            writer.Write(color.Green);
            writer.Write(color.Blue);
        }

        public static Material FromStream(D3D.Device device, BinaryReader reader, string FolderPath)
        {
            Material mat = new Material();
            Read(mat, device, reader, FolderPath);
            return mat;
        }

        /// <summary>
        /// Reads material information from model file and loads Textures using path specified in model file
        /// </summary>
        /// <param name="device">3D Rendering device</param>
        /// <param name="reader">BinaryReader for the model file</param>
        /// <param name="FolderPath">Path of folder where model file is stored</param>
        /// <param name="material">Material for which data is to be readout from model file</param>
        public static void Read(Material material, D3D.Device device, BinaryReader reader, string FolderPath)
        {
            string Folder = FolderPath;
            string tex = FolderPath;
            string bumpMap = FolderPath;

            material.ID = reader.ReadInt32();
            material.Name = reader.ReadString();

            Read(reader, ref material.Diffuse);
            Read(reader, ref material.Ambient);
            Read(reader, ref material.Specular);
            Read(reader, ref material.Emissive);

            tex = reader.ReadString();
            material.TextureType = (TextureType)reader.ReadInt32();
            bumpMap = reader.ReadString();
            material.BumpType = (TextureType)reader.ReadInt32();
            material.Transparency = reader.ReadSingle();
            material.Reflectivity = reader.ReadSingle();
            material.Refractivity = reader.ReadSingle();
            material.SpecularPower = reader.ReadSingle();
            material.EmissivePower = reader.ReadSingle();
            material.Dissolve = reader.ReadSingle();

            material.TextureName = "";
            material.BumpMapName = "";

            if (tex.Length > 0)
            {
                //if (Debug) System.Diagnostics.Debug.WriteLine(mat.Folder + mat.Texture);
                material.Texture = LoadTexture(device, Folder + tex, ref material.TextureType);

                material.TextureName = tex.Substring(0, tex.LastIndexOf("."));
            }
            else
            {
                material.TextureType = TextureType.None;
            }
            if (bumpMap.Length > 0)
            {
                //if (Debug) System.Diagnostics.Debug.WriteLine(mat.Folder + mat.BumpType);
                material.BumpMap = LoadTexture(device, Folder + bumpMap, ref material.BumpType);

                material.BumpMapName = bumpMap.Substring(0, bumpMap.LastIndexOf("."));
            }
            else
            {
                material.BumpType = TextureType.None;
            }
        }

        /// <summary>
        /// Reads Color4 colour using BinaryReader
        /// </summary>
        /// <param name="reader">BinaryWriter of model file</param>
        /// <param name="color">Output Color</param>
        public static void Read(BinaryReader reader, ref Color4 color)
        {
            color.Alpha = reader.ReadSingle();
            color.Red = reader.ReadSingle();
            color.Green = reader.ReadSingle();
            color.Blue = reader.ReadSingle();
        }

        /// <summary>
        /// Loads texture from path specified in model file
        /// </summary>
        /// <param name="dev">3D Rendering device</param>
        /// <param name="texture">Path of Texture file</param>
        /// <param name="type">Type of Texture</param>
        /// <returns>ShaderResourceView containing texture resource</returns>
        public static ShaderResourceView LoadTexture(D3D.Device dev, string texture, ref TextureType type)
        {

            if (texture == "") return null;

            if (type == TextureType.texture3D)
            {
                Texture2D tex = Texture2D.FromFile(dev, texture);

                ShaderResourceView output;
                output = new ShaderResourceView(dev, tex);
                tex.Dispose();
                return output;
            }
            else if (type == TextureType.texture2D)
            {
                string path = Path.GetFullPath(texture);

                if (File.Exists(path))
                    return ShaderResourceView.FromFile(dev, path);
                else
                {
                    WriteLine("File Not Found : " + path);
                    type = TextureType.None;
                }
                return null;
            }

            return null;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Texture?.Dispose();
                    BumpMap?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes Material
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}