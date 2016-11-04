using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VideoStudio
{
    partial class MainFrame
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing"> Истинно, если управляемый ресурс должен быть удален; иначе ложно. </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainFrame));
            this.SuspendLayout();

            // MainFrame
            AutoScaleDimensions = new SizeF(6F, 13F);
            //AutoScaleMode = AutoScaleMode.Font; // Ругается, что странно
            ClientSize = new Size(284, 261);
            Icon = ((Icon)(resources.GetObject("$this.Icon")));
            Name = "MainFrame";
            Text = "VideoStudio";
            Load += new EventHandler(this.MainFrameLoad);
            ResumeLayout(false);
        }

        #endregion

        // bigPictureBox
        private PictureBox bigPictureBox = new PictureBox();    // Окно вывода главного изображения       
        private Point bigPictureBoxLocation;    // Верхний левый угол главного окна вывода изображения
        private Size bigPictureBoxSize;    // Размер главного окна вывода изображения

        // panel
        private Panel panel = new Panel();    // Первая панель для маленьких окон
        private Point panelLocation;    // Верхний левый угол первой панели для маленьких окон 
        private Size panelSize;    // Размер первой панели для маленьких окон

        // smallPanels
        private Panel[] smallPanels = new Panel[smallPanelsNumber];    // Массив панелей маленьких окон маленьких окон        
        private Point[] smallPanelsLocation = new Point[smallPanelsNumber];    // Инициализация массива расположения маленьких панелей 
        private Size[] smallPanelsSize = new Size[smallPanelsNumber];    // Инициализация массива размера маленьких панелей 

        // smallButtons
        private Point[] smallButtonLocation = new Point[smallButtonsNumber];    // Инициализация массива расположения кнопок маленьких панелей 
        private Size[] smallButtonSize = new Size[smallPanelsNumber];    // Инициализация массива размера кнопок маленьких панелей 

        // smallPictureBox
        private Point smallPictureBoxLocation;    // Расположение маленького pictureBox
        private Size smallPictureBoxSize;    // Размер маленького pictureBox

        // recButton 
        private Button recButton = new Button();    // Кнопка записи
        private Point recButtonLocation;    // Верхний левый угол кнопки записи
        private Size recButtonSize;    // Размер кнопки записи

        // cutButton 
        private Button cutButton = new Button();    // Кнопка резки видео
        private Point cutButtonLocation;    // Верхний левый угол кнопки резки видео
        private Size cutButtonSize;    // Размер кнопки резки видео

        // settingButton 
        private Button settingButton = new Button();    // Кнопка настройки
        private Point settingButtonLocation;    // Верхний левый угол кнопки настройки
        private Size settingButtonSize;    // Размер кнопки настройки

        private Color buttonColor;

        /// <summary>
        /// Создаёт компоненты главной формы с нужными размерами.
        /// </summary>
        private void CreateConponents()
        {
            // Form  
            Size = new Size(formWidth, formHeight);    // Задаем размеры формы

            // pictureBox
            bigPictureBox.BackColor = SystemColors.ActiveCaptionText;    // Цвет 
            bigPictureBox.Location = bigPictureBoxLocation;
            bigPictureBox.Size = bigPictureBoxSize;
            bigPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(bigPictureBox);

            // panel
            panel.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
            panel.BackColor = MainFrame.DefaultBackColor;
            panel.Location = panelLocation;
            panel.Size = panelSize;
            Controls.Add(panel);

            // smallPanels from panel    
            for (int i = 0; i < smallPanelsNumber - 2; i++)
            {
                Panel pn = new Panel();
                pn.Location = smallPanelsLocation[i];
                pn.BackColor = Color.Gray;
                pn.Size = smallPanelsSize[i];
                smallPanels[i] = pn;
                panel.Controls.Add(smallPanels[i]);
            }

            // smallPanels from form
            for (int i = smallPanelsNumber - 2; i < smallPanelsNumber; i++)
            {
                Panel pn = new Panel();
                pn.Location = smallPanelsLocation[i];
                pn.BackColor = Color.Gray;
                pn.Size = smallPanelsSize[i];
                smallPanels[i] = pn;
                Controls.Add(smallPanels[i]);
            }

            // recButton
            recButton.Location = recButtonLocation;
            recButton.Size = recButtonSize;
            recButton.Text = "Rec/Stop";
            recButton.TabIndex = 1;
            Controls.Add(recButton);

            // cutButton
            cutButton.Location = cutButtonLocation;
            cutButton.Size = cutButtonSize;
            cutButton.Text = "Cut";
            cutButton.TabIndex = 3;
            Controls.Add(cutButton);

            // settingButton
            settingButton.Location = settingButtonLocation;
            settingButton.Size = settingButtonSize;
            settingButton.Text = "Setup";
            settingButton.TabIndex = 2;
            Controls.Add(settingButton);
        }

        /// <summary>
        /// Устанавливает размер элементов управления в соответствии с шириной экрана.
        /// </summary>
        /// <param name="DisplayWidth"> Ширина экрана. </param>
        private void SetControlsSize(int DisplayWidth)
        {
            if (DisplayWidth < 1900)
            {
                // form
                formWidth = 1366;    // Ширина формы
                formHeight = 730;    // Высота окна формы
                MinimumSize = new Size(1350, 725);

                // bigPictureBox
                bigPictureBoxLocation = new Point(125, 5);    // Верхний левый угол главного окна вывода изображения
                bigPictureBoxSize = new Size(767, 420);    // Размер главного окна вывода изображения

                // panel
                panelLocation = new Point(12, 457);    // Верхний левый угол первой панели для маленьких окон 
                panelSize = new Size(1326, 231);    // Размер первой панели для маленьких окон

                // smallPanel1
                smallPanelsLocation[0] = new Point(3, 3);
                smallPanelsSize[0] = new Size(324, 225);

                // smallPanel2
                smallPanelsLocation[1] = new Point(333, 3);
                smallPanelsSize[1] = new Size(324, 225);

                // smallPanel3
                smallPanelsLocation[2] = new Point(663, 3);
                smallPanelsSize[2] = new Size(324, 225);

                // smallPanel4
                smallPanelsLocation[3] = new Point(993, 3);
                smallPanelsSize[3] = new Size(324, 225);

                // smallPanel5
                smallPanelsLocation[4] = new Point(1005, 230);
                smallPanelsSize[4] = new Size(324, 225);

                // smallPanel6
                smallPanelsLocation[5] = new Point(1005, 2);
                smallPanelsSize[5] = new Size(324, 225);

                // smallButton1
                smallButtonLocation[0] = new Point(34, 184);
                smallButtonSize[0] = new Size(70, 41);

                // smallButton2
                smallButtonLocation[1] = new Point(106, 184);
                smallButtonSize[1] = new Size(70, 41);

                // smallButton3
                smallButtonLocation[2] = new Point(178, 184);
                smallButtonSize[2] = new Size(70, 41);

                // smallButton4
                smallButtonLocation[3] = new Point(250, 184);
                smallButtonSize[3] = new Size(70, 41);

                // smallPictureBox
                smallPictureBoxLocation = new Point(1, 3);
                smallPictureBoxSize = new Size(320, 180);

                // recButton                  
                recButtonLocation = new Point(12, 5);
                recButtonSize = new Size(107, 124);

                // cutButton 
                cutButtonLocation = new Point(12, 229);
                cutButtonSize = new Size(107, 100);

                // settingButton 
                settingButtonLocation = new Point(15, 153);
                settingButtonSize = new Size(107, 47);
            }
            else
            {
                // form
                formWidth = 1920;    // Ширина формы
                formHeight = 1080;    // Высота окна формы
                MinimumSize = new Size(1900, 1070);

                // bigPictureBox
                bigPictureBoxLocation = new Point(175, 7);    // Верхний левый угол главного окна вывода изображения
                bigPictureBoxSize = new Size(1078, 617);    // Размер главного окна вывода изображения

                // panel
                panelLocation = new Point(17, 672);    // Верхний левый угол первой панели для маленьких окон 
                panelSize = new Size(1856, 339);    // Размер первой панели для маленьких окон

                // smallPanel1
                smallPanelsLocation[0] = new Point(4, 4);
                smallPanelsSize[0] = new Size(453, 320);

                // smallPanel2
                smallPanelsLocation[1] = new Point(466, 4);
                smallPanelsSize[1] = new Size(453, 320);

                // smallPanel3
                smallPanelsLocation[2] = new Point(928, 4);
                smallPanelsSize[2] = new Size(453, 320);

                // smallPanel4
                smallPanelsLocation[3] = new Point(1390, 4);
                smallPanelsSize[3] = new Size(453, 320);


                // smallPanel5
                smallPanelsLocation[4] = new Point(1407, 328);
                smallPanelsSize[4] = new Size(453, 320);

                // smallPanel6
                smallPanelsLocation[5] = new Point(1407, 3);
                smallPanelsSize[5] = new Size(453, 320);

                // smallButton1
                smallButtonLocation[0] = new Point(40, 260);
                smallButtonSize[0] = new Size(98, 60);

                // smallButton2
                smallButtonLocation[1] = new Point(141, 260);
                smallButtonSize[1] = new Size(98, 60);

                // smallButton3
                smallButtonLocation[2] = new Point(243, 260);
                smallButtonSize[2] = new Size(98, 60);

                // smallButton4
                smallButtonLocation[3] = new Point(343, 260);
                smallButtonSize[3] = new Size(98, 60);

                // smallPictureBox
                smallPictureBoxLocation = new Point(10, 2);
                smallPictureBoxSize = new Size(430, 255);

                // recButton                  
                recButtonLocation = new Point(12, 5);
                recButtonSize = new Size(107, 124);

                // cutButton 
                cutButtonLocation = new Point(12, 229);
                cutButtonSize = new Size(107, 100);

                // settingButton 
                settingButtonLocation = new Point(15, 153);
                settingButtonSize = new Size(107, 47);
            }
        }
    }
}

