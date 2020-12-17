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

namespace SlimFrame
{
    /// <summary>
    /// The class to handle buffer.
    /// </summary>
    public class ConstantBuffer : IDisposable 
    {
        protected SlimDX.Direct3D11.Buffer buffer;
        protected DataStream ds;
        
        protected Device dev;

        public ConstantBuffer(Device dev, BufferDescription bd)
        {
            buffer = new Direct3D11.Buffer(dev, bd);
            this.dev = dev;
        }

        public ConstantBuffer()
        {

        }
        public virtual void Update<T>(T obj)
        {
            ds = dev.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None).Data;
            // TODO : code for writing data to DataStream.
            dev.ImmediateContext.UnmapSubresource(buffer, 0);
        }

        public void VertexShader(int slot)
        {
            dev.ImmediateContext.VertexShader.SetConstantBuffer(buffer, slot);
        }
        public void PixelShader(int slot)
        {
            dev.ImmediateContext.PixelShader.SetConstantBuffer(buffer, slot);
        }
        public void GeometryShader(int slot)
        {
            dev.ImmediateContext.GeometryShader.SetConstantBuffer(buffer, slot);
        }
        public void HullShader(int slot)
        {
            dev.ImmediateContext.HullShader.SetConstantBuffer(buffer, slot);
        }
        public void DomainShader(int slot)
        {
            dev.ImmediateContext.DomainShader.SetConstantBuffer(buffer, slot);
        }
        public void ComputeShader(int slot)
        {
            dev.ImmediateContext.ComputeShader.SetConstantBuffer(buffer, slot);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (buffer != null)
                        buffer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Buffer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
