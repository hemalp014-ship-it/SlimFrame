using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DW = SlimDX.DirectWrite;
using d2d = SlimDX.Direct2D;


namespace SlimFrame.UI
{
    /// <summary>
    /// Label : Text Container
    /// </summary>
    public class DXPictureBox : DXControl
    {
        #region Member Variables

        protected d2d.Bitmap m_bitmap;

        protected bool m_AutoSize = false; // Stores auto size state of label

        #endregion

        #region Properties

        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// Image of the PictureBox
        /// </summary>
        public d2d.Bitmap Image
        {
            get
            {
                return m_bitmap;
            }
            set
            {
                m_bitmap = value;
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

        #endregion

        #region Constructors

        public DXPictureBox()
                : base()
        {
            Init();
        }
        public DXPictureBox(string name)
                : base()
        {
            Name = name;
            Init();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the Label
        /// </summary>
        private void Init()
        {
            m_BorderWidth = 0.0f;

            m_TabStop = false;
            m_dTextMeasuredSize = false;
            AutoSize = false;
            m_TextEnabled = false;
        }

        /// <summary>
        /// Initializes the 2D rendering capability
        /// </summary>
        /// <param name="factory">factory for creating DirectWrite Objects</param>
        /// <param name="factory2D">factory for creating Direct2D Objects</param>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Init(DW.Factory factory, d2d.Factory factory2D, d2d.RenderTarget renderTarget)
        {
            base.Init(factory, factory2D, renderTarget);
        }

        /// <summary>
        /// Renders the Label
        /// </summary>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Render(d2d.RenderTarget renderTarget)
        {
            base.Render(renderTarget);

            if (!Visible) return;

            if (m_bitmap != null)
                renderTarget.DrawBitmap(m_bitmap, ClientRectangle, Opacity > 0.0 ? Opacity : CalculatedAlpha);
        }

        /// <summary>
        /// Updates the Label
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(double frameTime)
        {
            base.Update(frameTime);

            if (!Visible) return;

            if (!Enabled) return;

        }

        public void LoadImage(string Imagepath)
        {
            if (ParentHOD != null)
                m_bitmap = System2D.LoadBitmap(ParentHOD.RenderTarget, Imagepath);
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
                m_bitmap?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

    }
}