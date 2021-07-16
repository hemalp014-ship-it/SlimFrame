using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimFrame;
using SlimFrame.Buffers;
using SlimFrame.Cameras;
using SlimFrame.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    class Sky : IDisposable
    {
        #region Member Variables

        public Model SkyBox;

        DeviceContext deviceContext;
        SlimDX.Direct3D11.Device device;

        GameWindow3D m_Window;
        DepthStencilState DSLessEqual;

        #endregion

        #region Properties
        
        public RenderBuffer RenderBuffer { get; set; }                 // WVP Buffer
        public MaterialBuffer MaterialBuffer { get; set; }       // Material Buffer
        public VertexShader VertexShader { get; protected set; }  // This is the vertex shader.
        public PixelShader PixelShader { get; protected set; }  // This is the pixel shader.

        public SamplerState SkyTexSamplerState { get; set; }

        #endregion

        #region Methods

        public Sky(SlimDX.Direct3D11.Device Device, GameWindow3D window)
        { 
            device = Device;
            deviceContext = Device.ImmediateContext;

            m_Window = window;
            SkyBox = Model.FromFile(device, Application.StartupPath + "\\..\\..\\..\\..\\..\\My Models\\SkyDayBox1.model");
            SkyBox.Tranformation = Matrix.Scaling(10, 10, 10) * Matrix.Translation(0, 0, 0);
        }

        public void Init()
        {

            DepthStencilStateDescription desc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.LessEqual,
            };
            DSLessEqual = DepthStencilState.FromDescription(device, desc);

            string vsCompileError = "Vertex Shader Compile Error!!!";
            using (var bytecode = ShaderBytecode.CompileFromFile("Effects.fx",
                                                                 "Vertex_Shader_Sky",
                                                                 "vs_4_0",
#if DEBUG
                                                                         ShaderFlags.Debug,
#else

                                                                         ShaderFlags.None,
#endif
                                                                         SlimDX.D3DCompiler.EffectFlags.None,
                                                                 null,
                                                                 null,
                                                                 out vsCompileError))
            {
                VertexShader = new VertexShader(device, bytecode);
            }

            vsCompileError = "Pixel Shader Compile Error!!!";
            using (var bytecode = ShaderBytecode.CompileFromFile("Effects.fx",
                                                                 "Pixel_Shader_Sky",
                                                                 "ps_4_0",
#if DEBUG
                                                                         ShaderFlags.Debug,
#else

                                                                         ShaderFlags.None,
#endif
                                                                         SlimDX.D3DCompiler.EffectFlags.None,
                                                                 null,
                                                                 null,
                                                                 out vsCompileError))
            {
                PixelShader = new PixelShader(device, bytecode);
            }
        }
        
        public void Render(Matrix View, Matrix Proj, Camera camera)
        {
            View.M41 = 0.0f;
            View.M42 = 0.0f;
            View.M43 = 0.0f;

            deviceContext.VertexShader.Set(VertexShader);
            deviceContext.PixelShader.Set(PixelShader);

            deviceContext.PixelShader.SetSampler(SkyTexSamplerState, 0);
             
            Material mat;

            deviceContext.Rasterizer.State = m_Window.CullNone;
            deviceContext.OutputMerger.DepthStencilState = DSLessEqual;

            foreach (SlimFrame.Models.Object3D obj in SkyBox.Objects)
            {
                //System.Diagnostics.Debug.WriteLine(obj.SubsetCount);

                for (int i = 0; i < obj.SubsetCount; i++)
                {
                    mat = SkyBox.Materials[obj.MaterialIDs[i]];
                    //mat.TextureType = TextureType.texture2D;
                    
                    deviceContext.InputAssembler.SetVertexBuffers(0, obj.vertexBufferBinding);
                    deviceContext.InputAssembler.SetIndexBuffer(obj.IndexBuffer, SlimDX.DXGI.Format.R32_UInt, 0);

                    if (mat.TextureType == TextureType.texture2D)
                        deviceContext.PixelShader.SetShaderResource(mat.Texture, 0);
                    else
                        deviceContext.PixelShader.SetShaderResource(mat.Texture, 1);

                    RenderBuffer.Update(Proj, View, SkyBox.Tranformation * obj.Tranformation, camera.Position);
                    MaterialBuffer.Update(mat);

                    // Draw the Mesh.
                    obj.DrawSubset(deviceContext, i);

                    //deviceContext.PixelShader.SetShaderResource(null, 0);
                    //deviceContext.PixelShader.SetShaderResource(null, 1);
                    //deviceContext.PixelShader.SetShaderResource(null, 2);
                    //deviceContext.PixelShader.SetShaderResource(null, 3);
                }
            }
            deviceContext.OutputMerger.DepthStencilState = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SkyBox?.Dispose();
                    DSLessEqual?.Dispose();
                    VertexShader?.Dispose();
                    PixelShader?.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


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
