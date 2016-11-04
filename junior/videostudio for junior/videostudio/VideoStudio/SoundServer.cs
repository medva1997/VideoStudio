using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using LumiSoft.Net.RTP;
using LumiSoft.Net.RTP.Debug;
using LumiSoft.Net.Media.Codec.Audio;
using LumiSoft.Net.Media;
using NAudio.Codecs;



namespace VideoStudio
{
    class SoundServer
    {       
        
        private RTP_Session m_pSession = null;
        private RTP_SendStream sendStream;
        private RTP_MultimediaSession m_pRtpSession;
    
        public SoundServer(string m_pLocalIP, int m_pLocalPort, string m_pRemoteIP, int m_pRemotePort)
        {
            ////m_pLocalIP = "127.0.0.1";
            //////m_pLocalPort = 6000;
            ////m_pRemoteIP = "127.0.0.1";
            //////m_pRemotePort = 7000;

            //m_pRtpSession = new RTP_MultimediaSession(RTP_Utils.GenerateCNAME());
            //m_pRtpSession.CreateSession(new RTP_Address(IPAddress.Parse(m_pLocalIP), (int)m_pLocalPort, (int)m_pLocalPort + 1), new RTP_Clock(0, 8000));
            //m_pRtpSession.Sessions[0].AddTarget(new RTP_Address(IPAddress.Parse(m_pRemoteIP), (int)m_pRemotePort, (int)m_pRemotePort + 1));
            //m_pRtpSession.Sessions[0].NewSendStream += new EventHandler<RTP_SendStreamEventArgs>(m_pRtpSession_NewSendStream);
            //m_pRtpSession.Sessions[0].NewReceiveStream += new EventHandler<RTP_ReceiveStreamEventArgs>(m_pRtpSession_NewReceiveStream);
            //m_pRtpSession.Sessions[0].Payload = 0;
            //m_pRtpSession.Sessions[0].Start();
            //m_pSession = m_pRtpSession.Sessions[0];
            //sendStream = m_pSession.CreateSendStream();     
     
            }
        public void send(byte[] buffer)
        {
        //    if (buffer != null)
        //    {
        //        byte[] encodedData = Encode(buffer, 0, buffer.Length);
        //        //// Send audio frame.
        //        RTP_Packet packet = new RTP_Packet();
        //        packet.Timestamp = m_pSession.RtpClock.RtpTimestamp;
        //        packet.Data = encodedData;
        //        sendStream.Send(packet);

           //}
        }


        private void m_pRtpSession_NewSendStream(object sender, RTP_SendStreamEventArgs e)
        {
           
        }

        #region method m_pRtpSession_NewReceiveStream

        /// <summary>
        /// This method is called when RTP session gets new receive stream.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pRtpSession_NewReceiveStream(object sender, RTP_ReceiveStreamEventArgs e)
        {

        }

        #region Properties implementation

        ///// <summary>
        ///// Gets active codec.
        ///// </summary>
        //public AudioCodec ActiveCodec
        //{

        //    get { return m_pActiveCodec; }
        //}

       

        #endregion

        #endregion

        //public byte[] Encode(byte[] data, int offset, int length)
        //{
        //    var encoded = new byte[length / 2];
        //    int outIndex = 0;
        //    for (int n = 0; n < length; n += 2)
        //    {
        //        encoded[outIndex++] = MuLawEncoder.LinearToMuLawSample(BitConverter.ToInt16(data, offset + n));
        //    }
        //    return encoded;
        //}

        //public byte[] Decode(byte[] data, int offset, int length)
        //{
        //    var decoded = new byte[length * 2];
        //    int outIndex = 0;
        //    for (int n = 0; n < length; n++)
        //    {
        //        short decodedSample = MuLawDecoder.MuLawToLinearSample(data[n + offset]);
        //        decoded[outIndex++] = (byte)(decodedSample & 0xFF);
        //        decoded[outIndex++] = (byte)(decodedSample >> 8);
        //    }
        //    return decoded;
        //}


        public byte[] Encode(byte[] data, int offset, int length)
        {
            byte[] encoded = new byte[length / 2];
            int outIndex = 0;
            for (int n = 0; n < length; n += 2)
            {
                encoded[outIndex++] = ALawEncoder.LinearToALawSample(BitConverter.ToInt16(data, offset + n));
            }
            return encoded;
        }

        //public byte[] Decode(byte[] data, int offset, int length)
        //{
        //    byte[] decoded = new byte[length * 2];
        //    int outIndex = 0;
        //    for (int n = 0; n < length; n++)
        //    {
        //        short decodedSample = ALawDecoder.ALawToLinearSample(data[n + offset]);
        //        decoded[outIndex++] = (byte)(decodedSample & 0xFF);
        //        decoded[outIndex++] = (byte)(decodedSample >> 8);
        //    }
        //    return decoded;
        //}
                       

    }
}
