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

namespace SlimFrame
{
    namespace UI
    {

        #region Enumrations
        
        /// <summary>
        /// Horizontal allignment of Text
        /// </summary>
        public enum TextHAllignment
        {
            Left = DW.TextAlignment.Leading,
            Center = DW.TextAlignment.Center,
            Right = DW.TextAlignment.Trailing,
        }

        /// <summary>
        /// Vertical allignment of Text
        /// </summary>
        public enum TextVAllignment
        {
            Top = DW.ParagraphAlignment.Near,
            Center = DW.ParagraphAlignment.Center,
            Bottom = DW.ParagraphAlignment.Far,
        }
        
        #endregion 

        /// <summary>
        /// The HOD Control Base Class
        /// </summary>
        public class Control : IDisposable, IEnabler
        {

            #region Member Variables
            
            protected DW.TextFormat         m_textFormat;   // The textformat to draw the control Text
            protected DW.Factory            m_factory;      // The Factory for Creating the text format
            protected D2D.SolidColorBrush   m_brush;        // The Brush to draw text, border, etc.
            protected D2D.StrokeStyle       m_strokeStyle;  // The style of the strock for drawing
            
            protected SizeF     m_Size;         // The Size of the control.
            protected PointF    m_Location;     // The Location of the control.
            protected Font      m_font;         // The Font of the control.

            protected Control   m_parent;       // Stores the parent variable for the control.
            protected HOD       m_parentHOD;    // Stores the parent variable for the control.
            
            protected string    m_Name;     // The Name of the control.
            protected string    m_Text;     // The Text of the control.
            protected string    m_Tag;      // The Tag of the control.

            protected bool  m_TabStop;      // Determines if control to be selected by Tab or not.
            protected int   m_TabIndex;     // The TabIndex of the control.

            protected Color m_BorderColor   = Color.White;        // The Color of the control
            protected Color m_ForeColor     = Color.White;        // The Color of the control
            protected Color m_BackColor     = Color.Transparent;  // The Color of the control

            protected float m_BorderWidth   = 0.0f;     // Stores the width of control border.
            protected bool  m_Initialized   = false;    //Stores the Initialization state of control in boolean format

            protected bool  m_MouseOver     = false;    //Stores boolean value indicating whether mouse is over control or not.
            protected bool  m_MouseDown     = false;    //Stores boolean value indicating whether mouse button is pressed or not.
            
            protected bool  m_Focused     = false;    //Stores boolean value indicating whether control is active control in its parent or not.

            protected bool  m_Visible        = true;    //Stores boolean value indicating whether control is visible or not.
            protected bool  m_TextEnabled    = true;    //Stores boolean value indicating whether text is enabled for control or not.
            protected bool  m_BackEnabled    = true;    //Stores boolean value indicating whether Background is enabled for control or not.
            protected bool  m_BorderEnabled  = true;    //Stores boolean value indicating whether Border is enabled for control or not.
            
            protected bool m_dTextMeasuredSize = true; //Stores boolean value indicating whether Control Use auto size calculated on default text measurement basis.
            protected SizeF m_TextSize;                 //Stores the size of text of the Control if above value is false to help the control to calculate its own auto size if it also considers this

            protected bool  m_Enabled = true;           //Stores boolean value indicating whether the Control is enabled or not.

            protected float m_Padding = 3.0f;           //Stores the padding offset

            protected static _MouseDown focus = (c,s,o) =>     // Basic Mouse up event handler
            {
                c.Focus();
            };

            protected static Action<Control> ChangedText = (c) =>
            {
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

            };

            #endregion

            #region Event System

            public delegate void _MouseHover(Control ctrl);
            /// <summary>
            /// Occures when the mouse is over control.
            /// </summary>
            public event _MouseHover MouseOver;

            public delegate void _MouseOut(Control ctrl);
            /// <summary>
            /// Occures when the mouse gones out of control.
            /// </summary>
            public event _MouseOut MouseOut;

            public delegate void _MouseMove(Control ctrl, MouseState state, MouseState stateOld);
            /// <summary>
            /// Occures when the mouse moves.
            /// </summary>
            public event _MouseMove MouseMove;

            public delegate void _MouseUp(Control ctrl, MouseState state, MouseState stateOld);
            /// <summary>
            /// Occures when the mouse button released.
            /// </summary>
            public event _MouseUp MouseUp;

            public delegate void _MouseDown(Control ctrl, MouseState state, MouseState stateOld);
            /// <summary>
            /// Occures when the mouse is over control.
            /// </summary>
            public event _MouseDown MouseDown;

            public delegate void _Click(Control ctrl);
            /// <summary>
            /// Occures when the control is clicked.
            /// </summary>
            public event _Click Click;

            public delegate void _DoubleClick(Control ctrl);
            /// <summary>
            /// Occures when the control is double clicked by Mouse.
            /// </summary>
            public event _DoubleClick DoubleClick;
            
            /// <summary>
            /// Occures when the control is double clicked by Mouse.
            /// </summary>
            public event Action<Control> LocationChanged;

            /// <summary>
            /// Occures when the text of control is changed.
            /// </summary>
            public event Action<Control> TextChanged;

            /// <summary>
            /// Occures when the control is double clicked by Mouse.
            /// </summary>
            public event Action<Control> SizeChanged;

            /// <summary>
            /// Occures when the control gains focus.
            /// </summary>
            public event Action<Control> GotFocus;
            
            /// <summary>
            /// Occures when the control is losts focus.
            /// </summary>
            public event Action<Control> LostFocus;

            /// <summary>
            /// Occures when the control is losts focus.
            /// </summary>
            public event Action<Control> VisibleChanged;

            /// <summary>
            /// Occures when the control is losts focus.
            /// </summary>
            public event Action<Control> EnabledChanged;

            /// <summary>
            /// Occures when the control is losts focus.
            /// </summary>
            public event Action<Control> ForeColorChanged;

            /// <summary>
            /// Occures when the control is losts focus.
            /// </summary>
            public event Action<Control> BackColorChanged;

            /// <summary>
            /// Occures when the control is losts focus.
            /// </summary>
            public event Action<Control> TabIndexChanged;

            /// <summary>
            /// Occures when the control is losts focus.
            /// </summary>
            public event Action<Control> ParentChanged;
            #endregion

            #region Properties

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
                    m_Visible = value;
                    if (ParentHOD != null)
                        ParentHOD.FindSelectable();
                    VisibleChanged?.Invoke(this);
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
                    m_Text = value;
                    TextChanged?.Invoke(this);
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
                    if(m_Size != value)
                    {
                        m_Size = value;
                        if(Enabled) SizeChanged?.Invoke(this);
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
                    m_Size.Width = value;
                    if (Enabled) SizeChanged?.Invoke(this);
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
                    m_Size.Height = value;
                    if (Enabled) SizeChanged?.Invoke(this);
                }
            }

            /// <summary>
            /// Location of the Control
            /// </summary>
            public PointF Location
            {
                get
                {
                    return m_Location;
                }
                set
                {
                    if (m_Location != value)
                    {
                        m_Location = value;
                        if (Enabled) LocationChanged?.Invoke(this);
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
                    return m_Location.X;
                }
                set
                {
                    m_Location.X = value;
                    if (Enabled) LocationChanged?.Invoke(this);
                }
            }

            /// <summary>
            /// Top Offset of the Control
            /// </summary>
            public float Top
            {
                get
                {
                    return Location.Y;
                }
                set
                {
                    m_Location.Y = value;
                    if (Enabled) LocationChanged?.Invoke(this);
                }
            }

            /// <summary>
            /// Left Offset of the Control
            /// </summary>
            public float Right
            {
                get
                {
                    return m_Location.X + m_Size.Width;
                }
                set
                {
                    m_Location.X = value - m_Size.Width;
                    if (Enabled) LocationChanged?.Invoke(this);
                }
            }

            /// <summary>
            /// Top Offset of the Control
            /// </summary>
            public float Bottom
            {
                get
                {
                    return Location.Y + m_Size.Height;
                }
                set
                {
                    m_Location.Y = value - m_Size.Height;
                    if (Enabled) LocationChanged?.Invoke(this);
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
                    m_font = value;
                    m_textFormat?.Dispose();
                    if (m_factory != null)
                        m_textFormat = new DW.TextFormat(m_factory, value.FontFamily.Name, DW.FontWeight.Normal, DW.FontStyle.Normal, DW.FontStretch.Normal, value.Size, "font");

                    TextChanged?.Invoke(this);
                }
            }

            /// <summary>
            /// The Bound of the Control, determining the rectangle covered by Control
            /// </summary>
            public RectangleF Bound
            {
                get
                {
                    return new RectangleF(m_Location, m_Size);
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
                    m_BorderColor = value;
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
                    m_ForeColor = value;
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
                    m_BackColor = value;
                }
            }

            /// <summary>
            /// Parent Control of the COntrol.
            /// </summary>
            public Control Parent
            {
                get
                {
                    return m_parent;
                }
                protected set
                {
                    m_parent = value;
                }
            }
          
            /// <summary>
            /// Parent HOD of the COntrol.
            /// </summary>
            public HOD ParentHOD
            {
                get
                {
                    return m_parentHOD;
                }
                protected set
                {
                    m_parentHOD = value;
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
                    m_TabIndex = value;
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
                    m_TabStop = value;
                    if (value)
                        MouseDown += focus;
                    else
                        MouseDown -= focus;
                }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Created new instance of the Control.
            /// </summary>
            public Control()
            {

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
                    m_textFormat = new DW.TextFormat(factory, Font.FontFamily.Name, DW.FontWeight.Normal, DW.FontStyle.Normal, DW.FontStretch.Normal, Font.Size, "None");
                    m_brush = new D2D.SolidColorBrush(renderTarget, new Color4(1, 1, 1, 1));

                    m_Initialized = true;
                }

                SizeChanged?.Invoke(this);
                LocationChanged?.Invoke(this);
                TextChanged?.Invoke(this);
                VisibleChanged?.Invoke(this);
                EnabledChanged?.Invoke(this);
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

                if (m_BackEnabled)
                {
                    m_brush.Color = BackColor;
                    renderTarget.FillRectangle(m_brush, Bound);
                }

                if (m_TextEnabled)
                {
                    m_brush.Color = ForeColor;
                    renderTarget.DrawText(m_Text, m_textFormat, Bound, m_brush);
                }

                if (m_BorderEnabled)
                {

                    if (ParentHOD != null)
                        if (ParentHOD.ActiveControl == this && ParentHOD.HighlightSelectedControl)
                        {
                            //if (m_MouseOver)
                            //{
                            //    m_brush.Color = new Color4(m_BorderColor) { Alpha = 0.7f };
                            //}
                            //else
                            //{
                                m_brush.Color = new Color4(m_BorderColor) { Alpha = 0.6f };
                            //}
                            renderTarget.DrawRectangle(m_brush, BoundPad(m_Padding));
                        }

                    if (m_BorderWidth > 0.0f)
                    {
                        m_brush.Color = new Color4(m_BorderColor) { Alpha = 1.0f };
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

                HOD hod = m_parentHOD;
                UserInput input = hod.UserInput;

                MouseState state = input.MouseState_Current;        // Current state of mouse
                MouseState stateOld = input.MouseState_Previous;    // Last state of mouse


                if (Bound.IntersectsWith(new Rectangle(m_parentHOD.MousePosition, new Size(1, 1))))
                { // Checks if the mouse pointer is inside the control.

                    if (!m_MouseOver)
                    { // Checks if mouse was not over the control to reduce redundant calling.

                        MouseOver?.Invoke(this); // Raises the MouseOver event
                        m_MouseOver = true;      // Notes that the MouseOver event is raised.
                    }
                    else
                    {
                        if (state.X != stateOld.X ||
                            state.Y != stateOld.Y ||
                            state.Z != stateOld.Z)
                        { // Checks if the mouse coordinates are changed.
                            MouseMove?.Invoke(this, state, stateOld); // Raises the MouseMove event
                        }

                        if ((state.GetButtons()[0] != stateOld.GetButtons()[0]) ||
                           (state.GetButtons()[1] != stateOld.GetButtons()[1]) ||
                           (state.GetButtons()[2] != stateOld.GetButtons()[2]))
                        { // Checks if the Mouse button state is changed.

                            if (state.GetButtons()[0] ||
                                state.GetButtons()[1] ||
                                state.GetButtons()[2])
                            { // Checks is any mouse button is pressed.
                                if (!m_MouseDown)
                                { // Checks if mouse was not down over the control to reduce redundant calls.
                                    MouseDown?.Invoke(this, state, stateOld); // Raises the MouseDown event
                                    m_MouseDown = true;                       // Notes that the MouseDown event is raised.
                                    
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (m_MouseOver && !m_MouseDown)
                    { // Checks if mouse was over the control to reduce redundant calls.
                       MouseOut?.Invoke(this); // Raises the MouseOut event
                        m_MouseOver = false;    // Notes that the MouseOut event is raised.
                    }

                }

                if (m_MouseDown)
                { // Checks if mouse was down over the control to reduce redundant calls.
                    if ((state.GetButtons()[0] != stateOld.GetButtons()[0]) ||
                           (state.GetButtons()[1] != stateOld.GetButtons()[1]) ||
                           (state.GetButtons()[2] != stateOld.GetButtons()[2]))
                    {
                        if (!state.GetButtons()[0] &&
                        !state.GetButtons()[1] &&
                        !state.GetButtons()[2])
                        { // Checks if all mouse buttons are not pressed.
                            MouseUp?.Invoke(this, state, stateOld); // Raises the MouseUp event
                            m_MouseDown = false;                    // Notes that the MouseUp event is raised.
                            
                            //m_parent.m_MouseDown = false;
                            Click?.Invoke(this);
                        }
                    }
                }

                if (hod.ActiveControl == this)
                {
                    if (input.IsKeyJustPressed(Key.Space) ||
                        input.IsKeyJustPressed(Key.NumberPadEnter) ||
                        input.IsKeyJustPressed(Key.Return))
                    {
                        Click?.Invoke(this);
                    }

                    if (input.IsKeyJustPressed(Key.Tab))
                    {
                        if (input.IsKeyPressed(Key.LeftShift) || input.IsKeyPressed(Key.RightShift))
                            SelectPreviousControl(this, m_parentHOD);
                        else
                            SelectNextControl(this, m_parentHOD);
                    }
                }

            }

            /// <summary>
            /// Focuses the given control
            /// </summary>
            public void Focus()
            {
                if (Enabled)
                    if (GetType() == typeof(HOD))
                    {
                        ((HOD)this).ActiveControl = this;
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
            public RectangleF BoundPad(float pad)
            {
                return new RectangleF(m_Location.X - pad, m_Location.Y - pad, m_Size.Width + 2* pad, m_Size.Height + 2 * pad);
            }

            #endregion

            #region Shared Methods

            /// <summary>
            /// Assigns the parent to a control
            /// </summary>
            /// <param name="child">the control to whose the parent to be assigned</param>
            /// <param name="parent">the control to be assigned as parent</param>
            protected static void setParent(Control child, Control parent)
            {
                child.m_parent = parent;
                if (parent.GetType() == typeof(HOD))
                    child.m_parentHOD = (HOD) parent;
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
            protected static bool IsMouseOver(Control ctrl)
            {
                return ctrl.m_MouseOver;
            }

            /// <summary>
            /// Checks if mouse is down over control.
            /// </summary>
            /// <param name="ctrl">Control for which we are finding mouse is over or not</param>
            /// <returns></returns>
            protected static bool IsMouseDown(Control ctrl)
            {
                return ctrl.m_MouseDown;
            }

            /// <summary>
            /// Selects next control in Tab Index Buffer from current in the parent
            /// </summary>
            /// <param name="ctrl"></param>
            /// <param name="parent"></param>
            protected static void SelectNextControl(Control ctrl, HOD parent)
            {
                Control next;

                if (parent.ActiveControl != parent)
                {
                    int index = 0;

                    parent.ActiveControl = null;

                    parent.UserInput.Update();


                    if (parent.ControlsSelectable.Count > 0)
                    {
                        index = parent.ControlsSelectable.IndexOf(ctrl);

                        if (index + 1 >= parent.ControlsSelectable.Count)
                            parent.SelectFirstControl();
                        else
                        {
                            parent.ControlsSelectable[index + 1].Focus();
                        }
                    }
                }
                else
                {
                    if (parent.ControlsSelectable.Count > 0)
                        ctrl.Focus();
                }
            }
       
            /// <summary>
            /// Selects previous control in Tab Index Buffer from current in the parent
            /// </summary>
            /// <param name="ctrl"></param>
            /// <param name="parent"></param>
            protected static void SelectPreviousControl(Control ctrl, HOD parent)
            {
                Control next;

                if (parent.ActiveControl != parent)
                {
                    int index = 0;

                    parent.ActiveControl = null;

                    parent.UserInput.Update();

                    next = null;

                    if (parent.ControlsSelectable.Count > 0)
                    {
                        index = parent.ControlsSelectable.IndexOf(ctrl);

                        if (index - 1 < 0)
                            parent.SelectLastControl();
                        else
                        {
                            next = parent.ControlsSelectable[index - 1];
                            next.Focus();
                        }
                    }
                }
                else
                {
                    if (parent.ControlsSelectable.Count > 0)
                        ctrl.Focus();
                }
            }

            /// <summary>
            /// Invokes Click event of the control
            /// </summary>
            /// <param name="c">Control to whom click event to be invoked!</param>
            protected static void PerformClick(Control c)
            {
                c.Click?.Invoke(c);
            }

            /// <summary>
            /// Invokes GotFocus event of the control
            /// </summary>
            /// <param name="c">Control to whom GotFocus event to be invoked!</param>
            protected static void ControlGotFocus(Control c)
            {
                c.m_Focused = true;
                c.GotFocus?.Invoke(c);
            }

            /// <summary>
            /// Invokes LostFocus event of the control
            /// </summary>
            /// <param name="c">Control to whom LostFocus event to be invoked!</param>
            protected static void ControlLostFocus(Control c)
            {
                c.m_Focused = false;
                c.LostFocus?.Invoke(c);
            }
            
            public static T ConvertToInterface<T>(object initial)
            {
                T output;
                try
                {
                output = (T)initial;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException("Cannot Covert " + initial.GetType().FullName + " to " + typeof(T).GetType().FullName);
                }

                return output;
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
                        m_textFormat?.Dispose();
                        m_brush?.Dispose();
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
}
