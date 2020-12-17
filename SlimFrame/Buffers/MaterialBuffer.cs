using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using SlimDX.Direct3D11;

using SlimFrame;
using SlimFrame.Buffers;
using SlimFrame.Models;

namespace SlimFrame.Buffers
{
    /// <summary>
    /// Encloses constant buffer of material
    /// </summary>
    public class MaterialBuffer : ConstantBuffer
    {

        /// <summary>
        /// Material buffer structure
        /// </summary>
        struct Mat
        {
            public Color4 Diffuse;
            public Color4 Ambient;
            public Color4 Specular;
            public Color4 Emissive;
            public int TextureType;
            public int BumpType;
            public float Transparency;
            public float Reflectivity;
            public float Refractivity;
            public float SpecularPower;
            public float EmissivePower;
            public float Dissolve;

            public void Load(Material mat)
            {

                Diffuse = mat.Diffuse;
                Ambient = mat.Ambient;
                Specular = mat.Specular;
                Emissive = mat.Emissive;
                TextureType = (int) mat.TextureType;
                BumpType = (int) mat.BumpType;
                Transparency = mat.Transparency;
                Reflectivity = mat.Reflectivity;
                Refractivity = mat.Refractivity;
                SpecularPower = mat.SpecularPower;
                EmissivePower = mat.EmissivePower;
                Dissolve = mat.Dissolve;
            }
        }

        #region Constructor

        /// <summary>
        /// Creates a new instance of MaterialBuffer
        /// </summary>
        /// <param name="dev">3D rendering device</param>
        public MaterialBuffer(Device dev)
        {
            this.dev = dev;

            BufferDescription bd = new BufferDescription();
            bd.BindFlags = BindFlags.ConstantBuffer;
            unsafe
            {
                bd.SizeInBytes = sizeof(Mat);
            }

            bd.Usage = ResourceUsage.Dynamic;
            bd.CpuAccessFlags = CpuAccessFlags.Write; // | CpuAccessFlags.Read;
            buffer = new SlimDX.Direct3D11.Buffer(dev, bd);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates MaterialBuffer
        /// </summary>
        /// <param name="mat">Material to the shader</param>
        public void Update(Material mat)
        {
            ds = dev.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None).Data;
            unsafe
            {
                Mat* pf = (Mat*)ds.DataPointer;
                pf->Diffuse = mat.Diffuse;
                pf->Ambient = mat.Ambient;
                pf->Specular = mat.Specular;
                pf->Emissive = mat.Emissive;
                pf->TextureType = (int)mat.TextureType;
                pf->BumpType = (int)mat.BumpType;
                pf->Transparency = mat.Transparency;
                pf->Reflectivity = mat.Reflectivity;
                pf->Refractivity = mat.Refractivity;
                pf->SpecularPower = mat.SpecularPower;
                pf->EmissivePower = mat.EmissivePower;
                pf->Dissolve = mat.Dissolve;
            }
            dev.ImmediateContext.UnmapSubresource(buffer, 0);
        }

        #endregion
    }
}
