using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace VideoStudio
{
    

    class smallwindow
    {

        public System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox(); // окно вывода картинки
        public System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();    // кнопка on air
        private System.Windows.Forms.Button button2 = new System.Windows.Forms.Button();
        private System.Windows.Forms.Button button3 = new System.Windows.Forms.Button();
        private System.Windows.Forms.Button button4 = new System.Windows.Forms.Button();
        private System.Windows.Forms.Label label = new System.Windows.Forms.Label();
        private  Panel outpanel;
        private System.Drawing.Point[] smallbuttonLocation; //  массив расположения кнопок  маленьких панелей 
        private System.Drawing.Size[] smallbuttonSize;      // массив размера кнопок  маленьких панелей 
        private System.Drawing.Point smallpictureBoxLocation; // расположение pictureBox
        private System.Drawing.Size smallpictureBoxSize;// размер pictureBox
        private int id;      
        public smallwindow()
        {

        }

        
        public smallwindow(Panel outpanel, System.Drawing.Point[] smallbuttonLocation, System.Drawing.Size[] smallbuttonSize, System.Drawing.Point smallpictureBoxLocation, System.Drawing.Size smallpictureBoxSize, int id)
        {
            this.outpanel = outpanel;
            this.smallbuttonLocation = smallbuttonLocation;
            this.smallbuttonSize = smallbuttonSize;
            this.smallpictureBoxLocation = smallpictureBoxLocation;
            this.smallpictureBoxSize = smallpictureBoxSize;
            this.id = id;
           
            drawing();

            // подписываемся на события кликов мышью          
            button2.Click += new System.EventHandler(button2_Click);
            button3.Click += new System.EventHandler(button3_Click);
            button4.Click += new System.EventHandler(button4_Click);
           

        }

        private void drawing()  // добавление и отрисовка элементов маленькой формы
        {   //smallbutton 1
            button1.Location = smallbuttonLocation[0];
            button1.Size = smallbuttonSize[0];            
            button1.UseVisualStyleBackColor = true;
            button1.Text = "on air";
            outpanel.Controls.Add(button1);

            //smallbutton 2
            button2.Location = smallbuttonLocation[1];
            button2.Size = smallbuttonSize[1];
            button2.UseVisualStyleBackColor = true;
            button2.Text = "звук";
            outpanel.Controls.Add(button2);

            //smallbutton 3
            button3.Location = smallbuttonLocation[2];
            button3.Size = smallbuttonSize[2];
            button3.Text = "PiP";
            button3.UseVisualStyleBackColor = true;
            outpanel.Controls.Add(button3);

            //smallbutton 4
            button4.Location = smallbuttonLocation[3];
            button4.Size = smallbuttonSize[3];
            button4.Text = "Настройка";
            button4.UseVisualStyleBackColor = true;
            outpanel.Controls.Add(button4);

            //smallpictureBox
            pictureBox.BackColor = System.Drawing.Color.Black;
            pictureBox.Location = smallpictureBoxLocation;
            pictureBox.Size = smallpictureBoxSize;
            pictureBox.BackgroundImageLayout = ImageLayout.Zoom;
            outpanel.Controls.Add(pictureBox);

            // label
            label.Location = new System.Drawing.Point(10, 195);
            label.Text=Convert.ToString(id+1);
            outpanel.Controls.Add(label);

        }
            
        private void button2_Click(object sender, EventArgs e)
        {
            //MessageBox.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("4");
        }
                     
    }
}
