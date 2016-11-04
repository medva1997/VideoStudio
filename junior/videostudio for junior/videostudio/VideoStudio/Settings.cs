using System;
using System.IO;
using System.Windows.Forms;

namespace VideoStudio
{
    public partial class Settings : Form
    {
        private bool allGood;
        private string param1, param2, param7, param4, param6, mainParam;

        public Settings()    // Основные окна настройки
        {            
            InitializeComponent();           
            textBox4.Visible = true;
            textBox5.Visible = true;           
            checkBox3.Checked = false;
            textBox7.Text = "-b:v 3500k -vcodec mpeg2video -f mpegts";
            TextBox1TextChanged(null, null);//принудительный вызов обработки событий
            TextBox2TextChanged(null, null);
            TextBox7TextChanged(null, null);
            TextBox4TextChanged(null, null);
            TextBox6TextChanged(null, null);
            TextBox7TextChanged(null, null);
            СheckBox3CheckedChanged(null, null);
            CommandUpdate();    // Обновление строки для ffmpeg
        }
      
        private void SettingsLoad(object sender, EventArgs e)
        {
            textBox3.Enabled = false;
            CommandUpdate();    // Обновление строки для ffmpeg
        }        

        private void Button1Click(object sender, EventArgs e)    // Открытие диалогового окна выбора папки
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
                textBox3.Text = folderBrowserDialog1.SelectedPath;
        }

        private void СheckBox3CheckedChanged(object sender, EventArgs e)    // Выбор онлайн трансляции
        {
            if(checkBox3.Checked)
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

        private void TextBox1TextChanged(object sender, EventArgs e)    // Ширина
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
                param1 = " -video_size " + textBox1.Text;
            else
                param1 = "";
        }

        private void TextBox2TextChanged(object sender, EventArgs e)    // Высота
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
                CommandUpdate();
            }
            catch
            {
                MessageBox.Show("Некорректные данные");
            }
        }

        private void Button2Click(object sender, EventArgs e)    // Сохранение
        {
            // Сохранить
            allGood = true;

            // textBox1
            try
            {
                if (textBox1.Text != "")
                {
                    int a = int.Parse(textBox1.Text);
                }
            }
            catch
            {
                allGood = false;
                MessageBox.Show("Некорректные данные  в поле: Ширина выходной картинки");
            }

            // textBox2
            try
            {
                if (textBox2.Text != "")
                {
                    int a = int.Parse(textBox2.Text);
                }
            }
            catch
            {
                allGood = false;
                MessageBox.Show("Некорректные данные  в поле: Высота выходной картинки");
            }

            // Проверка пути
            if (textBox3.Text != "")
            {
                if (!Directory.Exists(textBox3.Text))
                {
                    allGood = false;
                    MessageBox.Show("Заданного пути не существует");
                }
            }

            try
            {
                int a = int.Parse(textBox6.Text);
            }
            catch
            {
                allGood = false;
                MessageBox.Show("Некорректные данные  в поле: Порт");
            }
            
            if(allGood)   // Закрытие
                Close();
        }

        private void Button3Click(object sender, EventArgs e)    // Отмена
        {
            allGood = false;
            Close();
        }

        #region Свойства для сохранения настроек

        public bool AllGood
        {
            get { return allGood; }
        }

        public int PictureWidth
        {
            get { return Convert.ToInt32(textBox1.Text); }
            set { textBox1.Text = value.ToString(); }
        }

        public int PictureHeight
        {
            get { return Convert.ToInt32(textBox2.Text); }
            set { textBox2.Text = value.ToString(); }
        }

        public bool RecMain
        {
            get { return checkBox1.Checked; }
            set { checkBox1.Checked = value; }
        }

        public bool RecAll
        {
            get { return checkBox2.Checked; }
            set { checkBox2.Checked = value; }
        }

        public string Folder
        {
            get { return textBox3.Text; }
            set
            { textBox3.Text = value; }
        }

        public bool IsOnline
        {
            get { return checkBox3.Checked; }
            set { checkBox3.Checked = value; }
        }

        public string IPAddress
        {
            get { return textBox4.Text; }
            set { textBox4.Text = value; }
        }

        public int Port
        {
            get
            {
                if(textBox6.Text == "")
                    return 5000;
                return Convert.ToInt32(textBox6.Text);
            }
            set { textBox6.Text = value.ToString(); }
        }

        public string MoreSettings
        {
            get { return textBox7.Text; }
            set { textBox7.Text = value; }
        }

        public string MainParams
        {
            get { return textBox5.Text; }
        }
        #endregion

        private void TextBox6TextChanged(object sender, EventArgs e)
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
                param6 = ":" + textBox6.Text;
            else
                param6 = "";
            CommandUpdate();
        }

        private void TextBox4TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
                param4 = " udp:" + textBox4.Text;
            else
                param4 = "";
            CommandUpdate();
        }

        private void TextBox7TextChanged(object sender, EventArgs e)
        {
            if (textBox7.Text != "")
                param7 = " " + textBox7.Text;
            else
                param7= "";
            CommandUpdate();
        }
        public void CommandUpdate()
        {
            textBox5.Text = param1 + param2 + param7 + param4 + param6;
        }
       
    }
}
