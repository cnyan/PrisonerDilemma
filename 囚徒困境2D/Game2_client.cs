using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using 囚徒困境2D.AsyncSockUtil;
using 囚徒困境2D.common;

namespace 囚徒困境2D
{
    public partial class Game2_client : NForm
    {
        private static log4net.ILog log;
        public PlayerUser serverPlayerUser;//本机信息
        public PlayerUser clientPlayerUser;//本机信息
        public SocketClientManager m_socket;

        private int gameLoop = 0; //游戏轮次 客户端按下按钮的次数
        private int pressKeyLoopWithServer = 0; // 服务端用户按下键盘的次数
        private int doublePressKeyWithUser = 1; // 双方按下按钮的次数，用来校验双方游戏进度，同步黑屏过程
        private bool isAllowPressKeyKJ = true; //是否允许用户按下按钮K J
        private bool isAllowPressKeyNum = false; //是否允许用户按下 数字键.
        private string sendGameProcessStrToServer = null;

        private const int theMaxGameLoop = 21; //最大游戏次数

        //开辟线程，监控服务端发送来的数据
        private Thread serverUserPressFJThread = null;

        private Thread doublePressKeyWithUserThread = null;

        private IDictionary<int, GameUserInfo> gameUserInfoDictionaryWithClient = new Dictionary<int, GameUserInfo>();
        private IDictionary<int, GameUserInfo> gameUserInfoDictionaryWithServer = new Dictionary<int, GameUserInfo>();

        private System.Timers.Timer t; // 定时器
        private DateTime startTime = DateTime.Now;
        private DateTime endTime = DateTime.Now;

        public Game2_client()
        {
            InitializeComponent();
            // 控件自适应
            GetAllInitInfo(this.Controls[0]);
            InitGamePicture();
        }

        public Game2_client(SocketClientManager socketClientManager, int userCategory)
        {
            InitializeComponent();
            // 控件自适应
            GetAllInitInfo(this.Controls[0]);

            m_socket = socketClientManager.Clone() as SocketClientManager;
            serverPlayerUser = m_socket.serverPlayerUser.Clone() as PlayerUser;
            clientPlayerUser = m_socket.clientPlayerUser.Clone() as PlayerUser;

            //m_socket.SendStr("server strat gameing");

            //日志配置
            log4net.GlobalContext.Properties["LogName"] = clientPlayerUser.userName + "-" + m_socket.GetTimeStr();
            log = log4net.LogManager.GetLogger(typeof(Program));
            log.Info("客户端已进入囚徒困境页面");

            this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PG0;
            //this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PG十;
            //Thread.Sleep(1000);
            startTime = DateTime.Now;

            //开辟线程，监控服务端发送来的数据
            serverUserPressFJThread = new Thread(MonitorServerUserPressFJ);
            serverUserPressFJThread.IsBackground = true;
            serverUserPressFJThread.Start();

            doublePressKeyWithUserThread = new Thread(MonitorDublePressKeyWithUser);
            doublePressKeyWithUserThread.IsBackground = true;
            doublePressKeyWithUserThread.Start();

            //InitGamePicture();
        }

        private void Game2_client_Load(object sender, EventArgs e)
        {
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void GameRunningWithPressKey(string presssKeyWithClientKJ)
        {
            log.Info("++++++++++          玩家开始第轮  " + (gameLoop + 1).ToString() + "  次游戏,当前时间：" + startTime.ToString() + "        +++++++++++++");
            //Console.WriteLine("******玩家第" + gameLoop + "次按下：" + presssKeyWithClient.ToString() + ",消耗时间是：" + getTimeDeff(startTime, endTime) + "*****");
            //客户端游戏属性赋值
            GameUserInfo gameUserInfoWithClient = new GameUserInfo(gameLoop, clientPlayerUser, presssKeyWithClientKJ, "0", getTimeDeff(startTime, endTime), startTime.ToString().ToString());
            gameUserInfoDictionaryWithClient.Add(gameLoop, gameUserInfoWithClient);

            sendGameProcessStrToServer = ("game loop and press and timeDeff and startTime and predicted with server is!" + gameLoop.ToString() + "!" + presssKeyWithClientKJ.ToString()
              + "!" + getTimeDeff(startTime, endTime) + "!" + startTime.ToString() + "!");

            startTime = DateTime.Now;
        }

        /// <summary>
        /// 捕获玩家 预测对方的按键
        /// </summary>
        /// <param name="presssKeyWithClientNum"></param>
        private void GameRunningWithPressKeyNUm(string presssKeyWithClientNum)
        {
            if (isAllowPressKeyNum)
            {
                GameUserInfo gameUserInfoWithClient = gameUserInfoDictionaryWithClient[gameLoop];
                gameUserInfoWithClient.gamePredictRival = presssKeyWithClientNum;
                gameUserInfoDictionaryWithClient[gameLoop] = gameUserInfoWithClient;

                sendGameProcessStrToServer += presssKeyWithClientNum;
                m_socket.Send(sendGameProcessStrToServer);

                isAllowPressKeyNum = false;

                log.Info("---------      玩家结束第轮  " + (gameLoop + 1).ToString() + "  次游戏已经结束，用户按下：" + presssKeyWithClientNum + " 用户预测了对方按键：" + presssKeyWithClientNum
                + " ,这一轮次消耗用时：" + getTimeDeff(startTime, endTime) + "秒。      --------------");

                gameLoop++;
            }
        }

        // 用户按键响应
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!isAllowPressKeyKJ)
            {
                MessageBox.Show("请等待结果展示……", "客户端消息");
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (gameLoop < theMaxGameLoop - 1 && gameLoop <= pressKeyLoopWithServer)
            {
                if (keyData.ToString().ToUpper() == "F" || keyData.ToString().ToUpper() == "J")
                {
                    if (!isAllowPressKeyNum)
                    {
                        isAllowPressKeyNum = true;
                        if (keyData.ToString().ToUpper() == "F")
                        {
                            this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PG1;
                        }
                        else
                        {
                            this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PG2;
                        }
                        endTime = DateTime.Now;

                        GameRunningWithPressKey(keyData.ToString());//响应用户按键函数
                    }
                    // else { } //这一局游戏只需要点击一次 fj
                }
                else if (keyData.ToString() == "NumPad1" || keyData.ToString() == "NumPad2"
                     || keyData.ToString() == "D1" || keyData.ToString() == "D2")
                {
                    GameRunningWithPressKeyNUm(keyData.ToString());
                }
            }
            else if (gameLoop < theMaxGameLoop - 1 && gameLoop > pressKeyLoopWithServer)
            {
                MessageBox.Show("当前第" + gameLoop.ToString() + "次游戏，请等待对方按下按钮", "服务端消息");
            }
            else if (gameLoop >= theMaxGameLoop - 1 && gameLoop <= pressKeyLoopWithServer)
            {
                MessageBox.Show("当前第" + gameLoop.ToString() + "次游戏已经结束！", "ending");
            }
            else
            {
                MessageBox.Show("当前第" + gameLoop.ToString() + "次游戏不可操作，请等待！", "服务端消息");
            }

            return base.ProcessCmdKey(ref msg, keyData);////其余键位默认处理
        }

        // 计算时间差
        private string getTimeDeff(DateTime firstTime, DateTime secondTime)
        {
            TimeSpan ts = secondTime - firstTime;
            string timeDeff = ts.TotalSeconds.ToString();	//将时间差转换为秒
            return timeDeff;
        }

        private void MonitorServerUserPressFJ()
        {
            int gameLoopWithServer = 0;
            while (true)
            {
                Thread.Sleep(10);
                int serverPressKeyNumber = m_socket.gameUserInfoDictionaryWithServer.Count();

                if (serverPressKeyNumber != gameLoopWithServer) //服务端玩家按下按钮
                {
                    pressKeyLoopWithServer = serverPressKeyNumber;  //服务端端按下按钮的次数赋值

                    GameUserInfo userInfoWithServer = m_socket.gameUserInfoDictionaryWithServer[serverPressKeyNumber - 1];
                    gameUserInfoDictionaryWithServer.Add(serverPressKeyNumber - 1, userInfoWithServer);

                    gameLoopWithServer = serverPressKeyNumber;
                    //MessageBox.Show(userInfoWithServer.ToString(),"客户端消息");
                }
            }
        }

        private void MonitorDublePressKeyWithUser()
        {
            int numOfServerPressKey = 0;
            int numOfClientPressKey = 0;
            int randTime = 0; //随机时间.
            int nowIndexWithDictionary = 0;
            Random ran;

            GameUserInfo guiWithServer;
            GameUserInfo guiWithClient;

            while (true)
            {
                if (doublePressKeyWithUser > theMaxGameLoop)
                {
                    break;
                }

                Thread.Sleep(10);

                numOfServerPressKey = this.gameUserInfoDictionaryWithServer.Count();
                numOfClientPressKey = this.gameUserInfoDictionaryWithClient.Count();

                if (((numOfServerPressKey == numOfClientPressKey) && (numOfClientPressKey == doublePressKeyWithUser)) && (!isAllowPressKeyNum))
                {
                    Thread.Sleep(2000);

                    this.pictureBox1.Image = null;
                    isAllowPressKeyKJ = false;
                    nowIndexWithDictionary = doublePressKeyWithUser - 1;

                    ran = new Random(GenerateRandomSeed());
                    randTime = ran.Next(1000, 1500);
                    Thread.Sleep(randTime);

                    guiWithClient = this.gameUserInfoDictionaryWithClient[nowIndexWithDictionary].Clone() as GameUserInfo;
                    guiWithServer = this.gameUserInfoDictionaryWithServer[nowIndexWithDictionary].Clone() as GameUserInfo;

                    if ((guiWithServer.gamesPressKJ == guiWithClient.gamesPressKJ) && (guiWithClient.gamesPressKJ == "F")) //都选择合作
                    {
                        this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PR1;
                        guiWithServer.gamesGrade = 5;
                        guiWithClient.gamesGrade = 5;
                        this.gameUserInfoDictionaryWithClient[nowIndexWithDictionary] = guiWithClient;
                        this.gameUserInfoDictionaryWithServer[nowIndexWithDictionary] = guiWithServer;
                    }
                    else if ((guiWithServer.gamesPressKJ == guiWithClient.gamesPressKJ) && (guiWithClient.gamesPressKJ == "J")) //都选择合作
                    {
                        this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PR4;
                        guiWithServer.gamesGrade = 3;
                        guiWithClient.gamesGrade = 3;
                        this.gameUserInfoDictionaryWithClient[nowIndexWithDictionary] = guiWithClient;
                        this.gameUserInfoDictionaryWithServer[nowIndexWithDictionary] = guiWithServer;
                    }
                    else if ((guiWithServer.gamesPressKJ == "J") && (guiWithClient.gamesPressKJ == "F"))
                    {
                        this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PR2;
                        guiWithServer.gamesGrade = 10;
                        guiWithClient.gamesGrade = 0;
                        this.gameUserInfoDictionaryWithClient[nowIndexWithDictionary] = guiWithClient;
                        this.gameUserInfoDictionaryWithServer[nowIndexWithDictionary] = guiWithServer;
                    }
                    else if ((guiWithServer.gamesPressKJ == "F") && (guiWithClient.gamesPressKJ == "J"))
                    {
                        this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PR3;
                        guiWithServer.gamesGrade = 0;
                        guiWithClient.gamesGrade = 10;
                        this.gameUserInfoDictionaryWithClient[nowIndexWithDictionary] = guiWithClient;
                        this.gameUserInfoDictionaryWithServer[nowIndexWithDictionary] = guiWithServer;
                    }

                    doublePressKeyWithUser++;
                    isAllowPressKeyKJ = true;

                    ran = new Random(GenerateRandomSeed());
                    randTime = ran.Next(1000, 1500);
                    Thread.Sleep(randTime);

                    this.label1.Text = "得分：" + getTotalGradeWithUserDicti(this.gameUserInfoDictionaryWithClient);

                    if (doublePressKeyWithUser < theMaxGameLoop)
                    {
                        DialogResult dr = MessageBox.Show("请开始第" + doublePressKeyWithUser + "局游戏", "提醒", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.OK)
                        {
                            //点确定的代码
                            InitGamePicture();
                        }
                    }
                    else
                    {  //游戏结束
                        MessageBox.Show("游戏结束，收集到服务端发送来的" + gameUserInfoDictionaryWithServer.Count().ToString() + "次按键");

                        log.Info("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% 当前客户端游戏已经结束 %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
                        log.Info("当前客户端玩家游戏结果显示如下:");
                        foreach (KeyValuePair<int, GameUserInfo> kvp in gameUserInfoDictionaryWithClient)
                        {
                            //Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                            log.Info(kvp.Value);
                        }
                        log.Info("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% 当服务端游戏已经结束 %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
                        log.Info("当前服务端玩家游戏结果显示如下:");
                        foreach (KeyValuePair<int, GameUserInfo> kvp in gameUserInfoDictionaryWithServer)
                        {
                            //Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                            log.Info(kvp.Value);
                        }

                        serverUserPressFJThread.Abort();
                        doublePressKeyWithUserThread.Abort();
                        break;
                    }
                }
            }
        }

        public static int GenerateRandomSeed()
        {
            //产生随机数种子
            return (int)DateTime.Now.Ticks;
        }

        public void InitGamePicture()
        {
            isAllowPressKeyKJ = false;

            this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PG十;
            Random ran;
            int randTime = 0; //随机时间
            ran = new Random(GenerateRandomSeed());
            randTime = ran.Next(1000, 1500);
            Thread.Sleep(randTime);

            this.pictureBox1.Image = global::囚徒困境2D.Properties.Resources.PG0;

            isAllowPressKeyKJ = true;
        }

        public string getTotalGradeWithUserDicti(IDictionary<int, GameUserInfo> userDict)
        {
            int total_grade = 0;
            foreach (KeyValuePair<int, GameUserInfo> kvp in userDict)
            {
                //Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                total_grade += kvp.Value.gamesGrade;
            }
            return total_grade.ToString();
        }
    }
}