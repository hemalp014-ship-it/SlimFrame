using System;
using System.Drawing;
using System.Windows.Forms;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

using DXGI = SlimDX.DXGI;
using D11 = SlimDX.Direct3D11;

using SlimFrame;
using SlimFrame.UI;
using UI = SlimFrame.UI;

namespace SlimFrame
{
    /// <summary>
    /// The Window class to handle the 3D rendering
    /// </summary>
    public class GameWindow3D : SlimFrame.GameWindow
    {
        #region Member Variables

        protected D11.Device        m_Device;           // The Direct3D device
        protected DeviceContext     m_DeviceContext;    // This is just a convenience member.  It holds the context for the Direct3D device
        protected RenderTargetView  m_RenderTargetView; // Our render target
        protected SwapChain         m_SwapChain;        // Our swap chain
        protected Viewport          m_Viewport;         // The viewport

        //2D rendering support
        protected System2D          m_System2D;         // 2D rendering system

        protected Size              oldSize;            // Old size of the form

        // Depth stencil Support
        Texture2D           m_DepthStencilTexture = null;   // Holds the depth stencil texture.
        DepthStencilView    m_DepthStencilView    = null;   // The depth stencil view object.

        UI.Label state;

        public bool ShowFPS = true; 

        #endregion

        #region Properties

        /// <summary>
        /// Heads on Display for the GameWindow
        /// </summary>
        public HOD HOD
        {
            get
            {
                return m_System2D.HOD;
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
        /// The Rendering Viewport
        /// </summary>
        public Viewport Viewport
        {
            get
            {
                return m_Viewport;
            }
            protected set
            {
                m_Viewport = value;
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

        public delegate void _DeviceDisposed();
        /// <summary>
        /// Occures when the device is disposed. Dispose Device dependent variables.
        /// </summary>
        public event _DeviceDisposed DeviceDisposed;

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

        public delegate void _HODCreated(HOD hod);
        /// <summary>
        /// Occures when HOD is Created.
        /// </summary>
        public event _HODCreated HODCreated;

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
            // Initialize Direct3D.
            InitD3D();

            DeviceCreated?.Invoke(m_Device);
            SwapChainResized?.Invoke(m_Device, m_SwapChain);

            // Initialize the depth stencil.
            InitDepthStencil();

            // Initialize the UI
            Init2D();

            HODCreated?.Invoke(HOD);

            //Register the formObject's resize event to handle the resize event.
            FormObject.Resize += Resize;

            oldSize = FormObject.Size;

            Camera = new ThirdPersonCamera(new Vector3(0, 0, 0), 5.0f, new Vector3(0, 1, 0));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the resize event of the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Resize(object sender, EventArgs e)
        {
            float Width = FormObject.ClientSize.Width;
            float Height = FormObject.ClientSize.Height;

            SwapChainReleasing?.Invoke();

            m_RenderTargetView.Dispose();
            m_SwapChain.ResizeBuffers(
                    m_SwapChain.Description.BufferCount,
                    FormObject.ClientSize.Width,
                    FormObject.ClientSize.Height,
                    m_SwapChain.Description.ModeDescription.Format,
                    m_SwapChain.Description.Flags
                );

            using (var resource = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(m_SwapChain, 0))
            {
                m_RenderTargetView = new RenderTargetView(m_Device, resource);
            };
            m_DeviceContext.OutputMerger.SetTargets(m_RenderTargetView);

            m_Viewport = new Viewport(0.0f, 0.0f,
                                    Width, Height,
                                    0.0f, 1.0f);

            m_DeviceContext.Rasterizer.SetViewports(m_Viewport);

            m_System2D.Resize();

            SwapChainResized?.Invoke(m_Device, m_SwapChain);

            state.Width = FormObject.Width;
        }
 
        /// <summary>
        /// This method initializes Direct3D for us.
        /// </summary>
        private void InitD3D()
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
                SwapEffect = SwapEffect.Discard
            };

            // Create the SwapChain and check for errors.
            if (SlimDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
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
            m_Viewport = new Viewport(0.0f, 0.0f,
                                    FormObject.ClientSize.Width, FormObject.ClientSize.Height,
                                    0.0f, 1.0f);

            m_DeviceContext.Rasterizer.SetViewports(m_Viewport);
            m_DeviceContext.OutputMerger.SetTargets(m_RenderTargetView);


            // Prevent DXGI handling of Alt+Enter since it does not work properly with Winforms
            using (var factory = m_SwapChain.GetParent<Factory>())
            {
                factory.SetWindowAssociation(FormObject.Handle,
                                             WindowAssociationFlags.IgnoreAltEnter);
            };
        }

        /// <summary>
        /// Initializes the depth stencil for us.
        /// </summary>
        private void InitDepthStencil()
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

            // Create the DepthStencilView object.
            m_DepthStencilView = new DepthStencilView(m_Device, m_DepthStencilTexture, DepthStencilViewDesc);

            // Make the DepthStencilView active.
            m_DeviceContext.OutputMerger.SetTargets(m_DepthStencilView, m_RenderTargetView);
        }

        /// <summary>
        /// Initializes the 2D system.
        /// </summary>
        private void Init2D()
        {
            m_System2D = new System2D(m_Device, FormObject);

            m_System2D.Init();

            HOD.UserInput    = UserInput;
            HOD.Font         = new Font("Microsoft Sans Serif", 16);

            Slider slide     = new Slider("slide1");

            slide.ForeColor  = Color.White;
            slide.BackColor  = Color.White;
            slide.Location   = new PointF(100, 100);
            slide.ClientSize = new SizeF(150, HOD.Font.Height);
            slide.TabIndex   = 1;
            slide.TabStop    = true;
            slide.Text       = "Slider";
            slide.Maximum    = 100.0f;
            slide.Minimum    = 0.0f;
            slide.Value      = 0.0f;
            //slide.AutoSize  = true;

            HOD.Add(slide);

            Scroller scroll     = new Scroller("scroll1", UI.ScrollOrientation.Vertical);

            scroll.ForeColor  = Color.White;
            scroll.BackColor  = Color.White;
            scroll.Location   = new PointF(100, 170);
            scroll.Size       = new SizeF(25, 150);
            scroll.TabIndex   = 2;
            scroll.TabStop    = true;
            scroll.Maximum    = 100.0f;
            scroll.Minimum    = 0.0f;
            scroll.Value      = 0.0f;
            scroll.ValueChanged += (c,o) => {
                state.Top = c.Value;
            };

            HOD.Add(scroll);

            Scroller scroll1 = new Scroller("scroll2", UI.ScrollOrientation.Horizontal);

            scroll1.ForeColor = Color.White;
            scroll1.BackColor = Color.White;
            scroll1.Location = new PointF(100 + 25, 170 + 150 - scroll.Width);
            scroll1.Size = new SizeF(150, 25);
            scroll1.TabIndex = 2;
            scroll1.TabStop = true;
            scroll1.Maximum = 100.0f;
            scroll1.Minimum = 0.0f;
            scroll1.Value = 0.0f;
            scroll1.Orientation = UI.ScrollOrientation.Horizontal;
            scroll1.ValueChanged += (c, o) => {
                state.Left = c.Value;
            };

            HOD.Add(scroll1);

            UI.Button btn  = new UI.Button("btn1");

            btn.ForeColor  = Color.White;
            btn.BackColor  = Color.Orange;
            btn.Location   = new PointF(100, 50);
            btn.Size       = new SizeF(100, HOD.Font.Height);
            btn.TabIndex   = 0;
            btn.TabStop    = true;
            btn.Text       = "Button";
            btn.TabIndex = 0;
            btn.HorizontalAllignment = TextHAllignment.Center;
            btn.VerticalAllignment = TextVAllignment.Center;

            HOD.Add(btn);

            state = new UI.Label("label1");
            state.Text = "";
            state.Width = FormObject.Width;

            HOD.Add(state);
       
            HOD.TakeOwnerShip(FormObject);
        }

        #endregion 

        #region Public Methods

        /// <summary>
        /// This method is called once per frame and is where we update objects in our scene.
        /// <param name="frameTime">The amount of time (in seconds) that has elapsed since the previous update.</param>
        /// </summary>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            if (IsDisposed) return;

            HOD.FramePerSecond = FramesPerSecond;

            m_System2D.Update(frameTime);
            UpdateFrame?.Invoke(frameTime);
            
            if(ShowFPS)
                state.Text = "FPS : " + FramesPerSecond;
        }

        /// <summary>
        /// This function is called each time we need to render the next frame.
        /// </summary>
        public override void Render()
        {
            if (!this.IsInitialized || this.IsDisposed)
                return;

            // Clear the screen before we draw the next frame.
            m_DeviceContext.ClearRenderTargetView(m_RenderTargetView, ClearColor);
            m_DeviceContext.ClearDepthStencilView(m_DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            RenderFrame?.Invoke(m_DeviceContext);

            m_System2D.Render();

            // Present the frame we just rendered to the user.
            m_SwapChain.Present(0, PresentFlags.None);

        }

        /// <summary>
        /// This function toggles fullscreen mode.  It does nothing here (except for updating the m_bFullscreen member variable) because thats all it needs to do in this base class.
        /// This function is overridden by derived classes that need to add code to toggle fullscreen mode.
        /// </summary>
        public override void ToggleFullscreen()
        {
            base.ToggleFullscreen();


            if (IsFullscreen)
            {
               
                if (FormObject.WindowState == FormWindowState.Normal)
                {
                    oldSize = FormObject.Size;
                }

                FormObject.Size = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
                m_SwapChain.IsFullScreen = this.IsFullscreen;
            }
            else
            {
                m_SwapChain.IsFullScreen = this.IsFullscreen;

                FormObject.Size = oldSize;
            }

            System.Diagnostics.Debug.WriteLine("Size : " + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size);
        }

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
                /*
                * The following text is from MSDN  (http://msdn.microsoft.com/en-us/library/fs2xkftw%28VS.80%29.aspx)
                * 
                * 
                * Dispose(bool disposing) executes in two distinct scenarios:
                * 
                * If disposing equals true, the method has been called directly or indirectly by a user's code and managed and unmanaged resources can be disposed.
                * If disposing equals false, the method has been called by the runtime from inside the finalizer and only unmanaged resources can be disposed. 
                * 
                * When an object is executing its finalization code, it should not reference other objects, because finalizers do not execute in any particular order. 
                * If an executing finalizer references another object that has already been finalized, the executing finalizer will fail.
                */
                if (disposing)
                {
                    // Unregister events
                    FormObject.Resize -= Resize;
                    // get rid of managed resources

                    DeviceDisposed?.Invoke();

                    m_SwapChain.IsFullScreen = false;
                    m_SwapChain?.Dispose();

                    m_RenderTargetView?.Dispose();

                    m_DepthStencilTexture?.Dispose();

                    m_DepthStencilView?.Dispose();

                    m_Device?.Dispose();
                    m_DeviceContext?.Dispose();
                    m_System2D?.Dispose();

                }

                // get rid of unmanaged resources

            }


            base.Dispose(disposing);

        }

        #endregion
    }
}