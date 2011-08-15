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
    public partial class WizardHeader : UserControl
    {
        public WizardHeader()
        {
            InitializeComponent();
            this.Dock = DockStyle.Top;
        }

        [DisplayName("HeaderTitle"), Description("The title of the header")]
        public string HeaderTitle
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        [DisplayName("HeaderDescription"), Description("The description of the header")]
        public string HeaderDescription
        {
            get { return label2.Text; }
            set { label2.Text = value; }
        }

        [DisplayName("HeaderImage"), Description("The image of the header")]
        public Image HeaderImage
        {
            get { return pictureBox1.BackgroundImage; }
            set { pictureBox1.BackgroundImage = value; }
        }
    }
}
