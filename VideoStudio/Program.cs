using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VideoStudio
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //try
            //{
                Application.Run(new Form1());
            //}
            //catch
            //{
                
            //}
        }
    }
}
