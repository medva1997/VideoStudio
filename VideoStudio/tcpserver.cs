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

namespace VideoStudio
{
    class tcpserver
    {
       private Socket connection;
        private NetworkStream socketStream;
        private BinaryWriter writer;      
        TcpListener listener;
        IPAddress local;
        

        bool flag = false;// флаг работы функциии sender

        public tcpserver(int port)
        {
            local = IPAddress.Any;
            listener = new TcpListener(local, port);
            try
            {
                listener.Start();
                Thread client = new Thread(connector);// запускаем поток в котором мы дожидаемся подключения 
                client.Start();
            }
            catch
            {
                MessageBox.Show("Ошибка создания подключения");
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

        public void sender(Bitmap image)
        {
            if (flag == true)
            {
                ////зумим картинку
                //Bitmap res = new Bitmap(nWidth, nHeight);
                //using (Graphics g = Graphics.FromImage(res))
                //    g.DrawImage(image, 0, 0, nWidth, nHeight);
                //res.Save(ms, ImageFormat.Bmp);

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
            flag = false;
            writer.Flush();
            writer.Close();
            socketStream.Close();
            connection.Close();
            listener.Stop();
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

    }
}
