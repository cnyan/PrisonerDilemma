using System;
using System.Windows.Forms;

namespace 囚徒困境2D
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainPage());

            //Application.Run(new Game1_client());
        }
    }
}