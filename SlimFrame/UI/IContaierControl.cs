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
        Control ActiveControl { get; set; }

        /// <summary>
        /// Activate the Control
        /// </summary>
        /// <param name="control">The control to be activated</param>
        void ActivateControl(Control control);
    }
}
