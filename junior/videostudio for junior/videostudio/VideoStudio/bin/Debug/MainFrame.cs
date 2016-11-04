using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace VideoStudio
{
    public partial class MainFrame : Form
    {
        #region объявление переменных

        private SmallWindow[] preview = new SmallWindow[smallPanelsNumber];    // Объекты предпросмотраipAddress
        private static int smallPanelsNumber = 6;    // Количество маленьких панелей
        private static int smallButtonsNumber = 4;    // Количество маленьких кнопок
        private int formWidth;    // Ширина формы
        private int formHeight;    // Высота окна формы
        private int selectedDevice;    // Выбранный сейчас девайс
        private int lastSelectedDevice;    // Выбранный до этого девайс       
        private bool isOnline;    // Флаг необходимости транслировать
        private bool isRecord;    // Флаг необходимости записывать
        private string outFolder = Environment.CurrentDirectory;    // Путь к папке назначения для хранения записанных файлов
        private int secondSelectedDevice = -1;    // Номер устройства для PictureInPicture  (-1 - PictureInPicture отключено)      
        private Bitmap resultImage;    // Результирующая картинка
        private int outWidth = 1280;    // Разрешение выходного потока
        private int outHeight = 720;    // Разрешение выходного потока

        private string paramsForFFmpeg;    // Строка параметров для ffmpeg
        private int port = 5000;    // Номер порта для вывода информации
        private bool recMain;
        private bool recAll;
        private string moreSettings = "";    // Дополнительные настройки
        private string ipAddress;

        private TCPServer server = new TCPServer();

        private Settings settings;    // Объект формы настроек
        private bool settingsHadBeenOpened;    // Флаг необходимости восстановить установленные настройки в окне настроек

       // private int videoCouner;
        
        private byte[] buffer;
        private ImageWorker graphicsPart;

        private System.Windows.Forms.Timer timer;    // Система сохранения
        //запись основного потока
        private VideoRecorder videoWriter1;
        string videoOutputFile;
        bool Isrecordworkingnow;
        private System.Windows.Forms.Timer mainrecordtimer;


        private System.Windows.Forms.Timer audiorecordtimer;
        private int audioselecteddevice=0;

        private string audioOutputFile;
        private WaveFileWriter waveWriter;
        SoundServer audioserver;
        #endregion

        

        public MainFrame()
        {
            InitializeComponent();
            KeyPreview = true;    // Перелючаем клавиатуру на форму   
        }

        private void MainFrameLoad(object sender, EventArgs e)
        {
            selectedDevice = 0;
            SetControlsSize(Screen.PrimaryScreen.Bounds.Width);    // Установка параметров размеров элементов
            CreateConponents();    // Создаем визуальные элементы
            for (int i = 0; i < smallPanelsNumber; i++)    // Создаем объекты маленьких окон
                    preview[i] = new SmallWindow(smallPanels[i], smallButtonLocation, smallButtonSize, smallPictureBoxLocation, smallPictureBoxSize, i);
            buttonColor = recButton.BackColor;
            Click();  // Подключение к обработке событий

            graphicsPart = new ImageWorker(preview);
            graphicsPart.onImageСhange += GraphicsPartImageChanger;

            ChangeOnAir(0);

            timer = new System.Windows.Forms.Timer(); // Таймер1
            timer.Interval = 31;
            timer.Enabled = true;
            timer.Tick += TimerTick;
            timer.Start();
            mainrecordtimer = new System.Windows.Forms.Timer(); // Таймер для записи основного потока видео
            mainrecordtimer.Interval = 31;
            mainrecordtimer.Enabled = true;
            mainrecordtimer.Tick += mainTimerTick;
            mainrecordtimer.Start();

            audiorecordtimer = new System.Windows.Forms.Timer(); // Таймер для записи основного потока аудио
            audiorecordtimer.Interval =100;
            audiorecordtimer.Enabled = true;
            audiorecordtimer.Tick += audiorecordtimerTick;
            audiorecordtimer.Start();

           // audioserver = new SoundServer("127.0.0.1",6000,ipAddress,port+1000);

            //System.Threading.TimerCallback TimerDelegate = new System.Threading.TimerCallback(worker);

            //System.Threading.Timer TimerItem = new System.Threading.Timer(TimerDelegate);
            //TimerItem.Change(0, 40);
        }

        private void mainTimerTick(object sender, EventArgs e)    // Система записи видео (главного потока)
        {
            if (isRecord)    // Запись
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                MainFrameWriter();
            }
           GC.Collect();    // Cбор мусора
        }      

        private void TimerTick(object sender, EventArgs e)    // Система записи (на первом таймере)
        {            
            if (isRecord)    // Запись
            {
                for (int i = 0; i < smallPanelsNumber; i++)
                {
                    preview[i].VideoWriter();
                }              
            }
            GC.Collect();    // Cбор мусора
        }       

        private void MainFrameClosing(object sender, FormClosingEventArgs e)    // Обработка закрытия формы (завершение всех потоков)
        {
            for (int i = 0; i < preview.Length; i++)
            {
                preview[i].CloseCurrentVideoSource();
                preview[i].StopRec();
            }
        }

        private void SettingButtonClick(object sender, EventArgs e)    // Обработка нажатия кнопки настроек 
        {
            if (!settingsHadBeenOpened)
            {
                settings = new Settings();
                settings.Show();
                settings.FormClosing += SettingsClosing;
            }
            else
            {
                settings = new Settings();
                // Устанавливаем значения через свойства
                settings.PictureWidth = outWidth;
                settings.PictureHeight = outHeight;
                settings.RecMain = recMain;
                settings.RecAll = recAll;
                settings.Folder = outFolder;
                settings.IsOnline = isOnline;
                settings.MoreSettings = moreSettings;
                settings.Port = port;
                settings.IPAddress = ipAddress;
                // Отображаем форму
                settings.Show();
                settings.FormClosing += SettingsClosing;
            }
        }

        void SettingsClosing(object sender, FormClosingEventArgs e)    // Получение параметров с формы настроек
        {
            if (settings.AllGood)
            {
                outWidth = settings.PictureWidth;
                outHeight = settings.PictureHeight;
                recMain = settings.RecMain;
                recAll = settings.RecAll;
                outFolder = settings.Folder;
                
                if (settings.IsOnline)
                {
                    ipAddress = settings.IPAddress;
                    moreSettings = settings.MoreSettings;
                    port = settings.Port;
                    isOnline = settings.IsOnline;
                    paramsForFFmpeg = settings.MainParams;
                }
                else
                {
                    ipAddress = "";
                    moreSettings = "";
                    port = 5000;
                    isOnline = settings.IsOnline;
                }
                settingsHadBeenOpened = true;
                settings.Dispose();

                try
                {
                    server.Closing();
                }
                catch
                {

                }
                if (settings.IsOnline)
                {
                    server = new TCPServer(port+1,paramsForFFmpeg);    // Используется порт + 1 для отправки в ffmpeg
                    Thread.Sleep(10);
                    server.FFmpeg();
                }
            }
           graphicsPart.outHeight = outHeight;
           graphicsPart.outWidth = outWidth;
           audioserver = new SoundServer("127.0.0.1", 6000, ipAddress, port + 1000);
        }

        #region Стабильно работает
        /// <summary>
        /// Изменение текущего источника и цвета кнопок управления.
        /// </summary>
        /// <param name="number"> Номер источника. </param>
        private void ChangeOnAir(int number) 
        {
            lastSelectedDevice = selectedDevice;
            graphicsPart.CameraBefore = lastSelectedDevice;
            selectedDevice = number;
            graphicsPart.CameraNow = selectedDevice;
            for (int i = 0; i < smallPanelsNumber; i++)
                preview[i].button1.BackColor = buttonColor;
            preview[selectedDevice].button1.BackColor = Color.Red; 
        }

        /// <summary>
        /// Обнуление цвета клавиш PictureInPicture.
        /// </summary>
        private void ChangePictureInPictureColor()
        {
            for (int i = 0; i < smallPanelsNumber; i++)
                preview[i].button3.BackColor = buttonColor;
        }

        /// <summary>
        /// Cмена видеовхода PictureInPicture и смена цвета кнопки PictureInPicture.
        /// </summary>
        /// <param name="newSecondDevice"> Номер источника. </param>
        private void PictureInPicture(int newSecondDevice)
        {
            if (secondSelectedDevice != newSecondDevice)
            {
                secondSelectedDevice = newSecondDevice;
                ChangePictureInPictureColor();
                preview[secondSelectedDevice].button3.BackColor = Color.Red;
            }
            else
            {
                secondSelectedDevice = -1;
                ChangePictureInPictureColor();
            }
            graphicsPart.CameraPictureInPicture = secondSelectedDevice;
        }

        private void Click()    // Подключение к обработке событий
        {
            KeyDown += new KeyEventHandler(MainFrameKeyDown);    // Нажатие клавиш на клаве
            FormClosing += MainFrameClosing;    // Закрытие формы

            // Сlick on air
            preview[0].button1.Click += new EventHandler(Preview0PictureBoxDoubleClick);
            preview[1].button1.Click += new EventHandler(Preview1PictureBoxDoubleClick);
            preview[2].button1.Click += new EventHandler(Preview2PictureBoxDoubleClick);
            preview[3].button1.Click += new EventHandler(Preview3PictureBoxDoubleClick);
            preview[4].button1.Click += new EventHandler(Preview4PictureBoxDoubleClick);
            preview[5].button1.Click += new EventHandler(Preview5PictureBoxDoubleClick);

            // Сlick on PictureInPicture
            preview[0].button3.Click += new EventHandler(Preview0Button3Click);
            preview[1].button3.Click += new EventHandler(Preview1Button3Click);
            preview[2].button3.Click += new EventHandler(Preview2Button3Click);
            preview[3].button3.Click += new EventHandler(Preview3Button3Click);
            preview[4].button3.Click += new EventHandler(Preview4Button3Click);
            preview[5].button3.Click += new EventHandler(Preview5Button3Click);

            // Click to air
            preview[0].pictureBox.Click += new EventHandler(Preview0PictureBoxDoubleClick);
            preview[1].pictureBox.Click += new EventHandler(Preview1PictureBoxDoubleClick);
            preview[2].pictureBox.Click += new EventHandler(Preview2PictureBoxDoubleClick);
            preview[3].pictureBox.Click += new EventHandler(Preview3PictureBoxDoubleClick);
            preview[4].pictureBox.Click += new EventHandler(Preview4PictureBoxDoubleClick);
            preview[5].pictureBox.Click += new EventHandler(Preview5PictureBoxDoubleClick);

            // DoubleClick to air
            //preview[0].pictureBox.DoubleClick += new EventHandler(Preview0PictureBoxDoubleClick);
            //preview[1].pictureBox.DoubleClick += new EventHandler(Preview1PictureBoxDoubleClick);
            //preview[2].pictureBox.DoubleClick += new EventHandler(Preview2PictureBoxDoubleClick);
            //preview[3].pictureBox.DoubleClick += new EventHandler(Preview3PictureBoxDoubleClick);
            //preview[4].pictureBox.DoubleClick += new EventHandler(Preview4PictureBoxDoubleClick);
            //preview[5].pictureBox.DoubleClick += new EventHandler(Preview5PictureBoxDoubleClick);

            // Сlick on Sound
            preview[0].button2.Click += new EventHandler(Preview0Button2Click);
            preview[1].button2.Click += new EventHandler(Preview1Button2Click);
            preview[2].button2.Click += new EventHandler(Preview2Button2Click);
            preview[3].button2.Click += new EventHandler(Preview3Button2Click);
            preview[4].button2.Click += new EventHandler(Preview4Button2Click);
            preview[5].button2.Click += new EventHandler(Preview5Button2Click);

            // Клавиша настроек
            settingButton.Click += new EventHandler(SettingButtonClick);

            // Клавиша записи
            recButton.Click += new EventHandler(RecButtonClick);


            // Клавиша нарезки записи видео
            cutButton.Click += new EventHandler(CutButtonClick);

            // Подписка на событие  переинициализаци
            preview[0].pictureboxchanger += Preview0PictureBoxChanger;
            preview[1].pictureboxchanger += Preview1PictureBoxChanger;
            preview[2].pictureboxchanger += Preview2PictureBoxChanger;
            preview[3].pictureboxchanger += Preview3PictureBoxChanger;
            preview[4].pictureboxchanger += Preview4PictureBoxChanger;
            preview[5].pictureboxchanger += Preview5PictureBoxChanger;
        }

        private void RecButtonClick(object sender, EventArgs e)    // Обработка нажатия кнопки on rec
        {
            if (!isRecord)
            {
                isRecord = false;
                string newWay = Path.Combine(outFolder, Convert.ToString(DateTime.Now.ToString().Replace(':', '-')));    // Генерация нового пути
                newWay=newWay.Replace(' ','_');
                Directory.CreateDirectory(newWay);
                for (int i = 0; i < preview.Length; i++)
                    preview[i].Cut(newWay);
                MainFrameWriterCut(newWay);
                isRecord = true;
                recButton.BackColor = Color.Red;
            }
            else
            {
                isRecord = false;
                for (int i = 0; i < preview.Length; i++)
                    preview[i].StopRec();
                MainFrameWriterStopRec();
                recButton.BackColor = cutButton.BackColor;
            }
        }

        private void CutButtonClick(object sender, EventArgs e)    // Обработка нажатия кнопки on cut
        {
            isRecord = false;
            string newWay = Path.Combine(outFolder, Convert.ToString(DateTime.Now.ToString().Replace(':', '-')));    // Генерация нового пути
            newWay = newWay.Replace(' ', '_');
            Directory.CreateDirectory(newWay);
            for (int i = 0; i < preview.Length; i++)
                preview[i].Cut(newWay);
            MainFrameWriterCut(newWay);
            isRecord = true;
            recButton.BackColor = Color.Red;
        }

        public void MainFrameWriter()
        {
            if (Isrecordworkingnow && videoWriter1 != null)
            {
                if (videoWriter1.IsOpen)
                    videoWriter1.WriteVideoFrame((Bitmap)bigPictureBox.Image);
            }
        }      
       

        #region ImageWorker api

        protected Bitmap ImageFromCamera(int i, SmallWindow[] preview)
        {
            return preview[i].VideoCach;
        }

        protected void ImageToBigPictureBox(Bitmap result2)
        {
            resultImage = result2;
            bigPictureBox.Image = resultImage;
            if (isOnline)
            {
                server.Sender(resultImage);    // Отправка по сети
                //audioserver.send(preview[audioselecteddevice].AudioBuffer);
            }
        }

        void GraphicsPartImageChanger()
        {
            resultImage = graphicsPart.OutPicture;
            bigPictureBox.Image = resultImage;
            if (isOnline)
            {
                server.Sender(resultImage); // Отправка по сети
                audioserver.send(preview[audioselecteddevice].AudioBuffer);
            }

        }

        #endregion

        #region PictureInPictureClick

        private void Preview0Button3Click(object sender, EventArgs e)    // Обработка нажатия кнопки on PictureInPicture
        {
            PictureInPicture(0);
        }

        private void Preview1Button3Click(object sender, EventArgs e)    // Обработка нажатия кнопки on PictureInPicture
        {
            PictureInPicture(1);
        }

        private void Preview2Button3Click(object sender, EventArgs e)    // Обработка нажатия кнопки on PictureInPicture
        {
            PictureInPicture(2);
        }

        private void Preview3Button3Click(object sender, EventArgs e)    // Обработка нажатия кнопки on PictureInPicture
        {
            PictureInPicture(3);
        }

        private void Preview4Button3Click(object sender, EventArgs e)    // Обработка нажатия кнопки on PictureInPicture
        {
            PictureInPicture(4);
        }

        private void Preview5Button3Click(object sender, EventArgs e)    // Обработка нажатия кнопки on PictureInPicture
        {
            PictureInPicture(5);
        }

        #endregion

        #region PreviewDoubleClick

        private void Preview0PictureBoxDoubleClick(object sender, EventArgs e)    // Обработка двойного щелчка на окно превью
        {
            ChangeOnAir(0);
        }

        private void Preview1PictureBoxDoubleClick(object sender, EventArgs e)    // Обработка двойного щелчка на окно превью
        {
            ChangeOnAir(1);
        }

        private void Preview2PictureBoxDoubleClick(object sender, EventArgs e)    // Обработка двойного щелчка на окно превью
        {
            ChangeOnAir(2);
        }

        private void Preview3PictureBoxDoubleClick(object sender, EventArgs e)    // Обработка двойного щелчка на окно превью
        {
            ChangeOnAir(3);
        }

        private void Preview4PictureBoxDoubleClick(object sender, EventArgs e)    // Обработка двойного щелчка на окно превью
        {
            ChangeOnAir(4);
        }

        private void Preview5PictureBoxDoubleClick(object sender, EventArgs e)    // Обработка двойного щелчка на окно превью
        {
            ChangeOnAir(5);
        }

        #endregion

        #region Soundclick

        private void Preview0Button2Click(object sender, EventArgs e)    // Обработка нажатия кнопки Sound
        {
            SoundClick(0);
        }

        private void Preview1Button2Click(object sender, EventArgs e)    // Обработка нажатия кнопки Sound
        {
            SoundClick(1);
        }

        private void Preview2Button2Click(object sender, EventArgs e)    // Обработка нажатия кнопки Sound
        {
            SoundClick(2);
        }

        private void Preview3Button2Click(object sender, EventArgs e)    // Обработка нажатия кнопки Sound
        {
            SoundClick(3);
        }

        private void Preview4Button2Click(object sender, EventArgs e)    // Обработка нажатия кнопки Sound
        {
            SoundClick(4);
        }

        private void Preview5Button2Click(object sender, EventArgs e)    // Обработка нажатия кнопки Sound
        {
            SoundClick(5);
        }

        private void SoundClick(int num)
        {
            audioselecteddevice = num;           
            for (int i = 0; i < smallPanelsNumber; i++)
                preview[i].button2.BackColor = buttonColor;
            preview[audioselecteddevice].button2.BackColor = Color.Red; 
        }

        #endregion

        private void MainFrameKeyDown(object sender, KeyEventArgs e)    // Обработка нажатия клавиш на клаве
        {
            // recButton.Text = Convert.ToString(e.KeyValue);
            switch (Convert.ToString(e.KeyValue))
            {
                // numlock                
                case ("97"): ChangeOnAir(e.KeyValue - 97); break;    // 1
                case ("98"): ChangeOnAir(e.KeyValue - 97); break;
                case ("99"): ChangeOnAir(e.KeyValue - 97); break;
                case ("100"): ChangeOnAir(e.KeyValue - 97); break;
                case ("101"): ChangeOnAir(e.KeyValue - 97); break;
                case ("102"): ChangeOnAir(e.KeyValue - 97); break;    // 6 

                // Обычные клавиши
                case ("49"): ChangeOnAir(e.KeyValue - 49); break;    // 1
                case ("50"): ChangeOnAir(e.KeyValue - 49); break;
                case ("51"): ChangeOnAir(e.KeyValue - 49); break;
                case ("52"): ChangeOnAir(e.KeyValue - 49); break;
                case ("53"): ChangeOnAir(e.KeyValue - 49); break;
                case ("54"): ChangeOnAir(e.KeyValue - 49); break;    // 6

                // qwerty for pic
                case ("81"): PictureInPicture(0); break;    // 1
                case ("87"): PictureInPicture(1); break;
                case ("69"): PictureInPicture(2); break;
                case ("82"): PictureInPicture(3); break;
                case ("84"): PictureInPicture(4); break;
                case ("89"): PictureInPicture(5); break;    // 6
            }
        }

        #region pictureBoxChanger

        void Preview0PictureBoxChanger()
        {
            preview[0].pictureBox.Click -= new System.EventHandler(Preview0PictureBoxDoubleClick);    // Отписываемся от старых
            preview[0].pictureBox.DoubleClick -= new System.EventHandler(Preview0PictureBoxDoubleClick);
            preview[0].pictureBox.Click += new System.EventHandler(Preview0PictureBoxDoubleClick);    // Подписываемся на новые
            preview[0].pictureBox.DoubleClick += new System.EventHandler(Preview0PictureBoxDoubleClick);
        }

        void Preview1PictureBoxChanger()
        {
            preview[1].pictureBox.Click -= new System.EventHandler(Preview1PictureBoxDoubleClick);
            preview[1].pictureBox.DoubleClick -= new System.EventHandler(Preview1PictureBoxDoubleClick);
            preview[1].pictureBox.Click += new System.EventHandler(Preview1PictureBoxDoubleClick);
            preview[1].pictureBox.DoubleClick += new System.EventHandler(Preview1PictureBoxDoubleClick);
        }

        void Preview2PictureBoxChanger()
        {
            preview[2].pictureBox.Click -= new System.EventHandler(Preview2PictureBoxDoubleClick);
            preview[2].pictureBox.DoubleClick -= new System.EventHandler(Preview2PictureBoxDoubleClick);
            preview[2].pictureBox.Click += new System.EventHandler(Preview2PictureBoxDoubleClick);
            preview[2].pictureBox.DoubleClick += new System.EventHandler(Preview2PictureBoxDoubleClick);
        }

        void Preview3PictureBoxChanger()
        {
            preview[3].pictureBox.Click -= new System.EventHandler(Preview3PictureBoxDoubleClick);
            preview[3].pictureBox.DoubleClick -= new System.EventHandler(Preview3PictureBoxDoubleClick);
            preview[3].pictureBox.Click += new System.EventHandler(Preview3PictureBoxDoubleClick);
            preview[3].pictureBox.DoubleClick += new System.EventHandler(Preview3PictureBoxDoubleClick);
        }

        void Preview4PictureBoxChanger()
        {
            preview[4].pictureBox.Click -= new System.EventHandler(Preview4PictureBoxDoubleClick);
            preview[4].pictureBox.DoubleClick -= new System.EventHandler(Preview4PictureBoxDoubleClick);
            preview[4].pictureBox.Click += new System.EventHandler(Preview4PictureBoxDoubleClick);
            preview[4].pictureBox.DoubleClick += new System.EventHandler(Preview4PictureBoxDoubleClick);
        }

        void Preview5PictureBoxChanger()
        {
            preview[5].pictureBox.Click -= new System.EventHandler(Preview5PictureBoxDoubleClick);
            preview[5].pictureBox.DoubleClick -= new System.EventHandler(Preview5PictureBoxDoubleClick);
            preview[5].pictureBox.Click += new System.EventHandler(Preview5PictureBoxDoubleClick);
            preview[5].pictureBox.DoubleClick += new System.EventHandler(Preview5PictureBoxDoubleClick);
        }

        #endregion

        #endregion


        /// <summary>
        /// Меняет папку сохранения видео.
        /// </summary>
        /// <param name="folder"> Папка назначения. </param>
        public void MainFrameWriterCut(string folder)
        {
            MainFrameWriterStopRec();
            MainFrameWriterStartRec(folder);
        }

        /// <summary>
        /// Начинает запись видео.
        /// </summary>
        public void MainFrameWriterStartRec(string folder)
        {
            Isrecordworkingnow = true;
            videoOutputFile = Path.Combine(folder, ("mainvideo" + ".avi"));    // Генерация имени файла
            if (videoWriter1 != null)
                videoWriter1.Close();

            videoWriter1 = new VideoRecorder(port + 10);

            try
            {
                videoWriter1.Open(videoOutputFile, Width, Height, 30, "mpeg2ideo", 2000, port + 10);

            }
            catch
            {
                MessageBox.Show("Ошибка создания файла для записи главного  видео");
            }
            Isrecordworkingnow = true;



            audioOutputFile = Path.Combine(folder, "mainaudio" + ".wav");    // Генерация имени файла
            try
            {
                if (waveWriter != null)
                    waveWriter.Close();
                waveWriter = null;
               // WaveFormat format = new WaveFormat(44100, 16, 1);
                WaveFormat format = new WaveFormat(8000, 16, 1);
                waveWriter = new WaveFileWriter(audioOutputFile, format);
            }
            catch
            {
                MessageBox.Show("Ошибка создания файла для записи основного аудио");
            }            

        }

        /// <summary>
        /// Останавливает запись.
        /// </summary>
  
        public void MainFrameWriterStopRec()
        {
            Isrecordworkingnow = false;
            try
            {
                if (waveWriter != null)
                    waveWriter.Close();
                waveWriter = null;
                if (videoWriter1 != null)
                    videoWriter1.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка остановки  записи главного  видео" + 10);
            }
        }

        private void  audiorecordtimerTick(object sender, EventArgs e)
        {
            if (Isrecordworkingnow && waveWriter != null)
            {
                if (preview[audioselecteddevice].AudioBuffer != null)
                    waveWriter.Write(preview[audioselecteddevice].AudioBuffer, preview[audioselecteddevice].AudioSourceOffset, preview[audioselecteddevice].AudioBytesRecorded);
                waveWriter.Flush();
                //audioserver.send(preview[audioselecteddevice].AudioBuffer);
            }
            if (isOnline)
            {
               
               /// audioserver.send(preview[audioselecteddevice].AudioBuffer);
            }
        }
       
     

    }
}
