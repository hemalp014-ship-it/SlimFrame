using System;
using System.Windows.Forms;
using System.Drawing;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;

using SlimFrame;
using Models = SlimFrame.Models;

using SlimFrame.Buffers;
using static Demo.Window;
using SlimFrame.Models;
using Demo.Buffer;
using SlimFrame.Cameras;

using static System.Diagnostics.Debug;

namespace Demo
{
    //// This struct stores data for a single vertex.
    //struct Vertex
    //{
    //    public Vector4 Position;   // The position of the vertex in 3D space.
    //    public Vector3 Normal;       // The color to use for this vertex when we are not using textured mode.
    //    public Vector2 TexCoord;   // The textures coordinates for this vertex.  We need these when we are using textured mode.
    //}
    
    /// <summary>
    /// Handles the model Model
    /// </summary>
    public class Scene : IDisposable
    {

        #region Member Variables

        SlimDX.Direct3D11.Device device;
        DeviceContext deviceContext;
         
        RenderBuffer m_RenderBuffer;                  // WVP Buffer
        private const int m_RenderBufferSlot = 0;  // WVP Buffer Index

        MaterialBuffer m_MaterialBuffer;             // Material Buffer
        private const int m_MaterialBufferSlot = 1;  // Material Buffer Index

        Light[] m_Lights;                              // Light Buffer Data
        LightBuffer m_LightBuffer;                  // Light Buffer
        private const int m_LightBufferSlot = 2;    // Light Buffer Index

        InputLayout m_InputLayout;  // Tells Direct3D about the vertex format we are using.
        ShaderSignature m_VShaderSignature; // The vertex shader signature

        VertexShader m_VertexShader; // This is the vertex shader.
        PixelShader m_PixelShader; // This is the pixel shader.

        SamplerState m_modelTexSamplerState;      // The sampler state we will use with our model texture.

        public const string TexturePixelShader = "Pixel_Shader";

        Matrix m_Model;   // The world matrix for the model.  This controls the current position and rotation of the model.
        Models.Model cube;

        float cubeScalling = 0.5f;

        public Models.Model model { get; set; }
       
        Sky m_Sky;

        GameWindow3D m_Window;

        #endregion

        #region Constructor

        public Scene(SlimDX.Direct3D11.Device deviceice, GameWindow3D window)
        {
            device = deviceice;
            deviceContext = deviceice.ImmediateContext;

            m_Window = window;
        }

        #endregion

        #region Properties
               
        /// <summary>
        /// Model Matrix of model
        /// </summary>
        public Matrix ModelMatrix
        {
            get
            {
                return m_Model;
            }
            set
            {
                m_Model = value;
            }
        }

        /// <summary>
        /// Number of Indexes of model
        /// </summary>
        public int IndexCount { get; } = 36;

        #endregion

        #region Methods

        /// <summary>
        /// This method initializes our scene.
        /// </summary>
        public void InitScene()
        {
            model = Model.FromFile(device, Application.StartupPath + "\\..\\..\\..\\..\\..\\My Models\\Boy\\Boy.model");
            cube = Model.FromFile(device, Application.StartupPath +"\\..\\..\\..\\..\\..\\My Models\\Cube.model");
            //model = Model.FromFile(device, "Sphere.model");
            //model = Manipulate3DS.Load(device, "D:\\Programming\\3D Programming\\3D Object\\3Ds Mesh\\cube.3ds", Debug: true);

            if (model == null) return;
            if (cube == null) return;
                        
            m_Sky = new Sky(device, m_Window);
            m_Sky.Init();

            // Initilize the constant buffers.
            InitConstantBuffers();

            // Initialize our Shaders.
            InitShaders();

            m_Sky.RenderBuffer = m_RenderBuffer;
            m_Sky.MaterialBuffer = m_MaterialBuffer;

            //m_Sky.PixelShader = m_PixelShader;

            // Create a SamplerDescription
            SamplerDescription sd = new SamplerDescription();
            sd.Filter = Filter.MinMagMipLinear;
            sd.AddressU = TextureAddressMode.Wrap;
            sd.AddressV = TextureAddressMode.Wrap;
            sd.AddressW = TextureAddressMode.Wrap;
            sd.ComparisonFunction = Comparison.Never;
            sd.MinimumLod = 0;
            sd.MaximumLod = float.MaxValue;

            // Create our SamplerState
            m_modelTexSamplerState = SamplerState.FromDescription(device, sd);

            m_Sky.SkyTexSamplerState = m_modelTexSamplerState;

            m_Lights = new Light[4];

            m_Lights[0] = new Light()
            {
                pos = new Vector3(1, 1, 1),
                dir = new Vector3(0 - 1, 0 - 1, 0 - 1),
                diffuse = new Color4(1, 1f, 1f, 1f),
                ambient = new Color4(0.2f, 0.2f, 0.2f, 0.2f),
                type = (int)LightType.Dir,
                range = 1000.0f,
                att = new Vector3(0.02f, 0.001f, 0.02f),
            };

            m_Lights[1] = new Light()
            {
                pos = new Vector3(1, 1, 1),
                dir = new Vector3(0 - 1, 0 - 1, 0 - 1),
                diffuse = new Color4(1, 1f, 1f, 1f),
                ambient = new Color4(0.2f, 0.2f, 0.2f, 0.2f),
                type = (int)LightType.Point,
                range = 1000.0f,
                att = new Vector3(0.02f, 0.001f, 0.02f),
            };

            model.Tranformation = Matrix.Scaling(1, 1, 1);
            cube.Tranformation = Matrix.Scaling(cubeScalling, cubeScalling, cubeScalling) * 
                                 Matrix.Translation(m_Lights[1].pos);
        }

        /// <summary>
        /// Renders the model
        /// </summary>
        public void Render(Matrix View, Matrix Proj, Camera camera)
        {
            deviceContext.InputAssembler.InputLayout = m_InputLayout; // Set the Primitive Topology.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            m_Sky.Render(View, Proj, camera);

            deviceContext.VertexShader.Set(m_VertexShader);
            deviceContext.PixelShader.Set(m_PixelShader);

            deviceContext.PixelShader.SetSampler(m_modelTexSamplerState, 0);
            //m_Light.pos = camera.Position;
            m_LightBuffer.Update(m_Lights);

            //following loop will draw opaque Objects
            RenderOpaque(deviceContext, m_Window, model, m_RenderBuffer, m_MaterialBuffer, Proj, View, camera, m_Sky.SkyBox.Materials[0].Texture);
            RenderOpaque(deviceContext, m_Window, cube, m_RenderBuffer, m_MaterialBuffer, Proj, View, camera);

            //following loop will draw tarnsparent Objects
            RenderTransparent(deviceContext, m_Window, model, m_RenderBuffer, m_MaterialBuffer, Proj, View, camera, m_Sky.SkyBox.Materials[0].Texture);

        }

        /// <summary>
        /// Renders all Opaque subsets of an object
        /// </summary>
        /// <param name="deviceContext">Direct3D Device Contect</param>
        /// <param name="window">GameWindow3D onto we are rendering</param>
        /// <param name="model">Model to be rendered</param>
        /// <param name="renderBuffer">Buffer containing rendering information</param>
        /// <param name="materialBuffer">Buffer containing material information</param>
        /// <param name="Proj">Projection Matrix</param>
        /// <param name="View">View Matrix</param>
        /// <param name="camera">camera of the scene</param>
        public static void RenderOpaque(DeviceContext deviceContext,GameWindow3D window, Model model, RenderBuffer renderBuffer, MaterialBuffer materialBuffer, Matrix Proj, Matrix View, Camera camera, ShaderResourceView texture = null)
        {
            if (model == null)
                return;

            Material material;
            deviceContext.Rasterizer.State = window.CullNone;
            foreach (Object3D obj in model.Objects)
            {
                for (int i = 0; i < obj.SubsetCount; i++)
                {
                    material = model.Materials[obj.MaterialIDs[i]];

                    renderBuffer.Update(Proj, View, model.Tranformation * obj.Tranformation, camera.Position);
                    materialBuffer.Update(material);

                    RenderObjectSubset(deviceContext, model, obj, material, i, texture: texture);
                }
            }

        }

        /// <summary>
        /// Renders all Transparent subsets of an object
        /// </summary>
        /// <param name="deviceContext">Direct3D Device Contect</param>
        /// <param name="window">GameWindow3D onto we are rendering</param>
        /// <param name="model">Model to be rendered</param>
        /// <param name="renderBuffer">Buffer containing rendering information</param>
        /// <param name="materialBuffer">Buffer containing material information</param>
        /// <param name="Proj">Projection Matrix</param>
        /// <param name="View">View Matrix</param>
        /// <param name="camera">camera of the scene</param>
        public static void RenderTransparent(DeviceContext deviceContext, GameWindow3D window, Model model, RenderBuffer renderBuffer, MaterialBuffer materialBuffer, Matrix Proj, Matrix View, Camera camera, ShaderResourceView texture = null)
        {
            if (model == null)
                return;

            Material material;
            deviceContext.OutputMerger.BlendState = window.BlendTransparent;

            deviceContext.Rasterizer.State = window.CullNone;
            foreach (Object3D obj in model.Objects)
            {
                for (int i = 0; i < obj.SubsetCount; i++)
                {
                    material = model.Materials[obj.MaterialIDs[i]];
                     
                    renderBuffer.Update(Proj, View, model.Tranformation * obj.Tranformation, camera.Position);
                    materialBuffer.Update(material);

                    RenderObjectSubset(deviceContext, model, obj, material, i, true, texture);
                }
            }
            deviceContext.OutputMerger.BlendState = null;
            deviceContext.Rasterizer.State = window.CullNone;
        }
        /// <summary>
        /// Renders an object subset at a time
        /// </summary>
        /// <param name="deviceContext">Direct3D Device Contect</param>
        /// <param name="model">Model to be rendered</param>
        /// <param name="obj">Object to be rendered</param>
        /// <param name="mat">Material to be used</param>
        /// <param name="subset">Subset of the Object</param>
        /// <param name="transparent">Determines whether the transparent subsets to be rendered or Opaque subsets to be rendered</param>
        public static void RenderObjectSubset(DeviceContext deviceContext,Model model, Object3D obj, Material mat, int subset, bool transparent = false, ShaderResourceView texture = null)
        {
            if (transparent & mat.Transparency == 0.0f )
                return;

            if (!transparent & mat.Transparency > 0.0f)
                return;

            deviceContext.InputAssembler.SetVertexBuffers(0, obj.vertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(obj.IndexBuffer, SlimDX.DXGI.Format.R32_UInt, 0);

            if (mat.TextureType == TextureType.texture2D)
                deviceContext.PixelShader.SetShaderResource(mat.Texture, 0);
            else if (mat.TextureType == TextureType.texture3D)
                deviceContext.PixelShader.SetShaderResource(mat.Texture, 1);
            else
            {
                deviceContext.PixelShader.SetShaderResource(null, 0);
                deviceContext.PixelShader.SetShaderResource(null, 1);
            }

            if (mat.BumpType == TextureType.texture2D)
                deviceContext.PixelShader.SetShaderResource(mat.BumpMap, 2);
            else if (mat.BumpType == TextureType.texture3D)
                deviceContext.PixelShader.SetShaderResource(mat.BumpMap, 3);
            else
            {
                deviceContext.PixelShader.SetShaderResource(null, 2);
                deviceContext.PixelShader.SetShaderResource(null, 3);
            }

            if (texture != null)
                deviceContext.PixelShader.SetShaderResource(texture, 4);

            // Draw the Mesh.
            obj.DrawSubset(deviceContext, subset);
        }

        /// <summary>
        /// Updates the model
        /// </summary>
        /// <param name="frameTime">Time between current and last frame</param>
        public void Update(double frameTime)
        {
            if (m_Window.UserInput.InputPaused)
                return;

            if (m_Window.UserInput.IsKeyPressed(SlimDX.DirectInput.Key.U)){
                m_Lights[1].pos.Y += 10.0f * (float) frameTime;
            }
            if (m_Window.UserInput.IsKeyPressed(SlimDX.DirectInput.Key.D)){
                m_Lights[1].pos.Y -= 10.0f * (float) frameTime;
            }

            cube.Tranformation = Matrix.Scaling(cubeScalling, cubeScalling, cubeScalling) *
                                 Matrix.Translation(m_Lights[1].pos);
        }

        /// <summary>
        /// This method initializes our constant buffers.
        /// </summary>
        private void InitConstantBuffers()
        {
            // Create the Buffer WVP that changes per Object
            m_RenderBuffer = new RenderBuffer(device);

            // Tell the VertexShader to use our constant buffers.
            m_RenderBuffer.VertexShader(m_RenderBufferSlot); 
            m_RenderBuffer.PixelShader(m_RenderBufferSlot); 

            // Create the Material Buffer that changes per Object
            m_MaterialBuffer = new MaterialBuffer(device);

            // Tell the VertexShader to use our constant buffers.
            m_MaterialBuffer.PixelShader(m_MaterialBufferSlot);

            // Create the Light Buffer that changes per Frame
            m_LightBuffer = new LightBuffer(device);

            // Tell the VertexShader to use our constant buffers.
            m_LightBuffer.PixelShader(m_LightBufferSlot);
        }

        /// <summary>
        /// This method initializes our vertex shader and pixel shader.
        /// </summary>
        private void InitShaders()
        {

            string vsCompileError = "Vertex Shader Compile Error!!!";
            using (var bytecode = ShaderBytecode.CompileFromFile("Effects.fx",
                                                                 "Vertex_Shader",
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
                m_VShaderSignature = ShaderSignature.GetInputSignature(bytecode);
                m_VertexShader = new VertexShader(device, bytecode);
            }

            string psCompileError = "Pixel Shader Compile Error!!!";
            using (var bytecode = ShaderBytecode.CompileFromFile("Effects.fx",
                                                                 "Pixel_Shader",
                                                                 "ps_4_0",
#if DEBUG
                                                                         ShaderFlags.Debug,
#else

                                                                         ShaderFlags.None,
#endif
                                                                         SlimDX.D3DCompiler.EffectFlags.None,
                                                                 null,
                                                                 null,
                                                                 out psCompileError))
            {
                m_PixelShader = new PixelShader(device, bytecode);
            }
            deviceContext.PixelShader.Set(m_PixelShader);
            // Define the vertex format.
            // This tells Direct3D what information we are storing for each vertex, and how it is stored.
            InputElement[] InputElements = new InputElement[]
            {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                        new InputElement("NORMAL",    0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float,       InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
            };

            // Create the InputLayout using the vertex format we just created.
            m_InputLayout = new InputLayout(device,
                                            m_VShaderSignature,
                                            InputElements);
            // Setup the InputAssembler stage of the Direct3D 11 graphics pipeline.
            deviceContext.InputAssembler.InputLayout = m_InputLayout;


        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_VertexShader?.Dispose();
                    m_PixelShader?.Dispose();
                    m_InputLayout?.Dispose();

                    m_RenderBuffer?.Dispose();
                    m_MaterialBuffer?.Dispose();
                    m_LightBuffer?.Dispose();

                    m_modelTexSamplerState?.Dispose();

                    model?.Dispose();
                    cube?.Dispose();
                    m_Sky?.Dispose();
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
