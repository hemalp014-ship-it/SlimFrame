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

namespace SlimFrame
{
    namespace UI
    {
        class CheckBox : Control, ITextControl, IEnabler
        {

            #region Member Variables

            //Text allignment support.
            protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
            protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

            protected bool m_AutoSize = false; // Stores auto size state of label
            
            private float m_width = 20.0f;
            private float m_height = 20.0f;

            private Color m_CheckColor = Color.White;
            private Color m_CheckBorderColor = Color.Gray;
            private float m_CheckBorderWidth = 2.0f;
            
            private RoundedRectangle rectRound;     // Pointer rounded rectangle
                       
            private float xOffset = 22.0f;    // Stores y-axis offset from the text bundaries.
            private float yOffset = 0.0f;    // Stores y-axis offset from the text bundaries.

            private bool m_Checked = false;   // Determines the checked state of the Checkbox

            private PointF[] Check;

            private Action<Control> UpdateCheck = (c) =>
            {
                CheckBox ch = (CheckBox)c;

                float yOff = ch.rectRound.Top + ch.m_height * 0.4f;
                float xOff = ch.rectRound.Left + ch.m_width * 0.18f;

                if(ch.m_TextSize.Height > ch.m_height)
                {
                    ch.yOffset = (ch.m_TextSize.Height - ch.m_height) / 2.0f;
                }

                ch.Check[0] = new PointF(xOff, yOff);
                ch.Check[1] = new PointF(xOff + ch.m_width / 5, yOff + ch.m_height / 3);
                ch.Check[2] = new PointF(xOff + ch.m_width * 0.65f, yOff - ch.m_height * 0.2f);

                ch.rectRound.Left = ch.m_Location.X;
                ch.rectRound.Top = ch.m_Location.Y + ch.yOffset;

                ch.rectRound.Right = ch.rectRound.Left + ch.m_width;
                ch.rectRound.Bottom = ch.rectRound.Top + ch.m_height;

                ch.rectRound.RadiusX = 5.0f;
                ch.rectRound.RadiusY = 5.0f;
            };

            private Color FontRenderColor;

            #endregion

            #region Properties            
            
            /// <summary>
            /// Determines the Checked state of CheckBox
            /// </summary>
            public bool Checked
            {
                get
                {
                    return m_Enabled ? m_Checked : false;
                }
                set
                {
                    if (m_Checked != value)
                    {
                        m_Checked = value;
                        CheckChanged?.Invoke(this);
                    }
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
                    return new SizeF(m_Size.Width - xOffset, Math.Max(m_Size.Height, m_height));
                }
                set
                {
                    Size = new SizeF(value.Width + xOffset, Math.Max(value.Height, m_height));
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
            /// Occures when the Check state of CheckBox is changed
            /// </summary>
            public event Action<CheckBox> CheckChanged;

            #endregion

            #region Constructors

            public CheckBox()
            {
                Init();
            }
            public CheckBox(string name)
            {
                Name = name;

                Init();
            }

            #endregion

            #region Methods

            /// <summary>
            /// Initializes the CheckBox
            /// </summary>
            private void Init()
            {
                Check = new PointF[3];

                rectRound = new RoundedRectangle();

                m_BorderWidth = 0.0f;

                m_dTextMeasuredSize = false;
                m_TextEnabled = false;
                TabStop = true;

                FontRenderColor = m_ForeColor;

                MouseDown += (c, s, o) => Checked = !Checked;

                LocationChanged += UpdateCheck;
                SizeChanged += UpdateCheck;

                TextChanged += (c) =>
                {
                    ChangedText(this);

                    ClientSize = m_TextSize;
                };

                EnabledChanged += (c) =>
                {
                    if (c.Enabled)
                    {
                        m_CheckColor = Color.White;
                        m_ForeColor = FontRenderColor;
                    }
                    else
                    {
                        m_CheckColor = Color.DarkGray;

                        FontRenderColor = m_ForeColor;
                        m_ForeColor = Color.DarkGray;
                    }
                };

                UpdateCheck(this);
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

                CheckChanged?.Invoke(this);
            }

            /// <summary>
            /// Renders the Label
            /// </summary>
            /// <param name="renderTarget">Render Target for 2D rendering</param>
            public override void Render(D2D.RenderTarget renderTarget)
            {
                base.Render(renderTarget);

                if (!Visible) return;

                m_brush.Color = ForeColor;
                renderTarget.DrawText(m_Text, m_textFormat, new RectangleF(Left + xOffset, Top, ClientSize.Width, ClientSize.Height), m_brush);

                m_brush.Color = m_CheckColor;
                renderTarget.FillRoundedRectangle(m_brush, rectRound);
                m_brush.Color = m_CheckBorderColor;
                renderTarget.DrawRoundedRectangle(m_brush, rectRound, m_CheckBorderWidth);

                if (Checked)
                {
                    m_brush.Color = m_CheckBorderColor;

                    renderTarget.DrawLine(m_brush, Check[0], Check[1], 3.0f);
                    renderTarget.DrawLine(m_brush, Check[1], Check[2], 3.0f);
                }
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

                if (!Visible) return;

                if (!Enabled) return;


                if (hod.ActiveControl == this)
                    if (input.IsKeyJustPressed(Key.Space) ||
                            input.IsKeyJustPressed(Key.NumberPadEnter) ||
                            input.IsKeyJustPressed(Key.Return))
                    {
                        Checked = !Checked;
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

            }

            #endregion

        }
    }
}