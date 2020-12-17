using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D2D = SlimDX.Direct2D;
using DW = SlimDX.DirectWrite;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace SlimFrame
{
    namespace UI
    {
        /// <summary>
        /// Heads Over Display (HOD) for rendering 2D overlays, UI, etc.
        /// </summary>
        public class HOD : Control
        {

            #region Member Variables

            protected List<Control>     m_Controls;           // List containing the controls.

            //protected DW.Factory        m_factory;          // DirectWrite Factory for handling text.
            protected D2D.Factory       m_factory2D;        // Direct2D Factory for rending 2D objects.
            protected D2D.RenderTarget  m_renderTarget;     // Direct2D RenderTarget for rendering 2D objects.

            protected Point      m_MousePosition;    // Stores the mouse position
            protected Point      m_MousePositionOld;    // Stores the old mouse position

            protected Control   m_ActiveControl;// Stores the active control of the HOD
            protected Form      owner;         // Stores the owner form object.
            
            protected Color     m_ActivebrColor;// Stores the border color active control of the HOD

            public bool HighlightControl = true;
            public int FramePerSecond;

            #endregion

            #region Properties

            /// <summary>
            /// Active control of the HOD
            /// </summary>
            public Control ActiveControl {
                get
                {
                    return m_ActiveControl;
                }
                set
                {
                    if (value != null)
                    {
                        if (m_ActiveControl != null)
                            m_ActiveControl.BorderColor = m_ActivebrColor;

                        m_ActiveControl = value;
                        m_ActivebrColor = value.BorderColor;
                    }
                }
            }            

            /// <summary>
            /// Child controls of the HOD
            /// </summary>
            public List<Control> Controls
            {
                get
                {
                    return m_Controls;
                }
            }

            /// <summary>
            /// The position of the mouse pointer inside the form
            /// </summary>
            public Point MousePosition
            {
                get
                {
                    return m_MousePosition;
                }
                set
                {
                    m_MousePositionOld = m_MousePosition;
                    m_MousePosition = value;
                }
            }

            /// <summary>
            /// The old position of the mouse pointer inside the form
            /// </summary>
            public Point MousePositionOld
            {
                get
                {
                    return m_MousePositionOld;
                }
            }

            /// <summary>
            /// The Owner of the HOD, the form no which the HOD to be rendered.
            /// </summary>
            public Form Owner {
                get
                {
                    return owner;
                }
            }
            
            public D2D.RenderTarget RenderTarget
            {
                get
                {
                    return m_renderTarget;
                }
            }

            /// <summary>
            /// Handles the input from user.
            /// </summary>
            public UserInput UserInput;

            #endregion

            #region Constructor
                       
            /// <summary>
            /// Creates new instance of HOD
            /// </summary>
            /// <param name="factory">DirectWrite factory for writing support</param>
            /// <param name="factory2D">Direct2D factory for rendering support</param>
            /// <param name="renderTarget">Direct2D renderTarget for rendering</param>
            public HOD(DW.Factory factory, D2D.Factory factory2D, D2D.RenderTarget renderTarget)
            {
                m_Controls = new List<Control>();
                
                m_factory       = factory;
                m_factory2D     = factory2D;
                m_renderTarget  = renderTarget;

                Parent      = null;
                ParentHOD   = null;

                Name   = "HOD";
                m_font = new Font("Microsoft Sans Serif", 16);

                TabStop       = true;

                m_BackEnabled = false;
                m_TextEnabled = false;
                m_BorderWidth = 0.0f;
            }

            #endregion

            #region Public Methods
            
            public void TakeOwnerShip(Form form)
            {
                owner = form;

                SelelectFirstControl();
            }


            public bool SelelectFirstControl()
            {
                List<Control> c = m_Controls.FindAll(x => x.TabStop);
                c.Sort(delegate (Control x, Control y)
                {
                    return x.TabIndex.CompareTo(-1);
                });

                if (c[0] != null)
                {
                    c[0].Focus();
                    return true;
                }
                else
                {
                    ActiveControl = this;
                    return false;
                }
            }

            /// <summary>
            /// Add a control
            /// </summary>
            /// <param name="ctrl">The control to be added!</param>
            public void Add(Control ctrl)
            {
                m_Controls.Add(ctrl);
                setParent(ctrl, this);
                ctrl.Init(m_factory, m_factory2D, m_renderTarget);
            }

            /// <summary>
            /// Removes a control
            /// </summary>
            /// <param name="ctrl">The control to be removed</param>
            public void Remove(Control ctrl)
            {
                m_Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            #region Ovrriden Methods

            /// <summary>
            /// Releases some member variable for Releasing the RenderTarget
            /// </summary>
            public override void RenderTargetReleasing()
            {
                base.RenderTargetReleasing();

                if (!m_Initialized)
                    return;

                foreach (Control ctrl in m_Controls)
                    ctrl.RenderTargetReleasing();
            }

            /// <summary>
            /// Resized or Regenerates some member variable after Resizing the RenderTarget
            /// </summary>
            public override void RenderTargetResized(D2D.RenderTarget renderTarget)
            {
                base.RenderTargetResized(renderTarget);

                if (!m_Initialized)
                    return;

                foreach (Control ctrl in m_Controls)
                    ctrl.RenderTargetResized(renderTarget);
            }

            /// <summary>
            /// Initializes the HOD
            /// </summary>
            /// <param name="factory">DirectWrite factory for writing support</param>
            /// <param name="factory2D">Direct2D factory for rendering support</param>
            /// <param name="renderTarget">Direct2D renderTarget for rendering</param>
            public override void Init(DW.Factory factory,D2D.Factory factory2D, D2D.RenderTarget renderTarget)
            {
                base.Init(factory, factory2D, renderTarget);

                if (!m_Initialized)
                {
                    MousePosition = Owner.PointToClient(Cursor.Position);

                    foreach (Control ctrl in m_Controls)
                        ctrl.Init(factory, factory2D, renderTarget);

                    m_Initialized = true;
                }
            }
          
            /// <summary>
            /// Renders the HOD
            /// </summary>
            /// <param name="renderTarget">The Direct2D RenderTarget for rendering.</param>
            public override void Render(D2D.RenderTarget renderTarget)
            {
                base.Render(renderTarget);
                m_renderTarget = renderTarget;

                if (!m_Initialized)
                    return;

                foreach (Control ctrl in m_Controls)
                    ctrl.Render(renderTarget);
            }

            /// <summary>
            /// Updates the HOD
            /// </summary>
            public override void Update(double frameTime)
            {
                //base.Update(frameTime);
                UserInput input = UserInput;

                if (!m_Initialized)
                    return;
                bool check = false;

                MousePosition = Owner.PointToClient(Cursor.Position);

                foreach (Control ctrl in m_Controls)
                {
                    ctrl.Update(frameTime);
                    check = check || (IsMouseOver(ctrl) || IsMouseDown(ctrl));
                }

                // After operating || or OR operator on many boolean values 
                // If output having false means all other boolean values are false.
                // (It is OR operator so it is initialized with false value);

                // After operating && or AND operator on many boolean values 
                // If output having true means all other boolean values are true.
                // (for AND operator it should initialized with true value);

                if (!check) // Check if mouse is not over any control.
                    if (UserInput.IsMouseButtonPressed(MouseButtons.Left)   ||  
                        UserInput.IsMouseButtonPressed(MouseButtons.Center) ||
                        UserInput.IsMouseButtonPressed(MouseButtons.Right))
                        ActiveControl = this;

                if(ActiveControl == this)
                {
                    if (input.IsKeyJustPressed(Key.Space) ||
                        input.IsKeyJustPressed(Key.NumberPadEnter) ||
                        input.IsKeyJustPressed(Key.Return))
                        PerformClick(this);

                    if (input.IsKeyJustPressed(Key.Tab))
                        SelelectFirstControl();
                }
            }

            #endregion


            #endregion

            #region Disposal

            private bool m_Disposed = false; // To detect redundant calls

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (!m_Disposed)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                        foreach (Control ctrl in m_Controls)
                            ctrl?.Dispose();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    m_Disposed = true;
                }
            }

            #endregion
        }
    }
}
