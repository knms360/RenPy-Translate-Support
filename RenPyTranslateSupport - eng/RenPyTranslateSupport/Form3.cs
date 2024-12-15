using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RenPyTranslateSupport
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            textBox1.Text = Form1.replaceplayer;
            textBox2.Text = Form1.defaultprocess;
            textBox3.Text = Form1.kirisute;
            checkBox1.Checked = Form1.renpymodmode;
            comboBox1.SelectedIndex = 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.replaceplayer = textBox1.Text;
            Form1.defaultprocess = textBox2.Text;
            Form1.kirisute = textBox3.Text;
            Form1.renpymodmode = checkBox1.Checked;
            if (comboBox1.SelectedIndex == 0)
            {
                Form1.language = "JP";
            }
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to reset the settings? Resetting will restart the software.\n(The memo is not delete.)", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

            //何が選択されたか調べる
            if (result == DialogResult.Yes)
            {
                Form1.resetsetting = true;
                Close();
            }
        }
    }
}
