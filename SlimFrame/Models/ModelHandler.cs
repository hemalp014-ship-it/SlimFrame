using SlimDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame.Models
{
    /// <summary>
    /// Handles the Model information
    /// </summary>
    public class ModelHandler
    {
        #region Member Variables

        public List<ObjectBase> Objects;
        public List<MaterialBase> Materials;

        public MaterialBase DefaultMaterial = MaterialBase.Default;
        #endregion

        #region Constructor

        public ModelHandler()
        {
            Objects = new List<ObjectBase>();
            Materials = new List<MaterialBase>();
        }


        public ModelHandler(Vertex[] vertices, uint[] indices)
        {
            Objects = new List<ObjectBase>();
            Materials = new List<MaterialBase>();

            ObjectBase baseObject = new ObjectBase()
            {
                vertices = vertices,
                indices = indices,
                Tranformation = Matrix.Identity,
            };

            baseObject.MaterialIDs.Add(DefaultMaterial.ID);

            Objects.Add(baseObject);
            Materials.Add(DefaultMaterial);
        }
        public ModelHandler(Vertex[] vertices, uint[] indices, Matrix transformation)
        {
            Objects = new List<ObjectBase>();
            Materials = new List<MaterialBase>();

            ObjectBase Object = new ObjectBase()
            {
                vertices = vertices,
                indices = indices,
                Tranformation = transformation,
            };
            Object.MaterialIDs.Add(DefaultMaterial.ID);

            Objects.Add(Object);
            Materials.Add(DefaultMaterial);
        }
        public ModelHandler(Vertex[] vertices, uint[] indices, Matrix transformation, MaterialBase material)
        {
            Objects = new List<ObjectBase>();
            Materials = new List<MaterialBase>();

            ObjectBase Object = new ObjectBase()
            {
                vertices = vertices,
                indices = indices,
                Tranformation = transformation,
            };
            Object.MaterialIDs.Add(material.ID);

            Objects.Add(Object);
            Materials.Add(material);
        }

        #endregion

        #region Methods

        public void GenerateCube(string name)
        {
            GenerateCube(DefaultMaterial, name);
        }

        public void GenerateCube(MaterialBase material, string name)
        {

            Objects = new List<ObjectBase>();
            Materials = new List<MaterialBase>();

            float halfLength = 0.5f;
            //float lengthby2 = length / 2;

            Vertex[] vertices = {

                //Up
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 1, 1)},
                //Down
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 1, 1)},

                //Left
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 1, 1)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 0, 0)},
                //Right
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 1, 1)},
                
                //Front
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 1, 1)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 0, 0)},
                //Back
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 1, 1)},

            };

            //Note: If your indices are not working then try change vertices in clockwise or counter-clockwise direction

            uint[] indices = new uint[6 * 6];
            for (uint i = 0, j = 0; i < 6; i++, j += 4)
            {
                indices[6 * i + 0] = j + 0;
                indices[6 * i + 1] = j + 1;
                indices[6 * i + 2] = j + 2;

                indices[6 * i + 3] = j + 0;
                indices[6 * i + 4] = j + 2;
                indices[6 * i + 5] = j + 3;
            }

            ObjectBase Object = new ObjectBase()
            {
                vertices = vertices,
                indices = indices,
                Tranformation = Matrix.Scaling(1, 1, 1),
            };
            Object.meshSubsets.Add(0);
            Object.meshSubsets.Add(indices.Length);
            Object.MaterialIDs.Add(material.ID);
            //Object3D.ApplySubsets(new int[] { 0, indices.Length }, new MaterialBase[] { material });
            Object.SubsetNames = new List<string>();
            Object.SubsetNames.Add(name);

            Objects.Add(Object);
            Materials.Add(material);
        }

        public void GenerateCubeT(MaterialBase material)
        {

            Objects = new List<ObjectBase>();
            Materials = new List<MaterialBase>();

            float halfLength = 0.5f;
            //float lengthby2 = length / 2;

            Vertex[] vertices = {
                
                //Up Outside
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 1, 1)},
                //Down Outside
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 1, 1)},

                //Left Outside
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 1, 1)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 0, 0)},
                //Right Outside
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 1, 1)},
                
                //Front Outside
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 1, 1)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 0, 0)},
                //Back Outside
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 1, 1)},
                
                //Up inside
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 0,-1, 0),TexCoord = new Vector2( 1, 1)},
                //Down inside
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 1, 0),TexCoord = new Vector2( 1, 1)},

                //Left inside
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 1, 1)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3( 1, 0, 0),TexCoord = new Vector2( 0, 0)},
                //Right inside
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3(-1, 0, 0),TexCoord = new Vector2( 1, 1)},
                
                //Front inside
                new Vertex() { Position = new Vector3(-halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength, halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 1, 1)},
                new Vertex() { Position = new Vector3( halfLength, halfLength, halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3(-halfLength, halfLength, halfLength), Normal = new Vector3( 0, 0,-1),TexCoord = new Vector2( 0, 0)},
                //Back inside
                new Vertex() { Position = new Vector3(-halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 1, 0)},
                new Vertex() { Position = new Vector3( halfLength, halfLength,-halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 0, 0)},
                new Vertex() { Position = new Vector3( halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 0, 1)},
                new Vertex() { Position = new Vector3(-halfLength,-halfLength,-halfLength), Normal = new Vector3( 0, 0, 1),TexCoord = new Vector2( 1, 1)},

            };

            uint[] indices = new uint[12 * 6];
            for (uint i = 0, j = 0; i < 12; i++, j += 4)
            {
                indices[6 * i + 0] = j + 0;
                indices[6 * i + 1] = j + 1;
                indices[6 * i + 2] = j + 2;

                indices[6 * i + 3] = j + 0;
                indices[6 * i + 4] = j + 2;
                indices[6 * i + 5] = j + 3;
            }

            ObjectBase Object = new ObjectBase()
            {
                vertices = vertices,
                indices = indices,
                Tranformation = Matrix.Scaling(1, 1, 1),
            };
            Object.meshSubsets.Add(0);
            Object.meshSubsets.Add(indices.Length);

            Object.MaterialIDs.Add(material.ID);
            Object.ApplySubsets(new int[] { 0, indices.Length / 2, indices.Length }, new MaterialBase[] { material });

            Objects.Add(Object);
            Materials.Add(material);
        }

        public void Save(string filename)
        {
            List<int> In = new List<int>();
            List<int> Out = new List<int>();

            if (File.Exists(filename))
                File.Delete(filename);

            FileStream file = new FileStream(filename, FileMode.OpenOrCreate);
            BinaryWriter writer = new BinaryWriter(file);

            writer.Write((ushort)0xA000);
            In.Add((int)writer.BaseStream.Position);
            writer.Write(0);

            writer.Write((ushort)0xA100);
            In.Add((int)writer.BaseStream.Position);
            writer.Write(0);

            foreach (ObjectBase obj in Objects)
            {
                obj.Write(writer);
            }

            Out.Add((int)writer.BaseStream.Position);

            writer.Write((ushort)0xA200);
            In.Add((int)writer.BaseStream.Position);
            writer.Write(0);

            foreach (MaterialBase mat in Materials)
                mat.Write(writer);

            Out.Add((int)writer.BaseStream.Position);

            for (int i = 1; i < In.Count; i++)
            {
                writer.Seek(In[i], SeekOrigin.Begin);
                writer.Write(Out[i - 1] - In[i] + 2);
            }

            writer.Seek(In[0], SeekOrigin.Begin);
            writer.Write(Out[1]);

            writer.Close();
            file.Close();
        }

        public static ModelHandler Load(string filename)
        {
            ModelHandler model = new ModelHandler();

            string folder = filename.Substring(0, filename.LastIndexOf("\\") + 1);

            FileStream file = new FileStream(filename, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);

            Chunk c, c1;
            ObjectBase b = new ObjectBase();
            MaterialBase m = new MaterialBase();

            c = new Chunk();
            c1 = new Chunk();
            c.Read(reader);

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
                                b.Read(reader, c1.End);
                                model.Objects.Add(b);
                            }
                            reader.BaseStream.Seek(c1.End, SeekOrigin.Begin);
                        } while (reader.BaseStream.Position + 6 < c.End);
                        break;

                    case 0xA200:
                        do
                        {
                            c1.Read(reader);

                            if (c1.id == 0xB100)
                            {
                                m.Read(reader, folder);
                                model.Materials.Add(m);
                                //WriteLine("Model Added!");
                                reader.BaseStream.Seek(c1.End, SeekOrigin.Begin);
                            }
                        } while (reader.BaseStream.Position + 6 < c.End);
                        break;
                    case 0xA300:
                        break;
                    case 0xA400:
                        break;
                }

                //reader.BaseStream.Seek(c.End, SeekOrigin.Begin);
            } while (reader.BaseStream.Position + 6 < reader.BaseStream.Length);

            reader.Close();
            file.Close();
            return model;
        }

        public void Clear()
        {
            foreach (var item in Objects)
                item.Clear();

            Objects.Clear();
            Materials.Clear();
        }

        #endregion
    }

}
