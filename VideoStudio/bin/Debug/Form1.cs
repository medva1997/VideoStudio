using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using AForge.Video;
using AForge;
using AForge.Video.DirectShow;

namespace VideoStudio
{
    
    public partial class Form1 : Form
    {
        #region объявление переменных
        private smallwindow[] preview = new smallwindow[number_of_small_panels];            // объекты предпросмотра
        private static int number_of_small_panels = 6;                                      // количество маленьких панелей
        private static int number_of_small_buttons= 4;                                      // количество маленьких кнопок
        private int FormWidth;                                                              // ширина формы
        private int FormHeight;                                                             // высота окна формы
        private int selected_device = 0;                                                    // выбранный сейчас девайс
        private int lastselected_device = 0;                                                // выбранный до этого девайс (не используется тк не используется функция)     
        private int counter = 0;                                                            // переменная счетчик при смене источников(не используется тк не используется функция)     
        bool flag_to_out = false;                                                           // флаг необходимости транслировать
        private bool Record_is_work = false;                                                // флаг необходимости записывать
        private string way_to_folder = "";                                                  // путь к папке назначения для хранения записанных файлов
        private int second_selected_device = -1;                                            // номер устройства для pip  (-1 -- pip отключено)      
        Bitmap result;                                                                      // результирующая картинка

        //BigpictureBox
        private System.Windows.Forms.PictureBox BigpictureBox = new System.Windows.Forms.PictureBox();  // окно вывода главного изображения       
        private  System.Drawing.Point BigpictureBoxLocation;                                            // верхний левый угол главного окна вывода изображения
        private  System.Drawing.Size BigpictureBoxSize;                                                 //размер главного окна вывода изображения

        //panel
        private System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();                    // первая панель для маленьких окон
        private  System.Drawing.Point panelLocation;                                                    // верхний левый угол первой панели для маленьких окон 
        private  System.Drawing.Size panelSize;                                                         //размер первой панели для маленьких окон

        //smallpanels
        private System.Windows.Forms.Panel[] smallpanels = new System.Windows.Forms.Panel[number_of_small_panels];  // массив панелей маленьких окон маленьких окон        
        private System.Drawing.Point[] smallpanelsLocation = new System.Drawing.Point[number_of_small_panels];      // инициализация массива расположения маленьких панелей 
        private System.Drawing.Size[] smallpanelsSize = new System.Drawing.Size[number_of_small_panels];            // инициализация массива размера маленьких панелей 

        //smallbuttons
        private System.Drawing.Point[] smallbuttonLocation = new System.Drawing.Point[number_of_small_buttons];     // инициализация массива расположения кнопок  маленьких панелей 
        private System.Drawing.Size[] smallbuttonSize = new System.Drawing.Size[number_of_small_panels];            // инициализация массива размера кнопок  маленьких панелей 

        //smallpictureBox
        private System.Drawing.Point smallpictureBoxLocation; // расположение маленького pictureBox
        private System.Drawing.Size smallpictureBoxSize;      // размер маленького pictureBox

        //rec/stop button 
        private System.Windows.Forms.Button recbutton = new System.Windows.Forms.Button();              // кнопка записи
        private System.Drawing.Point recbuttonLocation;                                                 // верхний левый угол кнопки записи
        private System.Drawing.Size recbuttonSize;                                                      //размер кнопки записи

        //cut button 
        private System.Windows.Forms.Button cutbutton = new System.Windows.Forms.Button();              // кнопка резки видео
        private System.Drawing.Point cutbuttonLocation;                                                 // верхний левый угол кнопки резки видео
        private System.Drawing.Size cutbuttonSize;                                                      //размер кнопки резки видео

        //setting button 
        private System.Windows.Forms.Button settingbutton = new System.Windows.Forms.Button();          // кнопка настройки
        private System.Drawing.Point settingbuttonLocation;                                             // верхний левый угол кнопки настройки
        private System.Drawing.Size settingbuttonSize;                                                  //размер кнопки настройки


       

        #endregion

        //временные переменные
        int nWidth = 1280;  // разрешение выходного потока
        int nHeight = 720;  // разрешение выходного потока
        int port = 5000;   // номер порта для вывода информации

        private tcpserver server;

        Form3 settings;
        int videocouner;
        int audiocounter;
        byte[] buffer;

        System.Windows.Forms.Timer time;
        System.Windows.Forms.Timer time2;


        private void Form1_Load(object sender, EventArgs e)
        {
            selected_device = 0;
            if (Sets_sizes(1366) == true)// установка параметров размеров элементов и возврат состояния корректности инициализации(true-все хорошо)
            {
                createconponents();// создаем визуальные элементы

                for (int i = 0; i < number_of_small_panels; i++)// создаем объекты маленьких окон
                {
                      preview[i] = new smallwindow(smallpanels[i], smallbuttonLocation, smallbuttonSize, smallpictureBoxLocation, smallpictureBoxSize, i);
                }
            }          
            
            Click_event();  // подключение к обработке событий


            time = new System.Windows.Forms.Timer(); //таймер1
            time.Interval = 31;
            time.Enabled = true;
            time.Tick += time_Tick;
            time.Start();

            time2 = new System.Windows.Forms.Timer(); //таймер1
            time2.Interval = 31;
            time2.Enabled = true;
            time2.Tick += time2_Tick;
            time2.Start();

        }

        private void time_Tick(object sender, EventArgs e)// обработка пеервого таймера
        {
            worker1(); 

        }

        private void time2_Tick(object sender, EventArgs e)// обработка второго таймера
        {


            Bitmap result2 = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(result2))
                g.DrawImage(preview[selected_device].Video_sourse_cach, 0, 0, nWidth, nHeight);

            if (second_selected_device != -1)//pip
            {
                using (Graphics g = Graphics.FromImage(result2))
                    g.DrawImage(preview[second_selected_device].Video_sourse_cach, nWidth - nWidth / 3 - 20, nHeight - nHeight / 3 - 20, nWidth / 3, nHeight / 3);
            }

            result = result2;
            BigpictureBox.Image = result;// вывод в главное окно

            if (flag_to_out == true)
            {
                server.sender(result); //отправка по сети

            }  
        }


        

        private void worker1()
        {
            //for (int i = 0; i < number_of_small_panels; i++) // переделать стуктуру приема по сети
            //{
            //    preview[i].update_information();
            //}

            if (Record_is_work == true)// запись
            {
                //if ((videocouner != 15 && videocouner != 30) && videocouner != 8)//
                //{
                for (int i = 0; i < number_of_small_panels; i++)
                {
                    preview[i].videowriter();
                }
                videocouner++;
                //}
                //else
                //{
                //    videocouner++;
                //}
                if (videocouner > 29)
                {
                    videocouner = 0;
                }


                if (videocouner % 3 == 0)
                {
                    for (int i = 0; i < number_of_small_panels; i++)
                    {
                        preview[i].audiowriter();
                    }
                }
            }


           

            //if (flag_to_out == true)
            //{
            //    server.sender(result); //отправка по сети

            //    //if (audiocounter % 3 == 0)
            //    //{
            //    //    buffer = preview[0].Audiobuffer;
            //    //    server.Audiobuffer = buffer;
            //    //    server.Recorded();
            //    //}
            //}

            GC.Collect(); // сбор мусора
        }
       
        private void Image_changer(int newselected_device, int lastselected_device1)//(не используется)смена изображения с переходом
        {
            if (newselected_device == lastselected_device1)
            {
                BigpictureBox.Image = preview[selected_device].Video_sourse_cach;
            }
            else
            {
                if(counter<=10)
                {
                    if (counter %2==0)
                        BigpictureBox.Image = preview[lastselected_device1].Video_sourse_cach;
                    else
                        BigpictureBox.Image = preview[newselected_device].Video_sourse_cach;
                    counter++;
                }
                else
                {
                    counter = 0;
                    lastselected_device = selected_device;
                }

            }

        }

        private void settingbutton_Click(object sender, EventArgs e)//обработка нажатия кнопки настроек 
        {
            settings = new Form3();
             settings.Show();
             settings.FormClosing += settings_FormClosing;
            
        }

        void settings_FormClosing(object sender, FormClosingEventArgs e)
        {
           
           // Record_is_work =settings.do_record;
            way_to_folder = settings.folder_for_records;
            server = new tcpserver(port);
            flag_to_out = true;
                   
        }


        #region Стабильно работает

        #region обработка  событий

        #region  Click on pip

        private void preview0button3_Click(object sender, EventArgs e)//обработка нажатия кнопки on pip
        {
            pip(0);
        }

        private void preview1button3_Click(object sender, EventArgs e)//обработка нажатия кнопки on pip
        {
            pip(1);
        }

        private void preview2button3_Click(object sender, EventArgs e)//обработка нажатия кнопки on pip
        {
            pip(2);
        }

        private void preview3button3_Click(object sender, EventArgs e)//обработка нажатия кнопки on pip
        {
            pip(3);
        }

        private void preview4button3_Click(object sender, EventArgs e)//обработка нажатия кнопки on pip
        {
            pip(4);
        }

        private void preview5button3_Click(object sender, EventArgs e)//обработка нажатия кнопки on pip
        {
            pip(5);
        }

        #endregion

        

        #region Click on Air

        private void preview0button1_Click(object sender, EventArgs e)//обработка нажатия кнопки on air
        {
            selected_device = 0;
        }

        private void preview1button1_Click(object sender, EventArgs e)//обработка нажатия кнопки on air
        {
            selected_device = 1;
        }

        private void preview2button1_Click(object sender, EventArgs e)//обработка нажатия кнопки on air
        {
            selected_device = 2;
        }

        private void preview3button1_Click(object sender, EventArgs e)//обработка нажатия кнопки on air
        {
            selected_device = 3;

        }

        private void preview4button1_Click(object sender, EventArgs e)//обработка нажатия кнопки on air
        {
            selected_device = 4;
        }

        private void preview5button1_Click(object sender, EventArgs e)//обработка нажатия кнопки on air
        {
            selected_device = 5;
        }

        #endregion

        #region DoubleClick on preview

        private void preview0pictureBox_DoubleClick(object sender, EventArgs e)// обработка двойного щелчка на окно превью
        {
            selected_device = 0;
        }

        private void preview1pictureBox_DoubleClick(object sender, EventArgs e)// обработка двойного щелчка на окно превью
        {
            selected_device = 1;
        }

        private void preview2pictureBox_DoubleClick(object sender, EventArgs e)// обработка двойного щелчка на окно превью
        {
            selected_device = 2;
        }

        private void preview3pictureBox_DoubleClick(object sender, EventArgs e)// обработка двойного щелчка на окно превью
        {
            selected_device = 3;
        }

        private void preview4pictureBox_DoubleClick(object sender, EventArgs e)// обработка двойного щелчка на окно превью
        {
            selected_device = 4;
        }

        private void preview5pictureBox_DoubleClick(object sender, EventArgs e)// обработка двойного щелчка на окно превью
        {
            selected_device = 5;
        }

        #endregion

        private void Form1_KeyDown(object sender, KeyEventArgs e) //обработка нажатия клавиш на клаве
        {
            // recbutton.Text =Convert.ToString(e.KeyValue);
            switch (Convert.ToString(e.KeyValue))
            {
                // numlock                
                case ("97"): selected_device = e.KeyValue - 97; break;//1
                case ("98"): selected_device = e.KeyValue - 97; break;
                case ("99"): selected_device = e.KeyValue - 97; break;
                case ("100"): selected_device = e.KeyValue - 97; break;
                case ("101"): selected_device = e.KeyValue - 97; break;
                case ("102"): selected_device = e.KeyValue - 97; break;//6

                //обычные клавиши
                case ("49"): selected_device = e.KeyValue - 49; break;//1
                case ("50"): selected_device = e.KeyValue - 49; break;
                case ("51"): selected_device = e.KeyValue - 49; break;
                case ("52"): selected_device = e.KeyValue - 49; break;
                case ("53"): selected_device = e.KeyValue - 49; break;
                case ("54"): selected_device = e.KeyValue - 49; break;//6

                //qwerty for pic
                case ("81"): pip(0); break;//1
                case ("87"): pip(1); break;
                case ("69"): pip(2); break;
                case ("82"): pip(3); break;
                case ("84"): pip(4); break;
                case ("89"): pip(5); break;//6

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)// обработка закрытия формы (завершение всех потоков)
        {
            for (int i = 0; i < preview.Length; i++)
            {
                preview[i].CloseCurrentVideoSource();
                preview[i].stoprec();
            }          

        }

        #endregion

        private void pip(int new_second_dev)// смена видеовхода картинка в картинке
        {
            if (second_selected_device != new_second_dev)
            {
                second_selected_device = new_second_dev;
            }
            else
            {
                second_selected_device = -1;
            }

        }

        private void Click_event()// подключение к обработке событий
        {
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(Form1_KeyDown);// нажатие клавиш на клаве
            this.FormClosing += Form1_FormClosing;                                  // закрытие формы

            // click on air
            preview[0].button1.Click += new System.EventHandler(preview0button1_Click);
            preview[1].button1.Click += new System.EventHandler(preview1button1_Click);
            preview[2].button1.Click += new System.EventHandler(preview2button1_Click);
            preview[3].button1.Click += new System.EventHandler(preview3button1_Click);
            preview[4].button1.Click += new System.EventHandler(preview4button1_Click);
            preview[5].button1.Click += new System.EventHandler(preview5button1_Click);

            //click on pip
            preview[0].button3.Click += new System.EventHandler(preview0button3_Click);
            preview[1].button3.Click += new System.EventHandler(preview1button3_Click);
            preview[2].button3.Click += new System.EventHandler(preview2button3_Click);
            preview[3].button3.Click += new System.EventHandler(preview3button3_Click);
            preview[4].button3.Click += new System.EventHandler(preview4button3_Click);
            preview[5].button3.Click += new System.EventHandler(preview5button3_Click);

            // DoubleClick to air
            preview[0].pictureBox.DoubleClick += new System.EventHandler(preview0pictureBox_DoubleClick);
            preview[1].pictureBox.DoubleClick += new System.EventHandler(preview1pictureBox_DoubleClick);
            preview[2].pictureBox.DoubleClick += new System.EventHandler(preview2pictureBox_DoubleClick);
            preview[3].pictureBox.DoubleClick += new System.EventHandler(preview3pictureBox_DoubleClick);
            preview[4].pictureBox.DoubleClick += new System.EventHandler(preview4pictureBox_DoubleClick);
            preview[5].pictureBox.DoubleClick += new System.EventHandler(preview5pictureBox_DoubleClick);
            // клавиша настроек
            settingbutton.Click += new System.EventHandler(settingbutton_Click);
            // клавиша записи
            recbutton.Click += new System.EventHandler(recbutton_Click);
            // клавиша нарезки записи видео
            cutbutton.Click += new System.EventHandler(cutbutton_Click);

        }

        private void createconponents()// создание объектов с нужными размерами
        {
            // Form  

            this.Size = new Size(FormWidth, FormHeight);// задаем размеры формы

            // pictureBox

            BigpictureBox.BackColor = System.Drawing.SystemColors.ActiveCaptionText;// цвет 
            BigpictureBox.Location = BigpictureBoxLocation;
            BigpictureBox.Size = BigpictureBoxSize;
            // pictureBox.TabIndex = 0;
            BigpictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(BigpictureBox);

            // panel

            panel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            panel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            panel.Location = panelLocation;
            panel.Size = panelSize;
            this.Controls.Add(panel);

            // smallpanels from panel    

            for (int i = 0; i < number_of_small_panels - 2; i++)
            {
                Panel pan2 = new Panel();
                pan2.Location = smallpanelsLocation[i];
                pan2.BackColor = System.Drawing.Color.Gray;
                pan2.Size = smallpanelsSize[i];
                smallpanels[i] = pan2;
                panel.Controls.Add(smallpanels[i]);

            }

            // smallpanels from form

            for (int i = number_of_small_panels - 2; i < number_of_small_panels; i++)
            {
                Panel pan2 = new Panel();
                pan2.Location = smallpanelsLocation[i];
                pan2.BackColor = System.Drawing.Color.Gray;
                pan2.Size = smallpanelsSize[i];
                smallpanels[i] = pan2;
                this.Controls.Add(smallpanels[i]);
            }

            //rec/stop button

            recbutton.Location = recbuttonLocation;
            recbutton.Size = recbuttonSize;
            recbutton.Text = "rec/stop";
            recbutton.TabIndex = 1;
            this.Controls.Add(recbutton);

            //cut button

            cutbutton.Location = cutbuttonLocation;
            cutbutton.Size = cutbuttonSize;
            cutbutton.Text = "cut";
            cutbutton.TabIndex = 3;
            this.Controls.Add(cutbutton);

            //setting button

            settingbutton.Location = settingbuttonLocation;
            settingbutton.Size = settingbuttonSize;
            settingbutton.Text = "Настройка";
            settingbutton.TabIndex = 2;
            this.Controls.Add(settingbutton);


        }

        private bool Sets_sizes(int DisplayWidth)// установка параметров размеров элементов (ширина экрана)
        {
            if (DisplayWidth == 1366)
            {
                // переменные размеров

                //form
                FormWidth = 1366;// ширина формы
                FormHeight = 730;// высота окна формы

                //BigpictureBox
                BigpictureBoxLocation = new System.Drawing.Point(125, 5);// верхний левый угол главного окна вывода изображения
                BigpictureBoxSize = new System.Drawing.Size(767, 420);//размер главного окна вывода изображения

                //panel
                panelLocation = new System.Drawing.Point(12, 457);// верхний левый угол первой панели для маленьких окон 
                panelSize = new System.Drawing.Size(1326, 231);//размер первой панели для маленьких окон

                //smallpanel 1
                smallpanelsLocation[0] = new System.Drawing.Point(3, 3);
                smallpanelsSize[0] = new System.Drawing.Size(324, 225);

                //smallpanel 2
                smallpanelsLocation[1] = new System.Drawing.Point(333, 3);
                smallpanelsSize[1] = new System.Drawing.Size(324, 225);

                //smallpanel 3
                smallpanelsLocation[2] = new System.Drawing.Point(663, 3);
                smallpanelsSize[2] = new System.Drawing.Size(324, 225);

                //smallpanel 4
                smallpanelsLocation[3] = new System.Drawing.Point(993, 3);
                smallpanelsSize[3] = new System.Drawing.Size(324, 225);


                //smallpanel 5
                smallpanelsLocation[4] = new System.Drawing.Point(1005, 2); ;
                smallpanelsSize[4] = new System.Drawing.Size(324, 225);

                //smallpanel 6
                smallpanelsLocation[5] = new System.Drawing.Point(1005, 230);
                smallpanelsSize[5] = new System.Drawing.Size(324, 225);

                //smallbutton 1
                smallbuttonLocation[0] = new System.Drawing.Point(34, 184);
                smallbuttonSize[0] = new System.Drawing.Size(70, 41);

                //smallbutton 2
                smallbuttonLocation[1] = new System.Drawing.Point(106, 184);
                smallbuttonSize[1] = new System.Drawing.Size(70, 41);

                //smallbutton 3
                smallbuttonLocation[2] = new System.Drawing.Point(178, 184);
                smallbuttonSize[2] = new System.Drawing.Size(70, 41);

                //smallbutton 4
                smallbuttonLocation[3] = new System.Drawing.Point(250, 184);
                smallbuttonSize[3] = new System.Drawing.Size(70, 41);

                //smallpictureBox
                smallpictureBoxLocation = new System.Drawing.Point(1, 3);
                smallpictureBoxSize = new System.Drawing.Size(320, 180);

                //rec/stop button                  
                recbuttonLocation = new System.Drawing.Point(12, 5);
                recbuttonSize = new System.Drawing.Size(107, 124);

                //cut button 
                cutbuttonLocation = new System.Drawing.Point(12, 229);
                cutbuttonSize = new System.Drawing.Size(107, 100);

                //setting button 
                settingbuttonLocation = new System.Drawing.Point(15, 153);
                settingbuttonSize = new System.Drawing.Size(107, 47); ;

                return true;
            }
            else
            {
                if (DisplayWidth == 1920)
                {
                    MessageBox.Show("не поддерживается");
                }
                return false;
            }

        }

        public Form1()
        {
            InitializeComponent();
            KeyPreview = true; //перелючаем клавиатуру на форму   
        }

        private void recbutton_Click(object sender, EventArgs e)//обработка нажатия кнопки on rec
        {
            if (Record_is_work == false)
            {
                Record_is_work = false;
                string new_way = System.IO.Path.Combine(way_to_folder, Convert.ToString(DateTime.Now.ToString().Replace(':', '-')));
                System.IO.Directory.CreateDirectory(new_way);
                for (int i = 0; i < preview.Length; i++)
                {
                    preview[i].Cutter(new_way);
                }
                Record_is_work = true;
                recbutton.BackColor = Color.Red;
            }
            else
            {
                Record_is_work = false;
                for (int i = 0; i < preview.Length; i++)
                {
                    preview[i].stoprec();
                }
                recbutton.BackColor = cutbutton.BackColor;
            }
        }

        private void cutbutton_Click(object sender, EventArgs e)//обработка нажатия кнопки on cut
        {
            Record_is_work = false;
            string new_way = System.IO.Path.Combine(way_to_folder, Convert.ToString(DateTime.Now.ToString().Replace(':', '-')));
            for (int i = 0; i < preview.Length; i++)
            {
                System.IO.Directory.CreateDirectory(new_way);
                preview[i].Cutter(new_way);
            }
            Record_is_work = true;
            recbutton.BackColor = Color.Red;

        }

        #endregion

    }
}
