using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.FFMPEG;
using AForge.Video.DirectShow;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace VideoStudio
{
    public partial class SmallWindow
    {
        int port = 5002; 

        public delegate void MethodContainer();
        public event MethodContainer pictureboxchanger;    // Событие OnCount c типом делегата MethodContainer

        /// <summary>
        /// Завершение всех потоков, если они не остановлены.
        /// </summary>
        public void CloseCurrentVideoSource()
        {
            try
            {
                client.flag = true;
            }
            catch
            {

            }

            try
            {
                waveOut.Stop();
            }
            catch
            {

            }

            try
            {
                waveWriter.Close();
            }

            catch
            {

            }

            try
            {
                videoSource.SignalToStop();
            }
            catch
            {

            }

            try
            {
                videoSource.WaitForStop();
            }
            catch
            {

            }

            try
            {
                videoSource.Stop();
            }
            catch
            {

            }

            try
            {
                audioSource.StopRecording();
            }
            catch
            {

            }

            try
            {
                audioSource.Dispose();
            }
            catch
            {

            }

            try
            {
                client.Closing();
            }
            catch
            {

            }    
        }

        private void Button2Click(object sender, EventArgs e)
        {
            //MessageBox.Show("2");
        }

        #region запуск захвата аудио и видео

        /// <summary>
        /// Запускает захват аудио.
        /// </summary>
        private void OpenAudioSource()
        {
            audioSource.StartRecording();    // Получение потока
            audioSource.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(SourceStreamDataAvailable);  // Подписываемся на событие получения звука          
        }

        private void OpenVideoSource()    // Запуск захвата видео
        {
            CloseCurrentVideoSource();    // Останавливаем все предыдущие действия
            videoSource.NewFrame += new NewFrameEventHandler(ShowNewFrame);    // Подписываемся на событие прихода нового видео кадра
            videoSource.Start();
        }
       
        #endregion

        /// <summary>
        /// Создаёт bitmap, если нет изображения в папке.
        /// </summary>
        
        void CreateBitmap()
        {
           Bitmap flag = new Bitmap(720, 720);
            for (int x = 0; x < flag.Height; ++x)
                for (int y = 0; y < flag.Width; ++y)
                    flag.SetPixel(x, y, Color.Black);
            img = flag;
        }

        public SmallWindow(Panel outPanel, System.Drawing.Point[] smallButtonLocation, Size[] smallButtonSize, 
            System.Drawing.Point smallPictureBoxLocation, Size smallPictureBoxSize, int id)
        {
            //try    // Попытка получить картинку заставки
            //{
            //    FileStream fs = new FileStream("foto.jpg", FileMode.Open);
            //    img = (Bitmap)Image.FromStream(fs);
            //    fs.Close();
            //}
            //catch
            //{
                CreateBitmap();    // Создаем черный битмап
            //}
            // Передача параметров конструктора в поля
            this.outPanel = outPanel;
            this.smallButtonLocation = smallButtonLocation;
            this.smallButtonSize = smallButtonSize;
            this.smallPictureBoxLocation = smallPictureBoxLocation;
            this.smallPictureBoxSize = smallPictureBoxSize;
            this.id = id;
            videoSource = new VideoCaptureDevice();
            audioSource = new WaveIn();
            try
            {
                Drawing();    // Отрисовка 4 кнопок, номера камеры, picturebox                
            }
            catch
            {
                MessageBox.Show("Ошибка: 703 (не возможно создать элементы интерфейса)");
            }
            // Подписываемся на события кликов мышью  (первую и третью клавишу обрабатывает form1)    
            button2.Click += new EventHandler(Button2Click);          
            button4.Click += new EventHandler(Button4Click);
        }   

        //public void UpdateInformation()    // Обновленние полученной информации если выбран удаленный пк
        //{
        //    if (indexOfComboBox1 == 5)
        //    {
        //        videoSourseCach = client.Image;
        //        pictureBox.Image = client.Image;
        //        audioSourceBuffer = client.ByteData;
        //        audioSourceOffset = client.Offset;
        //        audioSourceBytesRecorded = client.Recv;
        //    }
        //}

        private void InputSetupClosing(object sender, FormClosingEventArgs e)    // Обработка закрытия формы настройки
        {
            if (formsOfSelection.allRight)    // Если пользователь нажал Ok и все параметры действительны
            {
                CopyDataFromInputSetup();    // Cохранение информации из form2
                formsOfSelection.Dispose();    // Уничтожение form2
                if(checkBox2Checked)
                    ChangeAudio();    // Смена аудио источника
                if (indexOfComboBox1 != 5)    // Если не выбран показ с друго пк
                {
                    bool flag = checkBox3;    // Блокировка записи
                    StopRec();    // Остановка записи
                    ChangeVideo();    // Смена видео источника
                    if (formHadBeenOpened && flag)
                        MessageBox.Show("Запись с этого источника начнется только после команды CUT или REC");
                    formHadBeenOpened = true;    // Поднимаем флаг, что мы уже открывали форму
                }
                else
                {
                    try
                    {
                        if (videoSource.IsRunning)    // Останавливаем видио поток
                        {
                            videoSource.SignalToStop();
                            videoSource.WaitForStop();
                            videoSource.Stop();
                        }
                        try
                        {
                            audioSource.StopRecording();
                            audioSource.Dispose();
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка остановки захвата с источников");
                    }
                // Запуск  получения  картинки с друго компа
                }
                formsOfSelection.allRight = false;    // Меняем значение на false, чтобы при дальнейшем закрытии формы, мы не вернулись сюда
                formsOfSelection.Close();    // Закрываем form2
            }
        }

        /// <summary>
        /// Cохраняет информацию из form2.
        /// </summary>
        private void CopyDataFromInputSetup()
        {
            try
            {
                cashVideoSource = formsOfSelection.Video;    // Устанавливаем значение будущего видео потока
                cashAudioSource = formsOfSelection.Audio;    // Устанавливаем значение будущего аудио потока

                // Сохранение информации для повторной инициализации формы
                CheckBox1Checked = formsOfSelection.CheckBox1Checked;
                checkBox2Checked = formsOfSelection.СheckBox2Checked;
                indexOfComboBox1 = formsOfSelection.ComboBox1Index;
                indexOfComboBox3 = formsOfSelection.ComboBox3Index;
                ComboBox2Text = formsOfSelection.ComboBox2Text;
                textBox = formsOfSelection.TextBoxText;
                checkBox3 = formsOfSelection.CheckBox3Сheched;
            }
            catch
            {

            }
        }

        /// <summary>
        /// Изменяет видеовыход.
        /// </summary>
        private void ChangeVideo()
        {
            try
            {
                if (CheckBox1Checked)    // Запуск процедуры захвата видео с нового потока
                {
                    if (videoSource.IsRunning)    // Останавливаем видио поток
                    {
                       videoSource.SignalToStop();
                       videoSource.WaitForStop();
                       videoSource.Stop();                      
                    }
                    pictureBox.Dispose();
                    pictureBox = null;
                    pictureBox = new PictureBox();
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Location = smallPictureBoxLocation;
                    pictureBox.Size = smallPictureBoxSize;
                    //pictureBox.BorderStyle = BorderStyle.Fixed3D;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    outPanel.Controls.Add(pictureBox);
                    pictureboxchanger();
                    videoSource = cashVideoSource;    // Меняем девайс
                    OpenVideoSource();    // Запускаем                           
                }
                else
                {
                    if (videoSource.IsRunning)    // Останавливаем видио поток
                    {
                        videoSource.SignalToStop();
                        videoSource.WaitForStop();
                        videoSource.Stop();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка: 705 (Ошибка смены видеовхода)");
            }
        }

        /// <summary>
        /// Изменяет аудиовход.
        /// </summary>
        private void ChangeAudio()
        {
            try
            {
                if (checkBox2Checked)            // запуск процедуры захвата с нового  потока аудио     
                {
                    audioSource.StopRecording();
                    audioSource.Dispose();
                    audioSource = cashAudioSource;
                    OpenAudioSource();
                }
                else
                {
                    audioSource.StopRecording();
                    audioSource.Dispose();
                }
            }
            catch
            {
                MessageBox.Show("Ошибка: 704 (Ошибка смены аудиовхода)");
            }
        }
        Bitmap images;

        private void ShowNewFrame(object sender, NewFrameEventArgs eventArgs)    // Событие связанное с  появление нового изображения в потоке
        {
            try
            {
                images = (Bitmap)eventArgs.Frame.Clone();

            }
            catch
            {

            }
            pictureBox.Image = images;    // Вывод в превью  
            videoSourseCach = images;
            GC.Collect();    // Сбор мусора, т.к clone засоряет оперативную память
           
        }

        private void SourceStreamDataAvailable(object sender, WaveInEventArgs e)    // Захват аудио потока и сохраниния в буфера
        {           
            audioSourceBytesRecorded = e.BytesRecorded;
            audioSourceBuffer = e.Buffer;
            audioSourceOffset = 0;
            if(Isrecordworkingnow)
                AudioWriter();
        }
       
        #region Свойства

        #region свойства аудио

        /// <summary>
        /// Возвращает аудио буфер.
        /// </summary>
        public byte[] AudioBuffer
        {
            get { return audioSourceBuffer; }
        }

        /// <summary>
        /// Возвращает количество элементов аудио буфера.
        /// </summary>
        public int AudioBytesRecorded
        {
            get { return audioSourceBytesRecorded; }
        }

        /// <summary>
        /// Возвращает количество элементов аудио.
        /// </summary>
        public int AudioSourceOffset
        {
            get { return audioSourceOffset; }
        }


        public WaveFormat audioSourceFormat
        {
            get { return audioSource.WaveFormat; }
        }


        #endregion

        /// <summary>
        /// Возвращает текущий кадр видео.
        /// </summary>
        public Bitmap VideoCach
        {
            get
            {
                if(videoSourseCach != null)
                    return videoSourseCach;   
                return img;
            }
        }

        #endregion

        #region Запись 

        /// <summary>
        /// Записывает звук.
        /// </summary>
        public void AudioWriter()
        {
            if (checkBox3 && waveWriter != null)
            {
                if (audioSourceBuffer!=null)
                    waveWriter.Write(audioSourceBuffer, audioSourceOffset, audioSourceBytesRecorded);
                waveWriter.Flush();
            }
        }

        /// <summary>
        /// Записывает видео.
        /// </summary>
        public void VideoWriter()
        {
            if (checkBox3 && videoWriter1 != null)
            {
                if (videoWriter1.IsOpen)
                    videoWriter1.WriteVideoFrame(videoSourseCach);
            }
        }

        /// <summary>
        /// Меняет папку сохранения видео.
        /// </summary>
        /// <param name="folder"> Папка назначения. </param>
        public void Cut(string folder)
        {
            StopRec();
            StartRec(folder);
        }

        /// <summary>
        /// Начинает запись видео.
        /// </summary>
        public void StartRec(string folder)
        {
            Isrecordworkingnow = true;
            if (checkBox3)
            {
                if (CheckBox1Checked)
                {
                    videoOutputFile = Path.Combine(folder, "camera" + id + ".avi");    // Генерация имени файла
                    if (videoWriter1 != null)
                        videoWriter1.Close();

                    videoWriter1 = new VideoRecorder(port + id);

                    try
                    {
                        videoWriter1.Open(videoOutputFile, videoSourseCach.Width, videoSourseCach.Height, 30, "mpeg2video", 2000, port + id);
                         
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка создания файла для записи видео" + id);
                    }
                    Isrecordworkingnow = true;
                }

                if (checkBox2Checked)
                {
                    audioOutputFile = Path.Combine(folder, "audio" + id + ".wav");    // Генерация имени файла
                    try
                    {
                        if (waveWriter != null)
                            waveWriter.Close();
                        waveWriter = null;
                        waveWriter = new WaveFileWriter(audioOutputFile, audioSource.WaveFormat);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка создания файла для записи аудио" + id);
                    }
                }
            }
        }

        /// <summary>
        /// Останавливает запись видео.
        /// </summary>
        public void StopRec()
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
                MessageBox.Show("Ошибка остановки  записи потоков " + id);
            }
        }

        #endregion
    }
}
