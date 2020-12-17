using SlimDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame.Models
{
    /// <summary>
    /// Holds Material information
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe class MaterialBase
    {
        public const ushort cMaterialID = 0xB100;

        public int ID;
        public string Name;
        public Color4 Diffuse;
        public Color4 Ambient;
        public Color4 Specular;
        public Color4 Emissive;
        public string Texture;
        public TextureType TextureType;
        public string BumpMap;
        public TextureType BumpType;
        public float Transparency;
        public float Reflectivity;
        public float Refractivity;
        public float SpecularPower;
        public float EmissivePower;
        public float Dissolve;

        public bool hasTexture
        {
            get { return TextureType != TextureType.None; }
        }

        public bool hasBumpMap
        {
            get { return TextureType != TextureType.None; }
        }

        public string Folder;

        public static MaterialBase Default
        {
            get
            {
                return new MaterialBase()
                {
                    ID = 0,
                    Name = "Default",
                    Diffuse = new Color4(1f, 1f, 1f, 1f),
                    Ambient = new Color4(1f, 0.1f, 0.1f, 0.1f),
                    Specular = new Color4(1f, 0.1f, 0.1f, 0.1f),
                    TextureType = TextureType.texture2D,
                    Texture = "",
                    BumpMap = "",
                    Reflectivity = 0.1f,
                    BumpType = TextureType.texture2D,
                    Refractivity = 0.1f,
                    SpecularPower = 10.0f,
                    EmissivePower = 10.0f,
                };
            }
        }

        public void Write(BinaryWriter writer)
        {
            int In, Out;

            writer.Write(cMaterialID);
            In = (int)writer.BaseStream.Position;
            writer.Write(0);
            writer.Write(ID);
            writer.Write(Name);
            Material.Write(writer, Diffuse);
            Material.Write(writer, Ambient);
            Material.Write(writer, Specular);
            Material.Write(writer, Emissive);
            writer.Write(Texture);
            writer.Write((int)TextureType);
            writer.Write(BumpMap);
            writer.Write((int)BumpType);
            writer.Write(Transparency);
            writer.Write(Reflectivity);
            writer.Write(Refractivity);
            writer.Write(SpecularPower);
            writer.Write(EmissivePower);
            writer.Write(Dissolve);
            Out = (int)writer.BaseStream.Position;

            writer.BaseStream.Position = In;
            writer.Write(Out - In + 2);
            writer.BaseStream.Position = Out;
        }

        public void Read(BinaryReader reader, string FolderPath)
        {
            ID = reader.ReadInt32();
            Name = reader.ReadString();

            Material.Read(reader, ref Diffuse);
            Material.Read(reader, ref Ambient);
            Material.Read(reader, ref Specular);
            Material.Read(reader, ref Emissive);

            Texture = reader.ReadString();
            TextureType = (TextureType)reader.ReadInt32();
            BumpMap = reader.ReadString();
            BumpType = (TextureType)reader.ReadInt32();
            Transparency = reader.ReadSingle();
            Reflectivity = reader.ReadSingle();
            Refractivity = reader.ReadSingle();
            SpecularPower = reader.ReadSingle();
            EmissivePower = reader.ReadSingle();
            Dissolve = reader.ReadSingle();

            Folder = FolderPath;

            //if (Texture != "")
            //    Texture = Texture;
            //if (BumpMap != "")
            //    BumpMap = BumpMap;
        }

    }

}
