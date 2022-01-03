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

namespace SlimFrame.UI
{
    /// <summary>
    /// Button : Performs action when clicked
    /// </summary>
    public class DXButton : DXControl, ITextControl
    {

        #region Member Variables

        //Text allignment support.
        protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
        protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

        //Auto size support
        private Color FontRenderColor;
        private Color BackRenderColor;

        protected bool m_AutoSize = false; // Stores auto size state of button

        #endregion

        #region Properties

        ///// <summary>
        ///// BackColor of the Control
        ///// </summary>
        //public override Color BackColor
        //{
        //    get
        //    {
        //        return m_BackColor;
        //    }
        //    set
        //    {
        //        m_BackColor = value;
        //    }
        //}

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

        public DXButton()
            : base()
        {
            Init();
        }

        public DXButton(string name)
            : base()
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

            m_BackEnabled = false;
            m_BorderEnabled = false;
            m_BorderWidth = 1.0f;

            TabStop = true;


            TextChanged += ChangedText;
            EnabledChanged += Button_Enabled;

            TextColorChanged += Button_TextColorChanged;
            BackColorChanged += Button_BackColorChanged;
        }

        private void Button_BackColorChanged(DXControl obj)
        {
            if (Enabled)
                BackRenderColor = BackColor;
        }

        private void Button_TextColorChanged(DXControl obj)
        {
            if (Enabled)
                FontRenderColor = TextColor;
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
        /// Renders the Button
        /// </summary>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Render(RenderTarget renderTarget)
        {
            base.Render(renderTarget);

            if (!Visible) return;

            renderTarget.PushAxisAlignedClip(Bound, AntialiasMode.Aliased);
            ControlImage.DrawImage(renderTarget, ControlImage.ButtonBase, ClientRectangle, CalculatedAlpha);

            m_brush.Color = TextColor;
            renderTarget.DrawText(m_Text, m_textFormat, Bound, m_brush);

            renderTarget.PopAxisAlignedClip();
        }

        /// <summary>
        /// Updates the Button
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            if (!Visible) return;

            if (!Enabled) return;


            HUD HUD = m_parentHOD;
            UserInput input = HUD.UserInput;


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
                if (m_Focused)
                {
                    targetAlpha = (int)MouseEffectState.Down;
                }
            }
            else
            {
                if (m_MouseOver)
                    if (m_MouseDown)
                        targetAlpha = (int)MouseEffectState.Down;
                    else
                        targetAlpha = (int)MouseEffectState.Over;
                else
                    targetAlpha = (int)MouseEffectState.Out;
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

        #region Event Handler


        /// <summary>
        /// Updates the BackSide of Button
        /// </summary>
        /// <param name="c"></param>
        private void Button_Enabled(DXControl c)
        {
            if (Enabled)
            {
                BackColor = BackRenderColor;
                TextColor = FontRenderColor;
            }
            else
            {
                BackColor = Color.DimGray;
                TextColor = Color.DarkGray;
            }
        }

        #endregion

        #region Disposal
        /// <summary>
        /// Disposes the Button
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

    }

}