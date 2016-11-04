using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
//using System.Collections.Generic;

namespace VideoStudio
{
    class VideoRecorder
    {
        private Socket connection;
        private NetworkStream socketStream;
        private BinaryWriter writer;
        private TcpListener listener;
        private IPAddress local;
        private Process processFFmpeg;    
        private string command;
        private int port;
        private bool flag;    // Флаг работы функциии sender
        Bitmap image;

        private System.Windows.Forms.Timer smalltimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer bigtimer = new System.Windows.Forms.Timer();
        private int bigconter = 0;
        private int smallconter = 0;
        private int lostframe = 0;
      

        public VideoRecorder(int port)// запуск для ожидания подключения ffmpeg
        {
            this.port = port;
            local = IPAddress.Any;
            listener = new TcpListener(local, port);
            Thread client;
            try
            {
                listener.Start();
                client = new Thread(Connector);    // Запускаем поток в котором мы дожидаемся подключения 
                client.Start();
            }
            catch
            {
                MessageBox.Show("Ошибка начала записи");
                try
                {
                    listener.Stop();
                    flag = false;
                }
                catch
                {

                }
            }
            timers();
        }       

        private void Connector()// ожидание подключение ffmpeg
        {
            while (connection == null)
                connection = listener.AcceptSocket();    // Принимаем новых клиентов
            socketStream = new NetworkStream(connection);
            writer = new BinaryWriter(socketStream);
            flag = true;
        }

        public void FFmpeg()// запус ffmpeg
        {
           
                processFFmpeg = new Process();
               
                ProcessStartInfo startInfo = new ProcessStartInfo(@"ffmpeg\bin\ffmpeg.exe", command);
                startInfo.CreateNoWindow = true;    // Показ/скрытие консоли
                startInfo.UseShellExecute = false;
                //startInfo.RedirectStandardError = true;              
                //startInfo.RedirectStandardOutput = true;
                processFFmpeg.StartInfo = startInfo;
             try
                {
                processFFmpeg.Start();    // Запуск
                processFFmpeg.PriorityClass = ProcessPriorityClass.RealTime;

                //StreamReader myStreamReader = processFFmpeg.StandardOutput;
                //// Read the standard error of net.exe and write it on to console.
                //MessageBox.Show(myStreamReader.ReadLine());
                flag = true;
            }
            catch
            {
                MessageBox.Show("Ошибка запуска записи");
                try
                {
                    listener.Stop();
                    flag = false;
                }
                catch
                {

                }
            }
        }

        public void Open(string videoOutputFile, int Width,int Height, int framerate,string VideoCodec, int bitrate, int port)// формирование команды для записи
        {
            this.port = port;
           // videoOutputFile = Environment.CurrentDirectory +"\\" +videoOutputFile;
            //string commandline = "-video_size " + Width + "x" + Height +  "-vcodec " + VideoCodec + " "+ videoOutputFile;
            //string commandline = " -video_size " + Width + "x" + Height +" -vcodec " + VideoCodec + " -r 30 " + videoOutputFile;
           // string commandline = "-c:v libx264 -b:v 4000k -minrate 4000k -maxrate 4000k -bufsize 1835k" + " -r 25 " + videoOutputFile ;
            string commandline = "-vcodec libx264 -preset ultrafast -qp 0 " + "-r 25 " + videoOutputFile;            
            string arg = "-i tcp:127.0.0.1:" + port + " " + commandline;         
            command = arg;
            FFmpeg();
        }

        public bool IsOpen// свойство для проверки родительским классом
        {
            get { return true; }
        }

        public void WriteVideoFrame(Bitmap image)//отправка кадра на запись
        {

           // qimage.Enqueue(image);
            this.image = image;
            if (flag)
            {

                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Bmp);

                byte[] arrImage = ms.GetBuffer();

                ms.Close();
                ms.Dispose();
                ms = null;
                try    // Если не возможно отправить данные абоненту, то мы все выключаем
                {

                    writer.Write(arrImage);
                    writer.Flush();
                    bigconter++;
                    smallconter++;
                }
                catch
                {
                    // MessageBox.Show("Запись остановлена");
                    Close();
                }
                GC.Collect();
            }           
           
        }

        public void Close()    // Остановка всего
        {
            try
            {
                flag = false;
                writer.Flush();
                writer.Close();
                socketStream.Close();
                connection.Close();
                listener.Stop();
                processFFmpeg.Kill();
                smalltimer.Stop();
                bigtimer.Stop();
                               
            }
            catch
            {
               
            }   
        }

        private void timers()
        {
            smalltimer = new System.Windows.Forms.Timer();
            smalltimer.Interval = 100;
            smalltimer.Enabled = true;
            smalltimer.Tick += smallTick;
            smalltimer.Start();
            bigtimer = new System.Windows.Forms.Timer();
            bigtimer.Interval = 500;
            bigtimer.Enabled = true;
            bigtimer.Tick += bigTick;
            bigtimer.Start();
        }

        private void smallTick(object sender, EventArgs e)
        {
            if (flag)
                if (smallconter < 3)
                {
                    WriteVideoFrame(image);
                }
            smallconter = 0;

            if (lostframe > 1)
            {
                WriteVideoFrame(image);
                lostframe--;
                bigconter--;
                smallconter--;
            }
        }

        private void bigTick(object sender, EventArgs e)
        {
            if (flag)
                if (smallconter < 15)
                {
                    lostframe = 15 - bigconter;
                }
            bigconter = 0;

            if (lostframe > 1)
            {
                WriteVideoFrame(image);
                lostframe--;
                bigconter--;
                smallconter--;
            }
        }

        private void bigsender(object counter)
        {
            for (int i = (int)counter; i < 25; i++)
            {
                WriteVideoFrame(image);

            }
        }


        // Queue<Image> qimage = new Queue<Image>();
        //private void bigTick(object sender, EventArgs e)
        //{

        //    Thread tt = new Thread(send);
        //    tt.Priority = ThreadPriority.Highest;
        //    tt.Start();
        //}

        //private void send()
        //{
        //    while (qimage.Count > 0)
        //    {
        //        bigsender();
        //    }
        //}


        //private void bigsender()
        //{
        //    Bitmap image = (Bitmap)qimage.Dequeue();
        //    if (flag)
        //    {

        //        MemoryStream ms = new MemoryStream();
        //        image.Save(ms, ImageFormat.Bmp);

        //        byte[] arrImage = ms.GetBuffer();

        //        ms.Close();
        //        ms.Dispose();
        //        ms = null;
        //        try    // Если не возможно отправить данные абоненту, то мы все выключаем
        //        {

        //            writer.Write(arrImage);
        //            writer.Flush();
        //            bigconter++;
        //            smallconter++;
        //        }
        //        catch
        //        {
        //            // MessageBox.Show("Запись остановлена");
        //            Close();
        //        }
        //        GC.Collect();
        //    }           
        //}


    }
}
