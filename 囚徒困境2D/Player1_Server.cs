using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using 囚徒困境2D.AsyncSockUtil;
using 囚徒困境2D.common;

namespace 囚徒困境2D
{
    public partial class Player1_Server : NForm
    {
        private static log4net.ILog log;
        public PlayerUser serverPlayerUser;//本机信息
        private SocketServerManager m_socket = new SocketServerManager(200, 1024); //socket
        private int space_num = 0;

        public Player1_Server()
        {
            InitializeComponent();
            // 控件自适应
            GetAllInitInfo(this.Controls[0]);
            ///解决线程间操作无效: 从不是创建控件的线程访问它
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        public Player1_Server(PlayerUser p)
        {
            // 先最小化再最大化窗口，避免字体大小发生变化
            //this.WindowState = FormWindowState.Minimized;
            //this.Activate();
            //this.WindowState = FormWindowState.Maximized;
            //this.Activate();

            this.serverPlayerUser = new PlayerUser(p);
            //Console.WriteLine(playerUser);
            InitializeComponent();
            // 控件自适应
            GetAllInitInfo(this.Controls[0]);
            ///解决线程间操作无效: 从不是创建控件的线程访问它
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;//全屏显示

            //日志配置
            log4net.GlobalContext.Properties["LogName"] = serverPlayerUser.userName + "-" + GetTimeStr();
            log = log4net.LogManager.GetLogger(typeof(Program));
            //end

            //日志输出
            log.Info("=============================================================================================================================");
            log.Info("游戏重新启动");
            string logStr = "服务端玩家登陆！";
            log.Info(logStr);
            log.Info("服务端玩家信息：" + this.serverPlayerUser);

            //根据用户类别，初始化Socket类型
            if (serverPlayerUser.userCategory == 1)
            {
                m_socket.serverPlayerUser = serverPlayerUser.Clone() as PlayerUser;
                Console.WriteLine(m_socket.serverPlayerUser);
                m_socket.Init();
                m_socket.Start(new IPEndPoint(IPAddress.Any, 6000));
                //m_socket.Start((new IPEndPoint(IPAddress.Parse("127.0.0.1"), DEFAULT_PORT)));
            }
            // end

            //ui rich text
            this.uiRichTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            this.uiRichTextBox1.AppendText("该游戏为两人博弈，双方不能用语言交流。\r\n");
            this.uiRichTextBox1.AppendText("游戏中，你们会看到一个博弈情景，每人都可以自由选择合作或者竞争。\r\n");
            this.uiRichTextBox1.AppendText("请按键作答：F合作（左手食指），J竞争（右手食指）。\r\n");
            this.uiRichTextBox1.AppendText("当双方选择合作时，每人能获得5个代币。\r\n");
            this.uiRichTextBox1.AppendText("当双方选择竞争时，每人能获得3个代币。\r\n");
            this.uiRichTextBox1.AppendText("当一方选择竞争，一方选择合作时，竞争方获得10个代币，合作方获得0个代币。\r\n");
            this.uiRichTextBox1.AppendText("报酬为获得的代币数计算而来，代币越多、获得的报酬越多。\r\n");
            this.uiRichTextBox1.AppendText("\r\n请点击空格键继续……");

            this.uiRichTextBox1.Select(53, 27);
        }

        private void Question()
        {
            this.uiRichTextBox1.Clear();
            this.uiRichTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            this.uiRichTextBox1.AppendText("阅读完毕后，玩家回答3个问题以确保理解游戏规则，分别为:\r\n");
            this.uiRichTextBox1.AppendText("1、当您选择合作，对方也选择合作时，您获得多少个代币?\r\n");
            this.uiRichTextBox1.AppendText("2、当您选择竞争，对方也选择竞争时，对方获得多少个代币?\r\n");
            this.uiRichTextBox1.AppendText("3、当您选择合作，对方选择竞争时，您获得多少个代币?\r\n");
            this.uiRichTextBox1.AppendText("请在实验规则测试通过之后，按空格键，进入正式实验。\r\n");
            this.uiRichTextBox1.AppendText("\r\n请点击空格键继续……");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space && space_num == 0)
            {
                //MessageUtil.ShowWarning("用户按下：" + keyData);
                log.Info("用户" + serverPlayerUser.userName + ",阅读完游戏规则,并按下：" + keyData);
                this.Question();
                space_num++;
            }
            else if (keyData == Keys.Space && space_num == 1)
            {
                //MessageUtil.ShowWarning("用户第"+space_num+"次按下：" + keyData);
                log.Info("用户" + serverPlayerUser.userName + ",回答完游戏规则,并按下：" + keyData);
                //MessageBox.Show("用户第" + space_num + "次按下：" + keyData,"通知");

                if (m_socket.ClientList.Count > 0)
                {
                    //跳转页面
                    Game1_server game1_server = new Game1_server(m_socket, 1);
                    Thread PlayertFromThread = new Thread(delegate () { game1_server.ShowDialog(); });
                    PlayertFromThread.Start();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("客户端还未连接", "警告！");
                    log.Info("警告！：客户端还未连接");
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);////其余键位默认处理
                                                        //return false;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        public string GetTimeStr()
        {
            /* 获取当前时间字符串函数 */
            string tempTimeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0')
                        + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0'); /* 以年-月-日时分秒的格式命名文件 */
            return tempTimeStr;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_socket.SendStr("77 99  我是服务端");
        }

        private void uiRichTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        //private void Btn_Exit_Click(object sender, EventArgs e)
        //{
        //    if (MessageBox.Show("确定退出？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
        //        Application.Exit();
        //}
    }
}