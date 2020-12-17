using System;
using System.Windows.Forms;

using SlimDX;
using DW = SlimDX.DirectWrite;
using D2D = SlimDX.Direct2D;
using System.Drawing;
using SlimDX.DirectInput;
using SlimDX.Direct2D;
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

            //Text allignment support.
            protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
            protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

            protected bool m_AutoSize = false; // Stores auto size state of label
            
            private float m_Value;

            private float m_Minimum;
            private float m_Maximum;

            private float width = 50.0f;
            private float height = 15.0f;

            private Color m_PointerColor = Color.SkyBlue;
            private Color m_PointerBorderColor = Color.Blue;
            private float m_PointerBorderWidth = 2.0f;
            private float m_Increment = 0.25f;
            private float m_smallIncrement = 0.05f;

            private bool m_useStaticPoniter = true;
            private bool m_TrackPoniter = false;

            private RoundedRectangle rectRoundBack; // scroller background rounded rectangle
            private RoundedRectangle rectRound;     // Pointer rounded rectangle

            private float factor = 0.25f;

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
                            slr.rectRoundBack.Top = slr.m_Location.Y + slr.height / 2 * slr.factor;

                            slr.rectRoundBack.Right = slr.rectRoundBack.Left + slr.Width;
                            slr.rectRoundBack.Bottom = slr.rectRoundBack.Top - slr.height * slr.factor + slr.height;
                            
                            slr.Height = slr.height;

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

                            slr.rectRoundBack.Left = slr.m_Location.X + slr.height / 2 * slr.factor;
                            slr.rectRoundBack.Top = slr.m_Location.Y;

                            slr.rectRoundBack.Right = slr.rectRoundBack.Left - slr.height * slr.factor + slr.height;
                            slr.rectRoundBack.Bottom = slr.rectRoundBack.Top + slr.Height;

                            slr.Width = slr.height;

                            break;
                        }

                }
            };

            #endregion

            #region Property

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
                }
            }

            /// <summary>
            /// Determines whether the size of control to be adjusted according the content or not.
            /// </summary>
            public bool AutoSize
            {
                get
                {
                    return m_AutoSize;
                }
                set
                {
                    m_AutoSize = value;
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

            /// <summary>
            /// Determines Horizontal Allignment of text
            /// </summary>
            public TextHAllignment HorizontalAllignment
            {
                get
                {
                    return m_textHAllignment;
                }
                set
                {
                    m_textHAllignment = value;
                    if (m_textFormat != null) m_textFormat.TextAlignment = (DW.TextAlignment)m_textHAllignment;
                }
            }
            
            /// <summary>
            /// Determines Vertical Allignment of text
            /// </summary>
            public TextVAllignment VerticalAllignment
            {
                get
                {
                    return m_textVAllignment;
                }
                set
                {
                    m_textVAllignment = value;
                    if (m_textFormat != null) m_textFormat.ParagraphAlignment = (DW.ParagraphAlignment)m_textVAllignment;
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

            private void Init()
            {

                m_BorderWidth = 0.0f;

                m_TextEnabled = false;
                m_BackEnabled = false;
                TabStop = true;

                rectRound = new RoundedRectangle();
                rectRoundBack = new RoundedRectangle();

                rectRoundBack.RadiusX = height / 2 * (1 - factor);
                rectRoundBack.RadiusY = height / 2 * (1 - factor);

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

                LocationChanged += UpdateBack;
                SizeChanged += UpdateBack;
                OrientationChanged += UpdateBack;

                UpdateBack.Invoke(this);
            }

            /// <summary>
            /// Initializes the Label
            /// </summary>
            /// <param name="factory">factory for creating DirectWrite Objects</param>
            /// <param name="factory2D">factory for creating Direct2D Objects</param>
            /// <param name="renderTarget">Render Target for 2D rendering</param>
            public override void Init(DW.Factory factory, D2D.Factory factory2D, D2D.RenderTarget renderTarget)
            {
                base.Init(factory, factory2D, renderTarget);

                m_textFormat.TextAlignment = (DW.TextAlignment)m_textHAllignment;
                m_textFormat.ParagraphAlignment = (DW.ParagraphAlignment)m_textVAllignment;
            }

            /// <summary>
            /// Renders the Label
            /// </summary>
            /// <param name="renderTarget">Render Target for 2D rendering</param>
            public override void Render(D2D.RenderTarget renderTarget)
            {
                base.Render(renderTarget);
                
                //renderTarget.FillRectangle(m_brush, New RectangleF(Button1.Right, Button1.Top, Button2.Left - Button1.Right, Button2.Bottom - Button1.Top))
                m_brush.Color = m_BackColor;
                renderTarget.FillRoundedRectangle(m_brush, rectRoundBack);

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

                HOD hod = m_parentHOD;
                UserInput input = hod.UserInput;

                float x, y;

                float apperantDimension = (ClientSize.Width - width);
                float valueDimension = (m_Maximum - m_Minimum);

                float factor = m_Value / valueDimension;
                
                switch (m_Orientation)
                {
                    case ScrollOrientation.Horizontal:

                        apperantDimension = (ClientSize.Width - width);
                        valueDimension = (m_Maximum - m_Minimum);

                        if (Value < Minimum || Value > Maximum)
                            return;

                        x = factor * apperantDimension; //Use Max to set minimum value

                        y = height;

                        rectRound.Left = m_Location.X + x;
                        rectRound.Top = m_Location.Y;

                        rectRound.Right = rectRound.Left + width;
                        rectRound.Bottom = rectRound.Top + height;

                        rectRound.RadiusX = 5.0f;
                        rectRound.RadiusY = 5.0f;

                        if (input.IsMouseButtonPressed(MouseButtons.Left))
                        {
                            float value = (ParentHOD.MousePosition.X - m_Location.X - width / 2) / apperantDimension * valueDimension;
                            
                                if (m_MouseOver && m_MouseDown)
                                {
                                    if (m_useStaticPoniter)
                                    {
                                        if (Value + m_Increment / 2.0f < value)
                                        {
                                            Value += m_Increment;
                                        }
                                        else if (Value - m_Increment / 2.0f > value)
                                        {
                                            Value -= m_Increment;
                                        }
                                    }

                                    m_TrackPoniter = true;
                                }

                            if (m_TrackPoniter && m_MouseOver && m_MouseDown)
                            {
                                Value += (hod.MousePosition.X - hod.MousePositionOld.X) / apperantDimension * valueDimension;
                            }
                        }

                        if (hod.ActiveControl == this)
                        {
                            if (input.IsKeyPressed(Key.RightArrow))
                            {
                                Value += m_Increment;
                            }
                            if (input.IsKeyPressed(Key.LeftArrow))
                            {
                                Value -= m_Increment;
                            }
                        }

                        break;
                    case ScrollOrientation.Vertical:

                        apperantDimension = (ClientSize.Height - width);
                        valueDimension = (m_Maximum - m_Minimum);

                        if (Value < Minimum || Value > Maximum)
                            return;

                        x = factor * apperantDimension; //Use Max to set minimum value

                        y = height;

                        rectRound.Left = m_Location.X;
                        rectRound.Top = m_Location.Y + x;

                        rectRound.Right = rectRound.Left + height;
                        rectRound.Bottom = rectRound.Top + width;

                        rectRound.RadiusX = 5.0f;
                        rectRound.RadiusY = 5.0f;

                        if (input.IsMouseButtonPressed(MouseButtons.Left))
                        {
                            float value = (ParentHOD.MousePosition.Y - m_Location.Y - width / 2) / apperantDimension * valueDimension;
                            
                                if (m_MouseOver && m_MouseDown)
                                {
                                    if (m_useStaticPoniter)
                                    {
                                        if (Value + m_Increment / 2.0f < value)
                                        {
                                            Value += m_Increment;
                                        }
                                        else if (Value - m_Increment / 2.0f > value)
                                        {
                                            Value -= m_Increment;
                                        }
                                    }

                                    m_TrackPoniter = true;
                                }

                            if (m_TrackPoniter && m_MouseOver && m_MouseDown)
                            {
                                Value += (hod.MousePosition.Y - hod.MousePositionOld.Y) / apperantDimension * valueDimension;
                            }
                        }

                        if (hod.ActiveControl == this)
                        {
                            if (input.IsKeyPressed(Key.UpArrow))
                            {
                                Value -= m_Increment;
                            }
                            if (input.IsKeyPressed(Key.DownArrow))
                            {
                                Value += m_Increment;
                            }
                        }

                        break;
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