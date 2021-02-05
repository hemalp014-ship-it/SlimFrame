using System;
using System.Drawing;
using System.Windows.Forms;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

using DXGI = SlimDX.DXGI;
using d11 = SlimDX.Direct3D11;
using d2d = SlimDX.Direct2D;

using SlimFrame;
using SlimFrame.UI;
using SlimFrame.Cameras;
using UI = SlimFrame.UI;
using SlimDX.Windows;
using SlimDX.DirectInput;
using System.Threading;

using static System.Math;
 
namespace SlimFrame
{

    /// <summary>
    /// Enumrates Vsync State
    /// </summary>
    public enum Vsync
    {
        Immediate,
        One = 1,
        Two = 2,
        Three = 4,
    }

    /// <summary>
    /// The Window class to handle the 3D rendering
    /// </summary>
    public class GameWindow3D : SlimFrame.GameWindow
    {
        #region Member Variables
        
        //3D rendering support
        protected d11.Device m_Device;           // The Direct3D device
        protected DeviceContext m_DeviceContext;    // This is just a convenience member.  It holds the context for the Direct3D device
        protected RenderTargetView m_RenderTargetView; // Our render target
        protected SwapChain m_SwapChain;        // Our swap chain
        public Viewport Viewport { get; protected set; }         // The viewport
                 
        public RasterizerState CullClockwise { get; protected set; }
        public RasterizerState CullCounterClockwise { get; protected set; }
        public RasterizerState CullNone { get; protected set; }
        public RasterizerState Wireframe { get; protected set; }

        public BlendState BlendTransparent { get; protected set; } //Direct3D 11 Blend state to render the transparency in 2D scene.

        //2D rendering support
        private System2D m_System2D;         // 2D rendering system

        protected Size oldSize;            // Old size of the form
        protected FormWindowState oldState;            // Old window state of the form

        //Depth stencil Support
        Texture2D m_DepthStencilTexture = null;   // Holds the depth stencil texture.
        DepthStencilView m_DepthStencilView = null;   // The depth stencil view Object.
        
        bool Resizing = false;

        //float speedX = 0.0f, speedY = 0.0f;
        //float sensitivity = 0.2f / 7.50f;
        //float speedMax = 7.50f;

        #endregion

        #region Properties

        public Vsync Vsync { get; set; } = Vsync.Immediate;

        public bool IsErrorOccured { get; protected set; } = false;

        //public float CameraSpeed
        //{
        //    get
        //    {
        //        return speedMax;
        //    }
        //    set
        //    {
        //        //float oldRestoration = speedMax * sensitivity;
        //        speedMax = value;
        //        //sensitivity = oldRestoration / speedMax;
        //    }
        //}

        //public float CameraSensitivity
        //{
        //    get
        //    {
        //        return sensitivity;
        //    }
        //    set
        //    {
        //        sensitivity = value;
        //    }
        //}

        /// <summary>
        /// Heads on Display for the GameWindow3D
        /// </summary>
        public HUD HOD { get; set; }

        /// <summary>
        /// 2D rendering system for GameWindow3D
        /// </summary>
        public System2D System2D
        {
            get
            {
                return m_System2D;
            }
        }

        /// <summary>
        /// The Direct3D11 Device
        /// </summary>
        public SlimDX.Direct3D11.Device Device
        {
            get
            {
                return m_Device;
            }
            protected set
            {
                m_Device = value;


                //text
            }
        }

        /// <summary>
        /// The Direct3D11 Device Context
        /// </summary>
        public DeviceContext DeviceContext
        {
            get
            {
                return m_DeviceContext;
            }
            protected set
            {
                m_DeviceContext = value;
            }
        }
        
        /// <summary>
        /// The rendering target
        /// </summary>
        public RenderTargetView RenderTargetView
        {
            get
            {
                return m_RenderTargetView;
            }
            protected set
            {
                m_RenderTargetView = value;
            }
        }

        #endregion

        #region Event System
         
        // Event variables
        public delegate void _RenderFrame(DeviceContext devCon);
        /// <summary>
        /// Occures when the Frame is to be rendered. Do rendering in this event.
        /// </summary>
        public event _RenderFrame RenderFrame;
        
        // Event variables 
        public delegate void _Render2D(d2d.RenderTarget renderTarget);
        /// <summary>
        /// Occures when the Frame is to be rendered. Do rendering in this event.
        /// </summary>
        public event _Render2D Render2D;

        public delegate void _UpdateFrame(double frameTime);
        /// <summary>
        /// Occures when Frame is to be updated. Do Updatation in this event.
        /// </summary>
        public event _UpdateFrame UpdateFrame;

        public delegate void _DeviceCreated(SlimDX.Direct3D11.Device device);
        /// <summary>
        /// Occures when the device is created. Create Device dependent variables.
        /// </summary>
        public event _DeviceCreated DeviceCreated;

        public delegate void _Resized2D(d2d.RenderTarget renderTarget);
        /// <summary>
        /// Occures when the 2D system is resized. Create 2D render target dependent variables.
        /// </summary>
        public event _Resized2D Resized2D;

        public delegate void _Initialize2D(d2d.RenderTarget renderTarget, d2d.Factory Factory);
        /// <summary>
        /// Occures when the 2D system is Initialized. Create 2D render target dependent variables.
        /// </summary>
        public event _Initialize2D Initialized2D;

        /// <summary>
        /// Occures when the 2D system is to be resized. release 2D render target dependent variables.
        /// </summary>
        public event Action Releasing2D;

        public delegate void _DeviceDisposing();
        /// <summary>
        /// Occures when the device is disposed. Dispose Device dependent variables.
        /// </summary>
        public event _DeviceDisposing DeviceDisposing;

        public delegate void _SwapChainReleasing();
        /// <summary>
        /// Occures when SwapChain is Releasing. Release SwapChain dependent variables.
        /// </summary>
        public event _SwapChainReleasing SwapChainReleasing;

        public delegate void _SwapChainResized(SlimDX.Direct3D11.Device device, SwapChain swapChain);
        /// <summary>
        /// Occures when SwapChain is Resized. Create SwapChain dependent variables.
        /// </summary>
        public event _SwapChainResized SwapChainResized;

        /// <summary>
        /// Occures when HOD is Loaded. Generate, Arrange & Add your UI elements to HOD.
        /// </summary>
        public event Action HODLoaded;

        #endregion

        #region Constructors

        /// <summary>
        /// This is the constructor.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="fullscreen">Whether the window should be fullscreen or not.</param>
        public GameWindow3D(string title, int width, int height, bool fullscreen)
            : base(title, width, height, fullscreen)
        {
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes 3D Game Window
        /// </summary>
        /// <param name="type"></param>
        private void Init(DriverType type = DriverType.Hardware)
        {

            Camera = new ThirdPersonCamera(new Vector3(0, 0, 0), 5.0f, new Vector3(0, 1, 0));

            // Initialize Direct3D.
            InitD3D(type);
            if (IsErrorOccured)
            {
                IsErrorOccured = false;
                Init(DriverType.Software);
                return;
            }

            // Initialize the depth stencil.
            InitDepthStencil();
            if (IsErrorOccured) return;

            // Initialize the depth stencil.
            InitStates(Device);
            if (IsErrorOccured) return;

            DeviceCreated?.Invoke(m_Device);

            // Initialize the UI
            Init2D();
            if (IsErrorOccured) return;

            //SwapChainResized?.Invoke(m_Device, m_SwapChain);

            oldSize = FormObject.Size;
        }
        
        /// <summary>
        /// This method initializes Direct3D for us.
        /// </summary>
        private void InitD3D(DriverType type)
        {
            // Setup the configuration for the SwapChain.
            var swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 1, // 1 back buffer (a.k.a. double Buffering).
                Usage = Usage.RenderTargetOutput,
                OutputHandle = FormObject.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard,                
            };

            // Create the SwapChain and check for errors.
            if (d11.Device.CreateWithSwapChain(type,
#if DEBUG
                                                         DeviceCreationFlags.Debug,
#else
                                                         DeviceCreationFlags.None,
#endif
                                                         new FeatureLevel[] { FeatureLevel.Level_11_0 },
                                                         swapChainDesc,
                                                         out m_Device,
                                                         out m_SwapChain).IsFailure)
            {
                // An error has occurred.  Initialization of the Direct3D device has failed for some reason.
                IsErrorOccured = true;
                return;
            }

            // Create a view of our render target, which is the backbuffer of the swap chain we just created
            using (var resource = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(m_SwapChain, 0))
            {
                m_RenderTargetView = new RenderTargetView(m_Device, resource);
            };


            // Get the device context and store it in our m_DeviceContext member variable.
            m_DeviceContext = m_Device.ImmediateContext;


            // Setting a viewport is required if you want to actually see anything
            Viewport = new Viewport(0.0f, 0.0f,
                                    FormObject.ClientSize.Width, FormObject.ClientSize.Height,
                                    0.0f, 1.0f);

            m_DeviceContext.Rasterizer.SetViewports(Viewport);
            m_DeviceContext.OutputMerger.SetTargets(m_RenderTargetView);


            // Prevent DXGI handling of Alt+Enter since it does not work properly with Winforms
            using (var factory = m_SwapChain.GetParent<Factory>())
            {
                factory.SetWindowAssociation(FormObject.Handle,
                                             WindowAssociationFlags.IgnoreAltEnter);
            };

            SwapChainReleasing += GameWindow3D_SwapChainReleasing;
            SwapChainResized += GameWindow3D_SwapChainResized;
        }

        /// <summary>
        /// Initializes the depth stencil for us.
        /// </summary>
        private void InitDepthStencil()
        {
            try
            {

                // Create the depth stencil texture description
                Texture2DDescription DepthStencilTextureDesc = new Texture2DDescription()
                {
                    Width = FormObject.ClientSize.Width,
                    Height = FormObject.ClientSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.D24_UNorm_S8_UInt,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };

                // Create the Depth Stencil View description
                DepthStencilViewDescription DepthStencilViewDesc = new DepthStencilViewDescription()
                {
                    Format = DepthStencilTextureDesc.Format,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    MipSlice = 0,
                };

                // Create the depth stencil texture.
                m_DepthStencilTexture = new Texture2D(m_Device, DepthStencilTextureDesc);

                // Create the DepthStencilView Object.
                m_DepthStencilView = new DepthStencilView(m_Device, m_DepthStencilTexture, DepthStencilViewDesc);
            }
            catch (Exception)
            {
                IsErrorOccured = true;
            }
        }

        /// <summary>
        /// Initializes 3D rendering State
        /// </summary>
        /// <param name="device"></param>
        private void InitCull(d11.Device device)
        {
            RasterizerStateDescription rasterdesc = new RasterizerStateDescription();
            rasterdesc.FillMode = FillMode.Solid;
            rasterdesc.CullMode = CullMode.Back;
            rasterdesc.IsFrontCounterclockwise = true;
            CullCounterClockwise = RasterizerState.FromDescription(device, rasterdesc);

            rasterdesc.IsFrontCounterclockwise = false;
            CullClockwise = RasterizerState.FromDescription(device, rasterdesc);

            rasterdesc.CullMode = CullMode.None;
            CullNone = RasterizerState.FromDescription(device, rasterdesc);

            rasterdesc.FillMode =  FillMode.Wireframe;
            Wireframe = RasterizerState.FromDescription(device, rasterdesc);

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
            BlendTransparent = BlendState.FromDescription(device, bsd);
        }
        
        /// <summary>
        /// Initializes 3D blend states
        /// </summary>
        /// <param name="device"></param>
        private void InitStates(d11.Device device)
        {
            RasterizerStateDescription rasterdesc = new RasterizerStateDescription();
            rasterdesc.FillMode = FillMode.Solid;
            rasterdesc.CullMode = CullMode.Back;
            rasterdesc.IsFrontCounterclockwise = true;
            CullCounterClockwise = RasterizerState.FromDescription(device, rasterdesc);

            rasterdesc.IsFrontCounterclockwise = false;
            CullClockwise = RasterizerState.FromDescription(device, rasterdesc);

            rasterdesc.CullMode = CullMode.None;
            CullNone = RasterizerState.FromDescription(device, rasterdesc);

            rasterdesc.FillMode =  FillMode.Wireframe;
            Wireframe = RasterizerState.FromDescription(device, rasterdesc);

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
            BlendTransparent = BlendState.FromDescription(device, bsd);
        }

        /// <summary>
        /// Initializes the 2D system.
        /// </summary>
        private void Init2D()
        {
            m_System2D = new System2D(m_Device, FormObject, this);
            if (m_System2D.IsErrorOccured)
            { 
                IsErrorOccured = true; GameRun = false; return; 
            }

            m_System2D.Render2D += M_System2D_Render2D;
            m_System2D.Releasing2D += M_System2D_Releasing2D;
            m_System2D.Resized2D += M_System2D_Resized2D;
            m_System2D.BlendTransparent = BlendTransparent;

            HOD = new HUD();

            m_System2D.HOD = HOD;
            m_System2D.Init();

            Initialized2D?.Invoke(m_System2D.RenderTarget, m_System2D.Direct2DFactory);

            if (HOD != null)
            {
                HODLoaded?.Invoke();
                HOD.UserInput = UserInput;
                HOD.TakeOwnerShip(this);
            }

            //HOD.Font = new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            //HODCreated?.Invoke(HOD);
            //ClearColor = Color.Gray;
            //HOD.TakeOwnerShip(FormObject);
        }

        private void M_System2D_Releasing2D()
        {
            Releasing2D?.Invoke();
        }

        private void M_System2D_Resized2D(d2d.RenderTarget renderTarget)
        {
            Resized2D?.Invoke(renderTarget);
        }

        private void M_System2D_Render2D(d2d.RenderTarget renderTarget)
        {
            renderTarget.BeginDraw();
            renderTarget.Clear(new Color4(0, 0, 0, 0));

            Render2D?.Invoke(renderTarget);
            HOD?.Render(renderTarget);

            renderTarget.EndDraw();
        }

        #endregion

        #region Methods 

        /// <summary>
        /// Starts Game Loop
        /// </summary>
        public override void StartGameLoop()
        {
            // if initialization is already finished, then simply return.
            if (m_IsInitialized)
                return;
            
            FormObject.FormClosing += (s, e) => { Dispose(); };
            FormObject.ResizeBegin += (s, e) => { Resizing = true; IsPaused = true; };
            FormObject.ResizeEnd += (s, e) => { Resizing = false; IsPaused = false; };
            FormObject.SizeChanged += (s, e) => {
                if (FormObject.WindowState != oldState)
                {
                    if (FormObject.WindowState != FormWindowState.Minimized)
                        IsPaused = true;
                    oldState = FormObject.WindowState;
                }
            };

            //Register the formObject's resize event to handle the resize event.
            FormObject.Resize += Form_Resize;

            FormObject.SystemSuspend += (s, e) =>
            {
                GameRun = false;
            };
            FormObject.SystemResume += (s, e) =>
            {
                GameRun = true;
            };

            FormObject.Paint += (s, e) =>
            {
                if (!m_IsInitialized)
                {
                    m_IsInitialized = true;
                    Init();

                    if (IsErrorOccured)                    
                    {
                        FormObject.Hide();
                        MessageBox.Show("Due to an error Application was Unable to start!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Exit();
                    }                      
                }

                if (GameRun && !FormObject.IsDisposed)
                    GameLoop();
                FormObject.Invalidate();
            };

            FormObject.Show();

            Application.Run(FormObject);
        }

        /// <summary>
        /// This method is called once per frame and is where we update objects in our scene.
        /// <param name="frameTime">The amount of time (in seconds) that has elapsed since the previous frame.</param>
        /// </summary>
        public override void Update(double frameTime)
        {
            double time = frameTime;
            double timeLimited = Max(time, 0.01667);

            base.Update(time);

            if (IsDisposed) return;

            if (HOD != null)
                HOD.FramePerSecond = FramesPerSecond;
            HOD.Update(time);

            m_System2D.Update(time);
            //HOD?.Update(frameTime);
            UpdateFrame?.Invoke(timeLimited);

            if (FormObject.WindowState == oldState && oldState != FormWindowState.Minimized)
            {
                IsPaused = false;
            }
        }

        /// <summary>
        /// Updates camera state
        /// </summary>
        /// <param name="frameTime">The amount of time (in seconds) that has elapsed since the previous frame.</param>
        protected override void UpdateCamera(double frameTime)
        {
            float time = (float)frameTime;
            float timeLimited = Max(time, 0.01667f);

            if (HOD.ActiveControl != HOD) return;
            if (!Focused) return;

            if (Resizing) return;

            if (m_UserInput.IsMouseButtonPressed(0) && DXControl.IsMouseDown(HOD))
            {
                int x = m_UserInput.MouseState_Current.X;
                int y = m_UserInput.MouseState_Current.Y;

                Camera.Pitch -= y * 0.4f * timeLimited;
                Camera.Yaw += x  * 0.4f * timeLimited;
            }

            var type = Camera.GetType();
            
            if (type == typeof(FirstPersonCamera))
            {
                if (DXControl.IsMouseOver(HOD))
                    Camera.Position += Camera.Front * (m_UserInput.MouseWheelMovement() * time);

                if (m_UserInput.IsKeyPressed(Key.UpArrow) || m_UserInput.IsKeyPressed(Key.W))
                    Camera.Position += Camera.Front * timeLimited;
                if (m_UserInput.IsKeyPressed(Key.DownArrow) || m_UserInput.IsKeyPressed(Key.S))
                    Camera.Position += Camera.Back * timeLimited;
                if (m_UserInput.IsKeyPressed(Key.LeftArrow) || m_UserInput.IsKeyPressed(Key.A))
                    Camera.Position += Camera.Left * timeLimited;
                if (m_UserInput.IsKeyPressed(Key.RightArrow) || m_UserInput.IsKeyPressed(Key.D))
                    Camera.Position += Camera.Right * timeLimited;

            }
            else if (type == typeof(ThirdPersonCamera))
            {
                ThirdPersonCamera cam1 = (ThirdPersonCamera)Camera;
                float factor = (float) Pow(cam1.Distance / 10, 1.2);

                if (DXControl.IsMouseOver(HOD))
                    cam1.Distance -= factor * m_UserInput.MouseWheelMovement() * time;

                if (m_UserInput.IsKeyPressed(Key.UpArrow) || m_UserInput.IsKeyPressed(Key.W))
                    cam1.Distance -= 5.0f * timeLimited;
                if (m_UserInput.IsKeyPressed(Key.DownArrow) || m_UserInput.IsKeyPressed(Key.S))
                    cam1.Distance += 5.0f * timeLimited;
            }

            Camera.Update();
        }
        
        /// <summary>
        /// This function is called each time we need to render the next frame.
        /// </summary>
        public override void Render()
        {
            if (!IsInitialized || IsDisposed)
                return;

            // Clear the screen before we draw the next frame.
            m_DeviceContext.ClearRenderTargetView(m_RenderTargetView, ClearColor);
            m_DeviceContext.ClearDepthStencilView(m_DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            // Make the DepthStencilView active.
            m_DeviceContext.OutputMerger.SetTargets(m_DepthStencilView, m_RenderTargetView);

            // Invokes the Rendering event to let the application know about that it can render the frame
            RenderFrame?.Invoke(m_DeviceContext);

            // Renders the 2D system
            m_System2D.Render();

            // Present the frame we just rendered to the user.
            m_SwapChain.Present((int) Vsync, PresentFlags.None);
        }

        /// <summary>
        /// This function toggles fullscreen mode.  It does nothing here (except for updating the m_bFullscreen member variable) because thats all it needs to do in this base class.
        /// This function is overridden by derived classes that need to add code to toggle fullscreen mode.
        /// </summary>
        public override void ToggleFullscreen()
        {
            //base.ToggleFullscreen();

            if (!IsFullscreen)
            {
                oldState = FormObject.WindowState;

                if (oldState == FormWindowState.Normal)
                {
                    // Stores the Size of the normal window when we are going to full screen
                    oldSize = FormObject.Size;
                }
                //FormObject.WindowState = FormWindowState.Normal;
                FormObject.FormBorderStyle = FormBorderStyle.None;
                FormObject.WindowState = FormWindowState.Maximized;
                // Changes the size of form Object to the screen size
                FormObject.ClientSize = Screen.PrimaryScreen.Bounds.Size;
                HOD.Focus();

                m_SwapChain.IsFullScreen = true;
                // It should be only in this order, because if you changes the size of form after changing
                // the full screen state of swap chain the size won't be changed.
            }
            else
            {
                m_SwapChain.IsFullScreen = false;

                FormObject.WindowState = oldState;

                FormObject.FormBorderStyle = FormBorderStyle.Sizable;
                // Changes the size of form Object to the old size
                FormObject.Size = oldSize;

                // It should be only in this order, because if you changes the size of form before changing
                // the full screen state of swap chain the size won't be changed.
            }
            IsFullscreen = !IsFullscreen;//m_SwapChain.IsFullScreen;
        }

        protected override void FocusLost()
        {
            if (IsFullscreen && !IsDisposed)
            {
                if (m_SwapChain != null)
                    m_SwapChain.IsFullScreen = false;
            }
        }

        protected override void FocusGiven()
        {
            if (m_SwapChain != null)
                if (m_SwapChain.IsFullScreen)
                {

                }
                else
                {
                    if (IsFullscreen)
                    {
                        IsFullscreen = false;
                        ToggleFullscreen();
                    }
                }
        }

        #endregion

        #region Event Handler

        /// <summary>
        /// Handles the resize event of the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_Resize( Object sender, EventArgs e)
        {
            if (!IsInitialized)
                return;

            if (FormObject.WindowState == FormWindowState.Minimized)
            {
                IsPaused = true;
                return;
            }

            // Gets the Size of the form Object.
            float Width = FormObject.ClientSize.Width;
            float Height = FormObject.ClientSize.Height;

                if (m_SwapChain.IsFullScreen)
                {
                    Width = Screen.PrimaryScreen.Bounds.Width;
                    Height = Screen.PrimaryScreen.Bounds.Height;
                }


            // Invokes the SwapChainResizing event to let the application know that
            // the swap chain is to be released, so it should release all swap chain
            // dependent variables. Otherwise there will be error while resizing the swap chain.
            SwapChainReleasing?.Invoke();

            // Disposes the render target view.
            m_RenderTargetView.Dispose();

            // Resizes the swap chain.
            m_SwapChain.ResizeBuffers(
                    m_SwapChain.Description.BufferCount,
                    FormObject.ClientSize.Width,
                    FormObject.ClientSize.Height,
                    m_SwapChain.Description.ModeDescription.Format,
                    m_SwapChain.Description.Flags
                );

            // Creates new render target view from the resized swap chin.
            using (var resource = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(m_SwapChain, 0))
            {
                m_RenderTargetView = new RenderTargetView(m_Device, resource);
            };
            m_DeviceContext.OutputMerger.SetTargets(m_RenderTargetView); // Sets render target view.

            Viewport = new Viewport(0.0f, 0.0f,
                                    Width, Height,
                                    0.0f, 1.0f); // Creates new ViewPort.

            m_DeviceContext.Rasterizer.SetViewports(Viewport); // Sets the viewport.

            m_System2D.Resize(); // Resizes 2D System.

            // Invokes the SwapChainResized event to let the application know that
            // swap chain is resized, so it should create all swap chain dependent
            // variable.
            SwapChainResized?.Invoke(m_Device, m_SwapChain);
            IsPaused = false;

            //System.Diagnostics.Debug.WriteLine(state.Width);
        }

        private void GameWindow3D_SwapChainResized(d11.Device device, SwapChain swapChain)
        {
            InitDepthStencil();
        }

        private void GameWindow3D_SwapChainReleasing()
        {
            m_DepthStencilTexture?.Dispose();
            m_DepthStencilView?.Dispose();
        }

        //private void GameWindow3D_HODCreated(HOD hod)
        //{
        //    slide = new Slider("slide1")
        //    {
        //        Text = "Value",
        //        ForeColor = Color.Black,
        //        BackColor = Color.White,
        //        Location = new PointF(100, 50),
        //        ClientSize = new SizeF(150, HOD.Font.Height),
        //        TabIndex = 1,
        //        Maximum = 100.0f,
        //        Minimum = 0.0f,
        //        Value = 0.0f,
        //    };
        //    HOD.Add(slide);

        //    state = new UI.Label("label1")
        //    {
        //        Text = "",
        //        Location = new PointF(0, 0),
        //        Height = HOD.Font.SizeInPoints,
        //        Width = FormObject.Width,
        //        ForeColor = Color.Black,
        //    };

        //    vScroll = new Scroller("vScroll", UI.ScrollOrientation.Vertical)
        //    {
        //        BackColor = Color.White,
        //        Location = new PointF(100, 170),
        //        Size = new SizeF(25,150),
        //        TabIndex = 4,
        //        Maximum = 1.0f,
        //        Increment = 0.9f,
        //        Minimum = 0.0f,
        //        Value = 0.0f,
        //    };
        //    vScroll.ValueChanged += vScroll_ValueChanged;

        //    hScroll = new Scroller("hscroll", UI.ScrollOrientation.Horizontal)
        //    {
        //        BackColor = Color.White,
        //        Location = new PointF(100 + 25, vScroll.Bottom - vScroll.Width),
        //        Size = new SizeF(150, 25),
        //        TabIndex = 5,
        //        Maximum = 1.0f,
        //        Increment = 0.9f,
        //        Minimum = 0.0f,
        //        Value = 0.0f,
        //        Orientation = UI.ScrollOrientation.Horizontal
        //    };
        //    hScroll.ValueChanged += HScroll_ValueChanged;

        //    btn = new UI.Button("btn1")
        //    {
        //        Text = "Button",
        //        ForeColor = Color.Black,
        //        BackColor = Color.DarkSlateBlue,
        //        Location = new PointF(100, 110),
        //        Size = new SizeF(100, HOD.Font.Height),
        //        TabIndex = 1,
        //        HorizontalAllignment = TextHAllignment.Center,
        //        VerticalAllignment = TextVAllignment.Center,
        //    };

        //    combo = new UI.ComboBox("combo1")
        //    {
        //        Text = "Combo",
        //        ForeColor = Color.Black,
        //        BackColor = Color.DarkSlateBlue,
        //        Location = new PointF(btn.Right + 10, 110),
        //        Size = new SizeF(100, HOD.Font.Height),
        //        TabIndex = 3,
        //        DropDownHeight = 100,
        //        HorizontalAllignment = TextHAllignment.Center,
        //        VerticalAllignment = TextVAllignment.Center,
        //    };
        //    combo.Items.AddRange(new string[] { "Auto", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" });
        //    combo.SelectedIndex = 0;
        //    combo.SelectedIndexChanged += Combo_SelectedIndexChanged;

        //    radioBtn = new UI.RadioButton("rad1")
        //    {
        //        Text = "Enable",
        //        ForeColor = Color.Black,
        //        Location = new PointF(100, 10),
        //        TabIndex = 0,
        //        HorizontalAllignment = TextHAllignment.Left,
        //        VerticalAllignment = TextVAllignment.Center,
        //    };
        //    radioBtn.CheckChanged += radioBtn_CheckChanged;

        //    radioBtn1 = new UI.RadioButton("rad2")
        //    {
        //        Text = "Disable",
        //        ForeColor = Color.Black,
        //        Location = new PointF(10, 10),
        //        TabIndex = 0,
        //        HorizontalAllignment = TextHAllignment.Left,
        //        VerticalAllignment = TextVAllignment.Center,
        //    };

        //    check = new UI.CheckBox("CheckBox1")
        //    {
        //        Text = "Check",
        //        ForeColor = Color.Black,
        //        Location = new PointF(10, 10),
        //        TabIndex = 0,
        //        HorizontalAllignment = TextHAllignment.Left,
        //        VerticalAllignment = TextVAllignment.Center,
        //    };

        //    pic = new UI.PictureBox()
        //    {
        //        Location = new PointF(200, 200),
        //        Size = new SizeF(100, 100),
        //    };

        //    HOD.Add(state);
        //    HOD.Add(vScroll);
        //    HOD.Add(hScroll);
        //    HOD.Add(btn);
        //    HOD.Add(combo);
        //    HOD.Add(check);
        //    HOD.Add(radioBtn);
        //    HOD.Add(radioBtn1);
        //    HOD.Add(pic);

        //    //radioBtn1.Left = radioBtn.Right + 10;
        //    //check.Left = 100;
        //    radioBtn1.Left = radioBtn.Right + 10;
        //    check.Left = radioBtn1.Right + 10;

        //    pic.LoadImage("My Love.jpg");  
        //}

        //private void Combo_SelectedIndexChanged(UI.ComboBox c, int o)
        //{
        //    if (c.SelectedIndex == 0)
        //        pic.Opacity = -1;
        //    else
        //        pic.Opacity = c.SelectedIndex / (c.Items.Count - 2.0f);
        //}

        //private void radioBtn_CheckChanged(UI.RadioButton c)
        //{
        //    btn.Enabled = radioBtn.Checked;
        //    slide.Enabled = radioBtn.Checked;
        //    vScroll.Enabled = radioBtn.Checked;
        //    hScroll.Enabled = radioBtn.Checked;
        //    check.Enabled = radioBtn.Checked;
        //    //check.Enabled = true;
        //    combo.Enabled =  radioBtn.Checked;
        //}

        //private void HScroll_ValueChanged(Scroller c, float arg2)
        //{
        //    state.Left = c.Value * (FormObject.ClientSize.Width - state.Width);
        //}

        //private void vScroll_ValueChanged(Scroller c, float oldValue)
        //{
        //    state.Top = c.Value * (FormObject.ClientSize.Height - state.Height);
        //}

        //Slider slide;
        //Scroller vScroll;
        //Scroller hScroll;
        //UI.RadioButton radioBtn;
        //UI.RadioButton radioBtn1;
        //UI.CheckBox check;
        //UI.Button btn;
        //UI.ComboBox combo;
        //UI.PictureBox pic;

        #endregion

        #region Disposal

        /// <summary>
        /// This is the GameWindow class's internal Dispose() method that actually disposes of the game window class.
        /// </summary>
        /// <param name="disposing">If this method is being called by this class's own code, then the value true should be passed into this parameter to indicate that this method was not called by the Garbage Collector.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    // Unregister events
                    FormObject.Resize -= Form_Resize;
                    // get rid of managed resources

                    DeviceDisposing?.Invoke();

                    m_SwapChain.IsFullScreen = false;
                    m_SwapChain?.Dispose();

                    m_RenderTargetView?.Dispose();

                    CullNone?.Dispose();
                    Wireframe?.Dispose();
                    CullClockwise?.Dispose();
                    CullCounterClockwise?.Dispose();
                    BlendTransparent?.Dispose();

                    m_DepthStencilTexture?.Dispose();
                    m_DepthStencilView?.Dispose();

                    m_Device?.Dispose();
                    m_DeviceContext?.Dispose();
                    m_System2D?.Dispose();
                    
                }
            }


            base.Dispose(disposing);

        }

        #endregion
    }
}