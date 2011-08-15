using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CSBConfig.WizardControls
{
    [DefaultEvent("ValueChanged")]
    public partial class TwoLinedInputField : UserControl
    {
        public TwoLinedInputField()
        {
            InitializeComponent();
            textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
        }

        void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (this.ValueChanged != null)
                this.ValueChanged(this, e);
        }

        [Description("Raises whenever the value of the textbox is changed. (Same as TextChanged for TextBox)"), DisplayName("ValueChanged")]
        public event EventHandler ValueChanged;

        [Description("The label text of this input field."), DisplayName("InputName")]
        public string InputName
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        [Description("The value of the textbox."), DisplayName("InputValue")]
        public string InputValue
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        [Description("The font of the text box."), DisplayName("InputTextBoxFont")]
        public Font InputTextBoxFont
        {
            get { return textBox1.Font; }
            set { textBox1.Font = value; }
        }

        [Description("Alignment of the value text."), DisplayName("TextAlign")]
        public HorizontalAlignment TextAlign
        {
            get { return textBox1.TextAlign; }
            set { textBox1.TextAlign = value; }
        }

        [Description("Use system password char instead of visible letters."), DisplayName("UseSystemPasswordChar")]
        public bool UseSystemPasswordChar
        {
            get { return textBox1.UseSystemPasswordChar; }
            set { textBox1.UseSystemPasswordChar = value; }
        }

        [Description("Maximum length."), DisplayName("MaxLength")]
        public int MaxLength
        {
            get { return textBox1.MaxLength; }
            set { textBox1.MaxLength = value; }
        }
    }
}
