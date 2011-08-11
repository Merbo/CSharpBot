using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GUI
{
    public partial class GUI : Form
    {
        public GUI()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
            if (openFileDialog1.FileName.EndsWith(".csb"))
            {
                label2.ForeColor = Color.Green;
                label2.Text = "Compilable!";
            }
            else
            {
                label2.ForeColor = Color.Red;
                label2.Text = "Not Compilable.";
            }
        }

        private void GUI_Load(object sender, EventArgs e)
        {
            label2.ForeColor = Color.Yellow;
            label2.Text = "Not Selected.";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Scanner scanner = null;
                using (TextReader input = File.OpenText(openFileDialog1.FileName))
                {
                    scanner = new Scanner(input);
                }
                Parser parser = new Parser(scanner.Tokens);
                CodeGen codeGen = new CodeGen(parser.Result, Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + ".exe");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

    }
}
