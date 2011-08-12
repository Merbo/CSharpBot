using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSharpBot
{
    public partial class ControlWindow : Form
    {
        public ControlWindow()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(ControlWindow_FormClosed);
            
        }

        void ControlWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            CSharpBot.bot.Shutdown();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CSharpBot.bot.Join(this.textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CSharpBot.bot.Leave(this.textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CSharpBot.bot.Shutdown();
            this.Enabled = false;
            while (CSharpBot.bot.IsRunning)
            {
                System.Threading.Thread.Sleep(1000);
            }
            this.Close();
        }

  

       private void timer1_Tick(object sender, EventArgs e)
       {
           if (!CSharpBot.bot.IsRunning){
               this.Close();
           }
       }

       private void button4_Click(object sender, EventArgs e)
       {
           LiveScript ls = new LiveScript();
           ls.RunScript(textBox2.Text);
           textBox2.Text = "";
       }
    }
}
