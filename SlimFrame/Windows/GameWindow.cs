using System;
using System.Windows.Forms;
using System.Diagnostics;
using SlimDX;
using SlimDX.Windows;
using SlimDX.DirectInput;

using SlimFrame.Cameras;
using SlimDX.D3DCompiler;

namespace SlimFrame
{
    /// <summary>
    /// The GameWindow class
    /// </summary>
    public class GameWindow : IDisposable
    {

        #region Member Variables

        protected bool m_IsInitialized = false;  // Indicates whether or not the game window has been initialized yet.
        private Color4 m_ClearColor;     // The color to use when clearing the screen.

        // These are protected in case a subclass wants to override the GameLoop() method to create a different type
        // of game loop.
        protected long m_CurrFrameTime;  // Stores the time for the current frame.
        protected long m_LastFrameTime;  // Stores the time of the last frame.
        private double m_ElapsedTime;    // Stores the elapsed time from start of the game.

        private int m_FrameCount;        // Stores the number of frames completed so far during last Frame_Count_Timimg second.
        private readonly double m_FrameCountTimimg = 1;  // Stores the Frame_Count_Timimg seconds for which frames should be counted.
        protected int m_FPS = 0;           // Stores the number of frames we rendered during the previous second.

        protected UserInput m_UserInput; // Stores the UserInput class to handle player Input
        private Camera cam;         // Stores the Camera class to handle the camera;

        protected bool GameRun = true;

        #endregion

        #region Constructors

        /// <summary>
        /// This is the constructor.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="fullscreen">Whether the window should be fullscreen or not.</param>
        public GameWindow(string title, int width, int height, bool fullscreen)
        {
            // Store parameters in member variables.
            IsFullscreen = fullscreen;
            m_ClearColor = new Color4(1.0f, 0.0f, 0.0f, 0.0f);


            // Create the game window that will display the game.
            FormObject = new RenderForm(title)
            {
                ClientSize = new System.Drawing.Size(width, height)
            };

            // Hook up event handlers so we can recieve events from the RenderForm Object.
            FormObject.FormClosed += FormClosed;
            FormObject.GotFocus += GotFocus;
            FormObject.LostFocus += LostFocus;

            // Initializes the UserInput variable
            m_UserInput = new UserInput();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the color to clear the screen to before drawing the next frame.
        /// </summary>
        public Color4 ClearColor
        {
            get
            {
                return m_ClearColor;
            }
            set
            {
                m_ClearColor = value;
            }
        }

        /// <summary>
        /// Camera of the 3D rendering System
        /// </summary>
        public Camera Camera
        {
            get
            {
                return cam;
            }
            set
            {
                cam = value;
                cam.Update();
            }
        }

        /// <summary>
        /// Input from User for the System
        /// </summary>
        public UserInput UserInput
        {
            get
            {
                return m_UserInput;
            }
            protected set
            {
                m_UserInput = value;
            }
        }

        /// <summary>
        /// Time in ticks when we started processing the current frame.
        /// </summary>
        public long CurrentFrameTime
        {
            get
            {
                return m_CurrFrameTime;
            }
            protected set
            {
                m_CurrFrameTime = value;
            }
        }

        /// <summary>
        /// Underlying RenderForm Object that actually represents the window itself.
        /// </summary>
        public RenderForm FormObject { get; protected set; }

        /// <summary>
        /// The number of frames we are rendering per second.
        /// </summary>
        public int FramesPerSecond
        {
            get
            {
                return m_FPS;
            }
            protected set
            {
                m_FPS = value;
            }
        }

        /// <summary>
        /// Indicates whether or not system is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Gets or Sets whether the game window is fullscreen or not.
        /// </summary>
        public bool IsFullscreen { get; protected set; } = false;

        /// <summary>
        /// Indicates whether the GameWindow has yet finished initializing or not.
        /// </summary>
        virtual public bool IsInitialized
        {
            get
            {
                return m_IsInitialized;
            }
        }

        /// <summary>
        /// Indicates whether or not the game is paused.
        /// </summary>
        public bool IsPaused { get; protected set; } = false;

        /// <summary>
        /// Time in ticks when the we started processing the previous frame.
        /// </summary>
        public long LastFrameTime
        {
            get
            {
                return m_LastFrameTime;
            }
            protected set
            {
                m_LastFrameTime = value;
            }
        }

        /// <summary>
        /// Determines if the FormObject is focued or not
        /// </summary>
        public bool Focused { get; set; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// The Loop of the game.
        /// </summary>
        public virtual void GameLoop()
        {
            double time;

            if (m_FrameCount == 0) // checks if this is the first frame of the System to be counted or not
            {
                if (m_ElapsedTime == 0.0)   // checks is this the first frame of the system
                    m_LastFrameTime = Stopwatch.GetTimestamp(); // Stores current time as ticks.
                else
                    m_ElapsedTime = 0.0;    // Resets the elapsed time to zero so we can recount frames
            }
            else
                m_LastFrameTime = m_CurrFrameTime;  // Swaps the current frame time of last frame to last frame time

            m_CurrFrameTime = Stopwatch.GetTimestamp();  // Stores current time as ticks.

            time = ((double)(m_CurrFrameTime - m_LastFrameTime)) / Stopwatch.Frequency; // Converts ticks to time

            if (m_ElapsedTime < m_FrameCountTimimg) // Counts the frame if elapsed time is less than Frame_Count_Timimg.
            {
                m_ElapsedTime += time;
                m_FrameCount++;
            }
            else
            {
                m_FPS = (int)(m_FrameCount / m_ElapsedTime); // Calculates FPS using frame count and elapsed time

                m_FrameCount = 0;   // Resets the frame count
            }

            Update(time);
            Render();
        }

        /// <summary>
        /// Starts the game Loop.
        /// </summary>
        public virtual void StartGameLoop()
        {
            // if initialization is already finished, then simply return.
            if (m_IsInitialized)
                return;
            m_IsInitialized = true;

            //start the message pump
            MessagePump.Run(FormObject, GameLoop);
        }

        /// <summary>
        /// Updates the game scene.
        /// </summary>
        /// <param name="frameTime"></param>
        public virtual void Update(double frameTime)
        {
            if (m_UserInput.InputPaused)
                return;
            // Get the latest user input.
            m_UserInput.Update();

            if (m_UserInput.InputPaused)
                return;

            // Toggle fullscreen mode.  
            if (m_UserInput.IsKeyJustPressed(Key.Return) && (m_UserInput.IsKeyPressed(Key.LeftAlt) || m_UserInput.IsKeyPressed(Key.RightAlt)))
                ToggleFullscreen();
            else if (m_UserInput.IsKeyJustPressed(Key.Escape))   // Exit fullscreen if in fullscreen.    
            {
                if (IsFullscreen)
                    ToggleFullscreen();
            }

            if (IsPaused)
                return;

            if (cam != null)
                UpdateCamera(frameTime); // Updates camera if it exists
        }

        /// <summary>
        /// Updates the camera variables.
        /// </summary>
        /// <param name="frameTime"></param>
        protected virtual void UpdateCamera(double frameTime)
        {
        }

        /// <summary>
        /// Renders the game scene.
        /// </summary>
        public virtual void Render()
        {

        }

        /// <summary>
        /// This function toggles fullscreen mode.  It does nothing here (except for updating the m_bFullscreen member variable) because thats all it needs to do in this base class.
        /// This function is overridden by derived classes that need to add code to toggle fullscreen mode.
        /// </summary>
        public virtual void ToggleFullscreen()
        {
            IsFullscreen = !IsFullscreen;
        }

        /// <summary>
        /// Pauses the System
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
        }

        /// <summary>
        /// Resumes the System
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
        }

        #endregion

        #region Event Handler

        private void FormClosed( Object sender, FormClosedEventArgs e)
        {
            if (!IsDisposed)
                Dispose();
        }

        /// <summary>
        /// Handles the focus lost event of window
        /// </summary>
        /// <param name="sender">Object3D who invoked the event</param>
        /// <param name="e"></param>
        private void LostFocus( Object sender, EventArgs e)
        {
            Focused = false; 
            FocusLost();           
        }

        protected virtual void FocusLost()
        {

        }
        protected virtual void FocusGiven()
        {

        }

        /// <summary>
        /// Handles the focus retained event of window.
        /// </summary>
        /// <param name="sender">Object3D who invoked the event</param>
        /// <param name="e"></param>
        private void GotFocus( Object sender, EventArgs e)
        {
            Focused = true;
            FocusGiven();
        }

        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Unregister events
                    FormObject.FormClosed -= this.FormClosed;

                    m_UserInput.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GameWindow()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        public void Dispose()
        {
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}