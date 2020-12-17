using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D2D = SlimDX.Direct2D;
using DW = SlimDX.DirectWrite;

namespace SlimFrame.UI
{
    public class ContainerControl : Control, IContaierControl
    {
        #region Member Variables

        protected Control m_ActiveControl;    // Stores the active control of the Container Control

        protected List<Control> m_Controls;           // List containing the controls.
        protected List<Control> m_ControlsSelectable; // List containing the controls.

        protected D2D.Factory m_factory2D;        // Direct2D Factory for rending 2D objects.
        protected D2D.RenderTarget m_renderTarget;     // Direct2D RenderTarget for rendering 2D objects.


        #endregion

        #region Properties

        /// <summary>
        /// Active control of the HOD
        /// </summary>
        public Control ActiveControl
        {
            get
            {
                return m_ActiveControl;
            }
            set
            {
                if (value != null)
                { 
                    // Invokes LostFocus event of last active control if it is not null
                    if (m_ActiveControl != null)
                        ControlLostFocus(m_ActiveControl);

                    m_ActiveControl = value; // Sets the value to active control if it is not null
                    
                    // Invokes GotFocus event of new active control
                    ControlGotFocus(m_ActiveControl);
                }
            }
        }

        /// <summary>
        /// Selectable child controls of HOD
        /// </summary>
        public List<Control> ControlsSelectable
        {
            get
            {
                return m_ControlsSelectable;
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

        #endregion

        #region Constructor

        public ContainerControl()
        {
            m_Controls = new List<Control>();
            m_ControlsSelectable = new List<Control>();
        }

        #endregion 

        #region Methods

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
        /// Initializes the Container Control
        /// </summary>
        /// <param name="factory">DirectWrite factory for writing support</param>
        /// <param name="factory2D">Direct2D factory for rendering support</param>
        /// <param name="renderTarget">Direct2D renderTarget for rendering</param>
        public override void Init(DW.Factory factory, D2D.Factory factory2D, D2D.RenderTarget renderTarget)
        {
            base.Init(factory, factory2D, renderTarget);

            m_factory2D = factory2D;
            m_renderTarget = renderTarget;

            foreach (Control ctrl in m_Controls)
                ctrl.Init(factory, factory2D, renderTarget);
        }


        /// <summary>
        /// Renders the HOD
        /// </summary>
        /// <param name="renderTarget">The Direct2D RenderTarget for rendering.</param>
        public override void Render(D2D.RenderTarget renderTarget)
        {
            base.Render(renderTarget);
            m_renderTarget = renderTarget;

            if (!Visible) return;

            if (!m_Initialized)
                return;

            foreach (Control ctrl in m_Controls)
                ctrl.Render(renderTarget);
        }
                          
        /// <summary>
        /// Activates the control
        /// </summary>
        /// <param name="control">Control to be activated</param>
        public void ActivateControl(Control control)
        {
            if (control == null)
                return;

            ActiveControl = control;
            control.Focus();
        }
                              
        /// <summary>
        /// Selects first Selectable Control, if exists otherwise selects the HOD
        /// </summary>
        /// <returns>True if it succceds selecting first selectable control otherwise False</returns>
        public bool SelectFirstControl()
        {
            // Selects first control if exists

            if (m_ControlsSelectable.Count > 0 && m_ControlsSelectable.First() != null)
            {
                m_ControlsSelectable.First().Focus();
                return true;
            }
            else
            {
                ActiveControl = this;
                return false;
            }
        }

        /// <summary>
        /// Selects last Selectable Control, if exists otherwise selects HOD itself
        /// </summary>
        /// <returns>returns true, if it succceds selecting last selectable control; otherwise returns false</returns>
        public bool SelectLastControl()
        {
            if (m_ControlsSelectable.Count > 0 && m_ControlsSelectable.Last() != null)
            {
                m_ControlsSelectable.Last().Focus();
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
        /// <param name="ctrl">Control to be added</param>
        public void Add(Control ctrl)
        {
            m_Controls.Add(ctrl);
            setParent(ctrl, this); // Sets Container Control itself as parent
            ctrl.Init(m_factory, m_factory2D, m_renderTarget);

            FindSelectable();
            Sort();
        }

        /// <summary>
        /// Sorts controls based on TabIndex of Control
        /// </summary>
        public void Sort()
        {
            m_ControlsSelectable.Sort(delegate (Control x, Control y)
            {
                return x.TabIndex.CompareTo(y.TabIndex);
            });
        }

        /// <summary>
        /// Finds all selectable control and saves all of them to ControlsSelectable List
        /// </summary>
        public void FindSelectable()
        {
            m_ControlsSelectable = m_Controls.FindAll((x) =>
            {
                return x.TabStop && x.Enabled;
            });
        }

        /// <summary>
        /// Removes a control
        /// </summary>
        /// <param name="ctrl">Control to be removed</param>
        public void Remove(Control ctrl)
        {
            m_Controls.Remove(ctrl);
        }
        
        /// <summary>
        /// Updates the HOD
        /// </summary>
        public override void Update(double frameTime)
        {
            UserInput input;
            bool check = false;

            if (!Visible) return;

            if (!Enabled)
            {
                m_MouseOver = false;
                m_MouseDown = false;
                return;
            }

            if (!m_Initialized)
                return;

            if (GetType() != typeof(HOD))
            {
                base.Update(frameTime);
                input = m_parentHOD.UserInput;
            }
            else
            {
                input = ((HOD) this).UserInput;
            }


            foreach (Control ctrl in m_Controls)
            {
                ctrl.Update(frameTime);
                check |= IsMouseOver(ctrl) | IsMouseDown(ctrl);
            }

            // After operating | or OR operator on many boolean values 
            // If output having false means all other boolean values are false otherwise atleast one is true.
            // (It is OR operator so it is initialized with false value);

            // After operating & or AND operator on many boolean values 
            // If output having true means all other boolean values are true otherwise atleast one is false.
            // (for AND operator it should initialized with true value);

            if (!check) // Check if mouse is not over any control.
                if (input.IsMouseButtonPressed(MouseButtons.Left) ||
                    input.IsMouseButtonPressed(MouseButtons.Center) ||
                    input.IsMouseButtonPressed(MouseButtons.Right))
                    ActiveControl = this;

            if (ActiveControl == this)
            {
                if (input.IsKeyJustPressed(Key.Space) ||
                    input.IsKeyJustPressed(Key.NumberPadEnter) ||
                    input.IsKeyJustPressed(Key.Return))
                    PerformClick(this);

                if (input.IsKeyJustPressed(Key.Tab))
                {
                    if (input.IsKeyPressed(Key.LeftShift) || input.IsKeyPressed(Key.LeftShift))
                        SelectFirstControl();
                    else
                        SelectLastControl();
                }
            }
        }

        #endregion

        #region Disposal

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                foreach (Control ctrl in m_Controls)
                    ctrl?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

        }

        #endregion
    }
}
