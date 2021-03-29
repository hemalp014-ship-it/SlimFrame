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
using SlimFrame.Buffers;
using System.Windows.Forms;

using static System.Diagnostics.Debug;

namespace SlimFrame.Models
{
    /// <summary>
    /// Holds Vertex data
    /// </summary>
    public struct Vertex
    {
        public Vector3 Position;   // The position of the vertex in 3D space.
        public Vector3 Normal;       // The color to use for this vertex when we are not using textured mode.
        public Vector2 TexCoord;   // The textures coordinates for this vertex.  We need these when we are using textured mode.

        static public int Size{get;} = 32;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);
            writer.Write(Normal.X);
            writer.Write(Normal.Y);
            writer.Write(Normal.Z);
            writer.Write(TexCoord.X);
            writer.Write(TexCoord.Y);
        }

        public static unsafe Vertex Read(BinaryReader reader)
        {
            fixed (byte* data = reader.ReadBytes(sizeof(Vertex)))
            {
                return *(Vertex*)data;
            }
        }

    }

    /// <summary>
    /// Type of the Texture
    /// </summary>
    public enum TextureType : int
    {
        None = -1,      // No texture is associated
        texture2D = 0,  // 2D texture is associated
        texture3D,      // 3D texture is associated
    }

    /// <summary>
    /// Holds Chunk data
    /// </summary>
    struct Chunk
    {
        BinaryReader m_reader;

        /// <summary>
        /// ID of chunk, for indentifying the data stored in chunk
        /// </summary>
        public ushort id;
        /// <summary>
        /// Lenth of chunk
        /// </summary>
        public long length;
        /// <summary>
        /// Position of chunk
        /// </summary>
        public long position;

        /// <summary>
        /// End of chunk
        /// </summary>
        public long End
        {
            get
            {
                return position + length;
            }
        }

        /// <summary>
        /// Reads chunk information from model file.
        /// </summary>
        /// <param name="reader">BinaryReader for the model file</param>
        public void Read(BinaryReader reader)
        {
            m_reader = reader;
            position = reader.BaseStream.Position;
            id = reader.ReadUInt16();
            length = reader.ReadUInt32();
        }

        /// <summary>
        /// Seeks the stream from which the chuck had been readout to the end of chunk
        /// </summary>
        public void SeektoEnd()
        {
            m_reader.BaseStream.Seek(End, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// Model handling rendering of Scene Items
    /// </summary>
    public class Model : IDisposable
    {
        #region Member Variables

        /// <summary>
        /// Objects of Model
        /// </summary>
        public List<Object3D> Objects;
        /// <summary>
        /// Materials of Model
        /// </summary>
        public List<Material> Materials;
        /// <summary>
        /// Transformation of Model
        /// </summary>
        public Matrix Tranformation;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of Model
        /// </summary>
        public Model()
        {
            Objects = new List<Object3D>();
            Materials = new List<Material>();
        }

        /// <summary>
        /// Creates a new instance of Model from ModelHandler structure
        /// </summary>
        /// <param name="dev">3D Rendering device</param>
        /// <param name="Model">ModelHandler structure holding model data</param>
        /// <param name="Debug"></param>
        public Model(Device dev, ModelHandler Model, bool Debug = false)
        {
            Objects = new List<Object3D>();
            Materials = new List<Material>();

            foreach (ObjectBase obj in Model.Objects)
                Objects.Add(new Object3D(dev, obj, Debug));
            foreach (MaterialBase mat in Model.Materials)
                Materials.Add(new Material(dev, mat, Debug));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads data for current Model to file
        /// </summary>
        /// <param name="device">3D Rendering device</param>
        /// <param name="filepath">path of file where model is stored</param>
        /// <param name="Debug">Set true for get debugging information or otherwise false</param>
        public void Load(Device device, string filepath, bool Debug = false)
        {
            string folder = filepath.Substring(0, filepath.LastIndexOf("\\") + 1);

            //WriteLine(folder);

            FileStream file = new FileStream(filepath, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);

            Chunk c, c1;
            Object3D b = new Object3D();
            Material m = new Material();

            c = new Chunk();
            c1 = new Chunk();
            c.Read(reader);

            if (c.id != 0xA000)
            {
                if (Debug) WriteLine("It's not a Model file!");
                return;
            }

            do
            {
                c.Read(reader);

                switch (c.id)
                {
                    case 0xA100:
                        do
                        {
                            c1.Read(reader);

                            if (c1.id == 0xB000)
                            {
                                b.Read(device, reader, c1.End);
                                Objects.Add(b);
                            }
                            c1.SeektoEnd();
                            //reader.BaseStream.Seek(c1.End, SeekOrigin.Begin);
                        } while (reader.BaseStream.Position + 6 < c.End);
                        break;

                    case 0xA200:
                        do
                        {
                            c1.Read(reader);

                            if (c1.id == 0xB100)
                            {
                                m.Read(device, reader, folder);
                                Materials.Add(m);
                                //WriteLine("Model Added!");
                                c1.SeektoEnd();
                                //reader.BaseStream.Seek(c1.End, SeekOrigin.Begin);
                            }
                        } while (reader.BaseStream.Position + 6 < c.End);
                        break;
                    case 0xA300:
                        break;
                    case 0xA400:
                        break;
                }
                c.SeektoEnd();
            } while (reader.BaseStream.Position + 6 < reader.BaseStream.Length);

            reader.Close();
            file.Close();
        }
        
        /// <summary>
        /// Saves data of current Model to file
        /// </summary>
        /// <param name="devCon">3D Rendering device context</param>
        /// <param name="filepath">path of file where model is to be stored</param>
        /// <param name="ReplaceOldFiles">Set true for replace old files or otherwise false</param>
        public void Save(DeviceContext devCon, string filepath, bool ReplaceOldFiles = false)
        {
            Save(devCon, filepath, this, ReplaceOldFiles);
        }

        #endregion

        #region Shared Methods

        public static FileStream file;

        /// <summary>
        /// Loads data for Model from File
        /// </summary>
        /// <param name="device">3D rendering device</param>
        /// <param name="filepath">path of Model file</param>
        /// <param name="Debug">Set true for get debugging information or otherwise false</param>
        /// <returns>A model loaded from file</returns>
        public static void Load(Device device,ref Model model, string filepath, bool Debug = false)
        {
            if (model == null)
                model = new Model();

            string folder = filepath.Substring(0, filepath.LastIndexOf("\\") + 1);

            //WriteLine(folder);

            file = new FileStream(filepath, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);

            Chunk c, c1;

            c = new Chunk();
            c1 = new Chunk();
            c.Read(reader);

            if(c.id != 0xA000)
            {
                if (Debug) WriteLine("It's not a Model file!");
                return;
            }

            do
            {
                c.Read(reader);

                switch (c.id)
                {
                    case 0xA100:
                        do
                        {
                            c1.Read(reader);

                            if (c1.id == 0xB000)
                            {
                                model.Objects.Add(Object3D.FromStream(device, reader, c1.End));
                                c1.SeektoEnd();
                            }
                            //reader.BaseStream.Seek(c1.End, SeekOrigin.Begin);
                        } while (reader.BaseStream.Position < c.End);
                        break;

                    case 0xA200:
                        do
                        {
                            c1.Read(reader);

                            if (c1.id == 0xB100)
                            {
                                model.Materials.Add(Material.FromStream(device,reader,folder));
                                c1.SeektoEnd();
                            }
                                //WriteLine("Model Added!");
                                //reader.BaseStream.Seek(c1.End, SeekOrigin.Begin);
                        } while (reader.BaseStream.Position < c.End);
                        break;
                    case 0xA300:
                        break;
                    case 0xA400:
                        break;
                }
                //reader.BaseStream.Seek(c.End, SeekOrigin.Begin);
            } while (reader.BaseStream.Position < reader.BaseStream.Length);

            reader.Close();
            file.Close();
            file = null;
        }

        private static void CheckDirectory(string path)
        {
            string foldercheck = path;
            do
            {
                foldercheck = Directory.GetParent(foldercheck).FullName;
            } while (!Directory.Exists(foldercheck));

            string[] directories = path.Replace(foldercheck, "").Split('\\');
            foreach (string directory in directories)
            {
                foldercheck = Path.Combine(foldercheck, directory);
                Directory.CreateDirectory(foldercheck);
            }
        }

        /// <summary>
        /// Saves data of Model to file
        /// </summary>
        /// <param name="devCon">3D Rendering device context</param>
        /// <param name="filepath">path of file where model is stored</param>
        /// <param name="model">Model to be saved</param>
        /// <param name="ReplaceOldFiles">Set true for replace old files or otherwise false</param>
        public static void Save(DeviceContext devCon, string filepath, Model model, bool ReplaceOldFiles = false)
        {
            List<int> In = new List<int>();
            List<int> Out = new List<int>();

            string path = Path.GetFullPath(filepath);
            string folder = Directory.GetParent(filepath.Substring(0, filepath.LastIndexOf("\\") + 1)).FullName;

            CheckDirectory(folder);

            FileStream file = new FileStream(path, FileMode.OpenOrCreate);
            BinaryWriter writer = new BinaryWriter(file);

            writer.Write((ushort)0xA000);
            In.Add((int)writer.BaseStream.Position);
            writer.Write(0);

            writer.Write((ushort)0xA100);
            In.Add((int)writer.BaseStream.Position);
            writer.Write(0);

            foreach (Object3D obj in model.Objects)
            {
                obj.Write(devCon, writer);
            }

            Out.Add((int)writer.BaseStream.Position);

            writer.Write((ushort)0xA200);
            In.Add((int)writer.BaseStream.Position);
            writer.Write(0);

            foreach (Material mat in model.Materials)
            {
                mat.Write(devCon, writer, folder, ReplaceOldFiles);
            }

            Out.Add((int)writer.BaseStream.Position);

            for (int i = 1; i < In.Count; i++)
            {
                writer.Seek(In[i], SeekOrigin.Begin);
                writer.Write(Out[i - 1] - In[i] + 2);
            }

            writer.Seek(In.First(), SeekOrigin.Begin);
            writer.Write(Out.Last());

            writer.Close();
            file.Close();
        }

        public static Model FromFile(Device device, string filepath, bool Debug = false)
        {
            Model model = new Model();
            Load(device, ref model, filepath, Debug);
            return model;
        }

        #endregion

        #region Disposal
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    foreach (Object3D obj in Objects)
                        obj?.Dispose();
                    Objects.Clear();

                    foreach (Material mat in Materials)
                        mat?.Dispose();
                    Materials.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}