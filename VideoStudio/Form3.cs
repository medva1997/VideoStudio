using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VideoStudio
{
    public partial class Form3 : Form
    {
      
        private bool recorder = false;
        private bool workserver = false;
        private string ipadress;
        private string folder;

        public Form3()
        {
            InitializeComponent();
            textBox2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                recorder = true;
            }
            else
            {
                recorder = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                textBox2.Enabled = true;
            }
            else
            {
                textBox2.Enabled = false;
                textBox2.Text = "";

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            ipadress = textBox2.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                recorder = true;
            }
            else
            {
                recorder = false;
            }

            folder = textBox1.Text;
            ipadress = textBox2.Text;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        public string folder_for_records
        {
            get
            {
                return textBox1.Text;
            }
        }

        public string ip
        {
            get
            {
                return ipadress;
            }
        }

        public bool do_record
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        public bool have_ip
        {
            get
            {
                return checkBox2.Checked;
            }
        }
    }
}
