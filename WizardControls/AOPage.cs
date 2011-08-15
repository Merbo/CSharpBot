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
    public partial class AOPage : UserControl
    {
        public AOPage()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.pictureBox1.BackColor = Color.LightGray;
        }

        [Description("The title."), DisplayName("PageTitle")]
        public string PageTitle
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        [Description("The description."), DisplayName("PageDescription")]
        public string PageDescription
        {
            get { return label2.Text; }
            set { label2.Text = value; }
        }

        [Description("The title font."), DisplayName("PageTitleFont")]
        public Font PageTitleFont
        {
            get { return label1.Font; }
            set { label1.Font = value; }
        }

        [DisplayName("PageImage"), Description("The image.")]
        public Image PageImage
        {
            get { return pictureBox1.BackgroundImage; }
            set { pictureBox1.BackgroundImage = value; }
        }
    }
}
