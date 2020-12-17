using System.Windows.Forms;
using System.Drawing;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;

using D2D = SlimDX.Direct2D;
using D11 = SlimDX.Direct3D11;

using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

using System.Runtime.InteropServices;
using System;

using SlimFrame;
using SlimFrame.UI;

namespace SlimDX
{
    
    //[StructLayout(LayoutKind.Sequential)]
    struct VertexPositionTexture
    {
        public Vector4 Position;
        public Vector2 TexCoord;
        public static readonly InputElement[] inputElements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD",0,Format.R32G32_Float, 16 ,0)
            };
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(VertexPositionTexture));
        public VertexPositionTexture(Vector4 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }
        public VertexPositionTexture(Vector3 position, Vector2 texCoord)
        {
            Position = new Vector4(position, 1);
            TexCoord = texCoord;
        }
    }



    /// <summary>
    /// To handle the 2D System
    /// </summary>
    public class System2D : IDisposable
    {
        static RenderForm           m_form;                 //The form handler to handle drawing.

        Direct3D10_1.Device1        device10_1;             //Direct3D 10.1 device.
        KeyedMutex                  mutexD3D10;             // KeyedMutex for shared resources in Direct3D 10 device.
        KeyedMutex                  mutexD3D11;             // KeyedMutex for shared resources in Direct3D 11 device.
        D2D.Factory                 d2Factory;              // D2D factory for creating D2D resources.
        DirectWrite.Factory         dwFactory;              // DirectWrite factory for creating DirectWrite resources.
        DirectWrite.TextFormat      textFormat;             // TextFormat for rendering text using DirectWrite.
        D2D.SolidColorBrush         brushSolidWhite;        // SolidColorBrush for rendering 2D objects.
        Texture2D                   textureD3D11;           //Direct3D 11 shared texture resource to render onto the 3D scene.
        Direct3D10.Texture2D        textureD3D10;           //Direct3D 10 shared texture resource to render 2D scene onto.

        Effect                      effect;                 //Direct3D 11 Effect class to hold whole shader file for rendering.
        InputLayout                 layoutText;             //Direct3D 11 Input layout to render the 2D scene.
        BlendState                  BlendState_Transparent; //Direct3D 11 Blend state to render the transparency in 2D scene.
        D11.Buffer                  vertexBufferText;       // Vertex buffer to handle the primitive
        D2D.RenderTarget            dwRenderTarget;         // RenderTarget to render the Control.



        Adapter1 adapter1;
        Factory1 factory1;

        Device device;
        DeviceContext deviceContext;

        Surface surface;
        DXGI.Resource sharedResource;

        HOD m_HOD;

        protected bool m_Initialized = false;  // Stores the Initialization state of Surface in boolean format


        /// <summary>
        /// Hod member to handel the UI
        /// </summary>
        public HOD HOD
        {
            get
            {
                return m_HOD;
            }
        }         

        /// <summary>
        /// Generates the System2D variable.
        /// </summary>
        /// <param name="device11"></param>
        /// <param name="form"></param>
        public System2D(Device device11, RenderForm form)
        {

            // DirectX DXGI 1.1 factory
            factory1 = new Factory1();

            // The 1st graphics adapter
            adapter1 = factory1.GetAdapter1(0);

            device = device11;
            deviceContext = device11.ImmediateContext;
            m_form = form;

            // A DirectX 10.1 device is required because DirectWrite/D2D are unable
            // to access DirectX11.  BgraSupport is required for DXGI interaction between
            // DirectX10/D2D/DirectWrite.
            device10_1 = new Direct3D10_1.Device1(
                adapter1,
                Direct3D10.DriverType.Hardware,
                Direct3D10.DeviceCreationFlags.BgraSupport | Direct3D10.DeviceCreationFlags.Debug,
                Direct3D10_1.FeatureLevel.Level_10_0
            );

            // Create the DirectX11 texture2D.  This texture will be shared with the DirectX10
            // device.  The DirectX10 device will be used to render text onto this texture.  DirectX11
            // will then draw this texture (blended) onto the screen.
            // The KeyedMutex flag is required in order to share this resource.
            textureD3D11 = new Texture2D(device11, new Texture2DDescription
            {
                Width = form.ClientSize.Width,
                Height = form.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.KeyedMutex
            });

            // A DirectX10 Texture2D sharing the DirectX11 Texture2D
            sharedResource = new DXGI.Resource(textureD3D11);
            textureD3D10 = device10_1.OpenSharedResource<SlimDX.Direct3D10.Texture2D>(sharedResource.SharedHandle);
       

            // The KeyedMutex is used just prior to writing to textureD3D11 or textureD3D10.
            // This is how DirectX knows which DirectX (10 or 11) is supposed to be writing
            // to the shared texture.  The keyedMutex is just defined here, they will be used
            // a bit later.
            mutexD3D10 = new KeyedMutex(textureD3D10);
            mutexD3D11 = new KeyedMutex(textureD3D11);

            // D2D Factory
            d2Factory = new D2D.Factory(
                D2D.FactoryType.SingleThreaded,
                D2D.DebugLevel.Information
            );

            // Direct Write factory
            dwFactory = new DirectWrite.Factory(
                DirectWrite.FactoryType.Isolated
            );

            // The textFormat we will use to draw text with
            textFormat = new DirectWrite.TextFormat(
                dwFactory,
                "Arial",
                DirectWrite.FontWeight.Normal,
                DirectWrite.FontStyle.Normal,
                DirectWrite.FontStretch.Normal,
                24,
                "en-US"
            );
            textFormat.TextAlignment = DirectWrite.TextAlignment.Trailing;
            textFormat.ParagraphAlignment = DirectWrite.ParagraphAlignment.Near;

            // Query for a IDXGISurface.
            // DirectWrite and DirectX10 can interoperate thru DXGI.
            surface = textureD3D10.AsSurface();
            D2D.RenderTargetProperties rtp = new D2D.RenderTargetProperties();
            rtp.MinimumFeatureLevel = D2D.FeatureLevel.Direct3D10;
            rtp.Type = D2D.RenderTargetType.Hardware;
            rtp.Usage = D2D.RenderTargetUsage.None;
            rtp.PixelFormat = new D2D.PixelFormat(Format.Unknown, D2D.AlphaMode.Premultiplied);
            dwRenderTarget = D2D.RenderTarget.FromDXGI(d2Factory, surface, rtp);

            // Brush used to DrawText
            brushSolidWhite = new D2D.SolidColorBrush(
                dwRenderTarget,
                new Color4(1, 1, 1, 1)
            ); 

            // Think of the shared textureD3D10 as an overlay.
            // The overlay needs to show the text but let the underlying triangle (or whatever)
            // show thru, which is accomplished by blending.
            BlendStateDescription bsd = new BlendStateDescription();
            bsd.RenderTargets[0].BlendEnable = true;
            bsd.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
            bsd.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            bsd.RenderTargets[0].BlendOperation = BlendOperation.Add;
            bsd.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
            bsd.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            bsd.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
            bsd.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            BlendState_Transparent = BlendState.FromDescription(device11, bsd);

            // Load Effect. This includes both the vertex and pixel shaders.
            // Also can include more than one technique.
            using (ShaderBytecode shaderByteCode = ShaderBytecode.CompileFromFile(
                 "effectDx11.fx",
                 "fx_5_0",
                 ShaderFlags.EnableStrictness,
                 EffectFlags.None))
            {
                effect = new Effect(device11, shaderByteCode);
            }


            // create text vertex data, making sure to rewind the stream afterward
            // Top Left of screen is -1, +1
            // Bottom Right of screen is +1, -1
            var verticesText = new DataStream(VertexPositionTexture.SizeInBytes * 4, true, true);
            verticesText.Write(
                new VertexPositionTexture(
                        new Vector3(-1, 1, 0),
                        new Vector2(0, 0f)
                )
            );
            verticesText.Write(
                new VertexPositionTexture(
                    new Vector3(1, 1, 0),
                    new Vector2(1, 0)
                )
            );
            verticesText.Write(
                new VertexPositionTexture(
                    new Vector3(-1, -1, 0),
                    new Vector2(0, 1)
                )
            );
            verticesText.Write(
                new VertexPositionTexture(
                    new Vector3(1, -1, 0),
                    new Vector2(1, 1)
                )
            );

            verticesText.Position = 0;

            // create the text vertex layout and buffer
            layoutText = new InputLayout(device11, effect.GetTechniqueByName("Text").GetPassByIndex(0).Description.Signature, VertexPositionTexture.inputElements);
            vertexBufferText = new D11.Buffer(device11, verticesText, (int)verticesText.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            verticesText.Dispose();

            m_HOD = new HOD(dwFactory, d2Factory, dwRenderTarget);
        }

        public void Init()
        {
            if (!m_Initialized)
            {
                m_HOD.Init(dwFactory, d2Factory, dwRenderTarget);
            }
        }

        /// <summary>
        /// Resizes the 2D system with proper steps.
        /// </summary>
        public void Resize()
        {
            //brushSolidWhite?.Dispose();
            dwRenderTarget?.Dispose();
            surface?.Dispose();

            mutexD3D10?.Dispose();
            mutexD3D11?.Dispose();

            textureD3D10?.Dispose();
            sharedResource?.Dispose();
            textureD3D11?.Dispose();

            //m_HOD?.RenderTargetReleasing();

            textureD3D11 = new Texture2D(device, new Texture2DDescription 
            {
                Width = m_form.ClientSize.Width,
                Height = m_form.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.KeyedMutex
            });

            // A DirectX10 Texture2D sharing the DirectX11 Texture2D
            sharedResource = new DXGI.Resource(textureD3D11);
            textureD3D10 = device10_1.OpenSharedResource<SlimDX.Direct3D10.Texture2D>(sharedResource.SharedHandle);

            mutexD3D10 = new KeyedMutex(textureD3D10);
            mutexD3D11 = new KeyedMutex(textureD3D11);

            // Query for a IDXGISurface.
            // DirectWrite and DirectX10 can interoperate thru DXGI.
            surface = textureD3D10.AsSurface();
            D2D.RenderTargetProperties rtp = new D2D.RenderTargetProperties();
            rtp.MinimumFeatureLevel = D2D.FeatureLevel.Direct3D10;
            rtp.Type = D2D.RenderTargetType.Hardware;
            rtp.Usage = D2D.RenderTargetUsage.None;
            rtp.PixelFormat = new D2D.PixelFormat(Format.Unknown, D2D.AlphaMode.Premultiplied);
            dwRenderTarget = D2D.RenderTarget.FromDXGI(d2Factory, surface, rtp);

            // Brush used to DrawText
            //brushSolidWhite = new D2D.SolidColorBrush(
            //    dwRenderTarget,
            //    new Color4(1, 1, 1, 1)
            //);

            //m_HOD?.RenderTargetResized(dwRenderTarget);

        }
        
        /// <summary>
        /// Updates 2D Sytem.
        /// </summary>
        public void Update(double frameTime)
        {
            HOD.Update(frameTime);
        }

        /// <summary>
        /// Renders 2D System
        /// </summary>
        public void Render()
        {
            // Draw Text on the shared Texture2D
            // Need to Acquire the shared texture for use with DirectX10
            mutexD3D10.Acquire(0, 100);
            dwRenderTarget.BeginDraw();
            dwRenderTarget.Clear(new Color4(0, 0, 0, 0));

            m_HOD.Render(dwRenderTarget);

            dwRenderTarget.EndDraw();
            mutexD3D10.Release(0);

            // Draw the shared texture2D onto the screen
            // Need to Aquire the shared texture for use with DirectX11
            mutexD3D11.Acquire(0, 100);
            ShaderResourceView srv = new ShaderResourceView(device, textureD3D11);
            effect.GetVariableByName("g_textOverlay").AsResource().SetResource(srv);
            deviceContext.InputAssembler.InputLayout = layoutText;
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBufferText, VertexPositionTexture.SizeInBytes, 0));
            deviceContext.OutputMerger.BlendState = BlendState_Transparent;

            EffectTechnique currentTechnique = effect.GetTechniqueByName("Text");
            for (int pass = 0; pass < currentTechnique.Description.PassCount; ++pass)
            {
                EffectPass Pass = currentTechnique.GetPassByIndex(pass);
                System.Diagnostics.Debug.Assert(Pass.IsValid, "Invalid EffectPass");
                Pass.Apply(deviceContext);
                deviceContext.Draw(4, 0);
            }

            srv.Dispose();
            mutexD3D11.Release(0);
        }

        /// <summary>
        /// Measures the string dimensions
        /// </summary>
        /// <param name="text">The text to be measured</param>
        /// <param name="font">The font of the text.</param>
        /// <returns>Floating-point Size (type : SizeF) containing the dimension of string</returns>
        public static SizeF MeasureStringF(string text, Font font)
        {
            //create a bmp / graphic to use MeasureString on
            Graphics g = Graphics.FromHwnd(m_form.Handle);
            //measure the string
            SizeF sizeOfString = new SizeF();
            sizeOfString = g.MeasureString(text, font);

            return sizeOfString;
        }

        /// <summary>
        /// Measures the string dimensions
        /// </summary>
        /// <param name="text">The text to be measured</param>
        /// <param name="font">The font of the text.</param>
        /// <returns>Size (type:Size) containing the dimension of string</returns>
        public static Size MeasureString(string text, Font font)
        {
            SizeF size = MeasureStringF(text, font);
            return new Size((int)size.Width, (int)size.Height);
        }

        #region IDisposable Support
      
        private bool m_Disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //Unregister events

                    //dispose managed state (managed objects).
                    vertexBufferText?.Dispose();
                    layoutText?.Dispose();
                    effect?.Dispose();
                    device10_1?.Dispose();
                    mutexD3D10?.Dispose();
                    mutexD3D11?.Dispose();
                    textureD3D10?.Dispose();
                    textureD3D11?.Dispose();
                    factory1?.Dispose();
                    adapter1?.Dispose();
                    sharedResource?.Dispose();
                    d2Factory?.Dispose();
                    dwFactory?.Dispose();
                    textFormat?.Dispose();
                    surface?.Dispose();
                    dwRenderTarget?.Dispose();
                    brushSolidWhite?.Dispose();
                    BlendState_Transparent?.Dispose();
                    m_HOD?.Dispose();
                }

                //free unmanaged resources (unmanaged objects) and override a finalizer below.
                m_Disposed = true;
            }
        }

        /// <summary>
        /// Disposes the 2D System.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}