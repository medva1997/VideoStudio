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

        public tcpserver2(string ip_adress)
        {
            this.ip_adress = "192.168.1.4";
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

          
            local = IPAddress.Any;
            listener = new TcpListener(local, 5000);
            listener.Start();
            //connection = listener.AcceptSocket();

            while (connection==null)
            {
                // Принимаем новых клиентов
                connection = listener.AcceptSocket();
            }
            socketStream = new NetworkStream(connection);
            writer = new BinaryWriter(socketStream);
        }


        
           
       

        public void sender(Bitmap image)
        {
            //connection = listener.AcceptSocket();
            //socketStream = new NetworkStream(connection);
            //int nWidth = 2560;
            //int nHeight = 1440;

            int nWidth = 1280;
            int nHeight = 720;

           
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(image, 0, 0, nWidth, nHeight);
           

            MemoryStream ms = new MemoryStream();
            result.Save(ms, ImageFormat.Bmp);

            byte[] arrImage = ms.GetBuffer();

            ms.Close();
            ms.Dispose();
            ms = null;
            writer.Write(arrImage);
            writer.Flush();
           // writer.Close();
           // socketStream.Close();
           // connection.Close();
            GC.Collect();
        }



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
    }
}
