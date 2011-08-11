using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

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
                label2.ForeColor = Color.Lime;
                label2.Text = "Compilable!";
                button2.Enabled = true;
                button3.Enabled = false;
            }
            else
            {
                label2.ForeColor = Color.Red;
                label2.Text = "Not Compilable.";
                button2.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void GUI_Load(object sender, EventArgs e)
        {
            label2.ForeColor = Color.Yellow;
            label2.Text = "Not Selected.";
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label2.ForeColor = Color.Cyan;
            label2.Text = "Being Compiled.";
            try
            {
                Scanner scanner = null;
                using (TextReader input = File.OpenText(openFileDialog1.FileName))
                {
                    scanner = new Scanner(input);
                }
                Parser parser = new Parser(scanner.Tokens);
                CodeGen codeGen = new CodeGen(parser.Result, Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + ".exe");
                label2.ForeColor = Color.Lime;
                label2.Text = "Compiled! Check the directory this program is in, or click 'Test File' and check the console.";
                button3.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                label2.ForeColor = Color.Red;
                label2.Text = "Error-prone. Check the console.";
                button3.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            string[] fn = openFileDialog1.FileName.Split('\\');
            try
            {
                start.FileName = fn.Last().Replace(".csb", ".exe");
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        Console.Write(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
