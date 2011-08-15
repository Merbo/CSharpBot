using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CSharpBot;

namespace CSBConfig
{
    public partial class CSBConfigWizard : Form
    {
        XmlConfiguration config = new XmlConfiguration();
        
        public CSBConfigWizard()
        {
            InitializeComponent();

            config.Reset();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                this.groupBox1.Enabled = true;
            }
            else
            {
                this.twoLinedInputField7.InputValue = this.twoLinedInputField6.InputValue = "";
                this.groupBox1.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            config.EnableFileLogging = simpleInputField3.Enabled = checkBox2.Checked;
        }

        private void simpleInputField1_ValueChanged(object sender, EventArgs e)
        {
            if (simpleInputField1.InputValue.Trim() != "")
            {
                config.Server = simpleInputField1.InputValue;
            }
            else
            {
                simpleInputField1.Focus();
                MessageBox.Show("You need to enter a server.");
            }
        }

        private void simpleInputField2_Load(object sender, EventArgs e)
        {
            if (simpleInputField2.InputValue.Trim() != "")
            {
                int p;
                if (int.TryParse(simpleInputField2.InputValue, out p))
                {
                    config.Port = p;
                }
                else
                {
                    simpleInputField2.Focus();
                    MessageBox.Show("You need to enter a valid port.");
                }
            }
            else
            {
                config.Port = 6667;
                simpleInputField2.InputValue = "6667";
            }
        }

        private void twoLinedInputField2_Load(object sender, EventArgs e)
        {
            WizardControls.TwoLinedInputField ctl = (WizardControls.TwoLinedInputField)sender;
            if (ctl.InputValue.Trim() != "")
            {
                config.Channel = ctl.InputValue;
            }
            else
            {
                ctl.Focus();
                MessageBox.Show("You need to enter a channel.");
            }
        }

        private void twoLinedInputField3_Load(object sender, EventArgs e)
        {
            WizardControls.TwoLinedInputField ctl = (WizardControls.TwoLinedInputField)sender;
            if (ctl.InputValue.Trim() != "")
            {
                config.Nickname = ctl.InputValue;
            }
            else
            {
                ctl.Focus();
                MessageBox.Show("You need to enter a nickname.");
            }
        }

        private void twoLinedInputField1_Load(object sender, EventArgs e)
        {
            WizardControls.TwoLinedInputField ctl = (WizardControls.TwoLinedInputField)sender;
            if (ctl.InputValue.Trim() != "")
            {
                config.Realname = ctl.InputValue;
            }
            else
            {
                ctl.InputValue = "CSharpBot";
                config.Realname = "CSharpBot";
            }
        }

        private void twoLinedInputField6_Load(object sender, EventArgs e)
        {
            WizardControls.TwoLinedInputField ctl = (WizardControls.TwoLinedInputField)sender;
            config.NickServAccount = ctl.InputValue;
        }

        private void twoLinedInputField7_Load(object sender, EventArgs e)
        {
            WizardControls.TwoLinedInputField ctl = (WizardControls.TwoLinedInputField)sender;
            config.NickServPassword = ctl.InputValue;
            
        }

        private void simpleInputField3_Load(object sender, EventArgs e)
        {
            if (config.EnableFileLogging)
            {
                if (((WizardControls.SimpleInputField)sender).InputValue.Trim() != "")
                {
                    config.Logfile = ((WizardControls.SimpleInputField)sender).InputValue;
                }
                else
                {
                    ((WizardControls.SimpleInputField)sender).Focus();
                    MessageBox.Show("You need to enter a filename or a path.");
                }
            }
        }

        private void twoLinedInputField4_Load(object sender, EventArgs e)
        {
            WizardControls.TwoLinedInputField ctl = (WizardControls.TwoLinedInputField)sender;
            try
            {
                Regex HostmaskRegex = new Regex(config.OwnerHostMask = "^" + ctl.InputValue.Replace(".", "\\.").Replace("*", ".+") + "$");
            }
            catch (Exception)
            {
                ctl.Focus();
                MessageBox.Show("Not a valid hostmask.");
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = ((CheckBox)sender).Checked;
        }

        private void AddSummary(TabPage tab)
        {
            textBox1.AppendText("\r\n");
            textBox1.AppendText("== " + tab.Text + " ==\r\n");
        }
        private void AddSummary(WizardControls.SimpleInputField input)
        {
            textBox1.AppendText(input.InputName.Trim(':') + ": " + (input.InputValue != "" ? (input.UseSystemPasswordChar ? "(set)" : input.InputValue) : "(empty)") + "\r\n");
        }
        private void AddSummary(WizardControls.TwoLinedInputField input)
        {
            textBox1.AppendText(input.InputName.Trim(':') + ": " + (input.InputValue != "" ? (input.UseSystemPasswordChar ? "(set)" : input.InputValue) : "(empty)") + "\r\n");
        }
        private void AddSummary(string name, TextBox input)
        {
            textBox1.AppendText(name.Trim(':') + ": " + (input.Text != "" ? (input.UseSystemPasswordChar ? "(set)" : input.Text) : "(empty)") + "\r\n");
        }
        private void AddSummary(CheckBox input)
        {
            textBox1.AppendText(input.Text.Trim(':') + ": " + (input.Checked ? "Enabled" : "Disabled") + "\r\n");
        }

        private void textBox1_Validated(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.wizardControl1.SelectedIndex++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.wizardControl1.SelectedIndex--;
        }

        private void textBox1_Layout(object sender, LayoutEventArgs e)
        {
            DoSummaryReport();
        }

        private void DoSummaryReport()
        {

            textBox1.Clear();

            AddSummary(tabPage2);
            AddSummary(simpleInputField1);
            AddSummary(simpleInputField2);
            AddSummary(twoLinedInputField1);

            AddSummary(tabPage3);
            AddSummary(twoLinedInputField2);
            AddSummary(twoLinedInputField3);
            AddSummary(twoLinedInputField5);

            AddSummary(tabPage4);
            AddSummary(checkBox1);
            if (checkBox1.Checked)
            {
                AddSummary(twoLinedInputField6);
                AddSummary(twoLinedInputField7);
            }

            AddSummary(tabPage5);
            AddSummary(checkBox2);
            if (checkBox2.Checked)
            {
                AddSummary(simpleInputField3);
            }
            AddSummary(twoLinedInputField4);
            AddSummary(twoLinedInputField8);


            AddSummary(tabPage6);
            AddSummary(checkBox3);
            if (checkBox3.Checked)
            {
                AddSummary(twoLinedInputField9);
            }

        }

        private void wizardControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Text = "CSharpBot Configuration Tool - " + wizardControl1.SelectedTab.Text;
            btnBack.Enabled = wizardControl1.SelectedIndex > 0;

            if (wizardControl1.SelectedTab == tabPage8)
            {
                // Configuration should run now! "Configuring..." tab page open:
                panel1.Hide();
                configWorker.RunWorkerAsync();
            }
            else panel1.Show();

            if (wizardControl1.SelectedIndex >= wizardControl1.TabPages.Count - 2)
            {
                btnNext.Visible = btnBack.Visible = btnCancel.Visible = !(btnFinish.Visible = true);
            }
        }

        private void progressBar1_Layout(object sender, LayoutEventArgs e)
        {
        }

        

        private void configWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (wizardControl1.InvokeRequired)
            {
                wizardControl1.Invoke(new DoWorkEventHandler(configWorker_DoWork), sender, e);
                return;
            }
            try
            {
                config.Save();
                wizardControl1.SelectedIndex ++; // next page
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong while saving the XML configuration.");
                wizardControl1.SelectedIndex = wizardControl1.TabPages.Count - 1; // last page = cancel and error page
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to cancel the configuration wizard?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                wizardControl1.SelectedIndex = wizardControl1.TabPages.Count - 1; // last page = cancel and error page
            }
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void twoLinedInputField5_ValueChanged(object sender, EventArgs e)
        {
            config.ServerPassword = twoLinedInputField5.InputValue;
        }

        private void twoLinedInputField8_ValueChanged(object sender, EventArgs e)
        {
            if ((config.Prefix = twoLinedInputField8.InputValue).Length < 1)
            {
                twoLinedInputField8.Focus();
                MessageBox.Show("You need to enter a prefix!");
            }
        }

        private void twoLinedInputField9_ValueChanged(object sender, EventArgs e)
        {
            config.LiveserverPassword = twoLinedInputField9.InputValue;
        }

       
    }
}
