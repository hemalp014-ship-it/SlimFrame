using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame.UI
{
    public interface IContaierControl
    {
        /// <summary>
        /// Active Control of the Container
        /// </summary>
        DXControl ActiveControl { get; set; }

        /// <summary>
        /// Activate the Control
        /// </summary>
        /// <param name="control">The control to be activated</param>
        void ActivateControl(DXControl control);
    }
    
    public interface ITextControl
    {
        /// <summary>
        /// Determines whether the size of control to be adjusted according the content or not.
        /// </summary>
        bool AutoSize { get; set; }

        /// <summary>
        /// Determines Horizontal Allignment of text
        /// </summary>
         TextHAllignment HorizontalAllignment { get; set; }

        /// <summary>
        /// Determines Vertical Allignment of text
        /// </summary>
        TextVAllignment VerticalAllignment { get; set; }
    }
    public interface IEnabler
    {
        /// <summary>
        /// Determines whether the control is enabled or not.
        /// </summary>
        bool Enabled { get; set; }

    }
}
