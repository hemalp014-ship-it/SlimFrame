using System;
using System.Collections.Generic;
using System.Linq;
using SlimDX.DirectInput;
using SlimDX.XInput;

using Debug = System.Diagnostics.Debug;

namespace SlimFrame
{

    #region Enumrations

    public enum MouseButtons
    {
        Left = 0,
        Right = 1,
        Center = 2,
    }

    #endregion

    /// <summary>
    /// UserInput class handles the 
    /// </summary>
    public class UserInput : IDisposable
    {

        #region Member Variable

        DirectInput m_DirectInput;

        Keyboard m_Keyboard;
        KeyboardState m_KeyboardStateCurrent;
        KeyboardState m_KeyboardStateLast;

        Mouse m_Mouse;
        MouseState m_MouseStateCurrent;
        MouseState m_MouseStateLast;

        // DirectInput joystick variables
        Joystick m_Joystick1;
        JoystickState m_Joy1StateCurrent;
        JoystickState m_Joy1StateLast;

        private bool m_JoystickFound = false;


        // XInput joystick variables
        Controller m_Controller1;
        Gamepad m_Controller1StateCurrent;
        Gamepad m_Controller1StateLast;

        #endregion

        #region Constructor

        public UserInput()
        {
            InitDirectInput();
            InitXInput();

            // We need to intiailize these because otherwise we will get a null reference error
            // if the program tries to access these on the first frame.
            m_KeyboardStateCurrent = new KeyboardState();
            m_KeyboardStateLast = new KeyboardState();

            m_MouseStateCurrent = new MouseState();
            m_MouseStateLast = new MouseState();

            m_Joy1StateCurrent = new JoystickState();
            m_Joy1StateLast = new JoystickState();

            m_Controller1StateCurrent = new Gamepad();
            m_Controller1StateLast = new Gamepad();
        }

        #endregion 

        #region Properties

        /// <summary>
        /// Determines whether the Input Capture is paused or not
        /// </summary> 
        public bool InputPaused { get; protected set; }

        /// <summary>
        /// Determines whether or not this Object has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return m_IsDisposed;
            }
        }

        /// <summary>
        /// Gets the keyboard Object.
        /// </summary>
        public Keyboard Keyboard
        {
            get
            {
                return m_Keyboard;
            }
        }

        /// <summary>
        /// Gets the keyboard state for the current frame.
        /// </summary>
        public KeyboardState KeyboardState_Current
        {
            get
            {
                return m_KeyboardStateCurrent;
            }
        }

        /// <summary>
        /// Gets the keyboard state from the previous frame.
        /// </summary>
        public KeyboardState KeyboardState_Previous
        {
            get
            {
                return m_KeyboardStateLast;
            }
        }

        /// <summary>
        /// Gets the DirectInput joystick Object.
        /// </summary>
        public Joystick Joystick1
        {
            get
            {
                return m_Joystick1;
            }
        }

        /// <summary>
        /// Gets the Joystick1 state for the current frame.
        /// </summary>
        public JoystickState Joy1State_Current
        {
            get
            {
                return m_Joy1StateCurrent;
            }
        }

        /// <summary>
        /// Gets the Joystick1 state from the previous frame.
        /// </summary>
        public JoystickState Joy1State_Last
        {
            get
            {
                return m_Joy1StateLast;
            }
        }

        /// <summary>
        ///  Gets the mouse Object.
        /// </summary>
        public Mouse Mouse
        {
            get
            {
                return m_Mouse;
            }
        }

        /// <summary>
        /// Gets the mouse state for the current frame.
        /// </summary>
        public MouseState MouseState_Current
        {
            get
            {
                return m_MouseStateCurrent;
            }
        }

        /// <summary>
        /// Gets the mouse state from the previous frame.
        /// </summary>
        public MouseState MouseState_Previous
        {
            get
            {
                return m_MouseStateLast;
            }
        }

        #endregion

        #region Events

        public event Action KeyboardEvent;
        public event Action MouseEvent;

        #endregion

        #region Methods

        private void CheckInputPaused()
        {
            if(InputPaused)
                throw new InvalidOperationException("UserInput is accessed in Pause state!");
        }

        /// <summary>
        /// Pauses Input Capture
        /// </summary>
        public void PauseInput()
        {
            InputPaused = true;
        }

        /// <summary>
        /// Pauses Input Capture
        /// </summary>
        public void ResumeInput()
        { 
            InputPaused = false;
        }

        /// <summary>
        /// This method initializes DirectInput.
        /// </summary>
        private void InitDirectInput()
        {
            m_DirectInput = new DirectInput();

            //Create our Keyboard and Mouse devices.
            m_Keyboard = new Keyboard(m_DirectInput);
            m_Mouse = new Mouse(m_DirectInput);

            GetJoysticks();
        }

        /// <summary>
        /// This method initializes our XInput stuff.
        /// </summary>
        private void InitXInput()
        {
            m_Controller1 = new Controller(UserIndex.One);
        }

        /// <summary>
        /// Compares the keys of new and old keyboard state
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="oldState"></param>
        /// <returns>true if old and new keys are same, false </returns>
        private bool CompareKeys(KeyboardState newState, KeyboardState oldState)
        {
            if (newState.PressedKeys.Count != oldState.PressedKeys.Count)
                return false;

            if (newState.PressedKeys.SequenceEqual(oldState.PressedKeys))
                return false;

            return true;
        }
        /// <summary>
        /// Compares the keys of new and old keyboard state
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="oldState"></param>
        /// <returns>true if old and new keys are same, false </returns>
        private bool CompareMouse(MouseState newState, MouseState oldState)
        {
            if (!(newState.X == oldState.X && newState.Y == oldState.Y && newState.Z == oldState.Z))
                return false;

            if (newState.GetButtons().SequenceEqual(oldState.GetButtons()))
                return false;

            return true;
        }

        /// <summary>
        /// This function updates the state variables.  It should be called from the game's UpdateScene() function before
        /// it does any input processing.  
        /// </summary>
        public void Update()
        {
            //if (InputPaused)
            //    return;
            // Reacquire the devices in case another application has taken control of them and check for errors.
            if (m_Joystick1 == null)
            {
                if (m_Keyboard.Acquire().IsFailure ||
                    m_Mouse.Acquire().IsFailure)
                {
                    // We failed to successfully acquire one of the devices so abort updating the user input stuff by simply returning from this method.
                    return;
                }
            }
            else
            {
                if (m_Keyboard.Acquire().IsFailure ||
                    m_Mouse.Acquire().IsFailure ||
                    m_Joystick1.Acquire().IsFailure)
                {
                    // We failed to successfully acquire one of the devices so abort updating the user input stuff by simply returning from this method.
                    return;
                }
            }

            // Update our keyboard state variables.
            m_KeyboardStateLast = m_KeyboardStateCurrent;
            m_KeyboardStateCurrent = m_Keyboard.GetCurrentState();

            if(!CompareKeys(m_KeyboardStateCurrent, m_KeyboardStateLast))
                KeyboardEvent?.Invoke();

            if(!CompareMouse(m_MouseStateCurrent, m_MouseStateLast))
                MouseEvent?.Invoke();

            // NOTE: All of the if statements below are for testing purposes.  In a real program, you would remove them or comment them out
            //       and then recompile before releasing your game.  This is because we don't want debug code slowing down the finished game


            // Update our mouse state variables.
            m_MouseStateLast = m_MouseStateCurrent;
            m_MouseStateCurrent = m_Mouse.GetCurrentState();


            if (m_JoystickFound)
            {
                // Update our DirectInput joystick state variables.
                m_Joy1StateLast = m_Joy1StateCurrent;
                m_Joy1StateCurrent = m_Joystick1.GetCurrentState();

            }


            if (m_Controller1 != null && m_Controller1.IsConnected)
            {
                // Update our XInput controller state variables.
                m_Controller1StateLast = m_Controller1StateCurrent;
                m_Controller1StateCurrent = m_Controller1.GetState().Gamepad;


            }
        }

        /// <summary>
        /// In the first version of this method, it gets the list of available game controllers and outputs it into Visual Studio's Output Pane.
        /// The new version of this method gets the first joystick from the list of available game controllers (if there are any) and prepares it for use.
        /// </summary>
        public void GetJoysticks()
        {
            IList<DeviceInstance> deviceList = m_DirectInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);

            m_JoystickFound = deviceList.Count < 1 ? false : true;

            if (!m_JoystickFound)
                return;

            for (int i = 0; i < deviceList.Count; i++)
            {
                if (i == 0)
                {
                    m_Joystick1 = new Joystick(m_DirectInput, deviceList[0].InstanceGuid);
                    if (m_Joystick1 == null)
                        return;  // An error has occurred, initialization of the joystick failed for some reason so simply return from this method.


                    // Set the range to use for all of the axis on our game controller.
                    m_Joystick1.Properties.SetRange(-1000, 1000);
                }
            }
        }

        #endregion

        #region Keyboard Methods

        /// <summary>
        /// This method checks if the specified key is pressed.
        /// </summary>
        /// <param name="key">The key to check the state of.</param>
        /// <returns>True if the key is pressed or false otherwise.</returns>
        public bool IsKeyPressed(Key key)
        {
            CheckInputPaused();
            return m_KeyboardStateCurrent.IsPressed(key);
        }

        /// <summary>
        /// This method checks if the specified key was pressed during the previous frame.
        /// </summary>
        /// <param name="key">The key to check the state of.</param>
        /// <returns>True if the key was pressed during the previous frame, or false otherwise.</returns>
        public bool WasKeyPressed(Key key)
        {
            CheckInputPaused();
            return m_KeyboardStateLast.IsPressed(key);
        }

        /// <summary>
        /// This method checks if the specified key is released.
        /// </summary>
        /// <param name="key">The key to check the state of.</param>
        /// <returns>True if the key is released or false otherwise.</returns>
        public bool IsKeyReleased(Key key)
        {
            CheckInputPaused();
            return m_KeyboardStateCurrent.IsReleased(key);
        }

        /// <summary>
        /// This method checks if the specified key was released (not pressed) during the previous frame.
        /// </summary>
        /// <param name="key">The key to check the state of.</param>
        /// <returns>True if the key was not pressed during the previous frame, or false otherwise.</returns>
        public bool WasKeyReleased(Key key)
        {
            CheckInputPaused();
            return m_KeyboardStateLast.IsReleased(key);
        }

        /// <summary>
        /// This method checks if the specified key is held down (meaning it has been held down for 2 or more consecutive frames).
        /// </summary>
        /// <param name="key">The key to check the state of.</param>
        /// <returns>True if the key is being held down or false otherwise.</returns>
        public bool IsKeyHeldDown(Key key)
        {
            CheckInputPaused();
            return (m_KeyboardStateCurrent.IsPressed(key) && m_KeyboardStateLast.IsPressed(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key to check the state of.</param>
        /// <returns>True if the key is pressed at this frame or false otherwise.</returns>
        public bool IsKeyJustPressed(Key key)
        {
            CheckInputPaused();
            return (m_KeyboardStateCurrent.IsPressed(key) && !m_KeyboardStateLast.IsPressed(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key to check the state of.</param>
        /// <returns>True if the key is released at this frame or false otherwise.</returns>
        public bool IsKeyJustReleased(Key key)
        {
            CheckInputPaused();
            return (m_KeyboardStateCurrent.IsReleased(key) && !m_KeyboardStateLast.IsReleased(key));
        }

        #endregion

        #region Mouse Methods


        /// <summary>
        /// This method checks if the specified mouse button is pressed.
        /// </summary>
        /// <param name="button">The button to check the state of. 0 = left button, 1 = right button, 2 = middle button</param>
        /// <returns>True if the button is pressed or false otherwise.</returns>
        public bool IsMouseButtonPressed(MouseButtons button)
        {
            CheckInputPaused();
            return m_MouseStateCurrent.IsPressed((int)button);
        }

        /// <summary>
        /// This method checks if the specified mouse button was pressed during the previous frame.
        /// </summary>
        /// <param name="button">The button to check the state of. 0 = left button, 1 = right button, 2 = middle button</param>
        /// <returns>True if the button was pressed during the previous frame or false otherwise.</returns>
        public bool WasMouseButtonPressed(MouseButtons button)
        {
            CheckInputPaused();
            return m_MouseStateLast.IsPressed((int)button);
        }

        /// <summary>
        /// This method checks if the specified mouse button is pressed.
        /// </summary>
        /// <param name="button">The button to check the state of. 0 = left button, 1 = right button, 2 = middle button.</param>
        /// <returns>True if the button is released or false otherwise.</returns>
        public bool IsMouseButtonReleased(int button)
        {
            CheckInputPaused();
            return m_MouseStateCurrent.IsReleased(button);
        }

        /// <summary>
        /// This method checks if the specified mouse button was released (not pressed) during the previous frame.
        /// </summary>
        /// <param name="button">The button to check the state of. 0 = left button, 1 = right button, 2 = middle button</param>
        /// <returns>True if the button was released (not pressed) during the previous frame or false otherwise.</returns>
        public bool WasMouseButtonReleased(int button)
        {
            CheckInputPaused();
            return m_MouseStateLast.IsReleased(button);
        }

        /// <summary>
        /// This method checks if the specified mouse button is being held down (meaning it has been held down for 2 or more consecutive frames).
        /// </summary>
        /// <param name="button">The button to check the state of. 0 = left button, 1 = right button, 2 = middle button</param>
        /// <returns>True if the button is held down or false otherwise.</returns>
        public bool IsMouseButtonHeldDown(int button)
        {
            CheckInputPaused();
            return (m_MouseStateCurrent.IsPressed(button) && m_MouseStateLast.IsPressed(button));
        }

        /// <summary>
        /// This method checks if the mouse has moved since the previous frame.
        /// </summary>
        /// <returns>True if the mouse has moved since the previous frame, or false otherwise.</returns>
        public bool MouseHasMoved()
        {
            CheckInputPaused();

            if ((m_MouseStateCurrent.X != m_MouseStateLast.X) ||
                (m_MouseStateCurrent.Y != m_MouseStateLast.Y))
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// This method gets the mouse position for the current frame.
        /// </summary>
        /// <returns>A System.Drawing.Point Object containing the current mouse position.</returns>
        public System.Drawing.Point MousePosition()
        {
            CheckInputPaused();
            return new System.Drawing.Point(m_MouseStateCurrent.X, m_MouseStateCurrent.Y);
        }

        /// <summary>
        /// This method gets the mouse position for the previous frame.
        /// </summary>
        /// <returns>A System.Drawing.Point Object containing the mouse's position during the previous frame.</returns>
        public System.Drawing.Point LastMousePosition()
        {
            CheckInputPaused();
            return new System.Drawing.Point(m_MouseStateLast.X, m_MouseStateLast.Y);
        }

        /// <summary>
        /// This method gets the scrollwheel value in most cases.
        /// Note that this value is a delta, or in other words it is the amount the scroll wheel has been moved
        /// since the last frame.
        /// </summary>
        /// <returns>The amount the scroll wheel has moved.  This can be positive or negative depending on which way it has moved.</returns>
        public int MouseWheelMovement()
        {
            CheckInputPaused();
            int offset = 10;
            int newOffset = 0;

            if (m_MouseStateCurrent.Z > offset)
            {
                newOffset = m_MouseStateLast.Z;
            }
            else if (m_MouseStateCurrent.Z < -offset)
            {
                newOffset = m_MouseStateLast.Z;
            }

            return m_MouseStateCurrent.Z - newOffset;
        }


        #endregion

        #region DirectInput Gampad Methods


        /// <summary>
        /// This method checks if the specified button on the game controller is pressed.
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button is pressed or false otherwise.</returns>
        public bool DI_IsButtonPressed(int button)
        {
            CheckInputPaused();
            return m_Joy1StateCurrent.IsPressed(button);
        }


        /// <summary>
        /// This method checks if the specified button was pressed during the previous frame.
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button was pressed during the previous frame or false otherwise.</returns>
        public bool DI_WasButtonPressed(int button)
        {
            CheckInputPaused();
            return m_Joy1StateLast.IsPressed(button);
        }


        /// <summary>
        /// This method checks if the specified button is released (not pressed).
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button is released (not pressed) or false otherwise.</returns>
        public bool DI_IsButtonReleased(int button)
        {
            CheckInputPaused();
            return m_Joy1StateCurrent.IsReleased(button);
        }


        /// <summary>
        /// This method checks if the specified button was released (not pressed) during the previous frame.
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button was released (not pressed) during the previous frame.</returns>
        public bool DI_WasButtonReleased(int button)
        {
            CheckInputPaused();
            return m_Joy1StateLast.IsReleased(button);
        }


        /// <summary>
        /// This method gets the position of the left joystick for most gamepad style game controllers.
        /// </summary>
        /// <returns>A System.Drawing.Point Object containing the current position of the left stick in most cases.</returns>
        public System.Drawing.Point DI_LeftStickPosition()
        {
            CheckInputPaused();
            return new System.Drawing.Point(m_Joy1StateCurrent.X, m_Joy1StateCurrent.Y);
        }


        /// <summary>
        /// This method gets the position of the right joystick for most gamepad style game controllers.
        /// </summary>
        /// <returns>A System.Drawing.Point Object containing the current position of the right stick in most cases.</returns>
        public System.Drawing.Point DI_RightStickPosition()
        {
            CheckInputPaused();
            return new System.Drawing.Point(m_Joy1StateCurrent.RotationX, m_Joy1StateCurrent.RotationY);
        }

        /// <summary>
        /// This method gets the current value of the axis that is used under DirectInput to represent the trigger keys if your
        /// gamepad style controller has them.
        /// </summary>
        /// <returns></returns>
        public int DI_TriggersAxis()
        {
            CheckInputPaused();
            return m_Joy1StateCurrent.Z;
        }


        #endregion

        #region XInput Gamepad Methods


        /// <summary>
        /// This method checks if the specified button is pressed.
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button is pressed or false otherwise.</returns>
        public bool XI_IsButtonPressed(GamepadButtonFlags button)
        {
            CheckInputPaused();
            return m_Controller1StateCurrent.Buttons.HasFlag(button);
        }

        /// <summary>
        /// This method checks if the specified button was pressed during the previous frame.
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button was pressed during the previous frame or false otherwise.</returns>
        public bool XI_WasButtonPressed(GamepadButtonFlags button)
        {
            CheckInputPaused();
            return m_Controller1StateLast.Buttons.HasFlag(button);
        }

        /// <summary>
        /// This method checks if the specified button is released (not pressed).
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button is released (not pressed) or false otherwise.</returns>
        public bool XI_IsButtonReleased(GamepadButtonFlags button)
        {
            CheckInputPaused();
            return !(m_Controller1StateCurrent.Buttons.HasFlag(button));
        }

        /// <summary>
        /// This method checks if the specified button was released (not pressed) during the previous frame.
        /// </summary>
        /// <param name="button">The button to check the state of.</param>
        /// <returns>True if the button was released (not pressed) during the previous frame or false otherwise.</returns>
        public bool XI_WasButtonReleased(GamepadButtonFlags button)
        {
            CheckInputPaused();
            return !(m_Controller1StateLast.Buttons.HasFlag(button));
        }

        /// <summary>
        /// This method gets the current position for the left stick.
        /// </summary>
        /// <returns>A System.Drawing.Point Object containing the position of the left stick.</returns>
        public System.Drawing.Point XI_LeftStickPosition()
        {
            CheckInputPaused();
            return new System.Drawing.Point(m_Controller1StateCurrent.LeftThumbX, m_Controller1StateCurrent.LeftThumbY);
        }

        /// <summary>
        /// This method gets the current position for the right stick.
        /// </summary>
        /// <returns>A System.Drawing.Point Object containing the position of the right stick.</returns>
        public System.Drawing.Point XI_RightStickPosition()
        {
            CheckInputPaused();
            return new System.Drawing.Point(m_Controller1StateCurrent.RightThumbX, m_Controller1StateCurrent.RightThumbY);
        }

        /// <summary>
        /// This method gets the value of the left trigger.
        /// </summary>
        /// <returns>The value of the left trigger.  It ranges from 0-255.</returns>
        public int XI_LeftTrigger()
        {
            CheckInputPaused();
            return m_Controller1StateCurrent.LeftTrigger;
        }


        /// <summary>
        /// This method gets the value of the right trigger.
        /// </summary>
        /// <returns>The value of the right trigger.  It ranges from 0-255.</returns>
        public int XI_RightTrigger()
        {
            CheckInputPaused();
            return m_Controller1StateCurrent.RightTrigger;
        }


        #endregion

        #region Disposal

        bool m_IsDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                if (disposing)
                {
                    // Unregister events


                    // get rid of managed resources
                    m_DirectInput?.Dispose();
                    m_Keyboard?.Dispose();
                    m_Mouse?.Dispose();
                    m_Joystick1?.Dispose();
                }

                // get rid of unmanaged resources here

                m_IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}