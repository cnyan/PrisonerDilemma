using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using 囚徒困境2D.common;

namespace 囚徒困境2D
{
    public partial class MainPage : NForm
    {
        public MainPage()
        {
            InitializeComponent();
            GetAllInitInfo(this.Controls[0]);

            // 显示IP地址
            this.tb_ip_address_1.Text = GetLocalIpAddress();
            this.tb_ip_address_2.Text = GetLocalIpAddress();
        }

        private void userButton1_Click_1(object sender, EventArgs e)
        {
            //打开另一个窗口的同时关闭当前窗口
            PlayerUser playerUser = new PlayerUser();
            playerUser.userName = this.tb_user_name_1.Text;
            playerUser.userCategory = 1;    //1表示服务端
            playerUser.userDate = GetTimeStr();
            playerUser.userIp = GetLocalIpAddress();
            //Console.WriteLine(playerUser);

            Player1_Server player1 = new Player1_Server(playerUser);

            //跳转页面
            Thread PlayertFromThread = new Thread(delegate () { player1.ShowDialog(); });
            PlayertFromThread.Start();
            this.Close();
        }

        public string GetTimeStr()
        {
            /* 获取当前时间字符串函数 */
            string tempTimeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0')
                        + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Hour.ToString().PadLeft(2, '0')
                        + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0'); /* 以年-月-日时分秒的格式命名文件 */
            return tempTimeStr;
        }

        //获取IP地址
        public string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    //Console.WriteLine("IP Address = " + ip.ToString());
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //打开另一个窗口的同时关闭当前窗口
            PlayerUser playerUser = new PlayerUser();
            playerUser.userName = this.tb_user_name_2.Text;
            playerUser.userCategory = 2;//2表示客户端
            playerUser.userDate = GetTimeStr();
            playerUser.userIp = this.tb_ip_address_2.Text;
            //Console.WriteLine(playerUser);

            Player1_Client player2 = new Player1_Client(playerUser);

            //跳转页面
            Thread PlayertFromThread = new Thread(delegate () { player2.ShowDialog(); });
            PlayertFromThread.Start();
            this.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}