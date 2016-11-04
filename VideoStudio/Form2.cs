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
            //установка параметров по умолчанию
            comboBox1.Visible = false;
            comboBox2.Visible = false;
            comboBox3.Visible = false;
            label1.Visible = false;
            textBox1.Visible = false;
        }

        public Form2(bool checkBox1_Checked, bool checkBox2_Checked, int index_of_combobox1, int index_of_combobox3, string text_of_combobox2, string textbox, bool checkbox3)// восстановление значений  после пересоздаения формы
        {
            all_right = false;
            InitializeComponent();
            datacopping = true;
            try
            {
                if (checkBox1_Checked == true)// обработка видеовхода
                {
                    checkBox1.Checked = true;
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Видеокамера");
                    comboBox1.Items.Add("Захват монитора ПK");
                    comboBox1.Items.Add("Видеофайл");
                    comboBox1.Items.Add("JPEGStream");
                    comboBox1.Items.Add("MJPEGStream");
                    //comboBox1.Items.Add("Другой ПК");
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
                MessageBox.Show("Ошибка: 706 (ошибка восстановления данных)");
            }
            checkBox3.Checked = checkbox3;
            datacopping = false;
           
        }

       
        #region обработка событий

        private void checkBox1_CheckedChanged(object sender, EventArgs e) // постановка/снятия галочки видео
        {
            if (datacopping == false)//
            {
                if (checkBox1.Checked == true)// видео часть
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Видеокамера");
                    comboBox1.Items.Add("Захват монитора ПK");
                    comboBox1.Items.Add("Видеофайл");
                    comboBox1.Items.Add("JPEGStream");
                    comboBox1.Items.Add("MJPEGStream");
                  //  comboBox1.Items.Add("Другой ПК");

                    comboBox1.Visible = true;
                    checkBox3.Checked = true;
                }
                else
                {
                    comboBox1.Items.Clear();
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;

                    if (checkBox2.Checked == false)
                    {
                        checkBox3.Checked = false;
                    }
                    else
                    {
                        checkBox3.Checked = true;
                    }

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
                            comboBox2.Visible = true;//открываем путь к источнику                          
                        }
                        else //если окно закрыли без сохранения
                        {

                        }
                    }

                    if (comboBox1.SelectedIndex == 1)// выбрали монитор
                    {
                        comboBox2.Enabled = true;
                        comboBox2.Items.Clear();
                       for (int i=0; i< Screen.AllScreens.Length; i++)
                        comboBox2.Items.Add(Screen.AllScreens[i].DeviceName);

                      
                        comboBox2.SelectedIndex = 0;                      
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
                        }
                        else
                        {

                        }
                    }
                    if (comboBox1.SelectedIndex == 3)// url 
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
                MessageBox.Show("Ошибка: 707( проблема с выбором видео источника)");
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
                    checkBox3.Checked = true;
                }
                else
                {
                    comboBox3.Items.Clear();
                    comboBox3.Visible = false;
                    label1.Visible = false;
                    textBox1.Visible = false;
                    if (checkBox1.Checked == false)
                    {
                        checkBox3.Checked = false;
                    }
                    else
                    {
                        checkBox3.Checked = true;
                    }

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)// проверка на ошибки перед передачей параметров родитедительскому объекту
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

            if (all_right == true)// если все верно форма закроется 
            {
                this.Close();
            }
               
        }

        private void button2_Click(object sender, EventArgs e)// нажатие отмены 
        {
            all_right = false;// поднимаем флаг, что не все параметры корректны => переинициализация непроизойдет
            this.Close();// закрываем форму
        }

        #endregion

        #region Свойства

        #region Свойства передачи выбранных потоков

        public NAudio.Wave.WaveIn Audio// свойство для передачи аудиопотока
        {            
            get
            {
                if (checkBox2.Checked == true)
                {
                    NAudio.Wave.WaveIn sourceStream = new NAudio.Wave.WaveIn();
                    sourceStream.DeviceNumber =comboBox3.SelectedIndex;
                    sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(Convert.ToInt32(textBox1.Text), NAudio.Wave.WaveIn.GetCapabilities(index_combobox3).Channels);
                    return sourceStream;
                }
                else
                {
                    return null;
                }
            }
        }

        public IVideoSource Video// свойство для передачи видиопотока
        {
            get
            {
                try
                {
                    if (checkBox1.Checked == true)
                    {
                        if (comboBox1.SelectedIndex == 0)// поток с камеры
                        {
                            videosource = smallform_of_camera.VideoDevice;
                        }

                        if (comboBox1.SelectedIndex == 1)// поток с монитора
                        {

                            videosource = new ScreenCaptureStream(Screen.AllScreens[comboBox2.SelectedIndex].Bounds, 10);
                        }

                        if (comboBox1.SelectedIndex == 2)// поток из файла
                        {
                            videosource = new FileVideoSource(comboBox2.Text);
                        }
                        if (comboBox1.SelectedIndex == 3)// поток по url ссылке
                        {

                            videosource = new JPEGStream(comboBox2.Text);// создаем новый поток с ссылкой
                        }
                        if (comboBox1.SelectedIndex == 4)// поток по url ссылке
                        {

                            videosource = new MJPEGStream(comboBox2.Text);// создаем новый поток с ссылкой
                        }
                        if (comboBox1.SelectedIndex == 5)// если мы выбрали дрогой компьютер с нашей программой,
                        // обычный видео поток нам не подходит
                        {
                            videosource = null;
                        }

                        return videosource;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch 
                {
                    return null;
                }
            }   
        }

        #endregion

        //свойства настроек аудио и видео для сохранения 
        //информации о состоянии формы в родительский объект
        #region Свойства настроек видео

        public bool checkBox1_was_Checked // выбран ли видео вход
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        public int index_combobox1// тип выбранного видео устройства
        {
            get
            {
                return comboBox1.SelectedIndex;
            }
        }

        public string text_of_combobox2// моникер видео устройства
        {
            get
            {
                return comboBox2.Text;
            }
        }

        
        #endregion

        #region Свойства настроек звука

        public bool checkBox2_was_Checked  // выбран ли аудио вход
        {
            get
            {
                return checkBox2.Checked;
            }
        }

        public int index_combobox3// выбранное аудио устройство
        {
            get
            {
                return comboBox3.SelectedIndex;
            }
        }        

        public string text_of_textbox// частота дискретизации звука
        {
            get
            {
                return textBox1.Text;
            }
        }

        #endregion

        #region Свойства настроек записи

        public bool CheckBox3_cheched// необходимо ли производить запись
        {
            get
            {
                return checkBox3.Checked;
            }

        }

        #endregion

        #endregion

    }
}
