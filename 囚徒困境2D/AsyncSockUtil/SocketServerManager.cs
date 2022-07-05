using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using 囚徒困境2D.common;

/// <summary>
/// 核心,实现Socket监听,收发信息等操作.
/// </summary>
namespace 囚徒困境2D.AsyncSockUtil
{
    public class SocketServerManager : ICloneable
    {
        private int m_maxConnectNum;    //最大连接数
        private int m_revBufferSize;    //最大接收字节数
        private BufferManager m_bufferManager;
        private const int opsToAlloc = 2;
        private Socket listenSocket;            //监听Socket
        private SocketEventPool m_pool;
        private int m_clientCount;              //连接的客户端数量
        private Semaphore m_maxNumberAcceptedClients;
        private int m_totalBytesRead;           // counter of the total # bytes received by the server

        private List<AsyncUserToken> m_clients; //客户端列表

        public PlayerUser serverPlayerUser = new PlayerUser(); //声明本机玩家
        public PlayerUser clientPlayerUser = new PlayerUser(); //声明远程玩家

        //public GameInfo gameInfoWithClient; //声明玩家游戏状态
        private static log4net.ILog log;//声明日志

        public int numberOfKJwithServer = 0;
        public int numberOfKJwithClient = 0;

        #region 定义属性

        /// <summary>
        /// 获取客户端列表
        /// </summary>
        public List<AsyncUserToken> ClientList
        { get { return m_clients; } } // 储存已连接的客户端

        public IDictionary<int, GameUserInfo> gameUserInfoDictionaryWithClient { set; get; }

        #endregion 定义属性

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="numConnections">最大连接数</param>
        /// <param name="receiveBufferSize">缓存区大小</param>
        public SocketServerManager(int numConnections, int receiveBufferSize)
        {
            m_clientCount = 0;
            m_maxConnectNum = numConnections;
            m_revBufferSize = receiveBufferSize;
            m_totalBytesRead = 0;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and
            //write posted to the socket simultaneously
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToAlloc, receiveBufferSize);

            m_pool = new SocketEventPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            //日志初始化
            //日志配置
            log4net.GlobalContext.Properties["LogName"] = serverPlayerUser.userName + "-" + GetTimeStr();
            log = log4net.LogManager.GetLogger(typeof(Program));

            gameUserInfoDictionaryWithClient = new Dictionary<int, GameUserInfo>();

            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds
            // against memory fragmentation
            m_bufferManager.InitBuffer();
            m_clients = new List<AsyncUserToken>();
            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_maxConnectNum; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = new AsyncUserToken();

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                m_bufferManager.SetBuffer(readWriteEventArg);
                // add SocketAsyncEventArg to the pool
                m_pool.Push(readWriteEventArg);
            }

            log.Info("服务端玩家信息：" + this.serverPlayerUser);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="localEndPoint"></param>
        public bool Start(IPEndPoint localEndPoint)
        {
            try
            {
                m_clients.Clear();
                listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(localEndPoint);
                // start the server with a listen backlog of 100 connections
                listenSocket.Listen(m_maxConnectNum);
                // post accepts on the listening socket
                StartAccept(null);
                Console.WriteLine("服务器启动:" + localEndPoint + " the server process....");
                log.Info("服务器启动:" + localEndPoint + " the server process....");
                //Console.WriteLine("Press any key to terminate the server process....");
                return true;
            }
            catch (Exception me)
            {
                log.Info("服务器启动失败！:" + me.Message + "\r\n" + me.StackTrace);
                MessageUtil.ShowWarning("服务器启动失败！:");
                return false;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            foreach (AsyncUserToken token in m_clients)
            {
                try
                {
                    token.Socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }
            }
            try
            {
                listenSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { }

            listenSocket.Close();
            int c_count = m_clients.Count;
            lock (m_clients) { m_clients.Clear(); }
        }

        public void CloseClient(AsyncUserToken token)
        {
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { }
        }

        // Begins an operation to accept a connection request from the client
        //
        // <param name="acceptEventArg">The context object to use when issuing
        // the accept operation on the server's listening socket</param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.WaitOne();
            if (!listenSocket.AcceptAsync(acceptEventArg))
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync
        // operations and is invoked when an accept operation is complete
        //
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        // 处理连接
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                Interlocked.Increment(ref m_clientCount);

                Console.WriteLine("Client connection accepted. There are {0} clients connected to the server", m_clientCount);

                // Get the socket for the accepted client connection and put it into the
                //ReadEventArg object user token
                SocketAsyncEventArgs readEventArgs = m_pool.Pop();
                AsyncUserToken userToken = (AsyncUserToken)readEventArgs.UserToken;

                userToken.Socket = e.AcceptSocket;
                userToken.ConnectTime = DateTime.Now;
                userToken.Remote = e.AcceptSocket.RemoteEndPoint;
                userToken.IPAddress = ((IPEndPoint)(e.AcceptSocket.RemoteEndPoint)).Address;
                //userToken.playerUser = playerUser.Clone() as PlayerUser;  //客户端发送信息之后，初始化user信息

                Console.WriteLine(userToken);

                lock (m_clients) { m_clients.Add(userToken); }

                Console.WriteLine("已连接客户端：");
                log.Info("已连接客户端：");
                SendStr("我是服务端，我的玩家信息是：" + this.serverPlayerUser);

                if (!e.AcceptSocket.ReceiveAsync(readEventArgs))
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch (Exception me)
            {
                //RuncomLib.Log.LogUtils.Info(me.Message + "\r\n" + me.StackTrace);
                Console.WriteLine(me.Message + "\r\n" + me.StackTrace);
                log.Info("客户端链接失败！" + (me.Message + "\r\n" + me.StackTrace));
            }

            // Accept the next connection request
            if (e.SocketError == SocketError.OperationAborted) return;
            StartAccept(e);
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;

                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;

                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        // This method is invoked when an asynchronous receive operation completes.
        // If the remote host closed the connection, then the socket is closed.
        // If data was received then the data is echoed back to the client.
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                // check if the remote host closed the connection
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                    Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);

                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);

                    string str = System.Text.Encoding.UTF8.GetString(data);

                    if (str.Contains("我的玩家信息是"))
                    {
                        clientPlayerUser = new PlayerUser(userSplitMessage(str));
                    }
                    else if (str.Contains("game loop and press and timeDeff and startTime and predicted with server is"))
                    {
                        //("game loop and press and timeDeff and startTime and predicted with server is:" + gameLoop.ToString() + ":" + presssKeyWithClient.ToString()+":" + getTimeDeff(startTime, endTime) + ":" + startTime.ToString() + ":");
                        // 解析客户端端数据
                        int gameLoop = int.Parse(str.Split('!')[1]);
                        string gameKJ = str.Split('!')[2];
                        string gameDiffTime = str.Split('!')[3];
                        string gameStrTime = str.Split('!')[4];
                        string gamePredict = str.Split('!')[5];

                        GameUserInfo userinfo = new GameUserInfo(gameLoop, clientPlayerUser, gameKJ, gamePredict, gameDiffTime, gameStrTime);
                        gameUserInfoDictionaryWithClient.Add(gameLoop, userinfo);

                        //gameInfoWithClient.gamesKJparametersWithClientGF.Add(gameLoop,gameKF);

                        log.Info("**********         接收客户端" + clientPlayerUser.userName + "第" + (gameLoop + 1).ToString() + "轮次游戏，游戏信息是："
                            + str + " ************");
                        numberOfKJwithClient++;
                    }
                    else
                    {
                        log.Info("接收到：" + clientPlayerUser + "\r\n" + "发送来的数据：：  " + str);
                    }

                    //System.Text.Encoding gb2312 = System.Text.Encoding.GetEncoding("gb2312");
                    //string str = gb2312.GetString(data);
                    //Console.WriteLine(str);
                    //

                    //继续接收. 为什么要这么写,请看Socket.ReceiveAsync方法的说明
                    //e.SetBuffer(e.Offset, e.BytesTransferred);
                    if (!token.Socket.ReceiveAsync(e))
                        this.ProcessReceive(e);
                }
                else
                {
                    MessageUtil.ShowWarning("客户端连接中断，请检查");
                    log.Info("客户端" + token.IPAddress + "连接中断，请检查");
                    CloseClientSocket(e);
                }
            }
            catch (Exception xe)
            {
                //RuncomLib.Log.LogUtils.Info(xe.Message + "\r\n" + xe.StackTrace);
                log.Info("程序出现异常：" + xe.Message + "\r\n" + xe.StackTrace);
                Console.WriteLine(xe.Message + "\r\n" + xe.StackTrace);
            }
        }

        // This method is invoked when an asynchronous send operation completes.
        // The method issues another receive on the socket to read any additional
        // data sent from the client
        //
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                // read the next block of data send from the client
                bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        //关闭客户端
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;

            lock (m_clients) { m_clients.Remove(token); }

            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception) { }
            token.Socket.Close();
            // decrement the counter keeping track of the total number of clients connected to the server
            Interlocked.Decrement(ref m_clientCount);
            m_maxNumberAcceptedClients.Release();
            // Free the SocketAsyncEventArg so they can be reused by another client
            e.UserToken = new AsyncUserToken();
            m_pool.Push(e);
        }

        public void SendStr(string str)
        {
            SendMessage(m_clients[0], str);
        }

        /// <summary>
        /// 对数据进行打包,然后再发送
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private void SendMessage(AsyncUserToken token, string message)
        {
            if (token == null || token.Socket == null || !token.Socket.Connected)
                return;
            try
            {
                //对要发送的消息,制定简单协议,头4字节指定包的大小,方便客户端接收(协议可以自己定)
                byte[] buff = Encoding.UTF8.GetBytes(message);
                //token.Socket.Send(buff);  //这句也可以发送, 可根据自己的需要来选择
                //新建异步发送对象, 发送消息
                SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
                sendArg.UserToken = token;
                sendArg.SetBuffer(buff, 0, buff.Length);  //将数据放置进去.
                token.Socket.SendAsync(sendArg);
                log.Info("向客户端" + this.clientPlayerUser + "发送数据：" + message);
            }
            catch (Exception e)
            {
                //RuncomLib.Log.LogUtils.Info("SendMessage - Error:" + e.Message);
                Console.WriteLine("SendMessage - Error:" + e.Message);
            }
        }

        public string GetTimeStr()
        {
            /* 获取当前时间字符串函数 */
            string tempTimeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0')
                        + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0'); /* 以年-月-日时分秒的格式命名文件 */
            return tempTimeStr;
        }

        /// <summary>
        /// 用户打招呼信息解析
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private PlayerUser userSplitMessage(string str)
        {
            //userNmae:111 ,userCategory:1 ,userDate:2022-04-28-215910 ,userLogPath:111-2022-04-28-215910 ,user IP:172.18.51.162
            string[] strs = str.Split('：')[1].Split(',');
            PlayerUser user = new PlayerUser();
            user.userName = strs[0].Split(':')[1];
            user.userCategory = int.Parse(strs[1].Split(':')[1]);
            user.userDate = strs[2].Split(':')[1];
            user.userIp = strs[3].Split(':')[1];

            return user;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}