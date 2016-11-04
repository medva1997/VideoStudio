using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using AForge;
using AForge.Video;
using AForge.Video.FFMPEG;


namespace VideoStudio
{
    class Videomix
    {
        private PictureBox HidePictureBox;
        private VideoFileWriter Videowriter1;
        private Bitmap frame;
        private string VideoOutputfile;
        private int height;
        private int width;

        public Videomix(int width, int height )
        {
            frame = new Bitmap(width, height,System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            this.height = height;
            this.width = width;
            HidePictureBox = new System.Windows.Forms.PictureBox();
            HidePictureBox.BackColor = System.Drawing.Color.Black;
            HidePictureBox.Location = new System.Drawing.Point(0,0);
            HidePictureBox.Size = new Size(width, height);
            HidePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        }

        public void changepicture(Bitmap pic)
        {
            HidePictureBox.Image = pic;           
        }

        public void videowriter()// запись в поток на вывод
        {
            if (Videowriter1 != null)
            {
                Videowriter1.WriteVideoFrame((Bitmap)HidePictureBox.Image);
            }
        }


        public void Cutter(string new_folder)// Рeрезалка записи
        {
            stoprec();
            startrec(new_folder);

        }

        public void startrec(string new_folder)//запуск записи
        {
            try
            {
                VideoOutputfile = System.IO.Path.Combine(new_folder, "Mainvideo.avi");
               
            }
            catch
            {
                MessageBox.Show("Ошибка создания файла для сохранения потока");
            }
           
            try
            {
              
                    Videowriter1 = new VideoFileWriter();
                    Videowriter1.Open(VideoOutputfile, width, height, 30, VideoCodec.MPEG2, 45000000);
               
            }
            catch
            {
                MessageBox.Show("Ошибка инициаллизации видео файла");
            }
        }

        public void stoprec()//остановка записи
        {
            try
            {
                Videowriter1.Close();

            }
            catch
            {
                //  MessageBox.Show("Ошибка остановки потоков записи " + id);
            }
        }
    }
}
