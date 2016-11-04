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

namespace VideoStudio
{
    class tcpserver2
    {
        private Bitmap img;       
        private Socket connection;
        private Thread readThread;
        private NetworkStream socketStream;
        private BinaryWriter writer;
        private string ip_adress;
        private Socket server;
        TcpListener listener;
        IPAddress local;

        //audio
        private byte[] Audiosourcebuffer;
        private int AudiosourceBytesRecorded;
        private int Audiosourceoffset;


        public tcpserver2(string ip_adress)
        {
            this.ip_adress = ip_adress;
            System.Threading.Thread creater_of_connection = new Thread(connecter);
            creater_of_connection.Start();
            creater_of_connection.Name = "создание подключение";
            
        }

        private void connecter()
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            local = IPAddress.Any;
            listener = new TcpListener(local, 5000);            
            listener.Start();
        }

     

        public void sender(object image1)
        {
            try
            {
                try
                {
                    Bitmap image = (Bitmap)image1;
                    connection = listener.AcceptSocket();
                    socketStream = new NetworkStream(connection);
                    writer = new BinaryWriter(socketStream);
                    MemoryStream ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Bmp);

                    byte[] arrImage = ms.GetBuffer();

                    ms.Close();
                    ms.Dispose();
                    ms = null;
                    writer.Write(arrImage);
                    writer.Flush();
                    writer.Close();
                    socketStream.Close();
                    connection.Close();
                    GC.Collect();
                }
                catch
                {

                }
            }
            catch
            {

            }
        }

        public void Recorded()
        {
            try
            {
                IPEndPoint cIpEnd = new IPEndPoint(IPAddress.Parse(ip_adress), 12131);
                server.SendTo(Audiosourcebuffer, cIpEnd);
            }
            catch { }
        }

        public byte[] Audiobuffer// свойство возвращающее аудио буфер
        {
            set
            {
                Audiosourcebuffer = value;
            }
            get
            {
                return Audiosourcebuffer;
            }
        }

        public int Audio_source_Bytes_Recorded// свойство возвращающее количество элементов аудио буфера
        { 
            set
            {
                 AudiosourceBytesRecorded=value;
            }
            get
            {
                return AudiosourceBytesRecorded;
            }
        }

        public int Audio_source_offset// свойство возвращающее количество элементов аудио
        {
            set
            {
                Audiosourceoffset = value;
            }

            get
            {
                return Audiosourceoffset;
            }
        }

       
    }
}
