using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using D2D = SlimDX.Direct2D;
using DW = SlimDX.DirectWrite;
using SlimDX.Direct2D;
using System.Drawing;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using SlimDX.Windows;

namespace SlimFrame.UI
{
    /// <summary>
    /// ContainerControl : Contains controls inside
    /// </summary>
    public class DXContainerControl : DXControl, IContaierControl
    {
        #region Member Variables

        protected DXControl m_ActiveControl;    // Stores the active control of the Container Control

        protected List<DXControl> m_Controls;           // List containing the controls.
        protected List<DXControl> m_ControlsSelectable; // List containing the controls.

        protected Factory m_factory2D;        // Direct2D Factory for rending 2D objects.
        protected RenderTarget m_renderTarget;     // Direct2D RenderTarget for rendering 2D objects.


        #endregion

        #region Properties

        /// <summary>
        /// Active control of the HUD
        /// </summary>
        public DXControl ActiveControl
        {
            get
            {
                return m_ActiveControl;
            }
            set
            {
                if (value != null && value != m_ActiveControl)
                {
                    // Invokes LostFocus event of last active control if it is not null
                    if (m_ActiveControl != null)
                        if (m_ActiveControl != this)
                            ControlLostFocus(m_ActiveControl);

                    m_ActiveControl = value; // Sets the value to active control if it is not null

                    // Invokes GotFocus event of new active control
                    ControlGotFocus(m_ActiveControl);
                }
            }
        }

        /// <summary>
        /// Selectable child controls of HUD
        /// </summary>
        public List<DXControl> ControlsSelectable
        {
            get
            {
                return m_ControlsSelectable;
            }
        }
        /// <summary>
        /// Child controls of the HUD
        /// </summary>
        public List<DXControl> Controls
        {
            get
            {
                return m_Controls;
            }
        }

        #endregion

        #region Constructor

        public DXContainerControl()
                : base()
        {
            m_Controls = new List<DXControl>();
            m_ControlsSelectable = new List<DXControl>();
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

            foreach (DXControl ctrl in m_Controls)
                ctrl.RenderTargetReleasing();
        }

        /// <summary>
        /// Resized or Regenerates some member variable after Resizing the RenderTarget
        /// </summary>
        public override void RenderTargetResized(RenderTarget renderTarget)
        {
            base.RenderTargetResized(renderTarget);

            if (!m_Initialized)
                return;

            foreach (DXControl ctrl in m_Controls)
                ctrl.RenderTargetResized(renderTarget);
        }


        /// <summary>
        /// Initializes the Container Control
        /// </summary>
        /// <param name="factory">DirectWrite factory for writing support</param>
        /// <param name="factory2D">Direct2D factory for rendering support</param>
        /// <param name="renderTarget">Direct2D renderTarget for rendering</param>
        public override void Init(DW.Factory factory, Factory factory2D, RenderTarget renderTarget)
        {
            base.Init(factory, factory2D, renderTarget);

            m_factory2D = factory2D;
            m_renderTarget = renderTarget;

            foreach (DXControl ctrl in m_Controls)
                ctrl.Init(factory, factory2D, renderTarget);

            Loaded += (c) =>
            {
                foreach (var item in Controls)
                {
                    Load(item);

                    DXRadioButton radio = (DXRadioButton)ControlsSelectable.Find(x => x.GetType() == typeof(DXRadioButton));
                    if (radio != null)
                        radio.Checked = true;
                }
            };
        }


        /// <summary>
        /// Renders the HUD
        /// </summary>
        /// <param name="renderTarget">The Direct2D RenderTarget for rendering.</param>
        public override void Render(RenderTarget renderTarget)
        {
            base.Render(renderTarget);
            m_renderTarget = renderTarget;

            if (!Visible) return;

            if (!m_Initialized)
                return;

            renderTarget.PushAxisAlignedClip(Bound, AntialiasMode.Aliased);

            foreach (DXControl ctrl in m_Controls)
                ctrl.Render(renderTarget);

            renderTarget.PopAxisAlignedClip();
        }

        /// <summary>
        /// Activates the control
        /// </summary>
        /// <param name="control">Control to be activated</param>
        public void ActivateControl(DXControl control)
        {
            if (control == null)
                return;

            ActiveControl = control;
            control.Focus();
        }

        /// <summary>
        /// Selects first Selectable Control, if exists otherwise selects the HUD
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
        /// Selects last Selectable Control, if exists otherwise selects HUD itself
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
        public void Add(DXControl ctrl)
        {
            m_Controls.Add(ctrl);
            SetParent(ctrl, this); // Sets Container Control itself as parent
            ctrl.Init(m_factory, m_factory2D, m_renderTarget);

            Sort();
            FindSelectable();
        }

        /// <summary>
        /// Sorts controls based on TabIndex of Control
        /// </summary>
        public void Sort()
        {
            m_Controls.Sort(delegate (DXControl x, DXControl y)
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
                return x.TabStop && x.Enabled && x.Visible;
            });
        }

        /// <summary>
        /// Removes a control
        /// </summary>
        /// <param name="ctrl">Control to be removed</param>
        public void Remove(DXControl ctrl)
        {
            m_Controls.Remove(ctrl);
        }

        /// <summary>
        /// Updates the HUD
        /// </summary>
        public override void Update(double frameTime)
        {
            UserInput input;
            PointF mousePos;
            GameWindow window;

            bool check = false;
            bool mDownOld = m_MouseDown;
            m_MouseOver = false;
            m_MouseDown = false;

            if (!Visible) return;

            if (!Enabled) return;

            if (!m_Initialized) return;

            if (GetType() != typeof(HUD))
            {
                base.Update(frameTime);
                input = m_parentHOD.UserInput;
                mousePos = ParentHOD.MousePosition;
                window = ParentHOD.Owner;
            }
            else
            {
                input = ((HUD)this).UserInput;
                mousePos = ((HUD)this).MousePosition;
                window = ((HUD)this).Owner;
            }

            if (input.InputPaused)
                return;

            for (int i = m_Controls.Count - 1; i > 0; i--)
            {
                if (!check || i == 0)
                    MouseLookUp(m_Controls[i]);
                m_Controls[i].Update(frameTime);
                m_MouseDown |= IsMouseDown(m_Controls[i]);
                m_MouseOver |= IsMouseOver(m_Controls[i]);

                check = m_MouseDown | m_MouseOver;
            }


            m_MouseDown = !m_MouseDown;
            m_MouseOver = !m_MouseOver;
            
            if (m_MouseDown && IntersectMouse(mousePos, window.FormObject.ClientRectangle) && !window.IsPaused)
            {
                if (input.IsMouseButtonPressed(MouseButtons.Left) && !input.IsMouseButtonHeldDown(0))
                {
                    if (window.Focused)
                        PerformClick(this);
                }
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
                //    if (input.IsKeyJustPressed(Key.Space) ||
                //        input.IsKeyJustPressed(Key.NumberPadEnter) ||
                //        input.IsKeyJustPressed(Key.Return))
                //        PerformClick(this);

                if (input.IsKeyJustPressed(Key.Tab))
                {
                    if (input.IsKeyPressed(Key.LeftShift) || input.IsKeyPressed(Key.LeftShift))
                        SelectFirstControl();
                    else
                        SelectLastControl();
                }
            }

            if (this.GetType() == typeof(HUD))
                if (!IntersectMouse(((HUD)this).MousePosition, this.Bound))
                {
                    m_MouseOver = false;
                }
        }

        #endregion

        #region Disposal

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                foreach (DXControl ctrl in m_Controls)
                    ctrl?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            base.Dispose(disposing);
        }

        #endregion
    }
}
