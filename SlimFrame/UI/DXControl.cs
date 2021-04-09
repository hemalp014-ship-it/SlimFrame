using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

using SlimDX;
using SlimDX.DirectInput;
using D2D = SlimDX.Direct2D;
using DW = SlimDX.DirectWrite;

using Debug = System.Diagnostics.Debug;
using System.Windows.Forms;
using SlimDX.Direct2D;

namespace SlimFrame.UI
{


    #region Enumrations

    /// <summary>
    /// Horizontal allignment of Text
    /// </summary>
    public enum TextHAllignment
    {
        Default,                            // Default horizontal allignment
        Left = DW.TextAlignment.Leading,    // Left horizontal allignment
        Center = DW.TextAlignment.Center,   // Center horizontal allignment
        Right = DW.TextAlignment.Trailing,  // Right horizontal allignment
    }

    /// <summary>
    /// Vertical allignment of Text
    /// </summary>
    public enum TextVAllignment
    {
        Default,                                // Default vertical allignment
        Top = DW.ParagraphAlignment.Near,       // Top vertical allignment
        Center = DW.ParagraphAlignment.Center,  // Center vertical allignment
        Bottom = DW.ParagraphAlignment.Far,     // Bottom vertical allignment
    }

    enum MouseEffectState
    {
        None,       // No mouse effect
        Out = 80,   // mouse effect when pointer is away an object
        Over = 95,  // mouse effect when pointer is over an object
        Down = 100, // mouse effect when pointer is over an object with left button of mouse is pressed
    }

    #endregion

    /// <summary>
    /// The Control Class for Base of all HOD Control
    /// </summary>
    public class DXControl : IDisposable, IEnabler
    {

        #region Member Variables

        protected DW.TextFormat m_textFormat;   // The textformat to draw the control Text
        protected DW.Factory m_factory;      // The Factory for Creating the text format
        protected D2D.SolidColorBrush m_brush;        // The Brush to draw text, border, etc.
        protected D2D.StrokeStyle m_strokeStyle;  // The style of the strock for drawing

        protected SizeF m_Size;         // The Size of the control.
        protected PointF m_Location;     // The Location of the control.
        protected Font m_font;         // The Font of the control.

        protected DXControl m_parent;       // Stores the parent variable for the control.
        protected HUD m_parentHOD;    // Stores the parent variable for the control.
        protected static DXContolImages ControlImage; // Stores the Images for rendering the control.

        protected string m_Name;     // The Name of the control.
        protected string m_Text;     // The Text of the control.
        protected string m_Tag;      // The Tag of the control.

        protected bool m_TabStop;      // Determines if control to be selected by Tab or not.
        protected int m_TabIndex;     // The TabIndex of the control.

        protected Color m_BorderColor = Color.White;        // The Color of the control
        protected Color m_ForeColor = Color.White;        // The Color of the control
        protected Color m_BackColor = Color.Transparent;  // The Color of the control

        protected float m_BorderWidth = 0.0f;     // Stores the width of control border.
        protected bool m_Initialized = false;    //Stores the Initialization state of control in boolean format

        protected bool m_MouseOver = false;    //Stores boolean value indicating whether mouse is over control or not.
        protected bool m_MouseDown = false;    //Stores boolean value indicating whether mouse button is pressed or not.

        protected bool m_Focused = false;    //Stores boolean value indicating whether control is active control in its parent or not.

        protected bool m_Visible = true;    //Stores boolean value indicating whether control is visible or not.
        protected bool m_TextEnabled = true;    //Stores boolean value indicating whether text is enabled for control or not.
        protected bool m_BackEnabled = true;    //Stores boolean value indicating whether Background is enabled for control or not.
        protected bool m_BorderEnabled = true;    //Stores boolean value indicating whether Border is enabled for control or not.

        protected bool m_dTextMeasuredSize = true; //Stores boolean value indicating whether Control Use auto size calculated on default text measurement basis.
        protected SizeF m_TextSize;                 //Stores the size of text of the Control if above value is false to help the control to calculate its own auto size if it also considers this

        protected bool m_Enabled = true;           //Stores boolean value indicating whether the Control is enabled or not.

        protected float m_Padding = 2.0f;           //Stores the padding offset
        protected RectangleF m_Bound;           //Stores the bound of the control
        protected RectangleF m_ClientRect;           //Stores the bound of the control

        private bool m_Associated = false;      //Store boolean value indicating whether the control is associated to another control or not.

        protected float m_AnimationStep;     // Setps of transition
        protected readonly double transitionTime = 0.6; // amount of time in seconds for transition
        protected float alpha = 0.0f;
        protected float targetAlpha = 1.0f;    // Target Alpha value of background color
        protected bool m_UseDefaultAplhaAnimation = true;

        protected int m_page = 0;

        #endregion

        #region Event System

        public delegate void _MouseOver(DXControl ctrl);
        /// <summary>
        /// Occures when the mouse is over control.
        /// </summary>
        public event _MouseOver MouseOver;

        public delegate void _MouseOut(DXControl ctrl);
        /// <summary>
        /// Occures when the mouse gones out of control.
        /// </summary>
        public event _MouseOut MouseOut;

        public delegate void _MouseMove(DXControl ctrl, MouseState state, MouseState stateOld);
        /// <summary>
        /// Occures when the mouse moves.
        /// </summary>
        public event _MouseMove MouseMove;

        public delegate void _MouseUp(DXControl ctrl, MouseState state, MouseState stateOld);
        /// <summary>
        /// Occures when the mouse button released.
        /// </summary>
        public event _MouseUp MouseUp;

        public delegate void _MouseDown(DXControl ctrl, MouseState state, MouseState stateOld);
        /// <summary>
        /// Occures when the mouse is over control.
        /// </summary>
        public event _MouseDown MouseDown;

        public delegate void _Click(DXControl ctrl);
        /// <summary>
        /// Occures when the control is clicked.
        /// </summary>
        public event _Click Click;

        public delegate void _DoubleClick(DXControl ctrl);
        /// <summary>
        /// Occures when the control is double clicked by Mouse.
        /// </summary>
        public event _DoubleClick DoubleClick;

        /// <summary>
        /// Occures when the control is double clicked by Mouse.
        /// </summary>
        public event Action<DXControl> LocationChanged;

        /// <summary>
        /// Occures when the text of control is changed.
        /// </summary>
        public event Action<DXControl> TextChanged;

        /// <summary>
        /// Occures when the Font of control is changed.
        /// </summary>
        public event Action<DXControl> FontChanged;

        /// <summary>
        /// Occures when the control is double clicked by Mouse.
        /// </summary>
        public event Action<DXControl> SizeChanged;

        /// <summary>
        /// Occures when the control gains focus.
        /// </summary>
        public event Action<DXControl> GotFocus;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> LostFocus;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> VisibleChanged;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> EnabledChanged;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> ForeColorChanged;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> BackColorChanged;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> TabIndexChanged;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> ParentChanged;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        public event Action<DXControl> Loaded;

        /// <summary>
        /// Occures when the control is losts focus.
        /// </summary>
        protected event Action BoundChanged;

        public event Action<int, int> pageChanged;

        #endregion

        #region Properties

        public bool IsAssociated
        {
            get
            {
                return m_Associated;
            }
        }

        public float CalculatedAlpha
        {
            get
            {
                return alpha / 100;
            }
        }


        /// <summary>
        /// Determines whether the control in enabled or not.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_Enabled;
            }
            set
            {
                if (m_Enabled != value)
                {
                    m_Enabled = value;

                    if (ParentHOD != null)
                        ParentHOD.FindSelectable();

                    EnabledChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Determines whether the control is visible or not
        /// </summary>
        public bool Visible
        {
            get
            {
                return m_Visible;
            }
            set
            {
                if (m_Visible != value)
                {
                    m_Visible = value;
                    if (ParentHOD != null)
                        ParentHOD.FindSelectable();
                    VisibleChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// The width of control border.
        /// </summary>
        public float BorderWidth
        {
            get
            {
                return m_BorderWidth;
            }
            set
            {
                m_BorderWidth = value;
            }
        }

        /// <summary>
        /// Name of the control
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        /// <summary>
        /// Text of the Control
        /// </summary>
        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (m_Text != value)
                {
                    m_Text = value;
                    TextChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Tag of the Control
        /// </summary>
        public string Tag
        {
            get
            {
                return m_Tag;
            }
            set
            {
                m_Tag = value;
            }
        }

        /// <summary>
        /// Size of the Control
        /// </summary>
        public SizeF Size
        {
            get
            {
                return m_Size;
            }
            set
            {
                if (m_Size != value)
                {
                    m_Size = value;
                    if (Enabled) SizeChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Width of the Control
        /// </summary>
        public float Width
        {
            get
            {
                return m_Size.Width;
            }
            set
            {
                if (m_Size.Width != value)
                {
                    m_Size.Width = value;
                    if (Enabled) SizeChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Height of the Control
        /// </summary>
        public float Height
        {
            get
            {
                return m_Size.Height;
            }
            set
            {
                if (m_Size.Height != value)
                {
                    m_Size.Height = value;
                    if (Enabled)
                        SizeChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Location of the Control
        /// </summary>
        public PointF Location
        {
            get
            {
                if (m_parent != null)
                    return new PointF(m_Location.X + m_parent.Left, m_Location.Y + m_parent.Top);
                else
                    return m_Location;
            }
            set
            {
                PointF temp;

                if (m_parent != null)
                    temp = new PointF(value.X - m_parent.Left, value.Y - m_parent.Top);
                else
                    temp = value;

                if (m_Location != temp)
                {
                    m_Location = temp;
                    if (Enabled) LocationChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Left Offset of the Control
        /// </summary>
        public float Left
        {
            get
            {
                if (m_parent != null)
                    return m_Location.X + m_parent.Left;
                else
                    return m_Location.X;
            }
            set
            {
                if (m_Location.X != value)
                {
                    m_Location.X = value;
                    if (Enabled) LocationChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Top Offset of the Control
        /// </summary>
        public float Top
        {
            get
            {
                if (m_parent != null)
                    return m_Location.Y + m_parent.Top;
                else
                    return m_Location.Y;
            }
            set
            {
                if (m_Location.Y != value)
                {
                    m_Location.Y = value;
                    if (Enabled) LocationChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Left Offset of the Control
        /// </summary>
        public float Right
        {
            get
            {
                if (m_parent != null)
                    return m_Location.X + m_Size.Width + m_parent.Left;
                else
                    return m_Location.X + m_Size.Width;
            }
            set
            {
                float x = value - m_Size.Width;

                if (m_Location.X != x)
                {
                    m_Location.X = x;
                    if (Enabled) LocationChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Top Offset of the Control
        /// </summary>
        public float Bottom
        {
            get
            {
                if (m_parent != null)
                    return Location.Y + m_Size.Height + m_parent.Top;
                else
                    return Location.Y + m_Size.Height;
            }
            set
            {
                var y = value - m_Size.Height;

                if (m_Location.Y != y)
                {
                    m_Location.Y = y;
                    if (Enabled) LocationChanged?.Invoke(this);
                    CalculateBound();
                }
            }
        }

        /// <summary>
        /// Font for rendering the text of the Control
        /// </summary>
        public Font Font
        {
            get
            {
                return m_font;
            }
            set
            {
                if (m_font != value)
                {
                    m_font = value;
                    m_textFormat?.Dispose();
                    if (m_factory != null)
                        m_textFormat = new DW.TextFormat(m_factory, value.FontFamily.Name, DW.FontWeight.Normal, DW.FontStyle.Normal, DW.FontStretch.Normal, value.Size, "font");

                    FontChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// The Bound of the Control, determining the rectangle covered by Control
        /// </summary>
        public RectangleF Bound
        {
            get
            {
                return m_Bound;
            }
            set
            {
                m_Bound = value;
                //BoundChanged?.Invoke();
            }
        }

        /// <summary>
        /// The Bound of the Control, determining the rectangle covered by Control
        /// </summary>
        public RectangleF ClientRectangle
        {
            get
            {
                return m_ClientRect;
            }
            set
            {
                m_ClientRect = value;
                //BoundChanged?.Invoke();
            }
        }

        /// <summary>
        /// Color of Border of the Control
        /// </summary>
        public Color BorderColor
        {
            get
            {
                return m_BorderColor;
            }
            set
            {
                if (m_BorderColor != value)
                {
                    m_BorderColor = value;
                }
            }
        }

        /// <summary>
        /// Color of Text of the Control
        /// </summary>
        public Color ForeColor
        {
            get
            {
                return m_ForeColor;
            }
            set
            {
                if (m_ForeColor != value)
                {
                    m_ForeColor = value;
                    ForeColorChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Color of Background of the Control.
        /// </summary>
        public virtual Color BackColor
        {
            get
            {
                return m_BackColor;
            }
            set
            {
                if (m_BackColor != value)
                {
                    m_BackColor = value;
                    BackColorChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Parent Control of the COntrol.
        /// </summary>
        public DXControl Parent
        {
            get
            {
                return m_parent;
            }
            protected set
            {
                if (m_parent != value)
                {
                    m_parent = value;
                    if (m_parent == null) return;
                    ParentChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Parent HOD of the Control.
        /// </summary>
        public HUD ParentHOD
        {
            get
            {
                return m_parentHOD;
            }
            set
            {
                if (m_parentHOD != value)
                {
                    m_parentHOD = value;

                    if (m_parent == null) return;

                    ParentChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// The tab index of the control it is used when the control is selected by Tab
        /// </summary>
        public int TabIndex
        {
            get
            {
                return m_TabIndex;
            }
            set
            {
                if (m_TabIndex != value)
                {
                    m_TabIndex = value;
                    TabIndexChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Determines whether the control to be selected by Tab method or not.
        /// </summary>
        public bool TabStop
        {
            get
            {
                return m_TabStop;
            }
            set
            {
                if (m_TabStop != value)
                {
                    m_TabStop = value;

                    if (value)
                        MouseUp += focus_Control;
                    else
                        MouseUp -= focus_Control;

                    TabIndexChanged?.Invoke(this);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Created new instance of the Control.
        /// </summary>
        public DXControl()
        {
            m_Bound = new RectangleF();
            m_ClientRect = new RectangleF();

            if (m_UseDefaultAplhaAnimation)
            {
                alpha = (int)MouseEffectState.Out;
                targetAlpha = alpha;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the 2D rendering capability
        /// </summary>
        /// <param name="factory">factory for creating DirectWrite Objects</param>
        /// <param name="factory2D">factory for creating Direct2D Objects</param>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public virtual void Init(DW.Factory factory, D2D.Factory factory2D, D2D.RenderTarget renderTarget)
        {
            m_factory = factory;
            if (!m_Initialized)
            {
                if (Font != null)
                    m_textFormat = new DW.TextFormat(factory, Font.FontFamily.Name, DW.FontWeight.Normal, DW.FontStyle.Normal, DW.FontStretch.Normal, Font.Size, "None");
                m_brush = new D2D.SolidColorBrush(renderTarget, new Color4(1, 1, 1, 1));

                m_Initialized = true;
            }

            if (ControlImage == null)
                ControlImage = new DXContolImages(renderTarget);


            MouseOver += (c) => { if (m_UseDefaultAplhaAnimation) targetAlpha = (int)MouseEffectState.Over; };
            MouseOut += (c) => { if (m_UseDefaultAplhaAnimation) targetAlpha = (int)MouseEffectState.Out; };

            MouseUp += (c, s, o) => { if (m_UseDefaultAplhaAnimation) targetAlpha = (int)MouseEffectState.Over; };
            MouseDown += (c, s, o) => { if (m_UseDefaultAplhaAnimation) targetAlpha = (int)MouseEffectState.Down; };


            EnabledChanged += ActOnChange;
            VisibleChanged += ActOnChange;
            TabIndexChanged += ActOnChange;
            //LocationChanged += CalculateBound;
            //SizeChanged += CalculateBound;

            SizeChanged?.Invoke(this);
            LocationChanged?.Invoke(this);
            TextChanged?.Invoke(this);
            ForeColorChanged?.Invoke(this);
            BackColorChanged?.Invoke(this);
            VisibleChanged?.Invoke(this);
            EnabledChanged?.Invoke(this);
        }

        protected void CalculateBound()
        {
            BoundPad(m_Padding);
            ClientRect();
            BoundChanged?.Invoke();
        }

        void ActOnChange(DXControl c)
        {
            if (c.Parent == null) return;
            if (Parent.GetType() == typeof(DXContainerControl))
            {
                var cc = (DXContainerControl)c.Parent;

                cc.FindSelectable();
                cc.Sort();
            }
        }

        /// <summary>
        /// Releases render target dependent objects.
        /// </summary>
        public virtual void RenderTargetReleasing()
        {

        }

        /// <summary>
        /// Creates render target dependent objects.
        /// </summary>
        /// <param name="renderTarget"></param>
        public virtual void RenderTargetResized(D2D.RenderTarget renderTarget)
        {

        }

        /// <summary>
        /// Renders the control.
        /// </summary>
        public virtual void Render(D2D.RenderTarget renderTarget)
        {
            if (!Visible) return;

            renderTarget.PushAxisAlignedClip(m_Bound, AntialiasMode.Aliased);

            if (m_BackEnabled)
            {
                m_brush.Color = BackColor;
                renderTarget.FillRectangle(m_brush, Bound);
            }

            renderTarget.PopAxisAlignedClip();

            if (m_Focused)
            {
                if (ParentHOD != null && ParentHOD.HighlightSelectedControl)
                {
                    m_brush.Color = m_BorderColor;
                    renderTarget.DrawRectangle(m_brush, Bound, m_BorderWidth);
                }
            }

            if (m_BorderEnabled)
            {

                if (m_BorderWidth > 0.45f)
                {
                    m_brush.Color = m_BorderColor;
                    renderTarget.DrawRectangle(m_brush, Bound, m_BorderWidth);
                }
            }
        }

        /// <summary>
        /// Updates the boundaries of the control.
        /// </summary>
        public virtual void Update(double frameTime)
        {
            if (!Visible) return;

            if (!Enabled)
            {
                m_MouseOver = false;
                m_MouseDown = false;
                return;
            }

            float diff = targetAlpha - alpha;
            int fps = ParentHOD.FramePerSecond;

            m_AnimationStep = (float)(transitionTime * ((fps == 0) ? 1 / frameTime : fps)) / 5.0f;

            if (Math.Abs(diff) > 0.1f)
                alpha += diff / m_AnimationStep;

        }

        /// <summary>
        /// Focuses the given control
        /// </summary>
        public void Focus()
        {
            if (Enabled)
                if (GetType() == typeof(HUD))
                {
                    ((HUD)this).ActiveControl = this;
                }
                else
                {
                    if (m_parentHOD != null)
                        m_parentHOD.ActiveControl = this;
                }
        }

        /// <summary>
        /// The Bound of the Control with padding, determining the rectangle covered by Control with padding
        /// </summary>
        /// <param name="pad">All direction padding</param>
        /// <returns></returns>
        protected virtual void BoundPad(float pad)
        {
            m_Bound.X = Left - pad;
            m_Bound.Y = Top - pad;
            m_Bound.Width = Width + 2 * pad;
            m_Bound.Height = Height + 2 * pad;
        }

        protected virtual void ClientRect()
        {
            m_ClientRect.X = Left;
            m_ClientRect.Y = Top;
            m_ClientRect.Width = Width;
            m_ClientRect.Height = Height;
        }

        public void SelectNextControl()
        {
            SelectNextControl(this, m_parentHOD);
        }

        public void SendToBack()
        {
            if (m_parent != null && !IsAssociated)
            {
                DXContainerControl parentContainer = (DXContainerControl)m_parent;

                if (parentContainer.Controls.IndexOf(this) == 0) return;

                parentContainer.Controls.Remove(this);
                parentContainer.Controls.Insert(0, this);
            }
        }

        public void BringToFront()
        {
            if (m_parent != null && !IsAssociated)
            {
                DXContainerControl parentContainer = (DXContainerControl)m_parent;

                if (parentContainer.Controls.IndexOf(this) == parentContainer.Controls.Count) return;

                parentContainer.Controls.Remove(this);
                parentContainer.Controls.Insert(parentContainer.Controls.Count, this);
            }
        }

        public void SelectPreviousControl()
        {
            SelectPreviousControl(this, m_parentHOD);
        }

        // Here, no need for using POint and rectangle type because the .NET framework will auto convert Point to PointF and Rectangle tyo RectangleF
        public bool IntersectMouse(PointF mousePos, RectangleF rect)
        {
            if (mousePos.X >= rect.Left && mousePos.X <= rect.Right)
            {
                if (mousePos.Y >= rect.Top && mousePos.Y <= rect.Bottom)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Shared Method

        protected static void MouseLookUp(DXControl c)
        {

            HUD hod = c.m_parentHOD;
            UserInput input = hod.UserInput;

            MouseState state = input.MouseState_Current;        // Current state of mouse
            MouseState stateOld = input.MouseState_Previous;    // Last state of mouse

            if (c.IntersectMouse(c.m_parentHOD.MousePosition, c.m_Bound))
            { // Checks if the mouse pointer is inside the control.

                if (!c.m_MouseOver)
                { // Checks if mouse was not over the control.

                    c.MouseOver?.Invoke(c); // Raises the MouseOver event
                    c.m_MouseOver = true;      // Notes that the MouseOver event is raised.
                }
                else
                {
                    if (c.GetType() != typeof(HUD))
                        if (state.X != stateOld.X ||
                            state.Y != stateOld.Y ||
                            state.Z != stateOld.Z)
                        { // Checks if the mouse coordinates are changed.
                            c.MouseMove?.Invoke(c, state, stateOld); // Raises the MouseMove event
                        }

                    if ((state.GetButtons()[0] != stateOld.GetButtons()[0]) ||
                       (state.GetButtons()[1] != stateOld.GetButtons()[1]) ||
                       (state.GetButtons()[2] != stateOld.GetButtons()[2]))
                    { // Checks if the Mouse button state is changed.

                        if (state.GetButtons()[0] ||
                            state.GetButtons()[1] ||
                            state.GetButtons()[2])
                        { // Checks is any mouse button is pressed.
                            if (!c.m_MouseDown)
                            { // Checks if mouse was not down over the control
                                c.MouseDown?.Invoke(c, state, stateOld); // Raises the MouseDown event
                                c.m_MouseDown = true;                       // Notes that the MouseDown event is raised.
                            }
                        }
                    }
                }

            }
            else
            {
                if (c.m_MouseOver && !c.m_MouseDown)
                { // Checks if mouse was over the control.
                    c.MouseOut?.Invoke(c); // Raises the MouseOut event
                    c.m_MouseOver = false;    // Notes that the MouseOut event is raised.
                }
            }

            if (c.m_MouseDown)
            { // Checks if mouse was down over the control.
                if ((state.GetButtons()[0] != stateOld.GetButtons()[0]) ||
                       (state.GetButtons()[1] != stateOld.GetButtons()[1]) ||
                       (state.GetButtons()[2] != stateOld.GetButtons()[2]))
                {
                    if (!state.GetButtons()[0] &&
                    !state.GetButtons()[1] &&
                    !state.GetButtons()[2])
                    { // Checks if all mouse buttons are not pressed.
                        c.MouseUp?.Invoke(c, state, stateOld); // Raises the MouseUp event
                        c.m_MouseDown = false;                    // Notes that the MouseUp event is raised.

                        //m_parent.m_MouseDown = false;
                        c.Click?.Invoke(c);
                    }
                }
            }

            if (c.m_Focused)
            {
                if (input.IsKeyJustPressed(Key.Space) ||
                    input.IsKeyJustPressed(Key.NumberPadEnter) ||
                    input.IsKeyJustPressed(Key.Return))
                {
                    c.Click?.Invoke(c);
                }

                if (input.IsKeyJustPressed(Key.Tab))
                {
                    if (input.IsKeyPressed(Key.LeftShift) || input.IsKeyPressed(Key.RightShift))
                        SelectPreviousControl(c, c.m_parentHOD);
                    else
                        SelectNextControl(c, c.m_parentHOD);
                }
            }
        }

        protected static void Associate(DXControl master, DXControl slave)
        {
            if (!slave.m_Associated)
            {
                slave.m_Associated = true;
                SetParent(slave, master);
            }
        }

        /// <summary>
        /// Loads the Control
        /// </summary>
        /// <param name="c"></param>
        protected static void Load(DXControl c)
        {
            c.Loaded?.Invoke(c);
        }

        protected static void TextChange(DXControl c)
        {
            c.TextChanged?.Invoke(c);
        }

        /// <summary>
        /// Assigns the parent to a control
        /// </summary>
        /// <param name="child">the control to whose the parent to be assigned</param>
        /// <param name="parent">the control to be assigned as parent</param>
        protected static void SetParent(DXControl child, DXControl parent)
        {
            bool Orphan = (child.m_parent == null);

            child.m_parent = parent;
            if (parent.GetType() == typeof(HUD))
                child.m_parentHOD = (HUD)parent;
            else
                child.m_parentHOD = parent.ParentHOD;

            if (child.Font == null)
                child.Font = parent.Font;
        }

        /// <summary>
        /// Checks if mouse is over control.
        /// </summary>
        /// <param name="ctrl">Control for which we are finding mouse is over or not</param>
        /// <returns></returns>
        public static bool IsMouseOver(DXControl ctrl)
        {
            return ctrl.m_MouseOver;
        }

        /// <summary>
        /// Checks if mouse is down over control.
        /// </summary>
        /// <param name="ctrl">Control for which we are finding mouse is over or not</param>
        /// <returns></returns>
        public static bool IsMouseDown(DXControl ctrl)
        {
            return ctrl.m_MouseDown;
        }

        /// <summary>
        /// Selects next control in Tab Index Buffer from current in the parent
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="parent"></param>
        protected static void SelectNextControl(DXControl ctrl, HUD parent)
        {
            int index;

            parent.UserInput.Update();

            if (parent.ControlsSelectable.Count > 0)
            {
                index = parent.ControlsSelectable.IndexOf(ctrl) + 1;

                if (index >= parent.ControlsSelectable.Count)
                    parent.SelectFirstControl();
                else
                {
                    parent.ControlsSelectable[index].Focus();
                }
            }
        }

        /// <summary>
        /// Selects previous control in Tab Index Buffer from current in the parent
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="parent"></param>
        protected static void SelectPreviousControl(DXControl ctrl, HUD parent)
        {
            int index;

            parent.UserInput.Update();

            if (parent.ControlsSelectable.Count > 0)
            {
                index = parent.ControlsSelectable.IndexOf(ctrl);

                if (index - 1 < 0)
                    parent.SelectLastControl();
                else
                {
                    DXControl next = parent.ControlsSelectable[index - 1];
                    if (next != null)
                        next.Focus();
                }
            }
        }

        /// <summary>
        /// Invokes Click event of the control
        /// </summary>
        /// <param name="c">Control to whom click event to be invoked!</param>
        protected static void PerformClick(DXControl c)
        {
            c.Click?.Invoke(null);
        }

        /// <summary>
        /// Invokes GotFocus event of the control
        /// </summary>
        /// <param name="c">Control to whom GotFocus event to be invoked!</param>
        protected static void ControlGotFocus(DXControl c)
        {
            c.m_Focused = true;
            c.GotFocus?.Invoke(c);
        }

        /// <summary>
        /// Invokes LostFocus event of the control
        /// </summary>
        /// <param name="c">Control to whom LostFocus event to be invoked!</param>
        protected static void ControlLostFocus(DXControl c)
        {
            c.m_Focused = false;
            c.LostFocus?.Invoke(c);
        }

        /// <summary>
        /// Converts Object to Interface Implemented by its class
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="input">Object3D to convert</param>
        /// <returns>Interface made up from the Object</returns>
        public static T ConvertToInterface<T>(Object input)
        {
            T output;
            try
            {
                output = (T)input;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("Cannot Covert " + input.GetType().FullName + " to " + typeof(T).GetType().FullName);
            }

            return output;
        }

        /// <summary>
        /// Converts RoundedRectangle to RectangleF
        /// </summary>
        /// <param name="roundRect">RoundedRectangle to be converted</param>
        /// <returns>RectangleF made up from RoundedRectangle</returns>
        public static RectangleF RectangleFromRoundRect(RoundedRectangle roundRect)
        {
            return new RectangleF(roundRect.Left, roundRect.Top, roundRect.Right - roundRect.Left, roundRect.Bottom - roundRect.Top);
        }


        #endregion

        #region Event Handler

        /// <summary>
        /// Handles Text Changed event for measuring size of text
        /// </summary>
        /// <param name="c"></param>
        protected static void ChangedText(DXControl c)
        {
            if (!c.m_TextEnabled) return;

            var ctrl = ConvertToInterface<ITextControl>(c);
            if (ctrl.GetType().GetInterfaces().Contains(typeof(ITextControl)))
                if (c.Font != null)
                    if (ctrl.AutoSize)
                    {
                        if (c.m_dTextMeasuredSize)
                            c.Size = System2D.MeasureStringF(c.Text, c.Font);
                        else
                            c.m_TextSize = System2D.MeasureStringF(c.Text, c.Font);
                    }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        /// <param name="o"></param>
        protected static void focus_Control(DXControl c, MouseState s, MouseState o)     // Basic Mouse up event handler
        {
            if (!c.m_Associated)
                c.Focus();
        }

        #endregion

        #region IDisposable Support

        protected bool m_Disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    EnabledChanged -= ActOnChange;
                    VisibleChanged -= ActOnChange;
                    TabIndexChanged -= ActOnChange;

                    m_textFormat?.Dispose();
                    m_brush?.Dispose();
                    ControlImage?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                m_Disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}