using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;
using System.Linq;

using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace VideoStudio
{
    class tcpclient
    {
        private NetworkStream output;       
        private Thread readThread;        
        private int it = 0;                                                     // флаг для остановки потоков
        private int st = 0;                                                     // флаг для остановки потоков
        private Thread mListenThread;                                           //поток прослушки аудио
        private Socket server;                                                  // объект сокета
        private WaveIn wavein;                                                  //входящий аудио поток
       // private WaveOut waveout;
        private static BufferedWaveProvider wavProv;                            // работа со звуком
        private string text_ip;                                                 // переменная с айпи адресом

        private Bitmap image;                                                   // переменная хранит текущее изображание
        private byte[] data=null;                                               // хранит тукущее аудио
        private int offset=0;                                                   //часть звука
        private int recv = 0;                                                   //часть звука
        private System.Windows.Forms.Timer timer1;                              // таймер для завершения потоков в этом классе
        public bool flag = false;                                               // переменная для остановки потоков


        // client
        public tcpclient()                                                      //пустой конструкьор
        {
            flag = false;
        }

        public tcpclient(string text_ip )                                       //конструктор
        {
            flag = false;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval =200;
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;

             this.text_ip=text_ip;

            image=null;

            wavein = new WaveIn();
            wavein.WaveFormat = new WaveFormat(44100, 16, 2);
           // wavein.DataAvailable += new EventHandler<WaveInEventArgs>(Recorded);
           // waveout = new WaveOut();
             wavProv = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
           // waveout.Init(wavProv);
            mListenThread = new Thread(new ParameterizedThreadStart(startListen));
            mListenThread.Start();
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            wavein.StartRecording();

            it = 0;
            readThread = new Thread(new ThreadStart(RunClient));
            readThread.Start();
        }

        void timer1_Tick(object sender, EventArgs e)                            // завершаем потоки если необходимо
        {
            if (flag == true)
            {
                Closing();
            }
        }
                        
     

        public void RunClient()                                                 //запуск клиента приема изображения
        {
            TcpClient client;
            
            try
            {
                while (it == 0)
                {
                    client = new TcpClient();
                    client.Connect(text_ip, 5000);
                    output = client.GetStream();
                    image = (Bitmap)Bitmap.FromStream(output);
                    output.Flush();
                    output.Close();
                    client.Close();

                }
               
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Ошибка Соединения",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(System.Environment.ExitCode);
            }
        }

        public void Closing()                                                // завершаем потоки если необходимо 
        {
            it = 1;
            st = 1;           
            output.Close();           
            //waveout.Dispose();
            wavein.StopRecording();
        }

        private delegate void DisplayDelegate(string message);

        private void startListen(object sender)                                     //запуск клиента приема аудио
        {
            IPEndPoint sIpEnd = new IPEndPoint(IPAddress.Any, 12131);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(sIpEnd);

            IPEndPoint remoteIpEnd = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)remoteIpEnd;
            
           // waveout.Play();
           // waveout.Volume = 1;
            int offset1 = 0;
            while (st == 0)
            {               
                byte[] data1 = new byte[65535];               
                int recv1 = sock.ReceiveFrom(data1, ref Remote); // количество принятых блоков???
                wavProv.AddSamples(data1, offset1, recv1);
                data = data1;
                offset = offset1;
                recv = recv1;
                wavProv.ClearBuffer();
            }
            sock.Close();
        }
             
        // свойства

        public byte[] bytedata
        {
            get
            {
                return data;
            }
        }

        public Bitmap Image
        {
            get
            {
                return image;
            }
        }

        public int return_offset
        {
            get
            {
                return offset;
            }
        }

        public int return_recv
        {
            get
            {
                return recv;
            }
        }

    }
}
