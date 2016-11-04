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
using AForge.Video.DirectShow;

namespace VideoStudio
{
    

    class smallwindow
    {

        #region Объявление переменных
        public System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox();  // окно вывода картинки
        public System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();             // кнопка on air
        private System.Windows.Forms.Button button2 = new System.Windows.Forms.Button();            // кнопка работы со звуком
        private System.Windows.Forms.Button button3 = new System.Windows.Forms.Button();            // кнопка картинка в картинке
        private System.Windows.Forms.Button button4 = new System.Windows.Forms.Button();            // кнопка настройки
        private System.Windows.Forms.Label label = new System.Windows.Forms.Label();                // надпись с номером потока
        private  Panel outpanel;                                                                    // панель на которой настологаются 4 кнопки и picturebox
        private System.Drawing.Point[] smallbuttonLocation;                                         //  массив расположения кнопок  маленьких панелей 
        private System.Drawing.Size[] smallbuttonSize;                                              // массив размера кнопок  маленьких панелей 
        private System.Drawing.Point smallpictureBoxLocation;                                       // расположение pictureBox
        private System.Drawing.Size smallpictureBoxSize;                                            // размер pictureBox
        private int id;                                                                             //номер потока/камеры


        private Form2 forms_of_selection;                                                           // вспомогательная форма для настойка водных параметров
        private bool checkBox1_Checked;                                                             // выбрано ли использование видео устройства
        private bool checkBox2_Checked;                                                             // выбрано ли использование аудио устройства
        private int index_of_combobox1;                                                             // номер выбранного типа видеоустройства 
        private int index_of_combobox3;                                                             // номер выбранного аудио устройства 
        private string text_of_combobox2;                                                           // выбранное видео устройство
        private string textbox;                                                                     // частота дискретизации
        private bool form_had_been_opened;                                                          // флаг на использование ранее сохраненных данных true- испльзовать



        private IVideoSource videosource;                                                           // видео поток       
        private IVideoSource cashvideosource;                                                       // временная переменная для смены видеопотоков
        private NAudio.Wave.WaveIn AudiosourceStream;                                               // аудио поток
        private NAudio.Wave.WaveIn cashAudiosourceStream;                                           // временная переменная для смены аудиопотоков
        private NAudio.Wave.DirectSoundOut waveOut = null;                                          // объект воспроизведения звука
        private NAudio.Wave.WaveFileWriter waveWriter = null;                                       // объект записи в файл
        private byte[] Audiosourcebuffer;                                                           // буфер аудио
        private int AudiosourceBytesRecorded;                                                       // количество полученного звука
        private int Audiosourceoffset;      
        private string AudioOutputfile="audio.wav";                                                 // переменна для храенения пути к файлу записи аудио
        private string VideoOutputfile;                                                             //переменна для храенения пути к файлу записи видео
        private Bitmap Videosoursecach=null;                                                        // переменна для хранения текущего кадра 
        private tcpclient client= new tcpclient();       
        private Timer timer1;
        private Timer timer2;
        #endregion

        public smallwindow()
        {

        }

        
        public smallwindow(Panel outpanel, System.Drawing.Point[] smallbuttonLocation, System.Drawing.Size[] smallbuttonSize, System.Drawing.Point smallpictureBoxLocation, System.Drawing.Size smallpictureBoxSize, int id)
        {
            // передача параметров конструктора в поля
            this.outpanel = outpanel;
            this.smallbuttonLocation = smallbuttonLocation;
            this.smallbuttonSize = smallbuttonSize;
            this.smallpictureBoxLocation = smallpictureBoxLocation;
            this.smallpictureBoxSize = smallpictureBoxSize;
            this.id = id;
            videosource = new VideoCaptureDevice();
            AudiosourceStream =new  NAudio.Wave.WaveIn();
            try
            {
                drawing();// отрисовка 4 кнопок, номера камеры, picturebox
            }
            catch
            {
                MessageBox.Show("Ошибка: 703");
            }
            // подписываемся на события кликов мышью  (первую клавишу обрабатывает form1)    
            button2.Click += new System.EventHandler(button2_Click);
            button3.Click += new System.EventHandler(button3_Click);
            button4.Click += new System.EventHandler(button4_Click);          

        }

            
        private void button2_Click(object sender, EventArgs e)
        {
           MessageBox.Show("2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("3");
        }

        private void start_data_transfer()
        {
            CloseCurrentVideoSource();

            client = new tcpclient(text_of_combobox2);
            timer1 = new Timer();
            timer1.Interval = 31;
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;


        }

        void timer1_Tick(object sender, EventArgs e)
        {
            Videosoursecach = client.Image;
            pictureBox.Image = client.Image;
            Audiosourcebuffer = client.bytedata;
            Audiosourceoffset = client.return_offset;
            AudiosourceBytesRecorded = client.return_recv;
        }
       

      

        private void OpenVideoSource()
        {
            CloseCurrentVideoSource(); // останавливаем все предыдущие действия
            videosource.NewFrame += new NewFrameEventHandler(player_NewFrame); // подписываемся на событие прихода нового видио кадра
            videosource.Start();

        }

        private void player_NewFrame(object sender, NewFrameEventArgs eventArgs)// событие связанное с  появление нового изображения в потоке
         {
             pictureBox.Image = (Bitmap)eventArgs.Frame.Clone();     //вывод в превью  
             Videosoursecach = (Bitmap)eventArgs.Frame.Clone();
             GC.Collect();                                           // сбор мусора, т.к clone засоряет оперативную память
         }

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
             try
             {
                 timer1.Stop();
             }
             catch
             {

             }
                 
                
                
                

           

         }

        private void sourceStream_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)// захват аудио потока для записи в файл и сохраниния в буфера
        {
            if (waveWriter == null) return;

            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            AudiosourceBytesRecorded = e.BytesRecorded;
            Audiosourcebuffer = e.Buffer;
            Audiosourceoffset = 0;
            waveWriter.Flush();
        }

        private void OpenAudioSource()// запуск работы с аудио
        {
            AudiosourceStream.StartRecording();// получение потока
            try
            {
                //NAudio.Wave.WaveInProvider waveIn = new NAudio.Wave.WaveInProvider(AudiosourceStream);
                //waveOut = new NAudio.Wave.DirectSoundOut();
                //waveOut.Init(waveIn);
                //waveOut.Play();// вывод аудио потока на динамики
            }
            catch
            {
                MessageBox.Show("ошибка: 700");
            }

            AudiosourceStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(sourceStream_DataAvailable);
            waveWriter = new NAudio.Wave.WaveFileWriter(AudioOutputfile, AudiosourceStream.WaveFormat);
        }

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

        public Bitmap Video_sourse_cach// свойство возвращающее текущий кадр видео
        {
            get
            {
                return Videosoursecach;
            }
        }

        void forms_of_selection_FormClosing(object sender, FormClosingEventArgs e) // обработка закрытия формы настройки
        {
            if (forms_of_selection.all_right == true)           //(all_right==true если пользователь нажал Ok и все параметры действительны) 
            {
                cashvideosource = forms_of_selection.Video;         // устанавливаем значение видео потока
                cashAudiosourceStream = forms_of_selection.Audio; // устанавливаем значение аудио потока

                // сохранение информации для повторной инициализации формы
                checkBox1_Checked = forms_of_selection.checkBox1_was_Checked;
                checkBox2_Checked = forms_of_selection.checkBox2_was_Checked;
                index_of_combobox1 = forms_of_selection.index_combobox1;
                index_of_combobox3 = forms_of_selection.index_combobox3;
                text_of_combobox2 = forms_of_selection.text_of_combobox2;
                textbox = forms_of_selection.text_of_textbox;

                form_had_been_opened = true;
                forms_of_selection.Dispose();
                if (index_of_combobox1 != 5)
                {
                    try
                    {

                        if (checkBox1_Checked == true)            // запуск процедуры захвата с нового потока
                        {
                            if (videosource.IsRunning == true) //останавливаем видио поток
                            {
                                videosource.SignalToStop();
                                videosource.WaitForStop();
                                videosource.Stop();
                            }
                            videosource = cashvideosource;  //меняем девайс
                            OpenVideoSource();// запускаем
                            button3.Enabled = true;
                            button1.Enabled = true;
                        }
                        else
                        {
                            if (videosource.IsRunning == true) //останавливаем видио поток
                            {
                                videosource.SignalToStop();
                                videosource.WaitForStop();
                                videosource.Stop();
                            }
                            button3.Enabled = false;
                            button1.Enabled = false;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка: 705");
                    }
                    try
                    {

                        if (checkBox2_Checked == true)            // запуск процедуры захвата с нового  потока аудио     
                        {
                            AudiosourceStream.StopRecording();
                            AudiosourceStream.Dispose();
                            AudiosourceStream = cashAudiosourceStream;
                            OpenAudioSource();
                            button2.Enabled = true;
                        }
                        else
                        {
                            AudiosourceStream.StopRecording();
                            AudiosourceStream.Dispose();
                            button2.Enabled = false;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка: 704");
                    }
                }
                else
                {
                    CloseCurrentVideoSource();
                    start_data_transfer();
                }
                forms_of_selection.all_right = false;             // меняем значение на false, чтобы при дальнейшем закрытии формы, мы не вернулись сюда
                forms_of_selection.Close();                     // закрываем форм2
            }
            //throw new NotImplementedException();
        }

        private void button4_Click(object sender, EventArgs e)// нажатие клавиши настроек
        {
            try
            {
                if (form_had_been_opened == true)// если ранне уже настраивали
                {
                    forms_of_selection = new Form2(checkBox1_Checked, checkBox2_Checked, index_of_combobox1, index_of_combobox3, text_of_combobox2, textbox);// инициализируем новую форму
                    forms_of_selection.FormClosing += forms_of_selection_FormClosing;   // подписываемся на событие ее закрытия
                    forms_of_selection.Show();                                          // открываем форму

                }
                else
                {
                    forms_of_selection = new Form2();                                   // инициализируем новую форму
                    forms_of_selection.FormClosing += forms_of_selection_FormClosing;   // подписываемся на событие ее закрытия
                    forms_of_selection.Show();                                          // открываем форму
                }
                GC.Collect();
            }
            catch
            {
                MessageBox.Show("Ошибка 702");
            }
        }

        private void drawing()  // добавление и отрисовка элементов маленькой формы
         {
             //smallbutton 1
             button1.Location = smallbuttonLocation[0];
             button1.Size = smallbuttonSize[0];
             button1.UseVisualStyleBackColor = true;
             button1.Text = "on air";
             outpanel.Controls.Add(button1);

             //smallbutton 2
             button2.Location = smallbuttonLocation[1];
             button2.Size = smallbuttonSize[1];
             button2.UseVisualStyleBackColor = true;
             button2.Text = "звук";
             outpanel.Controls.Add(button2);

             //smallbutton 3
             button3.Location = smallbuttonLocation[2];
             button3.Size = smallbuttonSize[2];
             button3.Text = "PiP";
             button3.UseVisualStyleBackColor = true;
             outpanel.Controls.Add(button3);

             //smallbutton 4
             button4.Location = smallbuttonLocation[3];
             button4.Size = smallbuttonSize[3];
             button4.Text = "Настройка";
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
             label.Location = new System.Drawing.Point(10, 195);
             label.Text = Convert.ToString(id + 1);
             outpanel.Controls.Add(label);

         }


    }
}
