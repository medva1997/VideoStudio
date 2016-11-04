using System;
using System.Drawing;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.FFMPEG;
using AForge.Video.DirectShow;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace VideoStudio
{
    partial class SmallWindow
    {
        #region объекты формы
        
        public PictureBox pictureBox = new PictureBox();    // окно вывода картинки
        public Button button1 = new Button();    // кнопка on air
        public Button button2 = new Button();    // кнопка работы со звуком
        public Button button3 = new Button();    // кнопка картинка в картинке
        private Button button4 = new Button();    // кнопка настройки
        private Label label = new Label();    // надпись с номером потока
        private Panel outPanel;    // панель на которой настологаются 4 кнопки и picturebox
        private System.Drawing.Point[] smallButtonLocation;    //  массив расположения кнопок  маленьких панелей 
        private Size[] smallButtonSize;    // массив размера кнопок  маленьких панелей 
        private System.Drawing.Point smallPictureBoxLocation;    // расположение pictureBox
        private Size smallPictureBoxSize;    // размер pictureBox
        private int id;    // номер потока/камеры

        #endregion

        #region данные окна маленьких настроек

        private InputSetup formsOfSelection;    // Вспомогательная форма для настойка водных параметров
        private bool CheckBox1Checked;    // Выбрано ли использование видео устройства
        private bool checkBox2Checked;    // Выбрано ли использование аудио устройства
        private int indexOfComboBox1;    // Номер выбранного типа видеоустройства 
        private int indexOfComboBox3;    // Номер выбранного аудио устройства 
        private string ComboBox2Text;    // Выбранное видео устройство
        private string textBox;    // Частота дискретизации
        private bool formHadBeenOpened;    // Флаг на использование ранее сохраненных данных true- испльзовать
        private bool checkBox3;
       
        #endregion 

        private IVideoSource videoSource;    // Видео поток       
        private IVideoSource cashVideoSource;    // Временная переменная для смены видеопотоков
        private WaveIn audioSource;    // Аудио поток
        private WaveIn cashAudioSource;    // Временная переменная для смены аудиопотоков
        private DirectSoundOut waveOut;    // Объект воспроизведения звука
        private WaveFileWriter waveWriter;    // Объект записи в файл
        private byte[] audioSourceBuffer;    // Буфер аудио
        private int audioSourceBytesRecorded;    // Количество полученного звука
        private int audioSourceOffset;
        private string audioOutputFile;    // Переменна для храенения пути к файлу записи аудио
        private string videoOutputFile;    // Переменна для храенения пути к файлу записи видео
        private Bitmap videoSourseCach;    // Переменна для хранения текущего кадра 
        private TCPclient client = new TCPclient();    // Подкючение к другому пк
        private VideoRecorder videoWriter1;    // Запись видео в файл
        Bitmap img;    // Переменная хранит заставку и выдает ее на выход если нет видео входа

        private bool Isrecordworkingnow;    // Переменная хранит значение работает ли сейчас кнопка записи (по умолчанию false)

        /// <summary>
        /// Добавление и отрисовка элементов маленькой формы.
        /// </summary>
        private void Drawing()
        {
            // smallButton 1
            button1.Location = smallButtonLocation[0];
            button1.Size = smallButtonSize[0];
            button1.UseVisualStyleBackColor = true;
            button1.Text = "On air";
            outPanel.Controls.Add(button1);

            // smallButton 2
            button2.Location = smallButtonLocation[1];
            button2.Size = smallButtonSize[1];
            button2.UseVisualStyleBackColor = true;
            button2.Text = "Sound";
            outPanel.Controls.Add(button2);

            // smallButton 3
            button3.Location = smallButtonLocation[2];
            button3.Size = smallButtonSize[2];
            button3.Text = "Pic-in-Pic";
            button3.UseVisualStyleBackColor = true;
            outPanel.Controls.Add(button3);

            // smallButton 4
            button4.Location = smallButtonLocation[3];
            button4.Size = smallButtonSize[3];
            button4.Text = "Setup";
            button4.UseVisualStyleBackColor = true;
            outPanel.Controls.Add(button4);

            // smallPictureBox
            pictureBox.BackColor = System.Drawing.Color.Black;
            pictureBox.Location = smallPictureBoxLocation;
            pictureBox.Size = smallPictureBoxSize;
            //pictureBox.BorderStyle = BorderStyle.Fixed3D;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            outPanel.Controls.Add(pictureBox);

            // label
            if (Screen.PrimaryScreen.Bounds.Width < 1810)
                label.Location = new System.Drawing.Point(2, 190);
            else
                label.Location = new System.Drawing.Point(4, 290);
            label.Font = new Font("Arial", 12, FontStyle.Bold);
            switch (id)
            {
                case (0): label.Text = Convert.ToString("1 q"); break;
                case (1): label.Text = Convert.ToString("2 w"); break;
                case (2): label.Text = Convert.ToString("3 e"); break;
                case (3): label.Text = Convert.ToString("4 r"); break;
                case (4): label.Text = Convert.ToString("5 t"); break;
                case (5): label.Text = Convert.ToString("6 y"); break;
            }
            outPanel.Controls.Add(label);
        }

        private void Button4Click(object sender, EventArgs e)    // Нажатие клавиши настроек
        {
            try
            {
                if (formHadBeenOpened)    // Если ранне уже настраивали
                {
                    formsOfSelection = new InputSetup(CheckBox1Checked, checkBox2Checked, indexOfComboBox1, indexOfComboBox3, 
                        ComboBox2Text, textBox, checkBox3);    // Инициализируем новую форму со старыми параметрами
                    formsOfSelection.FormClosing += InputSetupClosing;    // Подписываемся на событие ее закрытия
                    formsOfSelection.Show();    // Открываем форму
                }
                else
                {
                    formsOfSelection = new InputSetup();    // Инициализируем новую форму
                    formsOfSelection.FormClosing += InputSetupClosing;    // Подписываемся на событие ее закрытия
                    formsOfSelection.Show();    // Открываем форму
                }
                GC.Collect();    // Сборщик мусора
            }
            catch
            {
                MessageBox.Show("Ошибка 702(открытие окна настроек источника)");
            }
        }
    }
}
