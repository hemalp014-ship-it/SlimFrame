using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame.Buffers
{
    /// <summary>
    /// Encloses constant buffer of WVP Matrix System
    /// </summary>
    public class RenderBuffer : ConstantBuffer
    {
        /// <summary>
        /// The constant buffer base
        /// </summary>
        struct Buffer 
        {
            public Matrix Proj;
            public Matrix View;
            public Matrix world;
            public Vector3 CamPos;
            public float pad;
        }

        #region Constructor

        /// <summary>
        /// Creates a new instance of WVPBuffer
        /// </summary>
        /// <param name="dev">3D rendering device</param>
        public RenderBuffer(Device dev)
        {
            this.dev = dev;

            BufferDescription bd = new BufferDescription();
            bd.BindFlags = BindFlags.ConstantBuffer;
            unsafe
            {
                bd.SizeInBytes = sizeof(Buffer);
            }


            bd.Usage = ResourceUsage.Dynamic;
            bd.CpuAccessFlags = CpuAccessFlags.Write; // | CpuAccessFlags.Read;
            buffer = new SlimDX.Direct3D11.Buffer(dev, bd);
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Updates WVPBuffer
        /// </summary>
        /// <param name="WVP">WVP matrix to the shader</param>
        /// <param name="world">World matrix to the shader</param>
        public void Update(Matrix Proj, Matrix View, Matrix world, Vector3 CameraPosition)
        {
            ds = dev.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None).Data;
            unsafe
            {
                Buffer* pf = (Buffer*)ds.DataPointer;
                pf->Proj = Matrix.Transpose(Proj);
                pf->View = Matrix.Transpose(View);
                pf->world = Matrix.Transpose(world);
                pf->CamPos = CameraPosition;
                pf->pad = 1.0f;
            }
            dev.ImmediateContext.UnmapSubresource(buffer, 0);
        }

        #endregion
    }
}
