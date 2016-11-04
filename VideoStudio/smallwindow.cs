using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.FFMPEG;
using AForge.Video.DirectShow;
using System.Threading;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace VideoStudio
{

    class smallwindow
    {

       
        #region Объявление переменных

        #region объекты формы
        
        public System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox();  // окно вывода картинки
        public System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();             // кнопка on air
        private System.Windows.Forms.Button button2 = new System.Windows.Forms.Button();            // кнопка работы со звуком
        public System.Windows.Forms.Button button3 = new System.Windows.Forms.Button();             // кнопка картинка в картинке
        private System.Windows.Forms.Button button4 = new System.Windows.Forms.Button();            // кнопка настройки
        private System.Windows.Forms.Label label = new System.Windows.Forms.Label();                // надпись с номером потока
        private  Panel outpanel;                                                                    // панель на которой настологаются 4 кнопки и picturebox
        private System.Drawing.Point[] smallbuttonLocation;                                         //  массив расположения кнопок  маленьких панелей 
        private System.Drawing.Size[] smallbuttonSize;                                              // массив размера кнопок  маленьких панелей 
        private System.Drawing.Point smallpictureBoxLocation;                                       // расположение pictureBox
        private System.Drawing.Size smallpictureBoxSize;                                            // размер pictureBox
        private int id;                                                                             //номер потока/камеры

        #endregion

        #region данные окна маленьких настроек

        private Form2 forms_of_selection;                                                           // вспомогательная форма для настойка водных параметров
        private bool checkBox1_Checked;                                                             // выбрано ли использование видео устройства
        private bool checkBox2_Checked;                                                             // выбрано ли использование аудио устройства
        private int index_of_combobox1=0;                                                           // номер выбранного типа видеоустройства 
        private int index_of_combobox3;                                                             // номер выбранного аудио устройства 
        private string text_of_combobox2;                                                           // выбранное видео устройство
        private string textbox;                                                                     // частота дискретизации
        private bool form_had_been_opened;                                                          // флаг на использование ранее сохраненных данных true- испльзовать
        private bool checkbox3;
       
        # endregion 

        private IVideoSource videosource;                                                           // видео поток       
        private IVideoSource cashvideosource;                                                       // временная переменная для смены видеопотоков
        private NAudio.Wave.WaveIn AudiosourceStream;                                               // аудио поток
        private NAudio.Wave.WaveIn cashAudiosourceStream;                                           // временная переменная для смены аудиопотоков
        private NAudio.Wave.DirectSoundOut waveOut = null;                                          // объект воспроизведения звука
        private NAudio.Wave.WaveFileWriter waveWriter = null;                                       // объект записи в файл
        private byte[] Audiosourcebuffer;                                                           // буфер аудио
        private int AudiosourceBytesRecorded;                                                       // количество полученного звука
        private int Audiosourceoffset;                                                              //
        private string AudioOutputfile;                                                             // переменна для храенения пути к файлу записи аудио
        private string VideoOutputfile;                                                             //переменна для храенения пути к файлу записи видео
        private Bitmap Videosoursecach=null;                                                        // переменна для хранения текущего кадра 
        private tcpclient client= new tcpclient();                                                  // подкючение к другому пк
        private VideoFileWriter Videowriter1;                                                       // запись видео в файл
        Bitmap img;                                                                                 // переменная хранит заставку и выдает ее на выход если нет видео входа

        private bool Isrecordworkingnow=false;                                                      // переменная хранит значение работает ли сейчас кнопка записи (по умолчанию false)

        #endregion


        public delegate void MethodContainer();

        //Событие OnCount c типом делегата MethodContainer.
        public event MethodContainer pictureboxchanger;
               
        public void CloseCurrentVideoSource() //завершаем все потоки, если он не остановлены
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
                 videosource.SignalToStop();
             }
            catch
             {

             }

            try
             {
                 videosource.WaitForStop();
             }
            catch
             {

             }

            try
             {
                 videosource.Stop();
             }
            catch
             {

             }

             try
             {
                 AudiosourceStream.StopRecording();
             }
            catch
             {

             }

             try
             {
                 AudiosourceStream.Dispose();
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

        private void button2_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("2");
        }

       

        #region запуск захвата аудио и видео

        private void OpenAudioSource()// запуск захвата аудио
        {
            AudiosourceStream.StartRecording();// получение потока
          
            AudiosourceStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(sourceStream_DataAvailable);  // подписываемся на событие получения звука          
        }

        private void OpenVideoSource()// запуск захвата видео
        {
            CloseCurrentVideoSource(); // останавливаем все предыдущие действия
            videosource.NewFrame += new NewFrameEventHandler(player_NewFrame); // подписываемся на событие прихода нового видео кадра
            videosource.Start();

        }

       
        #endregion

        void CreateBitmap() //создание битмапа если нет изображения в папке
        {
            System.Drawing.Bitmap flag = new System.Drawing.Bitmap(720, 720);
            for (int x = 0; x < flag.Height; ++x)
                for (int y = 0; y < flag.Width; ++y)
                    flag.SetPixel(x, y, Color.Black);
           
            img= flag;
        }

        public smallwindow(Panel outpanel, System.Drawing.Point[] smallbuttonLocation, System.Drawing.Size[] smallbuttonSize, System.Drawing.Point smallpictureBoxLocation, System.Drawing.Size smallpictureBoxSize, int id)
        {
            try
            {//попытка получить картинку заставку
                System.IO.FileStream fs = new System.IO.FileStream("foto.jpg", System.IO.FileMode.Open);
                img = (Bitmap)System.Drawing.Image.FromStream(fs);
                fs.Close();
            }
            catch
            {
                CreateBitmap();// создаем черный битмап
            }
            // передача параметров конструктора в поля
            this.outpanel = outpanel;
            this.smallbuttonLocation = smallbuttonLocation;
            this.smallbuttonSize = smallbuttonSize;
            this.smallpictureBoxLocation = smallpictureBoxLocation;
            this.smallpictureBoxSize = smallpictureBoxSize;
            this.id = id;
            videosource = new VideoCaptureDevice();
            AudiosourceStream = new NAudio.Wave.WaveIn();
            try
            {
                drawing();// отрисовка 4 кнопок, номера камеры, picturebox
                button2.Enabled = false;
                
                    
            }
            catch
            {
                MessageBox.Show("Ошибка: 703( не возможно создать элементы интерфейса");
            }
            // подписываемся на события кликов мышью  (первую и третью клавишу обрабатывает form1)    
            button2.Click += new System.EventHandler(button2_Click);          
            button4.Click += new System.EventHandler(button4_Click);
        }

       
         public smallwindow()
        {

        }    


        //public void update_information()//обновленние полученной информации если выбран удаленный пк
        //{
        //    if (index_of_combobox1 == 5)
        //    {
        //        Videosoursecach = client.Image;
        //        pictureBox.Image = client.Image;
        //        Audiosourcebuffer = client.bytedata;
        //        Audiosourceoffset = client.return_offset;
        //        AudiosourceBytesRecorded = client.return_recv;
        //    }
        //}

        private void forms_of_selection_FormClosing(object sender, FormClosingEventArgs e) // обработка закрытия формы настройки
        {
            if (forms_of_selection.all_right == true)           //(all_right==true если пользователь нажал Ok и все параметры действительны) 
            {
                copy_data_from_form2(); // сохранение информации из form2
                
                forms_of_selection.Dispose();//уничтожение form2

                if(checkBox2_Checked==true)
                {
                    change_audio();             // смена аудио источника
                }

                if (index_of_combobox1 != 5)// если не выбран показ с друго пк
                {
                    bool flag = checkbox3;      //блокировка записи
                    stoprec();                  // остановка записи
                    change_video();             // смена видео источника
                   
                    if (form_had_been_opened == true)
                    {
                        if (flag == true)
                        {
                           MessageBox.Show("Запись с этого источника начнется только после команды CUT или REC");
                        }                      
                        
                    }
                    form_had_been_opened = true;// поднимаем флаг, что мы уже открывали форму
                }
                else
                {
                    try
                    {
                        if (videosource.IsRunning == true) //останавливаем видио поток
                        {
                            videosource.SignalToStop();
                            videosource.WaitForStop();
                            videosource.Stop();
                        }
                        try
                        {
                            AudiosourceStream.StopRecording();
                            AudiosourceStream.Dispose();
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка остановки захвата с источников");
                    }

           //////// запуск  получения  картинки с друго компа

                }
                forms_of_selection.all_right = false;             // меняем значение на false, чтобы при дальнейшем закрытии формы, мы не вернулись сюда
                forms_of_selection.Close();                     // закрываем форм2
            }
        }

       private void copy_data_from_form2()// сохранение информации из form2
        {
            try
            {
                cashvideosource = forms_of_selection.Video;         // устанавливаем значение будущего видео потока
                cashAudiosourceStream = forms_of_selection.Audio; // устанавливаем значение будущего аудио потока

                // сохранение информации для повторной инициализации формы
                checkBox1_Checked = forms_of_selection.checkBox1_was_Checked;
                checkBox2_Checked = forms_of_selection.checkBox2_was_Checked;
                index_of_combobox1 = forms_of_selection.index_combobox1;
                index_of_combobox3 = forms_of_selection.index_combobox3;
                text_of_combobox2 = forms_of_selection.text_of_combobox2;
                textbox = forms_of_selection.text_of_textbox;
                checkbox3 = forms_of_selection.CheckBox3_cheched;
            }
            catch
            {

            }
        }

       private void change_video()
       {
           try
           {
               if (checkBox1_Checked == true)            // запуск процедуры захвата видео с нового потока
               {
                   if (videosource.IsRunning == true) //останавливаем видио поток
                   {
                       videosource.SignalToStop();
                       videosource.WaitForStop();
                       videosource.Stop();                      
                   }
                   pictureBox.Dispose();
                   pictureBox = null;
                   pictureBox = new PictureBox();
                   pictureBox.BackColor = System.Drawing.Color.Black;
                   pictureBox.Location = smallpictureBoxLocation;
                   pictureBox.Size = smallpictureBoxSize;
                   // pictureBox.BorderStyle = BorderStyle.Fixed3D;
                   pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                   outpanel.Controls.Add(pictureBox);
                   pictureboxchanger();
                   videosource = cashvideosource;  //меняем девайс
                   OpenVideoSource();// запускаем                           
               }
               else
               {
                   if (videosource.IsRunning == true) //останавливаем видио поток
                   {
                       videosource.SignalToStop();
                       videosource.WaitForStop();
                       videosource.Stop();
                      
                   }
               }
           }
           catch
           {
               MessageBox.Show("Ошибка: 705 (Ошибка смены видеовхода)");
           }
       }// смена видеовхода

       private void change_audio()
       {
           try
           {
               if (checkBox2_Checked == true)            // запуск процедуры захвата с нового  потока аудио     
               {
                   AudiosourceStream.StopRecording();
                   AudiosourceStream.Dispose();
                   AudiosourceStream = cashAudiosourceStream;
                   OpenAudioSource();
               }
               else
               {
                   AudiosourceStream.StopRecording();
                   AudiosourceStream.Dispose();
               }
           }
           catch
           {
               MessageBox.Show("Ошибка: 704 (Ошибка смены аудиовхода)");
           }
       }// смена аудиовхода


        private void player_NewFrame(object sender, NewFrameEventArgs eventArgs)// событие связанное с  появление нового изображения в потоке
        {
            try
            {
                pictureBox.Image = (Bitmap)eventArgs.Frame.Clone(); //вывод в превью  
                              
                Videosoursecach = (Bitmap)eventArgs.Frame.Clone();
               
                GC.Collect(); // сбор мусора, т.к clone засоряет оперативную память
            }
            catch
            {
            }

        }

        private void sourceStream_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)// захват аудио потока и сохраниния в буфера
        {           
            AudiosourceBytesRecorded = e.BytesRecorded;
            Audiosourcebuffer = e.Buffer;
            Audiosourceoffset = 0;

            if(Isrecordworkingnow == true)
            {
                audiowriter();
            }
        }
       
        # region Свойства

        # region свойства аудио

        public byte[] Audiobuffer// свойство возвращающее аудио буфер
        {
            get
            {
                return Audiosourcebuffer;
            }
        }

        public int Audio_source_Bytes_Recorded// свойство возвращающее количество элементов аудио буфера
        {
            get
            {
                return AudiosourceBytesRecorded;
            }
        }

        public int Audio_source_offset// свойство возвращающее количество элементов аудио
        {
            get
            {
                return Audiosourceoffset;
            }
        }

        #endregion

        public Bitmap Video_sourse_cach// свойство возвращающее текущий кадр видео
        {
            get
            {
               
             
                if(Videosoursecach!=null)
                {
                    return Videosoursecach;   
                }
                else
                {
                    return img;
                }
            }
        }

        #endregion

        #region Запись 

        public void audiowriter() // запись звука
        {
            if (checkbox3 == true)
            {
                if (waveWriter != null)
                {
                    if (Audiosourcebuffer!=null)
                    waveWriter.Write(Audiosourcebuffer, Audiosourceoffset, AudiosourceBytesRecorded);
                    waveWriter.Flush();
                }
            }
        }

        public void videowriter() // запись видео
        {
            if (checkbox3 == true)
            {
                if (Videowriter1 != null)
                {
                    if (Videowriter1.IsOpen != false)
                    {
                        Videowriter1.WriteVideoFrame(Videosoursecach);
                    }
                }
            }

        } 

        public void Cutter(string new_folder)// Рeрезалка записи
        {
            stoprec();
            startrec(new_folder);

        }

        public void startrec(string new_folder)//запуск записи
        {
            Isrecordworkingnow = true;
            if (checkbox3 == true)
            {
                if (checkBox1_Checked == true)
                {
                    VideoOutputfile = System.IO.Path.Combine(new_folder, "camera" + id + ".avi");// генерация имени файла

                    if (Videowriter1 != null)
                        Videowriter1.Close();
                    Videowriter1 = new VideoFileWriter();

                    try
                    {

                        Videowriter1.Open(VideoOutputfile, Videosoursecach.Width, Videosoursecach.Height, 30, VideoCodec.MPEG2, 45000000);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка создания файла для записи видео" + id);
                    }

                    Isrecordworkingnow = true;

                }

                if (checkBox2_Checked == true)
                {
                    AudioOutputfile = System.IO.Path.Combine(new_folder, "audio" + id + ".wav"); // генерация имени файла
                    try
                    {
                        if (waveWriter != null)
                            waveWriter.Close();
                        waveWriter = null;
                        waveWriter = new NAudio.Wave.WaveFileWriter(AudioOutputfile, AudiosourceStream.WaveFormat);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка создания файла для записи аудио" + id);
                    }
                }
            }
        }

        public void stoprec()//остановка записи 
        {
            Isrecordworkingnow = false;
            try
            {
                if (waveWriter != null)
                    waveWriter.Close();
                waveWriter = null;
                if (Videowriter1 != null)
                    Videowriter1.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка остановки  записи потоков " + id);
            }
        }

        #endregion

        private void button4_Click(object sender, EventArgs e)// нажатие клавиши настроек
        {
            try
            {
                if (form_had_been_opened == true)// если ранне уже настраивали
                {
                    forms_of_selection = new Form2(checkBox1_Checked, checkBox2_Checked, index_of_combobox1, index_of_combobox3, text_of_combobox2, textbox, checkbox3);// инициализируем новую форму с старыми параметрами
                    forms_of_selection.FormClosing += forms_of_selection_FormClosing;   // подписываемся на событие ее закрытия
                    forms_of_selection.Show();                                          // открываем форму

                }
                else
                {
                    forms_of_selection = new Form2();                                   // инициализируем новую форму
                    forms_of_selection.FormClosing += forms_of_selection_FormClosing;   // подписываемся на событие ее закрытия
                    forms_of_selection.Show();                                          // открываем форму
                }
                GC.Collect();// сборщик мусора
            }
            catch
            {
                MessageBox.Show("Ошибка 702(открытие окна настроек источника)");
            }
        }

        private void drawing()  // добавление и отрисовка элементов маленькой формы
         {
             //smallbutton 1
             button1.Location = smallbuttonLocation[0];
             button1.Size = smallbuttonSize[0];
             button1.UseVisualStyleBackColor = true;
             button1.Text = "On air";
             outpanel.Controls.Add(button1);

             //smallbutton 2
             button2.Location = smallbuttonLocation[1];
             button2.Size = smallbuttonSize[1];
             button2.UseVisualStyleBackColor = true;
             button2.Text = "Sound";
             outpanel.Controls.Add(button2);

             //smallbutton 3
             button3.Location = smallbuttonLocation[2];
             button3.Size = smallbuttonSize[2];
             button3.Text = "Pic-in-Pic";
             button3.UseVisualStyleBackColor = true;
             outpanel.Controls.Add(button3);

             //smallbutton 4
             button4.Location = smallbuttonLocation[3];
             button4.Size = smallbuttonSize[3];
             button4.Text = "Setup";
             button4.UseVisualStyleBackColor = true;
             outpanel.Controls.Add(button4);

             //smallpictureBox
             pictureBox.BackColor = System.Drawing.Color.Black;
             pictureBox.Location = smallpictureBoxLocation;
             pictureBox.Size = smallpictureBoxSize;
             // pictureBox.BorderStyle = BorderStyle.Fixed3D;
             pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
             outpanel.Controls.Add(pictureBox);

             // label
            if(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width<1810)
            {
                label.Location = new System.Drawing.Point(2, 190);
            }
            else
            {
                label.Location = new System.Drawing.Point(4, 290);
            }
             label.Font = new Font("Arial", 12, FontStyle.Bold);
            switch(id)
            {
                case (0): label.Text = Convert.ToString("1 q"); break;
                case (1): label.Text = Convert.ToString("2 w"); break;
                case (2): label.Text = Convert.ToString("3 e"); break;
                case (3): label.Text = Convert.ToString("4 r"); break;
                case (4): label.Text = Convert.ToString("5 t"); break;
                case (5): label.Text = Convert.ToString("6 y"); break;
            }
                        
             outpanel.Controls.Add(label);

         }

    }
}
