using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame
{
    /// <summary>
    /// The Constant Buffer to handle the matrix input to the shader.
    /// </summary>
    public class MatrixBuffer : ConstantBuffer
    {
        /// <summary>
        /// The constant buffer base
        /// </summary>
        struct PerFrame
        {
            public Matrix mat;
        }

        public MatrixBuffer(Device dev)
        {
            this.dev = dev;

            BufferDescription bd = new BufferDescription();
            bd.BindFlags = BindFlags.ConstantBuffer;
            bd.SizeInBytes = 64;


            bd.Usage = ResourceUsage.Dynamic;
            bd.CpuAccessFlags = CpuAccessFlags.Write; // | CpuAccessFlags.Read;
            buffer = new SlimDX.Direct3D11.Buffer(dev, bd);
        }
        
        public void Update(Matrix mat)
        {
            ds = dev.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None).Data;
            unsafe
            {
                PerFrame* pf = (PerFrame*)ds.DataPointer;
                pf->mat = Matrix.Transpose(mat);
            }
            dev.ImmediateContext.UnmapSubresource(buffer, 0);
        }
    }
}
