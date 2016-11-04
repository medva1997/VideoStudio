using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace VideoStudio
{
    class ImageWorker : MainFrame
    {
        public delegate void MethodContainer();
        // Событие OnCount c типом делегата MethodContainer.
        public event MethodContainer onImageСhange;

        private int fadeLength = 15;    // Количество кадров для перехода, по умолчанию 15, но может меняться свойством FadeLength
        private int selectedDevice;    // Номер текущего устройства, может меняться свойством CameraNow
        private int lastSelectedDevice;    // Номер предыдущего устройства,может меняться свойством CameraBefore
        private int secondSelectedDevice = -1;    // Номер устройства для PictureInPicture  (-1 - PictureInPicture отключено),
                                                  // может меняться свойством CameraPictureInPicture
        private int outWidth = 1280;    // Разрешение выходного потока
        private int outHeight = 720;    // Разрешение выходного потока
        private Bitmap mainPicture;    // Результирующее изображение, которое отдается функцией ImageToBigPictureBoxраз  в 1/30 сек
        private int counter;    // Количество отработанных изображений
        private System.Windows.Forms.Timer timer;    // Создание картинки 30 раз в сек
        private SmallWindow[] preview;
        // base.ImageFromCamera(int index) возвращает bitmap с нужной камеры

        public ImageWorker(SmallWindow[] preview)
        {
            this.preview = preview;
            CreateBitmap();    // Создаем пустой черный bitmap для избегания ошибок

            timer = new System.Windows.Forms.Timer();    // Таймер
            timer.Interval = 31;
            timer.Enabled = true;
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)    // Обработка таймера
        {
            ImageСhanger();
            //ImageToBigPictureBox(mainPicture);
            onImageСhange();    // Отправка картинки
        }

        ///<summary>
        /// Смена изображения с переходом.
        /// </summary>
        private void ImageСhanger()
        {
            if (selectedDevice == lastSelectedDevice)
            {
                Bitmap result2 = new Bitmap(outWidth, outHeight);    // Зум картинки
                using (Graphics g = Graphics.FromImage(result2))
                    g.DrawImage(ImageFromCamera(selectedDevice,preview), 0, 0, outWidth, outHeight);

                if (secondSelectedDevice != -1)    // PictureInPicture зум и наложение
                {
                    using (Graphics g = Graphics.FromImage(result2))
                        g.DrawImage(ImageFromCamera(secondSelectedDevice, preview), outWidth - outWidth / 3 - 20, 
                            outHeight - outHeight / 3 - 20, outWidth / 3, outHeight / 3);
                }

                mainPicture = result2;               
            }
            else
            {
                if (counter <= fadeLength)
                {
                    Bitmap result2 = new Bitmap(outWidth, outHeight);
                    using (Graphics g = Graphics.FromImage(result2))
                        g.DrawImage(ImageFromCamera(lastSelectedDevice, preview), 0, 0, outWidth, outHeight);
                    float[][] ptsArray = 
                    {
                        new float[] { 1, 0, 0, 0, 0},
                        new float[] { 0, 1, 0, 0, 0},
                        new float[] { 0, 0, 1, 0, 0},
                        new float[] { 0, 0, 0, (float) counter / 15, 0},
                        new float[] { 0, 0, 0, 0, 1}
                    };
                    ImageAttributes ImgAttributes = new ImageAttributes();
                    ImgAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    using (Graphics g = Graphics.FromImage(result2))
                        g.DrawImage(ImageFromCamera(selectedDevice, preview), new Rectangle(0, 0, outWidth, outHeight),
                        0, 0, ImageFromCamera(selectedDevice, preview).Width,
                        ImageFromCamera(selectedDevice, preview).Height, GraphicsUnit.Pixel, ImgAttributes);

                    if (secondSelectedDevice != -1)    // PictureInPicture зум и наложение
                    {
                        using (Graphics g = Graphics.FromImage(result2))
                            g.DrawImage(base.ImageFromCamera(secondSelectedDevice, preview), outWidth - outWidth / 3 - 20, outHeight - outHeight / 3 - 20, outWidth / 3, outHeight / 3);
                    }
                    mainPicture = result2;
                    
                    counter++;
                }
                else
                {
                    counter = 0;
                    lastSelectedDevice = selectedDevice;

                    Bitmap result2 = new Bitmap(outWidth, outHeight);    // Зум картинки
                    using (Graphics g = Graphics.FromImage(result2))
                        g.DrawImage(ImageFromCamera(selectedDevice, preview), 0, 0, outWidth, outHeight);

                    if (secondSelectedDevice != -1)    // PictureInPicture зум и наложение
                    {
                        using (Graphics g = Graphics.FromImage(result2))
                            g.DrawImage(ImageFromCamera(secondSelectedDevice, preview), outWidth - outWidth / 3 - 20, 
                                outHeight - outHeight / 3 - 20, outWidth / 3, outHeight / 3);
                    }

                    mainPicture = result2;    
                }
            }
        }

        /// <summary>
        /// Cоздаем пустой черный bitmap для избегания ошибок.
        /// </summary>
        void CreateBitmap()
        {
            Bitmap flag = new Bitmap(720, 720);
            for (int x = 0; x < flag.Height; ++x)
                for (int y = 0; y < flag.Width; ++y)
                    flag.SetPixel(x, y, Color.Black);

            mainPicture = flag;
        }

        /// <summary>
        /// Возврат готового  итового изображения.
        /// </summary>
        public Bitmap OutPicture
        {
            get { return mainPicture; }
        }

        /// <summary>
        /// Устанавливает высоту выходного потока.
        /// </summary>
        public int OutHeight
        {
            get { return outHeight; }
            set { outHeight = value; }
        }

        /// <summary>
        /// Устанавливает ширину выходного потока.
        /// </summary>
        public int OutWidth
        {
            get { return outWidth; }
            set { outWidth = value; }
        }

        /// <summary>
        /// Устанавливает номер источника для PictureInPicture.
        /// </summary>
        public int CameraPictureInPicture
        {
            get { return secondSelectedDevice; }
            set { secondSelectedDevice = value; }
        }

        /// <summary>
        /// Устанавливает номера предыдущего видеоисточника.
        /// </summary>
        public int CameraBefore
        {
            get { return lastSelectedDevice; }
            set { lastSelectedDevice = value; }
        }

        /// <summary>
        /// Устанавливает номера текущего видеоисточника.
        /// </summary>
        public int CameraNow
        {
            get { return selectedDevice; }
            set { selectedDevice = value; }
        }

        /// <summary>
        /// Устанавливает количество кадров для перехода.
        /// </summary>
        public int FadeLength
        {
            get { return fadeLength; }
            set { fadeLength = value; }
        }
    }
}
