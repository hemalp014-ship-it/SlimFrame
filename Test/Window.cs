using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

using SlimFrame;
using SlimFrame.Buffers;
using SlimFrame.UI;
using SlimFrame.Cameras;
using System.Windows.Forms;
using SlimFrame.Models;
using System.Windows.Forms.VisualStyles;
using System.Threading;
using System.IO;
using SlimDX.Windows;

namespace Demo
{
    class Window : GameWindow3D
    {
        #region Member Variables

        Scene scene;

        float fov = (float)Math.PI * 0.25f;
        Matrix m_ProjectionMatrix;  // The projection matrix.

        #endregion

        #region Constructor

        public Window(string title, int width, int height, bool fullscreen) : base(title, width, height, fullscreen)
        {

            DeviceCreated += Window_DeviceCreated;
            DeviceDisposing += Window_DeviceDisposing;
            UpdateFrame += Window_Update;
            RenderFrame += Window_Render;
            SwapChainResized += Window_SwapChainResized;
            SwapChainReleasing += Window_SwapChainReleasing;
            HODLoaded += Window_HODLoaded;

            UserInput.KeyboardEvent += UserInput_KeyboardEvent;

            ClearColor = Color.Black;
        }

        #endregion

        #region Methods

        private void InitScene(SlimDX.Direct3D11.Device device, DeviceContext devCon)
        {
            // Create our projection matrix.
            m_ProjectionMatrix = Matrix.PerspectiveFovLH(fov, // this is 90 degrees in radians
                                                         (float)FormObject.Width / (float)FormObject.Height,
                                                         1f,
                                                         1000.0f);



            scene = new Scene(device, this);
            scene.InitScene();

            Camera = new ThirdPersonCamera(new Vector3(0, 0, 0), 19.0f, new Vector3(0, 1, 0));
        }

        private void UserInput_KeyboardEvent()
        {
            bool isPressedR;

            if (UserInput.IsKeyPressed(SlimDX.DirectInput.Key.LeftControl))
            {
                if (UserInput.IsKeyJustPressed(SlimDX.DirectInput.Key.O))
                {
                    isPressedR = UserInput.IsKeyPressed(SlimDX.DirectInput.Key.R);

                    UserInput.PauseInput();
                    bool fullscreen = IsFullscreen;
                    if (fullscreen)
                        ToggleFullscreen();

                    OpenFileDialog openFile = new OpenFileDialog();
                    openFile.Filter = "Supported Models|*.3ds;*.obj;*.model";
                    if (openFile.ShowDialog() == DialogResult.OK)
                    {
                        FileName = openFile.SafeFileName;
                        FileName = FileName.Substring(0, FileName.LastIndexOf("."));

                        scene.model.Dispose();
                        scene.model = null;

                        bool process = true;
                        FileStream file = null;


                        new Thread(new ThreadStart(() =>
                        {
                            process = true;
                            if (openFile.FileName.ToLower().EndsWith("3ds"))
                                scene.model = Manipulate3DS.Load(Device, openFile.FileName, isRHCoordSys: isPressedR, Debug: true);
                            else if (openFile.FileName.ToLower().EndsWith("obj"))
                                scene.model = ManipulateObj.Load(Device, openFile.FileName, isRHCoordSys: isPressedR, Debug: true);
                            else if (openFile.FileName.ToLower().EndsWith("model"))
                                scene.model = Model.FromFile(Device, openFile.FileName, Debug: true);
                            scene.model.Tranformation = Matrix.Identity;

                            process = false;
                        })).Start();

                        //if (file == null)
                        //    FormObject.Invoke(new ChangeText(() => { FormObject.Text = "Null 00"; }));

                        new Thread(new ThreadStart(() =>
                        {
                            Loading.Visible = true;
                            Loading.Value = 0;

                            Loading.Width = HUD.Width;
                            Loading.HorizontalAllignment = TextHAllignment.Center;
                            Loading.Top = HUD.Height / 2 - Loading.Height / 2;

                            HUD.Enabled = false;

                            while (process)
                            {
                                if (Model.file != null)
                                    file = Model.file;
                                else if (ManipulateObj.file != null)
                                    file = ManipulateObj.file;
                                else if (Manipulate3DS.file != null)
                                    file = Manipulate3DS.file;

                                if (file != null && file.CanRead)
                                    Loading.Value = (float)file.Position / file.Length * 100;

                                Thread.Sleep(10);
                            }
                            HUD.Enabled = true;
                            Loading.Visible = false;
                        })).Start();
                    }

                    UserInput.ResumeInput();
                    if (fullscreen)
                        ToggleFullscreen();
                }
                else if (UserInput.IsKeyJustPressed(SlimDX.DirectInput.Key.S))
                {
                    if (FileName != null)
                    {
                        isPressedR = UserInput.IsKeyPressed(SlimDX.DirectInput.Key.R);
                        scene.model.Save(DeviceContext, "..\\..\\..\\..\\My Models\\" + FileName + "\\" + FileName + ".model", ReplaceOldFiles: isPressedR);
                    }
                }
            }
        }

        #endregion

        #region Event Handler

        private void Window_DeviceCreated(SlimDX.Direct3D11.Device device)
        {
            InitScene(device, device.ImmediateContext);

            Vsync = Vsync.Immediate;
        }

        private void Window_DeviceDisposing()
        {
            scene?.Dispose();
        }

        private void Window_SwapChainResized(SlimDX.Direct3D11.Device device, SwapChain swapChain)
        {
            float Width = FormObject.ClientSize.Width;
            float Height = FormObject.ClientSize.Height;

            // Update the on rezise constant buffers.
            m_ProjectionMatrix = Matrix.PerspectiveFovLH(fov, Width / Height, 1f, 1000000.0f);

            if (Loading.Visible)
            {
                Loading.Width = HUD.Width;
                Loading.Top = HUD.Height / 2 - Loading.Height / 2;
            }
        }

        private void Window_SwapChainReleasing()
        {
            
        }
        
        private void Window_HODLoaded()
        {
            HUD.Font = new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            HUD.HighlightSelectedControl = false;

            state = new DXLabel()
            {
                Location = new PointF(10, 10),
                Text = "FPS : -",
                TextColor = Color.Black,
                BackColor = Color.FromArgb(200, Color.White),
            };
            HUD.Add(state);

            trans = new DXSlider()
            {
                Location = new PointF(10, state.Bottom + 10),
                Text = "Linear ",
                TextColor = Color.Black,
                BackColor = Color.FromArgb(200, Color.White),
                ClientSize = new SizeF(200, HUD.Font.Height),
                Type = DXSliderType.Linear,
                Maximum = 100f,
                Value = 0.0f,
                Unit = "",
            };
            HUD.Add(trans);

            combo = new DXComboBox()
            {
                Location = new PointF(10, trans.Bottom + 10),
                Text = "Slider Type",
                TextColor = Color.Black,
                BackColor = Color.FromArgb(200, Color.White),
                ClientSize = new SizeF(200, HUD.Font.Height + 10),
                AutoSize = true,
                HorizontalAllignment = TextHAllignment.Center,
            };
            combo.Items.AddRange(new string[]{"Linear","Exp e", "Exp 10" });
            HUD.Add(combo);

            Loading = new DXSlider()
            {
                Location = new PointF(0, HUD.Height / 2),
                Text = "Loading",
                TextColor = Color.Black,
                ClientSize = new SizeF(200, HUD.Font.Height),
                BackColor = Color.FromArgb(200, Color.White),
                Maximum = 100.0f,
                Value = 0,
                Unit = "%",
                Visible = false,
                RoundingDigit = 5,
            };
            HUD.Add(Loading);
        }


        private void Window_Render(DeviceContext devCon)
        {            
            scene.Render(Camera.ViewMatrix, m_ProjectionMatrix, Camera);
        }

        private string FileName = "";

        private void Window_Update(double frameTime)
        {
            scene.Update(frameTime);
            state.Text = "FPS : " + HUD.FramePerSecond;

            Camera.Yaw += 0.0001f;

            if (UserInput.InputPaused)
                return;

        }

        delegate void ChangeText();
        delegate FileStream getStream();

        DXLabel state;
        DXSlider trans;
        DXComboBox combo;

        DXSlider Loading;

        #endregion
    }
}
