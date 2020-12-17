using System;
using System.Windows.Forms;

using SlimDX;
using DW = SlimDX.DirectWrite;
using D2D = SlimDX.Direct2D;

using System.Drawing;
using SlimDX.Direct2D;
using SlimDX.DirectInput;
using System.Diagnostics;

namespace SlimFrame
{
    namespace UI
    {
        public enum ScrollOrientation
        {
            Horizontal,
            Vertical
        }

        class Scroller : Control
        {

            #region Member Variables
                        
            private float m_Value;              // The value of control

            private float m_Minimum;            // The minimum value of control
            private float m_Maximum;            // The maximum value of control

            private float m_Width   = 50.0f;    // The width of pointer    
            private float m_Height  = 15.0f;    // The height of pointer

            private float m_PointerBorderWidth  = 2.0f;             // The border width of pointer
            private Color m_PointerBorderColor  = Color.Blue;       // The border color of pointer
            private Color m_PointerColor        = Color.SkyBlue;    // The color of pointer
            private float m_Increment           = 0.25f;            // The increment in value

            private bool m_useStaticPoniter = true;                 // Determines whether static mouse pointer should be used or not
            private bool m_TrackPoniter     = false;                // Determines whether moving mouse pointer should be tracked for changing the value

            private RoundedRectangle rectRoundBack; // scroller background rounded rectangle
            private RoundedRectangle rectRound;     // Pointer rounded rectangle

            private float reduceFactor = 0.0f;     // Stores factor which determines the portion of the height of Slider pointer to cut from Slider bar

            private ScrollOrientation m_Orientation = ScrollOrientation.Horizontal;            

            static Action<Control> UpdateBack = (c) =>
            {
                Scroller slr = (Scroller) c;

                switch (slr.Orientation)
                {
                    case ScrollOrientation.Horizontal:
                        {
                            //if (slr.Height < slr.Width)
                            //{
                            //    slr.Height = slr.Height + slr.Width;
                            //    slr.Width = slr.Height - slr.Width;
                            //    slr.Height = slr.Height - slr.Width;
                            //}

                            slr.rectRoundBack.Left = slr.m_Location.X;
                            slr.rectRoundBack.Top = slr.m_Location.Y + slr.m_Height / 2 * slr.reduceFactor;

                            slr.rectRoundBack.Right = slr.rectRoundBack.Left + slr.Width;
                            slr.rectRoundBack.Bottom = slr.rectRoundBack.Top - slr.m_Height * slr.reduceFactor + slr.m_Height;
                            
                            //slr.Height = slr.m_Height;

                            break;
                        }
                    case ScrollOrientation.Vertical:
                        {

                            //if (slr.Height < slr.Width)
                            //{
                            //    slr.Height = slr.Height + slr.Width;
                            //    slr.Width = slr.Height - slr.Width;
                            //    slr.Height = slr.Height - slr.Width;
                            //}

                            slr.rectRoundBack.Left = slr.m_Location.X + slr.m_Height / 2 * slr.reduceFactor;
                            slr.rectRoundBack.Top = slr.m_Location.Y;

                            slr.rectRoundBack.Right = slr.rectRoundBack.Left - slr.m_Height * slr.reduceFactor + slr.m_Height;
                            slr.rectRoundBack.Bottom = slr.rectRoundBack.Top + slr.Height;

                            //slr.Width = slr.m_Height;

                            break;
                        }

                }
            };

            static Action<Control> Changed = (c) =>
            {
                var s = (Scroller)c;

                float minWidth = 5.0f;
                float maxWidth = 100.0f;
                float cWidth = 50.0f;

                switch (s.Orientation)
                {
                    case ScrollOrientation.Horizontal:
                        minWidth = s.Width / 10.0f;
                        maxWidth = s.Width;

                        break;
                    case ScrollOrientation.Vertical:
                        minWidth = s.Height / 10.0f;
                        maxWidth = s.Height;
                        
                        break;
                }

                cWidth = maxWidth / ((s.Maximum - s.Minimum) / s.Increment);
                cWidth = Math.Max(Math.Min(cWidth, maxWidth), minWidth);
                s.m_Width = cWidth;
            };

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
                    var temp = Math.Min(Math.Max(value, m_Minimum), m_Maximum);

                    if (temp != m_Value)
                    {
                        var old = m_Value;
                        m_Value = temp;
                        ValueChanged?.Invoke(this, old);
                    }
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
                    Size = value;
                }
            }
            
            #endregion

            #region Event System

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Scroller, float> ValueChanged;

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Scroller> MaximumChanged;

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Scroller> MinimumChanged;

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Scroller> IncrementChanged;

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Scroller> OrientationChanged;

            #endregion

            #region Constructors

            public Scroller(ScrollOrientation orientation)
            {
                Init();
                m_Orientation = orientation;
            }
            public Scroller(string name, ScrollOrientation orientation)
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

                m_BorderWidth = 0.0f;

                m_TextEnabled = false;
                m_BackEnabled = false;
                m_BorderEnabled = false;
                TabStop = true;

                rectRound = new RoundedRectangle();
                rectRoundBack = new RoundedRectangle();

                rectRoundBack.RadiusX = m_Height / 2 * (1 - reduceFactor);
                rectRoundBack.RadiusY = m_Height / 2 * (1 - reduceFactor);

                MouseMove += (c, s, o) =>
                {
                    if (m_MouseDown)
                        m_useStaticPoniter = false;
                };
                MouseUp += (c, s, o) =>
                {
                    m_useStaticPoniter = true;
                    m_TrackPoniter = false;
                };

                MouseDown += focus;


                LocationChanged += UpdateBack;
                SizeChanged += UpdateBack;
                OrientationChanged += UpdateBack;

                OrientationChanged += Changed;
                MaximumChanged += Changed;
                MinimumChanged += Changed;
                IncrementChanged += Changed;
                
                EnabledChanged += (c) =>
                {
                    if (c.Enabled)
                    {
                        m_PointerBorderColor = Color.Blue;
                        m_PointerColor = Color.SkyBlue;
                    }
                    else
                    {
                        m_PointerBorderColor = Color.DarkGray;
                        m_PointerColor = Color.DimGray;
                    }
                };


                UpdateBack.Invoke(this);
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

                //renderTarget.FillRectangle(m_brush, New RectangleF(Button1.Right, Button1.Top, Button2.Left - Button1.Right, Button2.Bottom - Button1.Top))
                m_brush.Color = m_BackColor;
                renderTarget.FillRoundedRectangle(m_brush, rectRoundBack);

                m_brush.Color = m_BorderColor;
                renderTarget.DrawRoundedRectangle(m_brush, rectRoundBack, m_BorderWidth);

                m_brush.Color = m_PointerColor;
                renderTarget.FillRoundedRectangle(m_brush, rectRound);
                m_brush.Color = m_PointerBorderColor;
                renderTarget.DrawRoundedRectangle(m_brush, rectRound, m_PointerBorderWidth);

            }

            /// <summary>
            /// Updates the Label
            /// </summary>
            /// <param name="frameTime"></param>
            public override void Update(double frameTime)
            {
                base.Update(frameTime);

                if (!Visible) return;
                
                HOD hod = m_parentHOD;
                UserInput input = hod.UserInput;

                float x, y;

                float apperantDimension = (ClientSize.Width - m_Width);
                float valueDimension = (m_Maximum - m_Minimum);

                float reduceFactor = m_Value / valueDimension;
                float value = 0.0f;

                float delta = 0.0f;

                switch (m_Orientation)
                {
                    case ScrollOrientation.Horizontal:

                        apperantDimension = (ClientSize.Width - m_Width);
                        valueDimension = (m_Maximum - m_Minimum);

                        if (Value < Minimum || Value > Maximum)
                            return;

                        x = reduceFactor * apperantDimension ; //Use Max to set minimum value
                        y = 0;

                        rectRound.Left = m_Location.X + x;
                        rectRound.Top = m_Location.Y + y;

                        rectRound.Right = rectRound.Left + m_Width;
                        rectRound.Bottom = rectRound.Top + m_Height;

                        rectRound.RadiusX = 5.0f;
                        rectRound.RadiusY = 5.0f;

                        if (!Enabled) return;

                        if (input.IsMouseButtonPressed(MouseButtons.Left))
                        {
                            value = (ParentHOD.MousePosition.X - m_Location.X - m_Width / 2) / apperantDimension * valueDimension;
                            delta = ParentHOD.MousePosition.X - ParentHOD.MousePositionOld.X;
                        }

                        if (hod.ActiveControl == this)
                        {
                            if (input.IsKeyPressed(Key.RightArrow))
                                Value += m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;

                            if (input.IsKeyPressed(Key.LeftArrow))
                                Value -= m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;

                        }

                        break;
                    case ScrollOrientation.Vertical:

                        apperantDimension = (ClientSize.Height - m_Width);
                        valueDimension = (m_Maximum - m_Minimum);

                        if (Value < Minimum || Value > Maximum)
                            return;

                        x = reduceFactor * apperantDimension; //Use Max to set minimum value

                        y = 0;

                        rectRound.Left = m_Location.X;
                        rectRound.Top = m_Location.Y + x;

                        rectRound.Right = rectRound.Left + m_Height;
                        rectRound.Bottom = rectRound.Top + m_Width;

                        rectRound.RadiusX = 5.0f;
                        rectRound.RadiusY = 5.0f;

                        if (!Enabled) return;

                        if (input.IsMouseButtonPressed(MouseButtons.Left))
                        {
                            value = (ParentHOD.MousePosition.Y - m_Location.Y - m_Width / 2) / apperantDimension * valueDimension;
                            delta = ParentHOD.MousePosition.Y - ParentHOD.MousePositionOld.Y;
                        }
                        if (hod.ActiveControl == this)
                        {
                            if (input.IsKeyPressed(Key.UpArrow))
                                Value -= m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;
                            
                            if (input.IsKeyPressed(Key.DownArrow))
                                Value += m_Increment * 100.0f / apperantDimension * valueDimension * (float)frameTime;
                            
                        }

                       
                        break;
                }


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

            //private void Slider_MouseMove(Control ctrl, MouseState state, MouseState stateOld)
            //{
            //    HOD hod = m_parentHOD;
            //    UserInput input = hod.UserInput;

            //    if (hod.MousePosition.Y - m_Location.Y > ClientSize.Height)
            //    {
            //        if (input.IsMouseButtonPressed(MouseButtons.Left))
            //                Value += input.MouseState_Current.X;
            //    }
            //}

            #endregion

            #region Disposal
            /// <summary>
            /// Disposes the Label
            /// </summary>
            /// <param name="disposing"></param>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                LocationChanged += UpdateBack;
                SizeChanged += UpdateBack;

            }

            #endregion

        }
    }
}