using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using System.Net;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.Reflection;


namespace VideoStudio
{
    class tcpserver
    {
       private Socket connection;
        private NetworkStream socketStream;
        private BinaryWriter writer;      
        TcpListener listener;
        IPAddress local;
        Process process_ffmpeg;    
        private string command;
        int port;
     
        

        bool flag = false;// флаг работы функциии sender

        public tcpserver()
        {

        }

        public tcpserver(int port, string command)
        {
            this.command = command;
            this.port = port;
       
            local = IPAddress.Any;
            listener = new TcpListener(local, port);
            Thread client;
            try
            {
                listener.Start();
                client = new Thread(connector);// запускаем поток в котором мы дожидаемся подключения 
                client.Start();
            }
            catch
            {
                MessageBox.Show("Ошибка создания подключения");
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

        private void connector()
        {
           
            while (connection == null)
            {
                // Принимаем новых клиентов
                connection = listener.AcceptSocket();
            }
            socketStream = new NetworkStream(connection);
            writer = new BinaryWriter(socketStream);
            flag = true;
            
        }

        public void ffmpeg()
        {
            try
            {
                process_ffmpeg = new Process();
                //ProcessStartInfo startInfo = new ProcessStartInfo(@"ffmpeg\bin\ffplay.exe", "-i tcp:127.0.0.1:" + int.Parse(textBox1.Text));

                string arg = "-i tcp:127.0.0.1:" + port + " " + command;// формируем команду
                ProcessStartInfo startInfo = new ProcessStartInfo(@"ffmpeg\bin\ffmpeg.exe", arg);
                startInfo.CreateNoWindow = true;// показ/непоказ консоли
                startInfo.UseShellExecute = false;
                //startInfo.RedirectStandardError = true;              
                //startInfo.RedirectStandardOutput = true;
                process_ffmpeg.StartInfo = startInfo;
                process_ffmpeg.Start();//запуск

                //StreamReader myStreamReader = process_ffmpeg.StandardOutput;
                //// Read the standard error of net.exe and write it on to console.
                //MessageBox.Show(myStreamReader.ReadLine());

                flag = true;
            }
            catch
            {
                MessageBox.Show("Ошибка запуска ffmpeg");
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

        public void sender(Bitmap image)
        {
            if (flag == true)
            {
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Bmp);

                byte[] arrImage = ms.GetBuffer();

                ms.Close();
                ms.Dispose();
                ms = null;
                try//  если не возможно отправить данные абоненту, то мы все выключаем
                {

                    writer.Write(arrImage);
                    writer.Flush();
                }
                catch
                {
                    closing();
                }       
                GC.Collect();
            }
        }

        public void closing() //остановка всего
        {
            try
            {
                flag = false;
                writer.Flush();
                writer.Close();
                socketStream.Close();
                connection.Close();
                listener.Stop();
                process_ffmpeg.Kill();
                MessageBox.Show("Поток онлайн трансляции остановлен");
            }
            catch
            {
               
            }
          
                
        }   


      

    

    }
}
#region старая версия отправки
////private Thread mListenThread;
//private Socket server;
//private WaveIn wavein;
//// private WaveOut waveout;
////private static BufferedWaveProvider wavProv;
////server


//private Bitmap img;
//private Bitmap imgcash;
//private Socket connection;
//private Thread readThread;
//private NetworkStream socketStream;
//private BinaryWriter writer;
//private string ip_adress;
//private byte[] buffer = null;
//private System.Windows.Forms.Timer timer1;
//object locker = new object();



//public tcpserver(string ip_adress)
//{
//    this.ip_adress=ip_adress;
//    //wavein = new WaveIn();
//    //wavein.WaveFormat = new WaveFormat(44100, 16, 2);
//    //wavein.DataAvailable += new EventHandler<WaveInEventArgs>(Recorded);
//    // waveout = new WaveOut();
//    // wavProv = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
//    // waveout.Init(wavProv);

//    //timer1 = new System.Windows.Forms.Timer();
//    //timer1.Interval = 31;
//    //timer1.Enabled = true;
//    //timer1.Tick += timer1_Tick;

//    server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//    //wavein.StartRecording();
//    readThread = new Thread(new ThreadStart(RunServer));
//    readThread.Start();
//    //timer1.Start();
//}

////void timer1_Tick(object sender, EventArgs e)
////{
////    Recorded();
////}

////private void Recorded()
////{
////    try
////    {
////        IPEndPoint cIpEnd = new IPEndPoint(IPAddress.Parse(ip_adress), 12131);
////        server.SendTo(buffer, cIpEnd);
////    }
////    catch { }
////}


//public void RunServer()
//{
//    TcpListener listener;

//        // IPAddress local = IPAddress.Parse("192.168.1.3");
//        IPAddress local = IPAddress.Any;
//        listener = new TcpListener(local, 5000);
//        listener.Start();

//        while (true)
//        {

//           connection = listener.AcceptSocket();
//                    socketStream = new NetworkStream(connection);
//                    writer = new BinaryWriter(socketStream);
//                    MemoryStream ms = new MemoryStream();
//                    img.Save(ms, ImageFormat.Bmp);                          

//                    byte[] arrImage = ms.GetBuffer();

//                    ms.Close();
//                    ms.Dispose();
//                    ms = null;
//                    writer.Write(arrImage);
//                    writer.Flush();
//                    writer.Close();
//                    socketStream.Close();
//                    connection.Close();



//            Thread.Sleep(31);
//        }

//}

//public Bitmap image
//{
//    get
//    {
//        return img;
//    }
//    set
//    {
//        img = value;
//    }
//}

//public byte[] audiobyte
//{
//    set
//    {
//        buffer = value;
//    }
//}

#endregion
