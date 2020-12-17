using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using DW = SlimDX.DirectWrite;
using SlimDX.Direct2D;
using SlimDX.DirectInput;
using System.Drawing;
using System.Diagnostics;

namespace SlimFrame
{
    namespace UI
    {
        enum ButtonMouseState
        {
            Out  = 55,
            Over = 65,
            Down = 75,
        }

        class Button : Control, ITextControl, IEnabler
        {

            #region Member Variables

            //Text allignment support.
            protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
            protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

            //Auto size support
            protected bool  m_AutoSize = false; // Stores auto size state of button

            private float   alpha;          // Alpha value of background color
            private float   targetAlpha;    // Target Alpha value of background color
            private Color3  baseColor;      // Base background color
            
            private RoundedRectangle     rectRound;  // Button rounded rectangle
            private float       step;     // Setps of transition
            private double      transitionTime = 0.1; // amount of time in seconds for transition

            private Color FontRenderColor;
            private Color BackRenderColor;

            static Action<Control> UpdateBack = (c) =>
            {
                Button btn = (Button) c;


                btn.rectRound.Left = btn.m_Location.X;
                btn.rectRound.Top = btn.m_Location.Y;

                btn.rectRound.Right = btn.rectRound.Left + btn.m_Size.Width;
                btn.rectRound.Bottom = btn.m_Location.Y + btn.m_Size.Height;
            };

            #endregion

            #region Properties
            
            /// <summary>
            /// BackColor of the Control
            /// </summary>
            public override Color BackColor
            {
                get
                {
                    return m_BackColor;
                }
                set
                { 
                    m_BackColor = value;
                    baseColor = new Color4(value).ToColor3();
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

            #region Constructors

            public Button()
            {
                Init();
            }
            
            public Button(string name)
            {
                Name = name;

                Init();
            }

            #endregion

            #region Methods

            /// <summary>
            /// Initializes the Button
            /// </summary>
            private void Init()
            {
                rectRound = new RoundedRectangle();

                alpha = (int)ButtonMouseState.Out / 100.0f;
                targetAlpha = alpha;

                m_BackEnabled = false;
                m_BorderEnabled = false;
                m_TextEnabled = false;
                m_BorderWidth = 1.0f;

                rectRound.RadiusX = 10.0f;
                rectRound.RadiusY = 10.0f;

                FontRenderColor = m_ForeColor;
                BackRenderColor = m_BackColor;

                MouseOver += (c) => targetAlpha = (int)ButtonMouseState.Over / 100.0f;
                MouseOut += (c) => targetAlpha = (int)ButtonMouseState.Out / 100.0f;

                MouseUp += (c, s, o) => targetAlpha = (int)ButtonMouseState.Over / 100.0f;
                MouseDown += (c, s, o) => targetAlpha = (int)ButtonMouseState.Down / 100.0f;

                TextChanged += ChangedText;

                LocationChanged += UpdateBack;
                SizeChanged += UpdateBack;

                EnabledChanged += (c) =>
                {
                    if (c.Enabled)
                    {
                        m_BackColor = BackRenderColor;
                        m_ForeColor = FontRenderColor;
                    }
                    else
                    {
                        FontRenderColor = m_ForeColor;
                        BackRenderColor = m_BackColor;

                        m_BackColor = Color.DarkGray;
                        m_ForeColor = Color.DarkGray;
                    }
                };
            }

            /// <summary>
            /// Initializes the 2D rendering capability
            /// </summary>
            /// <param name="factory">factory for creating DirectWrite Objects</param>
            /// <param name="factory2D">factory for creating Direct2D Objects</param>
            /// <param name="renderTarget">Render Target for 2D rendering</param>
            public override void Init(DW.Factory factory, Factory factory2D, RenderTarget renderTarget)
            {
                base.Init(factory, factory2D, renderTarget);


                m_textFormat.TextAlignment = (DW.TextAlignment)m_textHAllignment;
                m_textFormat.ParagraphAlignment = (DW.ParagraphAlignment)m_textVAllignment;
            }

            /// <summary>
            /// Renders the Label
            /// </summary>
            /// <param name="renderTarget">Render Target for 2D rendering</param>
            public override void Render(RenderTarget renderTarget)
            {
                base.Render(renderTarget);

                if (!Visible) return;

                m_brush.Color = new Color4(baseColor) { Alpha = alpha };
                renderTarget.FillRoundedRectangle(m_brush, rectRound);


                if (!m_TextEnabled)
                {
                    m_brush.Color = ForeColor;
                    renderTarget.DrawText(m_Text, m_textFormat, Bound, m_brush);
                }

                m_brush.Color = BorderColor;
                renderTarget.DrawRoundedRectangle(m_brush, rectRound, BorderWidth);

                if (!m_BorderEnabled)
                {
                    if (ParentHOD != null)
                        if (ParentHOD.ActiveControl == this && ParentHOD.HighlightSelectedControl)
                        {
                            if (m_MouseOver)
                                m_brush.Color = new Color4(m_BorderColor) { Alpha = 0.7f };
                            else
                                m_brush.Color = new Color4(m_BorderColor) { Alpha = 0.6f };

                            renderTarget.DrawRoundedRectangle(m_brush, rectRound, BorderWidth);
                        }
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

                if (!Enabled) return;

                float diff = targetAlpha - alpha;
                int fps = ParentHOD.FramePerSecond;

                step = (float) (transitionTime * ((fps == 0) ? 1 / frameTime : fps)) / 5.0f;

                alpha += diff / step;                


                HOD hod = m_parentHOD;
                UserInput input = hod.UserInput;


                if (input.IsKeyJustPressed(Key.Space) ||
                        input.IsKeyJustPressed(Key.NumberPadEnter) ||
                        input.IsKeyJustPressed(Key.Return))
                {
                    PerformClick();
                }


                if (input.IsKeyPressed(Key.Space) ||
                    input.IsKeyPressed(Key.NumberPadEnter) ||
                    input.IsKeyPressed(Key.Return))
                {
                    if (hod.ActiveControl == this)
                    {
                        targetAlpha = (int)ButtonMouseState.Down / 100.0f;
                    }
                }
                else
                {
                    if (m_MouseOver)
                        if (m_MouseDown)
                            targetAlpha = (int)ButtonMouseState.Down / 100.0f;
                        else
                            targetAlpha = (int)ButtonMouseState.Over / 100.0f;
                    else
                        targetAlpha = (int)ButtonMouseState.Out / 100.0f;
                }

            }

            /// <summary>
            /// Invokes Click event of the Button
            /// </summary>
            public void PerformClick()
            {
                PerformClick(this);
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
