using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using DW = SlimDX.DirectWrite;
using D2D = SlimDX.Direct2D;

namespace SlimFrame.UI
{
    /// <summary>
    /// Label : Text Container
    /// </summary>
    public class DXLabel : DXControl, ITextControl
    {
        #region Member Variables

        //Text allignment support.
        protected TextHAllignment m_textHAllignment; // Stores horizontal text allignment
        protected TextVAllignment m_textVAllignment; // Stores Vertical text allignment

        protected bool m_AutoSize = false; // Stores auto size state of label

        #endregion

        #region Properties

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

        public DXLabel()
            : base()
        {
            Init();
        }
        public DXLabel(string name)
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

            TextChanged += ChangedText;

            m_TabStop = false;
            AutoSize = true;
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
        }

        /// <summary>
        /// Renders the Label
        /// </summary>
        /// <param name="renderTarget">Render Target for 2D rendering</param>
        public override void Render(D2D.RenderTarget renderTarget)
        {
            base.Render(renderTarget);

            if (!Visible) return;

            m_brush.Color = ForeColor;
            renderTarget.DrawText(m_Text, m_textFormat, Bound, m_brush);
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
                TextChanged -= ChangedText;
            }

            base.Dispose(disposing);
        }

        #endregion

    }
}