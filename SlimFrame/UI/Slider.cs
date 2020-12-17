using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        class Slider : Control
        {

            #region Member Variables

            //Text allignment support.
            protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
            protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

            protected bool m_AutoSize = false; // Stores auto size state of label
            
            private float m_Value;

            private float m_Minimum;
            private float m_Maximum;

            private float m_width = 20.0f;
            private float m_height = 15.0f;

            private Color m_PointerColor = Color.Gold;
            private Color m_PointerBorderColor = Color.Goldenrod;
            private float m_PointerBorderWidth = 2.0f;
            private float m_Increment = 0.25f;
            private float m_smallIncrement = 0.05f;

            private bool m_useStaticPoniter = true;
            private bool m_TrackPoniter = false;

            private RoundedRectangle rectRoundBack; // Slider background rounded rectangle
            private RoundedRectangle rectRound;     // Pointer rounded rectangle

            private float reduceFactor  = 0.75f;    // Stores factor which determines the portion of the height of Slider pointer to cut from Slider bar
            private float yOffset       = 0.0f;    // Stores y-axis offset from the text bundaries.

            private Color FontRenderColor;

            static Action<Control> UpdateBack = (c) =>
            {
                Slider sld = (Slider)c;

                sld.rectRoundBack.Left  = sld.m_Location.X;
                sld.rectRoundBack.Top   = sld.m_Location.Y + sld.yOffset * 0 + sld.ClientSize.Height + sld.m_height / 2 * sld.reduceFactor;

                sld.rectRoundBack.Right     = sld.rectRoundBack.Left + sld.m_Size.Width;
                sld.rectRoundBack.Bottom    = sld.rectRoundBack.Top - sld.m_height * sld.reduceFactor + sld.m_height;
            };

            #endregion

            #region Properties

            /// <summary>
            /// Value of slider
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
                        ValueChanged?.Invoke(this,old);
                    }
                }
            }

            /// <summary>
            /// Maximum value of slider
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
            /// Minimum value of slider
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
            /// Size of client area of slider
            /// </summary>
            public SizeF ClientSize
            {
                get
                {
                    return new SizeF(m_Size.Width, m_Size.Height - m_height - 2.0f + yOffset);
                }
                set
                {
                    Size = new SizeF(value.Width, value.Height + m_height + 2.0f + yOffset);
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
            public event Action<Slider,float> ValueChanged;

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Slider> MaximumChanged;

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Slider> MinimumChanged;

            /// <summary>
            /// Occures when the value of Slider is changed
            /// </summary>
            public event Action<Slider> IncrementChanged;

            #endregion

            #region Constructors

            public Slider()
            {
                Init();
            }
            public Slider(string name)
            {
                Name = name;

                Init();
            }

            #endregion

            #region Methods

            /// <summary>
            /// Initializes the Slider
            /// </summary>
            private void Init()
            {

                rectRound = new RoundedRectangle();
                rectRoundBack = new RoundedRectangle();

                m_BorderWidth = 0.0f;

                m_TextEnabled = false;
                m_BackEnabled = false;
                TabStop = true;

                rectRoundBack.RadiusX = m_height / 2 * (1 - reduceFactor);
                rectRoundBack.RadiusY = m_height / 2 * (1 - reduceFactor);
                
                MouseOver += (c) =>
                {
                    HOD hod = m_parentHOD;
                    UserInput input = hod.UserInput;
                    float apperantWidth = (ClientSize.Width - m_width);
                    float valueWidth = (m_Maximum - m_Minimum);

                    if (Math.Abs(input.MouseWheelMovement()) > 0)
                    {
                        Value -= input.MouseWheelMovement() * 100.0f / apperantWidth * valueWidth / (float) hod.FramePerSecond;
                    }
                };

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

                ForeColorChanged += (c) =>
                {
                    FontRenderColor = ForeColor;
                };

                EnabledChanged += (c) =>
                {
                    if (c.Enabled)
                    {
                        m_PointerColor = Color.Gold;
                        m_PointerBorderColor = Color.Goldenrod;

                        ForeColor = FontRenderColor;
                    }
                    else
                    {
                        m_PointerColor = Color.DimGray;
                        m_PointerBorderColor = Color.DarkGray;

                        ForeColor = Color.DarkGray;
                    }

                };

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

                m_textFormat.TextAlignment = (DW.TextAlignment)m_textHAllignment;
                m_textFormat.ParagraphAlignment = (DW.ParagraphAlignment)m_textVAllignment;

                FontRenderColor = m_ForeColor;

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

                if (!m_TextEnabled)
                {
                    m_brush.Color = ForeColor;
                    renderTarget.DrawText(m_Text + " : " + Value, m_textFormat, Bound, m_brush);
                }

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

                if (!Visible) return;

                HOD hod = m_parentHOD;
                UserInput input = hod.UserInput;

                float x, y;

                float apperantWidth = (ClientSize.Width - m_width);
                float valueWidth = (m_Maximum - m_Minimum);

                float reduceFactor = m_Value / valueWidth;
                
                if (Value < Minimum || Value > Maximum)
                    return;

                x = reduceFactor * apperantWidth; //Use Max to set minimum value

                y = 2 * yOffset + m_font.Height;

                rectRound.Left = m_Location.X + x;
                rectRound.Top = m_Location.Y + y;

                rectRound.Right = rectRound.Left + m_width;
                rectRound.Bottom = rectRound.Top + m_height;

                rectRound.RadiusX = 5.0f;
                rectRound.RadiusY = 5.0f;

                if (!Enabled) return;

                if (input.IsMouseButtonPressed(MouseButtons.Left))
                {
                    float value = (ParentHOD.MousePosition.X - m_Location.X - m_width/2) / apperantWidth * valueWidth;
                    
                    if (hod.MousePosition.Y  - m_Location.Y > ClientSize.Height)
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
                        Value += (hod.MousePosition.X - hod.MousePositionOld.X) / apperantWidth * valueWidth;
                    }
                }
                
                if (m_MouseOver && Math.Abs(input.MouseWheelMovement()) > 0)
                {
                    Value -= input.MouseWheelMovement() * 100.0f / apperantWidth * valueWidth * (float)frameTime;
                }

                if (hod.ActiveControl == this)
                {
                    if (input.IsKeyPressed(Key.UpArrow))
                    {
                            Value += m_smallIncrement;
                    }
                    if (input.IsKeyPressed(Key.DownArrow))
                    {
                            Value -= m_smallIncrement;
                    }
                    if (input.IsKeyPressed(Key.RightArrow))
                    {
                            Value += m_Increment;
                    }
                    if (input.IsKeyPressed(Key.LeftArrow))
                    {
                        Value -= m_Increment;
                    }
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
                base.Dispose(disposing);

                LocationChanged += UpdateBack;
                SizeChanged += UpdateBack;

            }

            #endregion

        }
    }
}