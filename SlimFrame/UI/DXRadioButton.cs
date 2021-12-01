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

namespace SlimFrame.UI
{
    /// <summary>
    /// RadioButton : Only one will be checked on a ContainerControl
    /// </summary>
    class DXRadioButton : DXControl, ITextControl
    {

        #region Member Variables

        //Text allignment support.
        protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
        protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

        protected bool m_AutoSize = false; // Stores auto size state of label

        private float m_width = 25;
        private float m_height = 25;

        private RectangleF radioBase;

        private Color m_CheckBaseColor = Color.White;
        private Color m_CheckBaseBorderColor = Color.DimGray;
        //private float m_CheckBorderWidth = 2.0f;

        private float xOffset = 0.0f;    // Stores y-axis offset from the text bundaries.
        private float yOffset = 0.0f;    // Stores y-axis offset from the text bundaries.

        //private float factor = 0.5f;

        private bool m_Checked = false;   // Determines the checked state of the RadioButton

        private Color FontRenderColor;
        private RectangleF m_TextBoundary;

        #endregion

        #region Properties            

        /// <summary>
        /// Determines the Checked state of RadioButton
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
        /// Occures when the Check state of RadioButton is changed
        /// </summary>
        public event Action<DXRadioButton> CheckChanged;

        #endregion

        #region Constructors

        public DXRadioButton()
            : base()
        {
            Init();
        }
        public DXRadioButton(string name)
            : base()
        {
            Name = name;

            Init();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the RadioButton
        /// </summary>
        private void Init()
        {
            m_TextBoundary = new RectangleF();

            radioBase = new RectangleF();
            radioBase.Width = m_width;
            radioBase.Height = m_height;

            m_BorderWidth = 1.0f;

            m_dTextMeasuredSize = false;
            m_BorderEnabled = false;

            TabStop = true;
            AutoSize = true;

            SizeChanged += UpdateCheck;
            LocationChanged += UpdateCheck;
            TextChanged += RadioButton_TextChanged;
            CheckChanged += RadioButton_CheckChanged;
            EnabledChanged += RadioButton_EnabledChanged;

            MouseOver += RadioButton_MouseOver;
            MouseOut += RadioButton_MouseOut;

            Click += RadioButton_Click;

            TextColorChanged += RadioButton_TextColorChanged;

            UpdateCheck(this);
        }

        private void RadioButton_TextColorChanged(DXControl obj)
        {
            FontRenderColor = m_TextColor;
        }

        private void RadioButton_Click(DXControl ctrl)
        {
            Checked = true;
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

            //CheckChanged?.Invoke(this);
        }

        /// <summary>
        /// Renders the Label
        /// </summary>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Render(D2D.RenderTarget renderTarget)
        {
            base.Render(renderTarget);

            if (!Visible) return;

            renderTarget.PushAxisAlignedClip(Bound, AntialiasMode.Aliased);

            m_brush.Color = TextColor;
            renderTarget.DrawText(m_Text, m_textFormat, m_TextBoundary, m_brush);

            ControlImage.DrawImage(renderTarget, ControlImage.RadioButtonBase, radioBase, CalculatedAlpha);

            if (m_Checked)
            {
                ControlImage.DrawImage(renderTarget, ControlImage.RadioButtonCheck, radioBase, CalculatedAlpha);
            }

            renderTarget.PopAxisAlignedClip();
        }

        /// <summary>
        /// Updates the Label
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            HUD hod = m_parentHOD;
            UserInput input = hod.UserInput;

            if (!Visible) return;

            if (!Enabled) return;


            if (m_Focused)
                if (input.IsKeyJustPressed(Key.Space) ||
                        input.IsKeyJustPressed(Key.NumberPadEnter) ||
                        input.IsKeyJustPressed(Key.Return))
                {
                    PerformClick(this);
                }
        }

        #endregion

        #region Event Handler

        private void RadioButton_CheckChanged(DXRadioButton r)
        {
            if (Checked)
            {
                var radios = ((DXContainerControl)Parent).Controls.FindAll(x => x.GetType() == typeof(DXRadioButton) && x != this);
                foreach (var item in radios)
                    ((DXRadioButton)item).Checked = false;
            }
        }

        private void RadioButton_EnabledChanged(DXControl c)
        {
            if (Enabled)
            {
                m_CheckBaseColor = Color.White;
                m_TextColor = FontRenderColor;
            }
            else
            {
                m_CheckBaseColor = Color.DarkGray;
                m_CheckBaseBorderColor = Color.DimGray;

                m_TextColor = Color.DarkGray;

                RadioButton_CheckChanged(null);
            }
        }

        private void RadioButton_TextChanged(DXControl c)
        {
            ChangedText(this);

            ClientSize = m_TextSize;
        }


        private void UpdateCheck(DXControl c)
        {
            DXRadioButton ch = (DXRadioButton)c;

            xOffset = m_width + 2.0f;

            float yOff = Top;
            float xOff = Left;

            if (m_TextSize.Height > m_height)
                yOffset = (m_TextSize.Height - m_height / 2) / 2.0f;

            radioBase.X = xOff;
            radioBase.Y = yOff;

            m_TextBoundary.X = Left + xOffset;
            m_TextBoundary.Y = Top;
            m_TextBoundary.Size = ClientSize;
        }

        private void RadioButton_MouseOut(DXControl ctrl)
        {
            if (Enabled)
                m_CheckBaseBorderColor = Color.DimGray;
        }

        private void RadioButton_MouseOver(DXControl ctrl)
        {
            if (Enabled)
                m_CheckBaseBorderColor = Color.RoyalBlue;
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

                SizeChanged -= UpdateCheck;
                LocationChanged -= UpdateCheck;
                TextChanged -= RadioButton_TextChanged;
                CheckChanged -= RadioButton_CheckChanged;
                EnabledChanged -= RadioButton_EnabledChanged;

                MouseOver -= RadioButton_MouseOver;
                MouseOut -= RadioButton_MouseOut;

                Click -= RadioButton_Click;

                TextColorChanged -= RadioButton_TextColorChanged;

            }

            base.Dispose(disposing);
        }

        #endregion

    }
}