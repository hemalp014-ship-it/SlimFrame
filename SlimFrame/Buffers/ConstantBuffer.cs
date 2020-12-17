using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using SlimDX.Direct3D11;
using Direct3D11 = SlimDX.Direct3D11;
//using SlimDX.DXGI;

namespace SlimFrame.Buffers
{
    /// <summary>
    /// Encloses constant buffer
    /// </summary>
    public class ConstantBuffer : IDisposable
    {

        #region Member Variables

        protected SlimDX.Direct3D11.Buffer buffer;
        protected DataStream ds;

        protected Device dev;

        #endregion

        #region Constructors

        public ConstantBuffer()
        {

        }

        /// <summary>
        /// Creates a new instance of ConstantBuffer
        /// </summary>
        /// <param name="dev">3D rendering device</param>
        public ConstantBuffer(Device dev, BufferDescription bd)
        {
            buffer = new Direct3D11.Buffer(dev, bd);
            this.dev = dev;
        }

        #endregion

        #region Shader Methods

        /// <summary>
        /// Appends constant buffer to the vertex shader
        /// </summary>
        /// <param name="slot"></param>
        public void VertexShader(int slot)
        {
            dev.ImmediateContext.VertexShader.SetConstantBuffer(buffer, slot);
        }

        /// <summary>
        /// Appends constant buffer to the pixel shader
        /// </summary>
        /// <param name="slot"></param>
        public void PixelShader(int slot)
        {
            dev.ImmediateContext.PixelShader.SetConstantBuffer(buffer, slot);
        }

        /// <summary>
        /// Appends constant buffer to the geometry shader
        /// </summary>
        /// <param name="slot"></param>
        public void GeometryShader(int slot)
        {
            dev.ImmediateContext.GeometryShader.SetConstantBuffer(buffer, slot);
        }

        /// <summary>
        /// Appends constant buffer to the hull shader
        /// </summary>
        /// <param name="slot"></param>
        public void HullShader(int slot)
        {
            dev.ImmediateContext.HullShader.SetConstantBuffer(buffer, slot);
        }

        /// <summary>
        /// Appends constant buffer to the domain shader
        /// </summary>
        /// <param name="slot"></param>
        public void DomainShader(int slot)
        {
            dev.ImmediateContext.DomainShader.SetConstantBuffer(buffer, slot);
        }

        /// <summary>
        /// Appends constant buffer to the compute shader
        /// </summary>
        /// <param name="slot"></param>
        public void ComputeShader(int slot)
        {
            dev.ImmediateContext.ComputeShader.SetConstantBuffer(buffer, slot);
        }

        #endregion

        /// <summary>
        /// Disposes constant buffer
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    buffer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion

    }
}