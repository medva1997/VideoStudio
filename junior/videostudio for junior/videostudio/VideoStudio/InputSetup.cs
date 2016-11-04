using System;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;

namespace VideoStudio
{
    public partial class InputSetup : Form
    {
        private VideoCaptureDeviceForm smallFormOfCamera;    // Панель настройки камеры
        private IVideoSource videoSource;    // Переменная потока видео
        public bool allRight;    // Переменная правильности введенных данных(см обработку button1)
        private bool dataCopping;    // true когда мы востанавливаем данные в нашу форму, для временного открытия

        public InputSetup()
        {
            allRight = false;
            InitializeComponent();
            //установка параметров по умолчанию
            comboBox1.Visible = false;
            comboBox2.Visible = false;
            comboBox3.Visible = false;
            label1.Visible = false;
            textBox1.Visible = false;
           
        }

        /// <summary>
        /// Восстанавливает значения после пересоздаения формы.
        /// </summary>
        public InputSetup(bool CheckBox1Checked, bool checkBox2Checked, int indexOfComboBox1, int indexOfComboBox3, 
            string ComboBox2Text, string textBox, bool checkBox3)
        {
            allRight = false;
            InitializeComponent();
            dataCopping = true;
            try
            {
                if (CheckBox1Checked)    // Обработка видеовхода
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

                    if (indexOfComboBox1 < 3)
                        comboBox2.Enabled = false;
                    else
                        comboBox2.Enabled = true;
                    try
                    {
                        comboBox1.SelectedIndex = indexOfComboBox1;
                    }
                    catch
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                    comboBox2.Text = ComboBox2Text;
                }
                else
                {
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;
                }

                if (checkBox2Checked)    // Работа с аудио выбором
                {
                    checkBox2.Checked = true;
                    comboBox3.Visible = true;
                    label1.Visible = true;
                    textBox1.Visible = true;
                    textBox1.Text = textBox;
                    comboBox3.Items.Clear();

                    for (int i = 0; i < WaveIn.DeviceCount; i++)
                        comboBox3.Items.Add(WaveIn.GetCapabilities(i).ProductName);
                    try
                    {
                        comboBox3.SelectedIndex = indexOfComboBox3;
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
            dataCopping = false;
            this.checkBox3.Checked = checkBox3;
        }
       
        #region обработка событий

        private void CheckBox1CheckedChanged(object sender, EventArgs e)    // Постановка/снятия галочки видео
        {
            if (!dataCopping)
            {
                if (checkBox1.Checked)    // Видео часть
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Видеокамера");
                    comboBox1.Items.Add("Захват монитора ПK");
                    comboBox1.Items.Add("Видеофайл");
                    comboBox1.Items.Add("JPEGStream");
                    comboBox1.Items.Add("MJPEGStream");
                    //comboBox1.Items.Add("Другой ПК");

                    comboBox1.Visible = true;
                    checkBox3.Checked = false;
                }
                else
                {
                    comboBox1.Items.Clear();
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;

                    if (checkBox2.Checked == false)
                        checkBox3.Checked = false;
                    else
                        checkBox3.Checked = true;
                }
            }
        }

        private void ComboBox1SelectedIndexChanged(object sender, EventArgs e)    // Выбор типа видео устройства
        {
            try
            {
                if (!dataCopping)
                {
                    if (comboBox1.SelectedIndex == 0)    // Выбераем камеру
                    {
                        comboBox2.Enabled = false;    // Блокируем изменение значения
                        comboBox2.Items.Clear();    // Очищаем

                        smallFormOfCamera = new VideoCaptureDeviceForm();    // Обновляем список доступных камер
                        if (smallFormOfCamera.ShowDialog(this) == DialogResult.OK)    // Открытие и настройка камер
                        {
                            comboBox2.Items.Add(smallFormOfCamera.VideoDeviceMoniker);    // Выводим значение камеры
                            comboBox2.SelectedIndex = 0;
                            comboBox2.Visible = true;    // Открываем путь к источнику                          
                        }
                    }
                    else if (comboBox1.SelectedIndex == 1)    // Выбрали монитор
                    {
                        comboBox2.Enabled = true;
                        comboBox2.Items.Clear();
                        for (int i=0; i< Screen.AllScreens.Length; i++)
                            comboBox2.Items.Add(Screen.AllScreens[i].DeviceName);

                        comboBox2.SelectedIndex = 0;                      
                        comboBox2.Visible = true;
                    }
                    else if (comboBox1.SelectedIndex == 2)
                    {
                        comboBox2.Enabled = true;
                        comboBox2.Items.Clear();
                        //comboBox2.Text = "";
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            comboBox2.Text = openFileDialog1.FileName;
                            comboBox2.Visible = true;                           
                        }
                    }
                    else if (comboBox1.SelectedIndex == 3)    // url 
                    {
                        comboBox2.Enabled = true;
                        comboBox2.Items.Clear();
                        comboBox2.Text = "";
                        comboBox2.Visible = true;
                    }
                    else if (comboBox1.SelectedIndex == 4)    // murl 
                    {
                        comboBox2.Enabled = true;
                        comboBox2.Items.Clear();
                        comboBox2.Text = "";
                        comboBox2.Visible = true;
                    }
                    else if (comboBox1.SelectedIndex == 5)    // Наша программа 
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

        private void СheckBox2CheckedChanged(object sender, EventArgs e)    // Постановка/снятие галочки аудио
        {
            if (!dataCopping)
            {
                if (checkBox2.Checked)
                {
                    comboBox3.Items.Clear();
                    for (int i = 0; i < WaveIn.DeviceCount; i++)
                        comboBox3.Items.Add(WaveIn.GetCapabilities(i).ProductName);
                    comboBox3.Visible = true;
                   // comboBox3.SelectedIndex = 0;
                    label1.Visible = true;
                    textBox1.Visible = true;
                    textBox1.Text = "44100";
                    checkBox3.Checked = false;
                }
                else
                {
                    comboBox3.Items.Clear();
                    comboBox3.Visible = false;
                    label1.Visible = false;
                    textBox1.Visible = false;
                    if (!checkBox1.Checked)
                        checkBox3.Checked = false;
                    else
                        checkBox3.Checked = true;
                }
            }
        }

        private void Button1Click(object sender, EventArgs e)    // Проверка на ошибки перед передачей параметров родитедительскому объекту
        {
            try
            {
                allRight = true;
                if (checkBox1.Checked)
                {
                    if (comboBox2.Text == null || comboBox2.Text == "")
                    {
                        allRight = false;
                        MessageBox.Show("Ошибка с форматом Видео источника");

                    }
                }
                else
                    videoSource = null;

                if (checkBox2.Checked)
                {
                    if (comboBox3.Text == null || comboBox3.Text == "" || textBox1.Text == "" || textBox1.Text == null)
                    {
                        allRight = false;
                        MessageBox.Show("Ошибка с форматом Аудио источника");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка: 708");
            }

            if (allRight)    // Если все верно форма закроется 
                Close();     
        }

        private void Button2Click(object sender, EventArgs e)    // Нажатие отмены 
        {
            allRight = false;    // Поднимаем флаг, что не все параметры корректны => переинициализация непроизойдет
            Close();    // Закрываем форму
        }

        #endregion

        #region Свойства

        #region Свойства передачи выбранных потоков

        /// <summary>
        /// Возвращает аудиопоток.
        /// </summary>
        public WaveIn Audio
        {            
            get
            {
                if (checkBox2.Checked)
                {
                    WaveIn sourceStream = new WaveIn();
                    sourceStream.DeviceNumber = comboBox3.SelectedIndex;
                   // sourceStream.WaveFormat = new WaveFormat(Convert.ToInt32(textBox1.Text), WaveIn.GetCapabilities(ComboBox3Index).Channels);
                    sourceStream.WaveFormat = new WaveFormat(8000,16, 1);
                    //sourceStream.WaveFormat = new WaveFormat(Convert.ToInt32(textBox1.Text), 1);
                    
                    return sourceStream;
                }
                return null;
            }
        }

        /// <summary>
        /// Возвращает видеопоток.
        /// </summary>
        public IVideoSource Video
        {
            get
            {
                try
                {
                    if (checkBox1.Checked)
                    {
                        if (comboBox1.SelectedIndex == 0)    // Поток с камеры
                            videoSource = smallFormOfCamera.VideoDevice;
                        else if (comboBox1.SelectedIndex == 1)    // Поток с монитора
                            videoSource = new ScreenCaptureStream(Screen.AllScreens[comboBox2.SelectedIndex].Bounds, 10);
                        else if (comboBox1.SelectedIndex == 2)    // Поток из файла
                            videoSource = new FileVideoSource(comboBox2.Text);
                        else if (comboBox1.SelectedIndex == 3)    // Поток по url ссылке
                            videoSource = new JPEGStream(comboBox2.Text);// создаем новый поток с ссылкой
                        else if (comboBox1.SelectedIndex == 4)    // Поток по url ссылке
                            videoSource = new MJPEGStream(comboBox2.Text);    // Создаем новый поток с ссылкой
                        else //if (comboBox1.SelectedIndex == 5)    // Если мы выбрали дрогой компьютер с нашей программой, обычный видео поток нам не подходит
                            videoSource = null;
                        return videoSource;
                    }
                    return null;
                }
                catch 
                {
                    return null;
                }
            }   
        }

        #endregion

        // Свойства настроек аудио и видео для сохранения 
        // Информации о состоянии формы в родительский объект
        #region Свойства настроек видео

        /// <summary>
        /// Выбран ли видео вход.
        /// </summary>
        public bool CheckBox1Checked
        {
            get { return checkBox1.Checked; }
        }

        /// <summary>
        /// Тип выбранного видео устройства.
        /// </summary>
        public int ComboBox1Index
        {
            get { return comboBox1.SelectedIndex; }
        }

        /// <summary>
        /// Моникер видео устройства.
        /// </summary>
        public string ComboBox2Text
        {
            get { return comboBox2.Text; }
        }
        
        #endregion

        #region Свойства настроек звука

        /// <summary>
        /// Выбран ли аудио вход.
        /// </summary>
        public bool СheckBox2Checked
        {
            get { return checkBox2.Checked; }
        }

        /// <summary>
        /// Выбранное аудио устройство.
        /// </summary>
        public int ComboBox3Index
        {
            get { return comboBox3.SelectedIndex; }
        }

        /// <summary>
        /// Частота дискретизации звука.
        /// </summary>
        public string TextBoxText
        {
            get { return textBox1.Text; }
        }

        #endregion

        #region Свойства настроек записи

        /// <summary>
        /// Необходимо ли производить запись.
        /// </summary>
        public bool CheckBox3Сheched
        {
            get { return checkBox3.Checked; }
        }

        #endregion

        #endregion
    }
}
