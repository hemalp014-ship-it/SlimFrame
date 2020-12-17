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

        private void UserInput_KeyboardEvent()
        {
            bool isPressedR;

            if (UserInput.IsKeyPressed(SlimDX.DirectInput.Key.LeftControl))
            {
                if (UserInput.IsKeyJustPressed(SlimDX.DirectInput.Key.O))
                {
                    isPressedR = UserInput.IsKeyPressed(SlimDX.DirectInput.Key.R);

                    UserInput.PauseInput();

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

                            UserInput.ResumeInput();
                            process = false;
                        })).Start();

                        //if (file == null)
                        //    FormObject.Invoke(new ChangeText(() => { FormObject.Text = "Null 00"; }));

                        new Thread(new ThreadStart(() =>
                        {
                            Loading.Visible = true;
                            Loading.Value = 0;

                            Loading.Width = HOD.Width;
                            Loading.HorizontalAllignment = TextHAllignment.Center;
                            Loading.Top = HOD.Height / 2 - Loading.Height / 2;

                            HOD.Enabled = false;

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
                            HOD.Enabled = true;
                            Loading.Visible = false;
                        })).Start();
                    }

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

        #endregion

        #region Event Handler

        private void Window_DeviceCreated(SlimDX.Direct3D11.Device device)
        {
            InitScene(device, device.ImmediateContext);

            Vsync = Vsync.One;
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
                Loading.Width = HOD.Width;
                Loading.Top = HOD.Height / 2 - Loading.Height / 2;
            }
        }

        private void Window_SwapChainReleasing()
        {
            
        }
        
        private void Window_HODLoaded()
        {
            HOD.Font = new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            HOD.HighlightSelectedControl = false;

            state = new DXLabel()
            {
                Location = new PointF(10, 10),
                Text = "FPS : -",
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(200, Color.White),
            };
            HOD.Add(state);

            trans = new DXSlider()
            {
                Location = new PointF(10, state.Bottom + 10),
                Text = "Transmissivity",
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(200, Color.White),
                ClientSize = new SizeF(200, HOD.Font.Height),
                Maximum = 100f,
                Value = 0.0f,
                Increment = 1.0f,
                Unit = "%",
            };
            trans.ValueChanged += trans_ValueChanged;
            HOD.Add(trans);

            refract = new DXSlider()
            {
                Location = new PointF(10, trans.Bottom + 10),
                Text = "Refractivity",
                ForeColor = Color.Black,
                ClientSize = new SizeF(200, HOD.Font.Height),
                BackColor = Color.FromArgb(200, Color.White),
                Maximum = 2.72f,
                Value = 1.33f,
                Minimum = 1.0f,
                Increment = 0.01f,
                Unit = "",
            };
            refract.ValueChanged += Refract_ValueChanged;
            HOD.Add(refract);

            reflect = new DXSlider()
            {
                Location = new PointF(10, refract.Bottom + 10),
                Text = "Reflectivity",
                ForeColor = Color.Black,
                ClientSize = new SizeF(200, HOD.Font.Height),
                BackColor = Color.FromArgb(200, Color.White),
                Maximum = 100.0f,
                Value = 100,
                Increment = 1.0f,
                Unit = "%",
            };
            reflect.ValueChanged += Reflect_ValueChanged;
            HOD.Add(reflect);

            Loading = new DXSlider()
            {
                Location = new PointF(0, HOD.Height / 2),
                Text = "Loading",
                ForeColor = Color.Black,
                ClientSize = new SizeF(200, HOD.Font.Height),
                BackColor = Color.FromArgb(200, Color.White),
                Maximum = 100.0f,
                Value = 0,
                Increment = 1.0f,
                Unit = "%",
                Visible = false,
                RoundingDigit = 5,
            };
            Loading.ValueChanged += Reflect_ValueChanged;
            HOD.Add(Loading);
        }

        private void Reflect_ValueChanged(DXSlider s, float o)
        {
            if (scene.model != null)
                foreach (Material mat in scene.model.Materials)
                    mat.Reflectivity = s.Value / 100;
        }

        private void Refract_ValueChanged(DXSlider s, float o)
        {
            foreach (Material mat in scene.model.Materials)
            {
                mat.Refractivity = s.Value;
            }
        }

        private void trans_ValueChanged(DXSlider s, float o)
        {
            foreach (Material mat in scene.model.Materials)
            {
                mat.Transparency = s.Value / 100;
            }
        }

        private void Window_Render(DeviceContext devCon)
        {            
            scene.Render(Camera.ViewMatrix, m_ProjectionMatrix, Camera);
        }

        private string FileName = "";

        private void Window_Update(double frameTime)
        {
            scene.Update(frameTime);
            state.Text = "FPS : " + HOD.FramePerSecond;

            if (UserInput.InputPaused)
                return;

        }

        delegate void ChangeText();
        delegate FileStream getStream();

        DXLabel state;
        DXSlider trans;
        DXSlider refract;
        DXSlider reflect;
        DXSlider Loading;

        #endregion
    }
}
