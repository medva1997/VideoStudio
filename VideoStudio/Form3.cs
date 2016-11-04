using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace VideoStudio
{
    public partial class Form3 : Form
    {
        private bool all_good = false;
        private string param1, param2, param7, param4, param6;
        public string mainparam;

        public Form3()//основные окна настройки
        {            
            InitializeComponent();           
            textBox4.Visible = true;
            textBox5.Visible = true;           
            checkBox3.Checked = false;
            textBox7.Text = "-b:v 2000k -vcodec mpeg2video -f mpegts";
            textBox1_TextChanged(null, null);//принудительный вызов обработки событий
            textBox2_TextChanged(null, null);
            textBox7_TextChanged(null, null);
            textBox4_TextChanged(null, null);
            textBox6_TextChanged(null, null);
            textBox7_TextChanged(null, null);
            checkBox3_CheckedChanged(null, null);
            command_update();//обновление строки для ffmpeg
        }
      
        private void Form3_Load(object sender, EventArgs e)
        {
            textBox3.Enabled = false;
            command_update();//обновление строки для ffmpeg
        }        

        private void button1_Click(object sender, EventArgs e)// открытие диалогового окна выбора папки
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox3.Text = folderBrowserDialog1.SelectedPath;
            }

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)// выбор онлайн трансляции
        {
            if(checkBox3.Checked==true)
            {
                textBox4.Visible = true;
                textBox5.Visible = true;
                label4.Visible = true;

                label5.Visible = true;
                label6.Visible = true;
                textBox6.Visible = true;
                textBox7.Visible = true;
                label7.Visible = true;
                textBox4.Text = "127.0.0.1";
                textBox6.Text = "5000";
            }
            else
            {
                textBox4.Visible = false;
                textBox5.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                textBox6.Visible = false;
                textBox7.Visible = false;
                label7.Visible = false;
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)//ширина
        {
            try
            {
                if (textBox1.Text != "")
                {
                    int a = int.Parse(textBox1.Text);                   
                }
                
            }
            catch
            {
                MessageBox.Show("Некорректные данные");
            }
            
            if (textBox1.Text != "")
            {
                param1 = " -video_size " + textBox1.Text;
            }
            else
            {
                param1 = "";
            }
            command_update();

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//высота
        {
            try
            {
                if (textBox2.Text != "")
                {
                    int a = int.Parse(textBox2.Text);
                }

                if (textBox2.Text != "")
                {
                    param2 = "x" + textBox2.Text;
                }
                else
                {
                    param2 = "";
                }
                command_update();
            }
            catch
            {
                MessageBox.Show("Некорректные данные");
            }
        }

        private void button2_Click(object sender, EventArgs e)//сохранить
        {
            // сохранить
            all_good = true;

            //textbox1
            try
            {
                if (textBox1.Text != "")
                {
                    int a = int.Parse(textBox1.Text);
                }
            }
            catch
            {
                all_good = false;
                MessageBox.Show("Некорректные данные  в поле: Ширина выходной картинки");
            }

            //textbox2
            try
            {
                if (textBox2.Text != "")
                {
                    int a = int.Parse(textBox2.Text);
                }
            }
            catch
            {
                all_good = false;
                MessageBox.Show("Некорректные данные  в поле: Высота выходной картинки");
            }

            // проверка пути
            if (textBox3.Text != "")
            {
                if (Directory.Exists(textBox3.Text) == true)
                {

                }
                else
                {
                    all_good = false;
                    MessageBox.Show("Заданного пути не существует");
                }
            }

            try
            {
                int a = int.Parse(textBox6.Text);
            }
            catch
            {
                all_good = false;
                MessageBox.Show("Некорректные данные  в поле: Порт");
            }
            
            if(all_good==true)//закрытие
            {
                this.Close();
            }
            


        }

        private void button3_Click(object sender, EventArgs e)//отмена
        {
            //отмена
            all_good = false;
            this.Close();

        }

        #region Свойства для сохранения настроек

        public bool allgood
        {
            get
            {
                return all_good;
            }
        }

        public  int Widthofpicture
        {
            get
            {
                return Convert.ToInt32( textBox1.Text);
            }
            set
            {
                textBox1.Text =Convert.ToString( value);
            }
        }

        public int Heightofpicture
        {
            get
            {
                return Convert.ToInt32( textBox2.Text);
            }
            set
            {
                textBox2.Text = Convert.ToString(value);
            }
        }

        public bool rec_main
        {
            get
            {
                return checkBox1.Checked;
            }
            set
            {
                checkBox1.Checked = value;
            }
        }

        public bool rec_all
        {
            get
            {
                return checkBox2.Checked;
            }
            set
            {
                checkBox2.Checked = value;
            }
        }

        public string folder
        {
            get
            {
                return textBox3.Text;
            }
            set
            {
                textBox3.Text = value;
            }
        }

        public bool online
        {
            get
            {
                return checkBox3.Checked;
            }
            set
            {
                checkBox3.Checked = value;
               
            }
        }

        public string ipaddress
        {
            get
            {
                return textBox4.Text;
            }
            set
            {
                textBox4.Text = value;
            }
        }

        public int port
        {
            get
            {if(textBox6.Text=="")
            {
                return 5000;
            }
                return Convert.ToInt32(textBox6.Text);
            }
            set
            {
                textBox6.Text = Convert.ToString(value);
            }
        }

        public string more_settings
        {
            get
            {
                return textBox7.Text;
            }
            set
            {
                textBox7.Text = value;
            }
        }

        public string mainparams
        {
            get
            {
                return textBox5.Text;
            }
        }
        #endregion

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int a = int.Parse(textBox6.Text);

                
            }
            catch
            {               
                MessageBox.Show("Некорректные данные  в поле: Порт");
            }
            if (textBox6.Text != "")
            {
                param6 = ":" + textBox6.Text;
            }
            else
            {
                param6 = "";
            }
            command_update();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                param4 = " udp:" + textBox4.Text;
            }
            else
            {
                param4 = "";
            }
            command_update();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if (textBox7.Text != "")
            {
                param7 = " " + textBox7.Text;
            }
            else
            {
                param7= "";
            }
            command_update();
        }
        public void command_update()
        {
            textBox5.Text = param1 + param2 + param7 + param4 + param6;
        }
       
    }
}
