using System;
using System.Windows.Forms;

using SlimDX;
using DW = SlimDX.DirectWrite;
using D2D = SlimDX.Direct2D;

using System.Drawing;
using SlimDX.Direct2D;
using SlimDX.DirectInput;
using System.Diagnostics;

namespace SlimFrame.UI
{
    public enum ScrollOrientation
    {
        NotInitiated,   // Scroller not initiated
        Horizontal,     // Horizontal scroller
        Vertical        // Vertical scroller
    }

    /// <summary>
    /// Scroller : Visually reprensentes / changes display area of control Horizontally or Vertically.
    /// </summary>
    public class DXScroller : DXControl
    {

        #region Member Variables

        private float m_Value;              // The value of control

        private float m_Minimum;            // The minimum value of control
        private float m_Maximum;            // The maximum value of control

        private float m_Width = 50.0f;    // The width of pointer    
        private float m_Height = 15.0f;    // The height of pointer

        private float m_Spacing = 15;
        private float m_bSpacing = 19;
        private float m_pSpacing = 10;
        //private float m_SpacingBase = 20.0f / 200;

        private float m_Increment = 0.25f;            // The increment in value

        private bool m_useStaticPoniter = true;                 // Determines whether static mouse pointer should be used or not
        private bool m_TrackPoniter = false;                // Determines whether moving mouse pointer should be tracked for changing the value

        //private RoundedRectangle rectRoundBack; // scroller background rounded rectangle
        private RectangleF rectRound;     // Pointer rounded rectangle

        private ScrollOrientation m_Orientation = ScrollOrientation.Horizontal;

        float apperantDimension;
        float valueDimension;


        #endregion

        #region Properties

        public ScrollOrientation Orientation
        {
            get
            {
                return m_Orientation;
            }
            set
            {
                if (m_Orientation == value) return;

                m_Orientation = value;
                OrientationChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Value of scroller
        /// </summary>
        public float Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (!Enabled) return;

                var temp = Math.Min(Math.Max(value, m_Minimum), m_Maximum);

                if (m_Value == temp) return;

                var old = m_Value;
                m_Value = temp;
                ValueChanged?.Invoke(this, old);
            }
        }
        /// <summary>
        /// Maximum value of scroller
        /// </summary>
        public float Maximum
        {
            get
            {
                return m_Maximum;
            }
            set
            {
                if (m_Maximum == value) return;

                m_Maximum = value;
                MaximumChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Increment in value of scroller
        /// </summary>
        public float Increment
        {
            get
            {
                return m_Increment;
            }
            set
            {
                if (m_Increment == value) return;

                m_Increment = value;
                IncrementChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Minimum value of scroller
        /// </summary>
        public float Minimum
        {
            get
            {
                return m_Minimum;
            }
            set
            {
                if (m_Minimum == value) return;

                m_Minimum = value;
                MinimumChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Size of client area of scroller
        /// </summary>
        public SizeF ClientSize
        {
            get
            {
                return m_Size;
            }
            set
            {
                if (m_Size == value) return;

                Size = value;
            }
        }

        #endregion

        #region Event System

        /// <summary>
        /// Occures when the value of Slider is changed
        /// </summary>
        public event Action<DXScroller, float> ValueChanged;

        /// <summary>
        /// Occures when the value of Slider is changed
        /// </summary>
        public event Action<DXScroller> MaximumChanged;

        /// <summary>
        /// Occures when the value of Slider is changed
        /// </summary>
        public event Action<DXScroller> MinimumChanged;

        /// <summary>
        /// Occures when the value of Slider is changed
        /// </summary>
        public event Action<DXScroller> IncrementChanged;

        /// <summary>
        /// Occures when the value of Slider is changed
        /// </summary>
        public event Action<DXScroller> OrientationChanged;

        #endregion

        #region Constructors

        public DXScroller()
            : base()
        {
            Init();
            m_Orientation = ScrollOrientation.Vertical;
        }

        public DXScroller(ScrollOrientation orientation)
            : base()
        {
            Init();
            m_Orientation = orientation;
        }
        public DXScroller(string name, ScrollOrientation orientation)
            : base()
        {
            Name = name;

            Init();
            m_Orientation = orientation;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the Scroller
        /// </summary>
        private void Init()
        {
            m_BorderWidth = 1.0f;

            m_BackEnabled = false;
            m_BorderEnabled = false;
            TabStop = true;

            rectRound = new RectangleF();
            //rectRoundBack = new RoundedRectangle();

            //rectRoundBack.RadiusX = m_Height / 2 * (1 - reduceFactor);
            //rectRoundBack.RadiusY = m_Height / 2 * (1 - reduceFactor);


            MouseDown += focus_Control;

            MouseMove += Scroller_MouseMove;
            MouseUp += Scroller_MouseUp;
            OrientationChanged += Changed;
            MaximumChanged += Changed;
            MinimumChanged += Changed;
            IncrementChanged += Changed;
            EnabledChanged += Scroller_EnabledChanged;

            SizeChanged += Scroller_SizeChanged;
        }

        /// <summary>
        /// Initializes the 2D rendering capability
        /// </summary>
        /// <param name="factory">factory for creating DirectWrite Objects</param>
        /// <param name="factory2D">factory for creating Direct2D Objects</param>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Init(DW.Factory factory, D2D.Factory factory2D, D2D.RenderTarget renderTarget)
        {
            base.Init(factory, factory2D, renderTarget);

            ValueChanged?.Invoke(this, 0.0f);
            MaximumChanged?.Invoke(this);
            MinimumChanged?.Invoke(this);
            IncrementChanged?.Invoke(this);
        }

        /// <summary>
        /// Renders the Label
        /// </summary>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Render(D2D.RenderTarget renderTarget)
        {
            base.Render(renderTarget);

            if (!Visible) return;

            //renderTarget.PushAxisAlignedClip(Bound, AntialiasMode.Aliased);

            RectangleF rect = ControlImage.ScrollerBase;
            RectangleF Clientrect = ClientRectangle;
            float minDimension = m_Spacing;

            if (Orientation == ScrollOrientation.Vertical)
            {
                if (Height > 2 * m_Spacing)
                {
                    rect.Width = m_bSpacing;
                    Clientrect.Height = m_Spacing;
                    ControlImage.DrawImageTranspose(renderTarget, rect, Clientrect, CalculatedAlpha);

                    rect.X = ControlImage.ScrollerBase.Right - m_bSpacing;
                    Clientrect.Y = Top + Height - m_Spacing;
                    ControlImage.DrawImageTranspose(renderTarget, rect, Clientrect, CalculatedAlpha);

                    rect.X = ControlImage.ScrollerBase.Left + m_bSpacing;
                    rect.Width = ControlImage.ScrollerBase.Right - 2 * m_bSpacing;
                    Clientrect.Y = Top + m_Spacing;
                    Clientrect.Height = Height - 2 * m_Spacing;

                    ControlImage.DrawImageTranspose(renderTarget, rect, Clientrect, CalculatedAlpha);
                    //ControlImage.DrawImageTranspose(renderTarget, ControlImage.ScrollerBase, ClientRectangle, CalculatedAlpha);
                }

                rect = ControlImage.ScrollerPointer;
                Clientrect = rectRound;
                Clientrect.Width += 3;
                if (rectRound.Height > 2 * m_pSpacing)
                {
                    rect.Width = m_pSpacing;

                    Clientrect.Height = m_pSpacing + 2;
                    Clientrect.X = rectRound.Left;
                    Clientrect.Width = rectRound.Width - 1;
                    ControlImage.DrawImageTranspose(renderTarget, rect, Clientrect);

                    rect.X = ControlImage.ScrollerPointer.Right - m_pSpacing;
                    Clientrect.Y = rectRound.Top + rectRound.Height - m_pSpacing - 1;
                    ControlImage.DrawImageTranspose(renderTarget, rect, Clientrect);

                    rect.X = ControlImage.ScrollerPointer.Left + m_pSpacing;
                    rect.Width = ControlImage.ScrollerPointer.Width - 2 * m_pSpacing;

                    Clientrect.Y = rectRound.Top + m_pSpacing;
                    Clientrect.Height = rectRound.Height - 2 * m_pSpacing;

                    ControlImage.DrawImageTranspose(renderTarget, rect, Clientrect);
                    //ControlImage.DrawImageTranspose(renderTarget, ControlImage.ScrollerPointer, rectRound);
                }
            }
            else
            {

                if (Width > 2 * m_Spacing)
                {
                    rect.Width = m_bSpacing;
                    Clientrect.Width = m_Spacing;
                    ControlImage.DrawImage(renderTarget, rect, Clientrect, CalculatedAlpha);

                    rect.X = ControlImage.ScrollerBase.Right - m_bSpacing;
                    Clientrect.X = Left + Width - m_Spacing;
                    ControlImage.DrawImage(renderTarget, rect, Clientrect, CalculatedAlpha);

                    rect.X = ControlImage.ScrollerBase.Left + m_bSpacing;
                    rect.Width = ControlImage.ScrollerBase.Right - 2 * m_bSpacing;
                    Clientrect.X = Left + m_Spacing;
                    Clientrect.Width = Width - 2 * m_Spacing;

                    ControlImage.DrawImage(renderTarget, rect, Clientrect, CalculatedAlpha);
                    //ControlImage.DrawImage(renderTarget, ControlImage.ScrollerBase, ClientRectangle, CalculatedAlpha);
                }

                rect = ControlImage.ScrollerPointer;
                Clientrect = rectRound;

                if (rectRound.Width > 2 * m_pSpacing)
                {
                    rect.Width = m_pSpacing + 1;
                    rect.X = ControlImage.ScrollerPointer.Left;

                    Clientrect.Width = m_pSpacing + 2;
                    Clientrect.X = rectRound.Left;
                    ControlImage.DrawImage(renderTarget, rect, Clientrect);

                    rect.Height = ControlImage.ScrollerPointer.Height;
                    rect.X = ControlImage.ScrollerPointer.Right - m_pSpacing;
                    Clientrect.X = rectRound.Left + rectRound.Width - m_pSpacing - 1;
                    ControlImage.DrawImage(renderTarget, rect, Clientrect);

                    rect.X = ControlImage.ScrollerPointer.Left + m_pSpacing;
                    rect.Width = ControlImage.ScrollerPointer.Width - 2 * m_pSpacing;

                    Clientrect.X = rectRound.Left + m_pSpacing;
                    Clientrect.Width = rectRound.Width - 2 * m_pSpacing;

                    ControlImage.DrawImage(renderTarget, rect, Clientrect);
                    //ControlImage.DrawImage(renderTarget, ControlImage.ScrollerPointer, rectRound);
                }
            }

            //renderTarget.PopAxisAlignedClip();

        }

        public void Update(double frameTime, UserInput input)
        {
            base.Update(frameTime);

            if (!Visible) return;

            float MajorDimension;

            apperantDimension = (ClientSize.Width - m_Width);
            valueDimension = (m_Maximum - m_Minimum);

            float reduceFactor = m_Value / valueDimension;
            float value = 0.0f;

            float delta = 0.0f;

            switch (m_Orientation)
            {
                case ScrollOrientation.Horizontal:

                    apperantDimension = (ClientSize.Width - m_Width - 2 * m_Spacing);
                    valueDimension = (m_Maximum - m_Minimum);

                    MajorDimension = m_Spacing + reduceFactor * apperantDimension; //Use Max to set minimum value

                    rectRound = ClientRectangle;
                    rectRound.X = Left + m_Spacing;
                    rectRound.Width = Width - 2 * m_Spacing;

                    if (!Enabled) return;

                    rectRound.X = Left + MajorDimension;
                    rectRound.Y = Top;

                    rectRound.Width = m_Width;
                    rectRound.Height = m_Height - 1;

                    if (input.IsMouseButtonPressed(MouseButtons.Left))
                    {
                        value = (ParentHOD.MousePosition.X - Left - m_Width / 2) / apperantDimension * valueDimension;
                        delta = ParentHOD.MousePosition.X - ParentHOD.MousePositionOld.X;
                    }

                    if (m_Focused)
                    {
                        if (input.IsKeyPressed(Key.RightArrow))
                            Value += m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;

                        if (input.IsKeyPressed(Key.LeftArrow))
                            Value -= m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;

                    }

                    break;
                case ScrollOrientation.Vertical:

                    apperantDimension = (ClientSize.Height - m_Width - 2 * m_Spacing);
                    valueDimension = (m_Maximum - m_Minimum);

                    MajorDimension = m_Spacing + reduceFactor * apperantDimension; //Use Max to set minimum value

                    rectRound = ClientRectangle;
                    rectRound.Y = Top + m_Spacing;
                    rectRound.Height = Height - 2 * m_Spacing;

                    if (!Enabled) return;

                    rectRound.X = Left;
                    rectRound.Y = Top + MajorDimension;

                    rectRound.Width = m_Height - 1;
                    rectRound.Height = m_Width;


                    if (input.IsMouseButtonPressed(MouseButtons.Left))
                    {
                        value = (ParentHOD.MousePosition.Y - Top - m_Width / 2) / apperantDimension * valueDimension;
                        delta = ParentHOD.MousePosition.Y - ParentHOD.MousePositionOld.Y;
                    }
                    if (m_Focused)
                    {
                        if (input.IsKeyPressed(Key.UpArrow))
                            Value -= m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;

                        if (input.IsKeyPressed(Key.DownArrow))
                            Value += m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;

                    }


                    break;
            }

            if (!Enabled) return;

            if (input.IsMouseButtonPressed(MouseButtons.Left))
            {
                if (m_MouseOver && m_MouseDown)
                {
                    if (m_useStaticPoniter)
                    {
                        if (Value + m_Increment / 2.0f < value)
                        {
                            Value += m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;
                        }
                        else if (Value - m_Increment / 2.0f > value)
                        {
                            Value -= m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;
                        }
                    }

                    m_TrackPoniter = true;
                }

                if (m_TrackPoniter && m_MouseOver && m_MouseDown)
                {
                    Value += delta / apperantDimension * valueDimension;
                }
            }

            if (m_MouseOver && Math.Abs(input.MouseWheelMovement()) > 0)
            {
                Value -= input.MouseWheelMovement() * 10.0f / apperantDimension * valueDimension * (float)frameTime;
            }
        }

        public void MouseWheel(double frameTime, UserInput input)
        {
            if (Enabled && IsAssociated && Math.Abs(input.MouseWheelMovement()) > 0 && apperantDimension > 0)
            {
                Value -= input.MouseWheelMovement() * 10.0f / apperantDimension * valueDimension * (float)frameTime;
            }
        }

        /// <summary>
        /// Updates the Label
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            if (!Visible) return;

            HUD HUD = m_parentHOD;
            UserInput input = HUD.UserInput;

            Update(frameTime, input);
        }

        #endregion

        #region Event Handler

        private void Changed(DXControl c)
        {
            float cWidth = 50.0f;
            float minWidth = 5.0f;
            float maxWidth = 100.0f;

            switch (Orientation)
            {
                case ScrollOrientation.Horizontal:
                    minWidth = Width / 4.0f;
                    maxWidth = Width - 2 * m_Spacing;

                    break;
                case ScrollOrientation.Vertical:
                    minWidth = Height / 4.0f;
                    maxWidth = Height - 2 * m_Spacing;

                    break;
            }

            cWidth = maxWidth / (Maximum - Minimum) * Increment;
            cWidth = Math.Max(Math.Min(cWidth, maxWidth), minWidth);

            m_Width = cWidth;
        }

        private void Scroller_MouseUp(DXControl ctrl, MouseState state, MouseState stateOld)
        {
            m_useStaticPoniter = true;
            m_TrackPoniter = false;
        }

        private void Scroller_MouseMove(DXControl ctrl, MouseState state, MouseState stateOld)
        {
            if (m_MouseDown)
                m_useStaticPoniter = false;
        }

        private void Scroller_EnabledChanged(DXControl obj)
        {

        }

        private void Scroller_SizeChanged(DXControl obj)
        {
            switch (Orientation)
            {
                case ScrollOrientation.Horizontal:
                    Height = m_Height;
                    break;
                case ScrollOrientation.Vertical:
                    Width = m_Height;
                    break;
            }
        }

        #endregion

        #region Disposal
        /// <summary>
        /// Disposes the Label
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                MouseDown -= focus_Control;

                MouseMove -= Scroller_MouseMove;
                MouseUp -= Scroller_MouseUp;
                OrientationChanged -= Changed;
                MaximumChanged -= Changed;
                MinimumChanged -= Changed;
                IncrementChanged -= Changed;
                EnabledChanged -= Scroller_EnabledChanged;

                SizeChanged -= Scroller_SizeChanged;
            }

            base.Dispose(disposing);

        }

        #endregion

    }
}