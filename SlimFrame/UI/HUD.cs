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

namespace SlimFrame.UI
{
    /// <summary>
    /// Heads Over Display (HOD) for rendering 2D overlays, UI, etc.
    /// </summary>
    public class HUD : DXContainerControl
    {

        #region Member Variables

        public int FramePerSecond;     // Stores the current FPS

        protected Point m_MousePosition;    // Stores the mouse position
        protected Point m_MousePositionOld; // Stores the old mouse position

        protected GameWindow owner;              // Stores the owner form Object. 

        #endregion

        #region Properties

        /// <summary>
        /// Position of the mouse pointer with respect to the form
        /// </summary>
        public Point MousePosition
        {
            get
            {
                return m_MousePosition;
            }
            set
            {
                m_MousePositionOld = m_MousePosition; // Stores the last value of mouse position to old mouse position variable
                m_MousePosition = value;
            }
        }

        /// <summary>
        /// Old position of the mouse pointer with respect to the form
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
        public GameWindow Owner
        {
            get
            {
                return owner;
            }
        }

        /// <summary>
        /// Direct2D render taregt for rendering 2D system.
        /// </summary>
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

        /// <summary>
        /// Determines whether selected control should be higlighted or not.
        /// </summary>
        public bool HighlightSelectedControl = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates new instance of HOD
        /// </summary>
        public HUD()
            : base()
        {


            Parent = null;
            ParentHOD = null;

            TabStop = true;

            m_BackEnabled = false;
            m_BorderWidth = 0.0f;

            Name = "HOD";
            m_font = new Font("Microsoft Sans Serif", 16);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This helps the form to take owenership of the HOD
        /// </summary>
        /// <param name="form"></param>
        public void TakeOwnerShip(GameWindow window)
        {
            owner = window;
            Size = owner.FormObject.Size;

            owner.FormObject.SizeChanged += Owner_SizeChanged;

            //SelectFirstControl();
            ActivateControl(this);
            Load(this);
        }


        /// <summary>
        /// Initializes the HOD
        /// </summary>
        /// <param name="factory">DirectWrite factory for writing support</param>
        /// <param name="factory2D">Direct2D factory for rendering support</param>
        /// <param name="renderTarget">Direct2D renderTarget for rendering</param>
        public override void Init(DW.Factory factory, D2D.Factory factory2D, D2D.RenderTarget renderTarget)
        {
            base.Init(factory, factory2D, renderTarget);
        }

        /// <summary>
        /// Updates the HOD
        /// </summary>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            if (!m_Initialized) return;

            MousePosition = Owner.FormObject.PointToClient(Cursor.Position);

        }

        #endregion

        #region Event Handler

        private void Owner_SizeChanged(Object sender, EventArgs e)
        {
            Size = owner.FormObject.Size;
        }

        #endregion

        #region Disposal
        /// <summary>
        /// Disposes the HOD
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (owner != null)
                    owner.FormObject.SizeChanged -= Owner_SizeChanged;
            }

            base.Dispose(disposing);
        }

        #endregion

    }
}
