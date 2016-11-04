using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;

namespace VideoStudio
{
    public partial class Form2 : Form
    {
        
        private VideoCaptureDeviceForm smallform_of_camera; // панель настройки камеры
        private IVideoSource videosource;                   //переменная потока видео
        public bool all_right;                              // переменная правильности введенных данных(см обработку button1)
        private bool datacopping;                           // true когда мы востанавливаем данные в нашу форму, для временного открытия
       
        public Form2()
        {
            all_right = false;
            InitializeComponent();
            comboBox1.Visible = false;
            comboBox2.Visible = false;
            comboBox3.Visible = false;
            label1.Visible = false;
            textBox1.Visible = false;
        }

        public Form2(bool checkBox1_Checked, bool checkBox2_Checked, int index_of_combobox1, int index_of_combobox3, string text_of_combobox2, string textbox)// восстановление значений  после пересоздаения формы
        {
            all_right = false;
            InitializeComponent();
            datacopping = true;
            try
            {
                if (checkBox1_Checked == true)// обработка видео входа
                {
                    checkBox1.Checked = true;
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Видео камера");
                    comboBox1.Items.Add("Захват монитора пк");
                    comboBox1.Items.Add("Видео файл");
                    comboBox1.Items.Add("JPEGStream");
                    comboBox1.Items.Add("MJPEGStream");
                    comboBox1.Items.Add("Другой пк");
                    comboBox1.Visible = true;
                    comboBox2.Visible = true;

                    if (index_of_combobox1 < 3)
                    {
                        comboBox2.Enabled = false;
                    }
                    else
                    {
                        comboBox2.Enabled = true;
                    }
                    try
                    {
                        comboBox1.SelectedIndex = index_of_combobox1;
                    }
                    catch
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                    comboBox2.Text = text_of_combobox2;
                }
                else
                {
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;
                }

                if (checkBox2_Checked == true)// работа с аудио выбором
                {
                    checkBox2.Checked = true;
                    comboBox3.Visible = true;
                    label1.Visible = true;
                    textBox1.Visible = true;
                    textBox1.Text = textbox;
                    comboBox3.Items.Clear();

                    for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
                    {
                        comboBox3.Items.Add(NAudio.Wave.WaveIn.GetCapabilities(i).ProductName);
                    }
                    try
                    {
                        comboBox3.SelectedIndex = index_of_combobox3;
                    }
                    catch
                    {
                        comboBox3.SelectedIndex = 0;
                    }

                }
                else
                {
                    comboBox3.Visible = false;
                    label1.Visible = false;
                    textBox1.Visible = false;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка: 706");
            }


            datacopping = false;
           
        }

        private void Form2_Load(object sender, EventArgs e)
        {
           
        }

        #region обработка событий

        private void checkBox1_CheckedChanged(object sender, EventArgs e) // постановка/снятия галочки видео
        {
            if (datacopping == false)
            {
                if (checkBox1.Checked == true)
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Видео камера");
                    comboBox1.Items.Add("Захват монитора пк");
                    comboBox1.Items.Add("Видео файл");
                    comboBox1.Items.Add("JPEGStream");
                    comboBox1.Items.Add("MJPEGStream");
                    comboBox1.Items.Add("Другой пк");

                    comboBox1.Visible = true;
                }
                else
                {
                    comboBox1.Items.Clear();
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;

                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)// выбор типа видео устройства
        {
            try
            {
                if (datacopping == false)
                {
                    if (comboBox1.SelectedIndex == 0)// выбераем камеру
                    {
                        comboBox2.Enabled = false;// блокируем изменение значения
                        comboBox2.Items.Clear();  // очищаем

                        smallform_of_camera = new VideoCaptureDeviceForm();// обновляем список доступных камер
                        if (smallform_of_camera.ShowDialog(this) == DialogResult.OK)// открытие и настройка камер
                        {
                            comboBox2.Items.Add(smallform_of_camera.VideoDeviceMoniker);// выводим значение камеры
                            comboBox2.SelectedIndex = 0;
                            comboBox2.Visible = true;//открываем форму
                            videosource = smallform_of_camera.VideoDevice;// запоминаем выбранное
                        }
                        else //если окно закрыли без сохранения
                        {

                        }
                    }

                    if (comboBox1.SelectedIndex == 1)// выбрали монитор
                    {
                        comboBox2.Enabled = false;
                        comboBox2.Items.Clear();
                        comboBox2.Items.Add(Screen.AllScreens[0].DeviceName);
                        comboBox2.SelectedIndex = 0;
                        videosource = new ScreenCaptureStream(Screen.AllScreens[0].Bounds, 40);// запоминаем выбранное
                        comboBox2.Visible = true;
                    }

                    if (comboBox1.SelectedIndex == 2)
                    {
                        comboBox2.Enabled = false;
                        comboBox2.Items.Clear();
                        comboBox2.Text = "";
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            comboBox2.Text = openFileDialog1.FileName;
                            comboBox2.Visible = true;
                            videosource = new FileVideoSource(openFileDialog1.FileName);// запоминаем выбранное
                        }
                        else
                        {

                        }
                    }
                    if (comboBox1.SelectedIndex == 3)// url дописать
                    {
                        comboBox2.Enabled = true;
                        comboBox2.Items.Clear();
                        comboBox2.Text = "";
                        comboBox2.Visible = true;
                    }
                     if (comboBox1.SelectedIndex == 4)// murl 
                    {
                        comboBox2.Enabled = true;
                        comboBox2.Items.Clear();
                        comboBox2.Text = "";
                        comboBox2.Visible = true;
                    }
                     if (comboBox1.SelectedIndex == 5)// наша программа 
                     {
                         comboBox2.Enabled = true;
                         comboBox2.Items.Clear();
                         comboBox2.Text = "";
                         comboBox2.Visible = true;
                     }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка: 707");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)// постановка/снятия галочки аудио
        {
            if (datacopping == false)
            {
                if (checkBox2.Checked == true)
                {
                    comboBox3.Items.Clear();
                    for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
                    {
                        comboBox3.Items.Add(NAudio.Wave.WaveIn.GetCapabilities(i).ProductName);
                    }
                    comboBox3.Visible = true;
                    comboBox3.SelectedIndex = 0;
                    label1.Visible = true;
                    textBox1.Visible = true;
                    textBox1.Text = "44100";
                }
                else
                {
                    comboBox3.Items.Clear();
                    comboBox3.Visible = false;
                    label1.Visible = false;
                    textBox1.Visible = false;

                }
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
             //deviceNumber = comboBox3.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)// проверка на ошибки перед измением параметров родитедительского объекта
        {
            try
            {
                all_right = true;
                if (checkBox1.Checked == true)
                {
                    if (comboBox2.Text == null)
                    {
                        all_right = false;
                        MessageBox.Show("Ошибка с форматом Видео источника");

                    }
                    if (comboBox2.Text == "")
                    {
                        all_right = false;
                        MessageBox.Show("Ошибка с форматом Видео источника");
                    }
                }
                else
                {
                    videosource = null;
                }

                    if (checkBox2.Checked == true)
                    {
                        if (comboBox3.Text == null)
                        {
                            all_right = false;
                            MessageBox.Show("Ошибка с форматом Аудио источника");

                        }
                        if (comboBox3.Text == "")
                        {
                            all_right = false;
                            MessageBox.Show("Ошибка с форматом Аудио источника");
                        }

                        if (textBox1.Text == "")
                        {
                            all_right = false;
                            MessageBox.Show("Ошибка с форматом Аудио источника");
                        }
                        if (textBox1.Text == null)
                        {
                            all_right = false;
                            MessageBox.Show("Ошибка с форматом Аудио источника");
                        }
                    }
                    else
                    {

                    }
                }
                catch
                {
                    MessageBox.Show("Ошибка: 708");
                }

                    if (all_right == true)
                    {
                        this.Close();
                    }
               
        }

        private void button2_Click(object sender, EventArgs e)
        {
            all_right = false;
            this.Close();
        }
        #endregion

        #region Свойства
        public NAudio.Wave.WaveIn Audio// свойство для передачи аудио потока
        {
            
            get
            {
                if (checkBox2.Checked == true)
                {
                    NAudio.Wave.WaveIn sourceStream = new NAudio.Wave.WaveIn();
                    sourceStream.DeviceNumber = index_combobox3;
                    sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(Convert.ToInt32(textBox1.Text), NAudio.Wave.WaveIn.GetCapabilities(index_combobox3).Channels);
                    return sourceStream;
                }
                else
                {
                    return null;
                }
            }
        }

        public IVideoSource Video// свойство для передачи видио потока
        {
            get
            {
                if (checkBox1.Checked == true)
                {
                    if (comboBox1.SelectedIndex == 3)
                    {

                        videosource = new JPEGStream(comboBox2.Text);// создаем новый поток с ссылкой
                    }
                    if (comboBox1.SelectedIndex == 4)
                    {

                        videosource = new MJPEGStream(comboBox2.Text);// создаем новый поток с ссылкой
                    }
                    if (comboBox1.SelectedIndex == 5)// если мы выбрали дрогой компьютер с нашей программой, обычный видео поток нам не подходит
                    {

                        return null;
                    }
                    else
                    {
                        return videosource;
                    }
                }
                else
                {
                    return null;
                }
            }   
        }

        // свойства для сохранения информации о состоянии формы в родительский объект
        public bool checkBox1_was_Checked
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        public bool checkBox2_was_Checked
        {
            get
            {
                return checkBox2.Checked;
            }
        }

        public int index_combobox1
        {
            get
            {
                return comboBox1.SelectedIndex;
            }
        }

        public int index_combobox3
        {
            get
            {
                return comboBox3.SelectedIndex;
            }
        }

        public string text_of_combobox2
        {
            get
            {
                return comboBox2.Text;
            }
        }

        public string text_of_textbox
        {
            get
            {
                return textBox1.Text;
            }
        }
        #endregion


    }
}
