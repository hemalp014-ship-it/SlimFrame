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
using SlimDX.D3DCompiler;

using static System.Math;
using static Microsoft.VisualBasic.Devices.Mouse;

namespace SlimFrame.UI
{

    public enum DXSliderType
    {
        NonInitiated,   // Slider is not initiated
        Linear,         // Value increases linearly with the sliding of the pointer in right direction
        ExponentialOf10,// Value increases as an exponent of 10 with the sliding of the pointer in right direction
        ExponentialOfe, // Value increases as an exponent of e with the sliding of the pointer in right direction
    }
    /// <summary>
    /// Slider : Visually reprensenting / changing value
    /// </summary>
    public class DXSlider : DXControl, ITextControl, IEnabler
    {

        #region Member Variables

        //Type of the Slider
        private DXSliderType m_sliderType = DXSliderType.Linear;

        //Text allignment support.
        protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
        protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

        protected bool m_AutoSize = false; // Stores auto size state of label

        private float m_Value;
        private string m_DisplayText;

        private float m_Minimum;
        private float m_Maximum;

        private float m_width = 20;
        private float m_height = 20;

        private float m_IncrementFactor = 1.0f;
        private float m_smallIncrementFactor = 0.1f;

        private bool m_useStaticPoniter = true;
        private bool m_TrackPoniter = false;

        private RectangleF rectRoundBack; // Slider background rounded rectangle
        private RectangleF rectRound;     // Pointer rectangle

        private float reduceFactor = 0;    // Stores factor which determines the portion of the height of Slider pointer to cut from Slider bar
        private float yOffset = 0.0f;    // Stores y-axis offset from the text bundaries.

        private Color FontRenderColor;

        private string m_Unit;

        private float WheelScrollFactor;
        #endregion

        #region Properties

        public DXSliderType Type 
        {
            get { return m_sliderType; }
            set
            {
                var val = Value;
                m_sliderType = value;
                m_Value = Transform(val);

                setDisplayText();
            }
        }

        /// <summary>
        /// Unit of Value of the Slider 
        /// </summary>
        public string Unit
        {
            get { return m_Unit; }
            set
            {
                m_Unit = value;
                setDisplayText();
            }
        }

        /// <summary>
        /// Value of slider
        /// </summary>
        public float Value
        {
            get{return InverseTransform(m_Value);}
            set
            {
                var temp = Min(Max(Transform(value), m_Minimum), m_Maximum);

                if (temp != m_Value)
                {
                    var old = m_Value;
                    m_Value = temp;
                    ValueChanged?.Invoke(this, old);

                    setDisplayText();

                }
            }
        }

        /// <summary>
        /// No. of digits to round off
        /// </summary>
        public int RoundingDigit { get; set; } = 2;

        /// <summary>
        /// Maximum value of slider
        /// </summary>
        public float Maximum
        {
            get{return InverseTransform(m_Maximum); }
            set
            {
                m_Maximum = Transform(value);
                MaximumChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Minimum value of slider
        /// </summary>
        public float Minimum
        {
            get{return InverseTransform(m_Minimum);}
            set
            {
                m_Minimum = Transform(value);
                MinimumChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Increment factor for value of scroller (per second)
        /// </summary>
        public float IncrementFactor
        {
            get { return m_IncrementFactor; }
            set
            {
                m_IncrementFactor = value;
                IncrementChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Small Increment factor for value of scroller (per second)
        /// </summary>
        public float smallIncrementFactor
        {
            get { return m_smallIncrementFactor; }
            set
            {
                m_smallIncrementFactor = value;
                IncrementChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Determines whether the size of control to be adjusted according the content or not.
        /// </summary>
        public bool AutoSize
        {
            get{return m_AutoSize;}
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
            get{return new SizeF(m_Size.Width, m_Size.Height - m_height - 2.0f + yOffset);}
            set{Size = new SizeF(value.Width, value.Height + m_height + 2.0f + yOffset);}
        }

        /// <summary>
        /// Determines Horizontal Allignment of text
        /// </summary>
        public TextHAllignment HorizontalAllignment
        {
            get{return m_textHAllignment;}
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
            get{return m_textVAllignment;}
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
        public event Action<DXSlider, float> ValueChanged;

        /// <summary>
        /// Occures when the maximum value of Slider is changed
        /// </summary>
        public event Action<DXSlider> MaximumChanged;

        /// <summary>
        /// Occures when the minimum value of Slider is changed
        /// </summary>
        public event Action<DXSlider> MinimumChanged;

        /// <summary>
        /// Occures when the value of Slider is changed
        /// </summary>
        public event Action<DXSlider> IncrementChanged;

        #endregion

        #region Constructors

        public DXSlider()
            : base()
        {
            Init();
        }
        public DXSlider(string name)
            : base()
        {
            Name = name;

            Init();
        }

        #endregion

        #region Methods

        private float Transform(float value)
        {
            var o = 0.0f;

            switch (Type)
            {
                case DXSliderType.Linear:
                    o = value;
                    break;
                case DXSliderType.ExponentialOf10:
                    o = (float) Log10(value);
                    break;
                case DXSliderType.ExponentialOfe:
                    o = (float) Log(value);
                    break;
                default:
                    break;
            }

            return o;
        }

        private float InverseTransform(float value)
        {
            var o = 0.0f;

            switch (Type)
            {
                case DXSliderType.Linear:
                    o = value;
                    break;
                case DXSliderType.ExponentialOf10:
                    o = (float)Pow(10, value);
                    break;
                case DXSliderType.ExponentialOfe:
                    o = (float)Pow(E, value);
                    break;
                default:
                    break;
            }

            return o;
        }

        private void setDisplayText()
        {
            m_DisplayText = m_Text + " : " + (int)(Value * Pow(10, RoundingDigit)) / Pow(10, RoundingDigit) + " " + Unit;

        }

        /// <summary>
        /// Initializes the Slider
        /// </summary>
        private void Init()
        {

            var g = new Microsoft.VisualBasic.Devices.Mouse();
            try
            {
                WheelScrollFactor = g.WheelScrollLines;
            }
            catch (InvalidOperationException e)
            {
                if(e.Message.Contains("no") && e.Message.Contains("wheel"))
                {
                    WheelScrollFactor = 1;
                }
            }
        
            rectRound = new RectangleF();
            rectRoundBack = new RectangleF();

            m_BorderWidth = 0.0f;

            m_BackEnabled = true;
            m_BorderEnabled = true;
            TabStop = true;

            setDisplayText();

            MouseOver += Slider_MouseOver;

            MouseUp += Slider_MouseUp;
            MouseMove += Slider_MouseMove;

            SizeChanged += UpdateBack;
            LocationChanged += UpdateBack;

            EnabledChanged += Slider_EnabledChanged;
            TextColorChanged += Slider_TextColorChanged;
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

            renderTarget.PushAxisAlignedClip(Bound, AntialiasMode.Aliased);

            m_brush.Color = TextColor;
            renderTarget.DrawText(m_DisplayText, m_textFormat, Bound, m_brush);

            ControlImage.DrawImage(renderTarget, ControlImage.SliderBase, rectRoundBack, CalculatedAlpha);

            ControlImage.DrawImage(renderTarget, ControlImage.SliderPointer, rectRound);

            renderTarget.PopAxisAlignedClip();
        }

        /// <summary>
        /// Updates the Label
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            if (!Visible) return;

            HUD hod = m_parentHOD;
            UserInput input = hod.UserInput;
            if (input.InputPaused) return;

            float x, y;

            float apperantWidth = (ClientSize.Width - m_width);
            float valueWidth = (m_Maximum - m_Minimum);

            float reduceFactor = (-m_Minimum + m_Value) / valueWidth;

            float oldValue = m_Value;

            x = reduceFactor * apperantWidth; //Use Max to set minimum value

            y = 2 * yOffset + m_font.Height;

            rectRound.X = m_Location.X + x;
            rectRound.Y = m_Location.Y + y;

            rectRound.Width = m_width;
            rectRound.Height = m_height;

            if (!Enabled) return;

            if (input.IsMouseButtonPressed(MouseButtons.Left))
            {
                float value = m_Minimum + (ParentHOD.MousePosition.X - m_Location.X - m_width / 2) / apperantWidth * valueWidth;

                if (hod.MousePosition.Y - m_Location.Y > ClientSize.Height)
                    if (m_MouseOver && m_MouseDown)
                    {
                        if (m_useStaticPoniter)
                        {
                            float increment = valueWidth * m_IncrementFactor * (float)frameTime;
                            if (m_Value + increment / 2 < value)
                            {
                                m_Value += increment;
                            }
                            else if (m_Value - increment / 2 > value)
                            {
                                m_Value -= increment;
                            }
                        }

                        m_TrackPoniter = true;
                    }

                if (m_TrackPoniter && m_MouseOver && m_MouseDown)
                {
                    m_Value += (hod.MousePosition.X - hod.MousePositionOld.X) / apperantWidth * valueWidth;
                }
            }

            if (m_MouseOver && Abs(input.MouseWheelMovement()) > 0)
            {

                m_Value -= valueWidth * m_IncrementFactor / 10.0f / WheelScrollFactor * input.MouseWheelMovement() * (float)frameTime;
            }

            if (m_Focused)
            {
                if (input.IsKeyPressed(Key.UpArrow))
                {
                    m_Value += valueWidth * m_smallIncrementFactor * (float) frameTime;
                }
                if (input.IsKeyPressed(Key.DownArrow))
                {
                    m_Value -= valueWidth * m_smallIncrementFactor * (float) frameTime;
                }
                if (input.IsKeyPressed(Key.RightArrow))
                {
                    m_Value += valueWidth * m_IncrementFactor * (float) frameTime;
                }
                if (input.IsKeyPressed(Key.LeftArrow))
                {
                    m_Value -= valueWidth * m_IncrementFactor * (float) frameTime;
                }
            }

            if (oldValue!= m_Value)
            {
                m_Value = Max(Min(m_Value, m_Maximum), m_Minimum);
                setDisplayText();
                ValueChanged?.Invoke(this, oldValue);
            }
            else
            {
                if (oldValue > m_Maximum | oldValue < m_Minimum)
                {
                    m_Value = Max(Min(m_Value, m_Maximum), m_Minimum);
                    setDisplayText();
                }
            }
        }

        #endregion

        #region Event Handler

        private void UpdateBack(DXControl c)
        {

            rectRoundBack.X = m_Location.X;
            rectRoundBack.Y = m_Location.Y + ClientSize.Height + m_height / 2 * reduceFactor;

            rectRoundBack.Width = m_Size.Width;
            rectRoundBack.Height = m_height * (1 - reduceFactor);
        }

        private void Slider_MouseUp(DXControl ctrl, MouseState state, MouseState stateOld)
        {
            m_useStaticPoniter = true;
            m_TrackPoniter = false;
        }

        private void Slider_EnabledChanged(DXControl obj)
        {
            if (Enabled)
            {
                TextColor = FontRenderColor;
            }
            else
            {
                TextColor = Color.DarkGray;
            }
        }

        private void Slider_MouseMove(DXControl ctrl, MouseState state, MouseState stateOld)
        {
            if (m_MouseDown)
                m_useStaticPoniter = false;
        }

        private void Slider_MouseOver(DXControl ctrl)
        {
            HUD hod = m_parentHOD;
            UserInput input = hod.UserInput;
            float apperantWidth = (ClientSize.Width - m_width);
            float valueWidth = (m_Maximum - m_Minimum);

            if (Abs(input.MouseWheelMovement()) > 0)
            {
                m_Value -= input.MouseWheelMovement() * 100.0f / apperantWidth * valueWidth / hod.FramePerSecond;
            }
        }

        private void Slider_TextColorChanged(DXControl c)
        {
            if (Enabled)
                FontRenderColor = TextColor;
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
                MouseOver -= Slider_MouseOver;

                MouseMove -= Slider_MouseMove;
                MouseUp -= Slider_MouseUp;

                LocationChanged -= UpdateBack;
                SizeChanged -= UpdateBack;

                EnabledChanged -= Slider_EnabledChanged;
                TextColorChanged -= Slider_TextColorChanged;
            }
            base.Dispose(disposing);
        }

        #endregion

    }
}