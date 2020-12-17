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
    /// Encloses Constant buffer of matrix
    /// </summary>
    public class MatrixBuffer : ConstantBuffer
    {
        /// <summary>
        /// The constant buffer base
        /// </summary> 
        struct Buffer
        {
            public Matrix mat;
        }

        #region Constructor

        /// <summary>
        /// Creates a new instance of MatrixBuffer
        /// </summary>
        /// <param name="dev">3D rendering device</param>
        public MatrixBuffer(Device dev)
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
        /// Updates MatrixBuffer
        /// </summary>
        /// <param name="mat">Matrix to the shader</param>
        public void Update(Matrix mat)
        {
            ds = dev.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None).Data;
            unsafe
            {
                Buffer* pf = (Buffer*) ds.DataPointer;
                pf->mat = Matrix.Transpose(mat);
            }
            dev.ImmediateContext.UnmapSubresource(buffer, 0);
        }

        #endregion
    }
}
