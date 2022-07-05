using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using 囚徒困境2D.AsyncSockUtil;
using 囚徒困境2D.common;

namespace 囚徒困境2D
{
    public partial class Player1_Client : NForm
    {
        private static log4net.ILog log;
        public PlayerUser clientPlayerUser;//声明本机玩家
        // public PlayerUser serverPlayerUser;//声明远程玩家

        // socket配置
        public SocketClientManager m_socketClientManager;

        // socket配置  end
        private int space_num = 0;

        public Player1_Client()
        {
            // 先最小化再最大化窗口，避免字体大小发生变化
            //this.WindowState = FormWindowState.Minimized;
            //this.Activate();
            //this.WindowState = FormWindowState.Maximized;
            //this.Activate();

            InitializeComponent();
            ///解决线程间操作无效: 从不是创建控件的线程访问它
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        public Player1_Client(PlayerUser p)
        {
            this.clientPlayerUser = p.Clone() as PlayerUser;
            //Console.WriteLine(playerUser);
            InitializeComponent();
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;//全屏显示

            // 控件自适应
            GetAllInitInfo(this.Controls[0]);
            ///解决线程间操作无效: 从不是创建控件的线程访问它
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

            //日志配置
            log4net.GlobalContext.Properties["LogName"] = clientPlayerUser.userName + "-" + GetTimeStr();
            log = log4net.LogManager.GetLogger(typeof(Program));
            //end

            //日志输出
            log.Info("=============================================================================================================================");
            log.Info("游戏重新启动");
            log.Info("客户端玩家登陆！");
            log.Info("客户端端玩家信息：" + this.clientPlayerUser);

            //根据用户类别，初始化Socket类型
            if (clientPlayerUser.userCategory == 2)
            {
                try
                {
                    m_socketClientManager = new SocketClientManager(new IPEndPoint(IPAddress.Parse(p.userIp), 6000));
                    m_socketClientManager.Init(clientPlayerUser);
                    m_socketClientManager.Connect();

                    //m_socketClientManager.clientPlayerUser = this.clientPlayerUser.Clone() as PlayerUser;
                    //m_socketClientManager.serverPlayerUser = this.serverPlayerUser.Clone() as PlayerUser;
                }
                catch (Exception me)
                {
                    DialogResult dr = MessageBox.Show("连接服务端失败！", "提醒", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dr == DialogResult.OK)
                    {
                        //点确定的代码
                        System.Environment.Exit(0);
                    }
                    else
                    {        //点取消的代码
                        System.Environment.Exit(0);
                    }
                    //MessageBox.Show("连接服务端失败！", "警告！");
                    log.Info("连接服务端失败！" + me.Message);
                }
            }
            // 先最小化再最大化窗口，避免字体大小发生变化
            //this.WindowState = FormWindowState.Minimized;
            //this.Activate();
            //this.WindowState = FormWindowState.Maximized;
            //this.Activate();

            //ui rich text

            this.uiRichTextBox2.Clear();
            this.uiRichTextBox2.SelectionAlignment = HorizontalAlignment.Center;
            this.uiRichTextBox2.SelectionFont = new Font("宋体", 16);

            this.uiRichTextBox2.AppendText("该游戏为两人博弈，双方不能用语言交流。\r\n");
            this.uiRichTextBox2.AppendText("游戏中，你们会看到一个博弈情景，每人都可以自由选择合作或者竞争。\r\n");
            this.uiRichTextBox2.AppendText("请按键作答：F合作（左手食指），J竞争（右手食指）。\r\n");
            this.uiRichTextBox2.AppendText("当双方选择合作时，每人能获得5个代币。\r\n");
            this.uiRichTextBox2.AppendText("当双方选择竞争时，每人能获得3个代币。\r\n");
            this.uiRichTextBox2.AppendText("当一方选择竞争，一方选择合作时，竞争方获得10个代币，合作方获得0个代币。\r\n");
            this.uiRichTextBox2.AppendText("报酬为获得的代币数计算而来，代币越多、获得的报酬越多。\r\n");
            this.uiRichTextBox2.AppendText("\r\n请点击空格键继续……");

            this.uiRichTextBox2.Select(53, 27);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space && space_num == 0)
            {
                //MessageUtil.ShowWarning("用户按下：" + keyData);
                log.Info("用户" + clientPlayerUser.userName + ",阅读完游戏规则,并按下：" + keyData);
                this.Question();
                space_num++;
            }
            else if (keyData == Keys.Space && space_num == 1)
            {
                log.Info("用户" + clientPlayerUser.userName + ",回答完游戏规则,并按下：" + keyData);
                //MessageBox.Show("用户第" + space_num + "次按下：" + keyData,"通知");
                space_num++;
                //跳转页面
                //Game1_server game1 = new Game1_server(m_socketClientManager, 1);
                //Thread PlayertFromThread = new Thread(delegate () { game1.ShowDialog(); });
                //PlayertFromThread.Start();
                //this.Close();

                if (m_socketClientManager.isConnected())
                {
                    log.Info("服务端信息是：" + m_socketClientManager.serverPlayerUser);
                    //跳转页面
                    Game1_client game1_client = new Game1_client(m_socketClientManager, 2);
                    Thread PlayertFromThread = new Thread(delegate () { game1_client.ShowDialog(); });
                    PlayertFromThread.Start();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("服务端还未连接", "警告！");
                    log.Info("警告！：服务端还未连接");
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);////其余键位默认处理
                                                        //return false;
        }

        private void Question()
        {
            this.uiRichTextBox2.Clear();
            this.uiRichTextBox2.SelectionAlignment = HorizontalAlignment.Center;
            this.uiRichTextBox2.AppendText("阅读完毕后，玩家回答3个问题以确保理解游戏规则，分别为:\r\n");
            this.uiRichTextBox2.AppendText("1、当您选择合作，对方也选择合作时，您获得多少个代币?\r\n");
            this.uiRichTextBox2.AppendText("2、当您选择竞争，对方也选择竞争时，对方获得多少个代币?\r\n");
            this.uiRichTextBox2.AppendText("3、当您选择合作，对方选择竞争时，您获得多少个代币?\r\n");
            this.uiRichTextBox2.AppendText("请在实验规则测试通过之后，按空格键，进入正式实验。\r\n");
            this.uiRichTextBox2.AppendText("\r\n请点击空格键继续……");
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        protected override void WndProc(ref Message msg)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            if (msg.Msg == WM_SYSCOMMAND && ((int)msg.WParam == SC_CLOSE))
            {
                // 点击winform右上关闭按钮
                // 加入想要的逻辑处理

                System.Environment.Exit(0);

                //return;//阻止了窗体关闭
            }
            base.WndProc(ref msg);
        }

        #region

        //void ReceiveMessage()//接收消息
        //{
        //    int length = 0;
        //    while (true)
        //    {
        //        if (clientScoket.Connected == true)
        //        {
        //            length = clientScoket.Receive(data);
        //            if (length != 0)
        //            {
        //                string message = Encoding.UTF8.GetString(data, 0, length);

        //                Console.WriteLine(message);
        //                log.Info("接收到：" + serverPlayerUser + "\r\n" + "发送来的数据：：  " + message);

        //                if (message.Contains("我的玩家信息是"))
        //                {
        //                    serverPlayerUser = new PlayerUser(userSplitMessage(message));
        //                    SendStr("我是客户端，我的玩家信息是：" + this.clientPlayerUser);
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        //void SendStr(string message)//发送消息
        //{
        //    byte[] data = Encoding.UTF8.GetBytes(message);
        //    clientScoket.Send(data);
        //    log.Info("向服务端端" + this.serverPlayerUser + "\r\n" + "发送数据：" + message);
        //}

        public string GetTimeStr()
        {
            /* 获取当前时间字符串函数 */
            string tempTimeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0')
                        + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0'); /* 以年-月-日时分秒的格式命名文件 */
            return tempTimeStr;
        }

        private void uiRichTextBox2_TextChanged(object sender, EventArgs e)
        {
        }
    }
}