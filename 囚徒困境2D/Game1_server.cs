using System;
using System.Threading;
using System.Windows.Forms;
using 囚徒困境2D.AsyncSockUtil;
using 囚徒困境2D.common;

namespace 囚徒困境2D
{
    public partial class Game1_server : NForm
    {
        private static log4net.ILog log;
        public PlayerUser serverPlayerUser;//本机信息
        public PlayerUser clientPlayerUser;//本机信息
        public SocketServerManager m_socket;
        private System.Timers.Timer t; // 定时器

        public Game1_server()
        {
            InitializeComponent();
        }

        // 服务端初始化
        /// <summary>
        ///  根据玩家类型，初始化不同的程序
        /// </summary>
        /// <param name="server_socket"></param>
        /// <param name="userCategory">1： 服务端  2：客户端</param>
        public Game1_server(SocketServerManager server_socket, int userCategory)
        {
            InitializeComponent();
            m_socket = server_socket.Clone() as SocketServerManager;
            serverPlayerUser = m_socket.serverPlayerUser.Clone() as PlayerUser;
            clientPlayerUser = m_socket.clientPlayerUser.Clone() as PlayerUser;

            //m_socket.SendStr("server strat gameing");
            // 控件自适应
            GetAllInitInfo(this.Controls[0]);
            //日志配置
            log4net.GlobalContext.Properties["LogName"] = serverPlayerUser.userName + "-" + m_socket.GetTimeStr();
            log = log4net.LogManager.GetLogger(typeof(Program));
            log.Info("服务端已进入游戏模式");

            //设置定时器
            Random ran = new Random(GenerateRandomSeed());
            int nrandTime = ran.Next(1000, 1500);
            t = new System.Timers.Timer(nrandTime);//实例化Timer类，设置间隔时间为10000毫秒；
            TimerToExecute();
        }

        // 设定计时器
        public void TimerToExecute()
        {
            t.Elapsed += new System.Timers.ElapsedEventHandler(Execute);//到达时间的时候执行事件；
            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            t.Start(); //启动定时器
        }

        public void Execute(object source, System.Timers.ElapsedEventArgs e)
        {
            t.Stop(); //先关闭定时器
            //跳转页面
            Game2_server game2 = new Game2_server(m_socket, 1);
            Thread PlayertFromThread = new Thread(delegate () { game2.ShowDialog(); });
            PlayertFromThread.Start();
            this.Close();
            //Application.ExitThread();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        public static int GenerateRandomSeed()
        {
            //产生随机数种子
            return (int)DateTime.Now.Ticks;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}