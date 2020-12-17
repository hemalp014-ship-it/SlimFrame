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
    /// ComboBox : Performs action when clicked
    /// </summary>
    public class DXComboBox : DXControl, ITextControl
    {
        private const float DefaultHeight = 180.0f;

        #region Member Variables

        //Text allignment support.
        protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
        protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

        //Auto size support
        protected bool m_AutoSize = false; // Stores auto size state of button

        private float m_width = 20;
        private float m_height = 20;

        private Color4 FontRenderColor;
        private Color BackRenderColor;

        private List<String> m_Items;

        private int m_SelectedIndex = -1;
        private int m_OverIndex = -1;
        private bool m_Opened = false;

        private float m_DropDownHeight = DefaultHeight;
        private float m_cDropDownHeight = 0.0f;
        private float m_ItemsHeight = 0.0f;
        private float m_Target = 0.0f;

        private DXScroller vScroll;

        private float m_TextOffset = 2;
        private float m_ItemsOffset = 0;

        private int renderMin = 0;
        private int renderMax = 0;

        private int oldCount = 0;
        private RectangleF rect;
        private RectangleF Body;

        private RectangleF m_IndicatorBoundary;
        private RectangleF m_TextBoundary;

        #endregion

        #region Event System

        /// <summary>
        /// Occures when the Selected Item is changed
        /// </summary>
        public event Action<DXComboBox, int> SelectedIndexChanged;

        #endregion

        #region Properties

        private bool Opened
        {
            get
            {
                return m_Opened;
            }
            set
            {
                if (m_Opened != value)
                {

                    if (!value)
                    {
                        m_Target = 0.0f;
                    }
                    else
                    {
                        m_Target = m_DropDownHeight;

                        m_OverIndex = SelectedIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Indicates the index of Selected Item
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return m_SelectedIndex;
            }
            set
            {
                if (value > -1 && value < Items.Count)
                    if (m_SelectedIndex != value)
                    {
                        var old = m_SelectedIndex;
                        m_SelectedIndex = value;
                        m_Text = Items[value];
                        SelectedIndexChanged?.Invoke(this, old);
                    }
            }
        }

        /// <summary>
        /// Size of client area of slider
        /// </summary>
        public SizeF ClientSize
        {
            get
            {
                return new SizeF(m_Size.Width - m_IndicatorBoundary.Width - 10, Math.Max(m_Size.Height, m_height));
            }
            set
            {
                Size = new SizeF(value.Width + m_IndicatorBoundary.Width + 10, Math.Max(value.Height, m_height));
            }
        }
        /// <summary>
        /// Items Collection for GroupBox
        /// </summary>
        public List<String> Items
        {
            get
            {
                return m_Items;
            }
            set
            {
                m_Items = value;
            }
        }

        /// <summary>
        /// Height of DropDown
        /// </summary>
        public float DropDownHeight
        {
            get
            {
                return m_DropDownHeight;
            }
            set
            {
                if (m_DropDownHeight != value)
                {
                    m_DropDownHeight = value;

                    vScroll.Height = m_DropDownHeight - 2;
                }
            }
        }

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

        public DXComboBox()
        {
            Init();
        }

        public DXComboBox(string name)
        {
            Name = name;

            Init();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the ComboBox
        /// </summary>
        private void Init()
        {
            rect = new RectangleF();

            m_IndicatorBoundary = new RectangleF();
            m_TextBoundary = new RectangleF();
            m_UseDefaultAplhaAnimation = false;
            m_dTextMeasuredSize = false;

            m_Items = new List<string>();

            m_IndicatorBoundary.Width = m_width;
            m_IndicatorBoundary.Height = m_height;

            vScroll = new DXScroller(ScrollOrientation.Vertical)
            {
                BackColor = Color.White,
                Location = new PointF(Width, 0),
                Size = new SizeF(25, 150),
                TabStop = false,
                Maximum = 1.0f,
                Increment = 0.9f,
                Value = 0.0f
            };

            alpha = (int)MouseEffectState.Out;
            targetAlpha = alpha;

            m_BackEnabled = false;
            m_BorderEnabled = false;
            m_BorderWidth = 1.0f;

            TabStop = true;
            m_Text = "";

            MouseOver += ComboBox_MouseOver;
            MouseOut += ComboBox_MouseOut;

            MouseMove += ComboBox_MouseMove;

            MouseUp += ComboBox_MouseUp;
            MouseDown += ComboBox_MouseDown;

            SizeChanged += UpdateBack;
            SizeChanged += ComboBox_SizeChanged;
            TextChanged += ComboBox_TextChanged;
            LocationChanged += UpdateBack;
            EnabledChanged += ComboBox_Enabled;

            ForeColorChanged += ComboBox_ForeColorChanged;
            BackColorChanged += ComboBox_BackColorChanged;

            LostFocus += ComboBox_LostFocus;

            Click += ComboBox_Click;
            Loaded += ComboBox_Loaded;

            vScroll.ValueChanged += VScroll_ValueChanged;
            FontChanged += ComboBoxFontChnaged;
            //BoundChanged += ComboBox_BoundChanged;
        }

        protected override void BoundPad(float pad)
        {
            m_Bound.X = Left - pad;
            m_Bound.Y = Top - pad;
            m_Bound.Width = Width + 2 * pad;
            m_Bound.Height = Height + m_cDropDownHeight + 2 * pad;
        }
        //protected override void ClientRect()
        //{
        //    m_Bound.X = Left;
        //    m_Bound.Y = Top;
        //    m_Bound.Width = Width;
        //    m_Bound.Height = Height + m_cDropDownHeight;             
        //}

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

            Associate(this, vScroll);
            vScroll.Init(factory, factory2D, renderTarget);

            SelectedIndexChanged?.Invoke(this, -1);
        }

        /// <summary>
        /// Renders the ComboBox
        /// </summary>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Render(RenderTarget renderTarget)
        {
            base.Render(renderTarget);

            if (!Visible) return;

            Body.Height = Height + 2 * m_Padding;
            Body.Width = Width + m_Padding;
            Body.X = Left;
            Body.Y = Top - m_Padding;

            renderTarget.PushAxisAlignedClip(Body, AntialiasMode.Aliased);

            ControlImage.DrawImage(renderTarget, ControlImage.ButtonBase, m_TextBoundary, CalculatedAlpha);
            ControlImage.DrawImage(renderTarget, Opened ? ControlImage.ComboBoxUp : ControlImage.ComboBoxDown, m_IndicatorBoundary, CalculatedAlpha);


            m_brush.Color = FontRenderColor;
            renderTarget.DrawText(m_Text, m_textFormat, m_TextBoundary, m_brush);

            renderTarget.PopAxisAlignedClip();

            DrawItems(renderTarget);
        }

        private void DrawItems(RenderTarget renderTarget)
        {

            //if (Opened)
            //{
                Body.Height = m_cDropDownHeight;
                Body.Width = Width;
                Body.Y = Top + Height + 1;
                Body.X = Left + 3;

                renderTarget.PushAxisAlignedClip(Body, AntialiasMode.Aliased);

                Body.Width = Width - 5;

                m_brush.Color = Color.White;
                renderTarget.FillRectangle(m_brush, Body);
                m_brush.Color = BorderColor;
                renderTarget.DrawRectangle(m_brush, Body);

                m_brush.Color = ForeColor;
                for (int i = renderMin; i < renderMax; i++)
                {
                    rect.X = Left + 3;
                    rect.Y = Top + Height + i * (Font.Height + m_TextOffset) + m_ItemsOffset;

                    if (i == m_OverIndex)
                    {
                        m_brush.Color = BackColor;
                        renderTarget.FillRectangle(m_brush, rect);
                    }

                    m_brush.Color = ForeColor;
                    renderTarget.DrawText(Items[i], m_textFormat, rect, m_brush);
                }
                vScroll.Render(renderTarget);

                renderTarget.PopAxisAlignedClip();

            //}
        }

        /// <summary>
        /// Updates the ComboBox
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            if (!Visible) return;

            if (!Enabled) return;

            if (oldCount != Items.Count)
            {
                m_ItemsHeight = rect.Height * Items.Count;


                if (DropDownHeight < m_ItemsHeight)
                {
                    vScroll.Enabled = true;
                    vScroll.Maximum = m_ItemsHeight / DropDownHeight - 1.0f;
                    vScroll.Increment = Math.Min(vScroll.Maximum * .9f, 1);
                    vScroll.Visible = true;
                }
                else
                {
                    vScroll.Enabled = false;
                    vScroll.Maximum = 1.0f;
                    vScroll.Value = 0.0f;
                    vScroll.Increment = 1.0f;
                    DropDownHeight = m_ItemsHeight;
                }

                CullItems();
            }

            oldCount = Items.Count;

            float diff;
            diff = m_Target - m_cDropDownHeight;

            if (Math.Abs(diff) > 0.3f)
            {
                m_cDropDownHeight += diff / m_AnimationStep * 3;
                Body.Height = m_cDropDownHeight + 2 * m_Padding;
                m_Bound.Height = m_cDropDownHeight + Height + m_Padding;
            }
            else
            {
                m_Opened = m_Target != 0.0f;
            }

            HUD hod = m_parentHOD;
            UserInput input = hod.UserInput;

            if (m_Focused)
            {
                if (input.IsKeyJustPressed(Key.Space) ||
                    input.IsKeyJustPressed(Key.NumberPadEnter) ||
                    input.IsKeyJustPressed(Key.Return))
                {
                    if (input.IsKeyJustPressed(Key.Return))
                    {
                        if (input.IsKeyReleased(Key.LeftAlt) && input.IsKeyReleased(Key.RightAlt))
                        {
                            PerformClick(this);
                        }
                    }
                    else
                        PerformClick(this);
                }
                else
                {
                    if (ParentHOD.MousePosition.Y - Top < Height)
                    {
                        if (m_MouseOver)
                            if (m_MouseDown)
                                targetAlpha = (int)MouseEffectState.Down;
                            else
                                targetAlpha = (int)MouseEffectState.Over;
                        else
                        {
                            if (Opened)
                            {
                                targetAlpha = (int)MouseEffectState.Over;
                            }
                            else
                            {
                                targetAlpha = (int)MouseEffectState.Out;
                            }
                        }
                    }
                    else
                    {
                        if (Opened)
                        {
                            targetAlpha = (int)MouseEffectState.Over;
                        }
                        else
                        {
                            targetAlpha = (int)MouseEffectState.Out;
                        }
                    }

                }


                if (input.IsKeyJustPressed(Key.DownArrow))
                {
                    if (m_OverIndex < Items.Count - 1)
                    {
                        m_OverIndex += 1;
                    }
                    else
                    {
                        m_OverIndex = 0;
                        vScroll.Value = 0.0f;
                    }

                    if (m_OverIndex > renderMax - 1)
                    {
                        float y = (renderMax) * rect.Height + m_ItemsOffset - Body.Height + Height;
                        vScroll.Value += (y) / DropDownHeight;
                    }
                }
                if (input.IsKeyJustPressed(Key.UpArrow))
                {
                    if (m_OverIndex > 0)
                    {
                        m_OverIndex -= 1;

                        if (m_OverIndex < renderMin)
                        {
                            float y = renderMin * rect.Height + m_ItemsOffset - Height;
                            vScroll.Value += (y) / DropDownHeight;
                        }
                    }
                    else
                    {
                        m_OverIndex = Items.Count - 1;
                        vScroll.Value = vScroll.Maximum;
                    }

                }
            }

            //if (m_MouseOver)
            vScroll.MouseWheel(frameTime, input);
            vScroll.Update(frameTime, input);
        }

        public void CullItems()
        {
            if (rect.Height > 0.0f)
            {
                float min, max;

                min = -(m_ItemsOffset / rect.Height);

                float off = m_DropDownHeight - m_ItemsOffset + rect.Height;

                max = Math.Min(off / rect.Height, Items.Count);

                renderMin = (int)min;
                renderMax = (int)max;
            }
        }

        #endregion

        #region Event Handler

        private void ComboBox_SizeChanged(DXControl c)
        {
            m_Size.Height = Math.Max(m_height, m_Size.Height);
        }

        private void VScroll_ValueChanged(DXScroller s, float arg2)
        {
            m_ItemsOffset = -s.Value * DropDownHeight;

            CullItems();
        }

        /// <summary>
        /// Updates the BackSide of ComboBox
        /// </summary>
        /// <param name="c"></param>
        void UpdateBack(DXControl c)
        {
            if (c != this) return;

            rect.Width = m_Size.Width - m_width - 2;
            Body.Width = m_Size.Width + 2 * m_Padding;

            Body.X = m_Location.X - m_Padding;
            Body.Y = m_Location.Y + Height - m_Padding;

            m_TextBoundary.X = m_Location.X + 1;
            m_TextBoundary.Y = m_Location.Y;
            m_TextBoundary.Width = rect.Width;
            m_TextBoundary.Height = m_Size.Height;

            m_IndicatorBoundary.X = m_TextBoundary.Right + 1;
            m_IndicatorBoundary.Y = m_TextBoundary.Top;

            vScroll.Left = m_IndicatorBoundary.X + m_IndicatorBoundary.Width / 2 - vScroll.Width / 2 - 2;
        }

        /// <summary>
        /// Updates the BackSide of ComboBox
        /// </summary>
        /// <param name="c"></param>
        private void ComboBox_Enabled(DXControl c)
        {
            if (Enabled)
            {
                BackColor = BackRenderColor;
                ForeColor = FontRenderColor.ToColor();
            }
            else
            {
                BackColor = Color.DimGray;
                ForeColor = Color.DarkGray;
            }
        }

        private void ComboBox_TextChanged(DXControl c)
        {
            ChangedText(this);

            ClientSize = m_TextSize;
        }

        private void ComboBox_Loaded(DXControl obj)
        {

            vScroll.Top = Height + 2;
            vScroll.Right = Width;
            vScroll.Height = m_DropDownHeight - 2;

            vScroll.Enabled = false;
            vScroll.Maximum = 1.0f;
            vScroll.Value = 0.0f;
            vScroll.Increment = 1.0f;

            vScroll.Left = Width - m_IndicatorBoundary.Width / 2 - vScroll.Width / 2 - 2;// + m_IndicatorBoundary.Width / 2 - vScroll.Width / 2;
        }

        private void ComboBox_MouseDown(DXControl ctrl, MouseState state, MouseState stateOld)
        {
            targetAlpha = (int)MouseEffectState.Down;
        }

        private void ComboBox_MouseUp(DXControl ctrl, MouseState state, MouseState stateOld)
        {
            float x = ParentHOD.MousePosition.X - vScroll.Left;

            if (x < 0)
                Opened = false;

            targetAlpha = (int)MouseEffectState.Over;
        }

        private void ComboBox_MouseOut(DXControl ctrl)
        {
            targetAlpha = (int)MouseEffectState.Out;
        }

        private void ComboBox_MouseOver(DXControl ctrl)
        {
            targetAlpha = (int)MouseEffectState.Over;
        }

        private void ComboBox_Click(DXControl ctrl)
        {
            BringToFront();

            if (ctrl != null)
            {
                if (ParentHOD.MousePosition.Y - Top < Height)
                    Opened = !Opened;

                if (m_OverIndex > -1 && ParentHOD.MousePosition.X < vScroll.Left)
                    SelectedIndex = m_OverIndex;
            }
            else
            {
                Opened = !Opened;
                SelectedIndex = m_OverIndex;
            }
        }

        private void ComboBox_LostFocus(DXControl obj)
        {
            Opened = false;
            targetAlpha = (int)MouseEffectState.Out;
        }

        private void ComboBox_BackColorChanged(DXControl obj)
        {
            if (Enabled)
                BackRenderColor = BackColor;
        }

        private void ComboBox_ForeColorChanged(DXControl obj)
        {
            if (Enabled)
                FontRenderColor = ForeColor;
        }

        private void ComboBox_MouseMove(DXControl ctrl, MouseState state, MouseState stateOld)
        {
            float y = ParentHOD.MousePosition.Y - Top - m_ItemsOffset - Height;
            float x = ParentHOD.MousePosition.X - vScroll.Left;

            if (y > 0 && x < 0 && !IsMouseOver(vScroll))
            {
                m_OverIndex = (int)((y) / rect.Height);
            }
            //else
            //    m_OverIndex = SelectedIndex;
        }

        private void ComboBoxFontChnaged(DXControl obj)
        {
            rect.Height = Font.Height + m_TextOffset;
        }

        #endregion

        #region Disposal
        /// <summary>
        /// Disposes the ComboBox
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                vScroll.Dispose();

            base.Dispose(disposing);
        }

        #endregion

    }

}