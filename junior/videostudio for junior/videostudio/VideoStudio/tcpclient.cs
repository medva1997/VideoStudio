using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace VideoStudio
{
    internal class TCPclient
    {
        private NetworkStream output;
        private Thread readThread;
        private int it; // Флаг для остановки потоков
        private int st; // Флаг для остановки потоков
        private Thread mListenThread; // Поток прослушки аудио
        private Socket server; // Объект сокета
        private WaveIn wavein; // Входящий аудио поток
        // private WaveOut waveout;
        private static BufferedWaveProvider wavProv; // Работа со звуком
        private string ipAddress; // Переменная с айпи адресом

        private Bitmap image; // Переменная хранит текущее изображание
        private byte[] data; // Хранит тукущее аудио
        private int offset; // Часть звука
        private int recv; // Часть звука
        private System.Windows.Forms.Timer timer; // Таймер для завершения потоков в этом классе
        public bool flag = false; // Переменная для остановки потоков

        // client
        public TCPclient()
        {
            flag = false;
        }

        public TCPclient(string ipAddress)
        {
            flag = false;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 200;
            timer.Enabled = true;
            timer.Tick += TimerTick;

            this.ipAddress = ipAddress;

            image = null;

            wavein = new WaveIn();
            wavein.WaveFormat = new WaveFormat(44100, 16, 2);
            //wavein.DataAvailable += new EventHandler<WaveInEventArgs>(Recorded);
            //waveout = new WaveOut();
            wavProv = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
            //waveout.Init(wavProv);
            mListenThread = new Thread(new ParameterizedThreadStart(ListenStart));
            mListenThread.Start();
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            wavein.StartRecording();

            it = 0;
            readThread = new Thread(new ThreadStart(RunClient));
            readThread.Start();
        }

        private void TimerTick(object sender, EventArgs e) // Завершаем потоки если необходимо
        {
            if (flag)
                Closing();
        }

        public void RunClient() // Запуск клиента приема изображения
        {
            TcpClient client;
            try
            {
                while (it == 0)
                {
                    client = new TcpClient();
                    client.Connect(ipAddress, 5000);
                    output = client.GetStream();
                    image = (Bitmap) Bitmap.FromStream(output);
                    output.Flush();
                    output.Close();
                    client.Close();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Ошибка Соединения",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(Environment.ExitCode);
            }
        }

        public void Closing() // Завершаем потоки если необходимо 
        {
            it = 1;
            st = 1;
            output.Close();
            //waveout.Dispose();
            wavein.StopRecording();
        }

        private delegate void DisplayDelegate(string message);

        private void ListenStart(object sender) // Запуск клиента приема аудио
        {
            IPEndPoint sIpEnd = new IPEndPoint(new IPAddress(Convert.ToInt64(ipAddress)), 12131);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(sIpEnd);

            IPEndPoint remoteIpEnd = new IPEndPoint(new IPAddress(Convert.ToInt64(ipAddress)), 0);
            EndPoint Remote = (EndPoint) remoteIpEnd;

            //waveout.Play();
            //waveout.Volume = 1;
            int offset1 = 0;
            while (st == 0)
            {
                byte[] data1 = new byte[65535];
                int recv1 = sock.ReceiveFrom(data1, ref Remote); // Количество принятых блоков???
                wavProv.AddSamples(data1, offset1, recv1);
                data = data1;
                offset = offset1;
                recv = recv1;
                wavProv.ClearBuffer();
            }
            sock.Close();
        }


        public byte[] ByteData
        {
            get { return data; }
        }

        public Bitmap Image
        {
            get { return image; }
        }

        public int Offset
        {
            get { return offset; }
        }

        public int Recv
        {
            get { return recv; }
        }
    }
}