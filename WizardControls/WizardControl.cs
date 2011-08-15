using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSBConfig.WizardControls
{
    public class WizardControl : TabControl
    {
        public WizardControl()
        {
                this.Appearance = TabAppearance.Normal;
                this.SizeMode = TabSizeMode.Fixed;
                this.ItemSize = new Size(16, 16);
        }

        protected override void WndProc(ref Message m)
        {
            // Hide tabs by trapping the TCM_ADJUSTRECT message
            if (m.Msg == 0x1328 && !DesignMode)
                m.Result = (IntPtr)1;
            else
                base.WndProc(ref m);
        }


        [Description("Show as a normal tab?"), DisplayName("TabControlStyle")]
        public bool TabControlStyle
        {
            get { return this.Appearance == TabAppearance.Normal; }
            set
            {
                if (!value)
                {
                    this.Appearance = TabAppearance.FlatButtons;
                    this.ItemSize = new Size(0, 1);
                    this.SizeMode = TabSizeMode.Fixed;
                }
                else
                {
                    this.ItemSize = new Size(16, 16);
                    this.Appearance = TabAppearance.Normal;
                    this.SizeMode = TabSizeMode.Fixed;
                }
            }
        }
    }
}
