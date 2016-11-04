using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;



namespace VideoStudio
{
    class TCPServer
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

        public TCPServer()
        {

        }

        public TCPServer(int port, string command)
        {
            this.command = command;
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

        private void Connector()
        {
            while (connection == null)
                connection = listener.AcceptSocket();    // Принимаем новых клиентов

            socketStream = new NetworkStream(connection);
            writer = new BinaryWriter(socketStream);
            flag = true;
        }

        public void FFmpeg()
        {
           
                processFFmpeg = new Process();
                //ProcessStartInfo startInfo = new ProcessStartInfo(@"ffmpeg\bin\ffplay.exe", "-i tcp:127.0.0.1:" + int.Parse(textBox1.Text));
                
                string arg = "-i tcp:127.0.0.1:" + port + " " + command;    // Формируем команду
                ProcessStartInfo startInfo = new ProcessStartInfo(@"ffmpeg\bin\ffmpeg.exe", arg);
                startInfo.CreateNoWindow = true;    // Показ/скрытие консоли
                startInfo.UseShellExecute = false;
                try
                {      
                processFFmpeg.StartInfo = startInfo;
                processFFmpeg.Start();    // Запуск

              

                flag = true;
               // MessageBox.Show("Трансляции успешно запущена");                
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

        public void Sender(Bitmap image)
        {
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
                }
                catch (Exception e)
                {
                    Closing();
                } 
     
                GC.Collect();
            }
        }

        public void Closing()    // Остановка всего
        {
            try
            {
                flag = false;
                processFFmpeg.Kill();
                MessageBox.Show("Поток онлайн трансляции остановлен");
                writer.Flush();
                writer.Close();
                socketStream.Close();
                connection.Close();
                listener.Stop();              
                Thread.Sleep(100);               
                
            }
            catch
            {
                
            }
            finally
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
//private System.Windows.Forms.Timer timer;
//object locker = new object();


//public TCPServer(string ip_adress)
//{
//    this.ip_adress=ip_adress;
//    //wavein = new WaveIn();
//    //wavein.WaveFormat = new WaveFormat(44100, 16, 2);
//    //wavein.DataAvailable += new EventHandler<WaveInEventArgs>(Recorded);
//    // waveout = new WaveOut();
//    // wavProv = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
//    // waveout.Init(wavProv);

//    //timer = new System.Windows.Forms.Timer();
//    //timer.Interval = 31;
//    //timer.Enabled = true;
//    //timer.Tick += TimerTick;

//    server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//    //wavein.StartRecording();
//    readThread = new Thread(new ThreadStart(RunServer));
//    readThread.Start();
//    //timer.Start();
//}

////void TimerTick(object sender, EventArgs e)
////{
////    Recorded();
////}

////private void Recorded()
////{
////    try
////    {
////        IPEndPoint cIpEnd = new IPEndPoint(ipAddress.Parse(ip_adress), 12131);
////        server.SendTo(buffer, cIpEnd);
////    }
////    catch { }
////}

//public void RunServer()
//{
//    TcpListener listener;

//        // ipAddress local = ipAddress.Parse("192.168.1.3");
//        ipAddress local = ipAddress.Any;
//        listener = new TcpListener(local, 5000);
//        listener.Start();

//        while (true)
//        {
//            connection = listener.AcceptSocket();
//            socketStream = new NetworkStream(connection);
//            writer = new BinaryWriter(socketStream);
//            MemoryStream ms = new MemoryStream();
//            img.Save(ms, ImageFormat.Bmp);                          

//            byte[] arrImage = ms.GetBuffer();

//            ms.Close();
//            ms.Dispose();
//            ms = null;
//            writer.Write(arrImage);
//            writer.Flush();
//            writer.Close();
//            socketStream.Close();
//            connection.Close();
//            Thread.Sleep(31);
//        }
//}

//public Bitmap image
//{
//    get { return img; }
//    set { img = value; }
//}

//public byte[] audiobyte
//{
//    set { buffer = value; }
//}

#endregion
