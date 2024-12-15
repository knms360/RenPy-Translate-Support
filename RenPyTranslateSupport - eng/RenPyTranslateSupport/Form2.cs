using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RenPyTranslateSupport
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            foreach (string strFilePath in Form1.untranslatepath)
            {
                comboBox1.Items.Add(strFilePath);
            }
            foreach (string strFilePath in Form1.translatedpath)
            {
                comboBox2.Items.Add(strFilePath);
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private List<string> untranslatepath = new List<string>();
        private List<string> translatedpath = new List<string>();

        private void button1_Click(object sender, EventArgs e)
        {
            untranslatepath.Add(comboBox1.Text);
            translatedpath.Add(comboBox2.Text);
            comboBox1.Items.Remove(comboBox1.Text);
            comboBox2.Items.Remove(comboBox2.Text);
            if (comboBox1.Items.Count == 0)
            {
                Form1.translatedpath = translatedpath;
                Form1.untranslatepath = untranslatepath;
                Form1.linkfilesuccess = true;
                Close();
            }
            else
            {
                comboBox1.SelectedIndex = 0;
                comboBox1.Refresh();
                comboBox2.SelectedIndex = 0;
                comboBox2.Refresh();
            }
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void comboBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }
    }
}
