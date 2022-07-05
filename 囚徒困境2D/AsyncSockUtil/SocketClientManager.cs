using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using 囚徒困境2D.common;

namespace 囚徒困境2D.AsyncSockUtil
{
    public class SocketClientManager : ICloneable
    {
        public SocketClientManager()
        {
        }

        private int bufferSize = 6000;
        private const int MessageHeaderSize = 4;

        private Socket clientSocket;
        private bool connected = false;
        private IPEndPoint hostEndPoint;
        private AutoResetEvent autoConnectEvent;
        private AutoResetEvent autoSendEvent;
        private SocketAsyncEventArgs sendEventArgs;
        private SocketAsyncEventArgs receiveEventArgs;
        private BlockingCollection<byte[]> sendingQueue;
        private BlockingCollection<byte[]> receivedMessageQueue;
        private Thread sendMessageWorker;
        // private Thread processReceivedMessageWorker;

        private int m_totalBytesRead;           // counter of the total # bytes received by the server

        private static log4net.ILog log;//声明日志

        public PlayerUser serverPlayerUser = new PlayerUser(); //声明本机玩家
        public PlayerUser clientPlayerUser = new PlayerUser(); //声明远程玩家

        public IDictionary<int, GameUserInfo> gameUserInfoDictionaryWithServer { set; get; }

        public SocketClientManager(IPEndPoint hostEndPoint)
        {
            this.hostEndPoint = hostEndPoint;
            this.autoConnectEvent = new AutoResetEvent(false);
            this.autoSendEvent = new AutoResetEvent(false);
            this.sendingQueue = new BlockingCollection<byte[]>();
            this.receivedMessageQueue = new BlockingCollection<byte[]>();
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.sendMessageWorker = new Thread(new ThreadStart(SendQueueMessage));
            //this.processReceivedMessageWorker = new Thread(new ThreadStart(ProcessReceivedMessage));

            this.sendEventArgs = new SocketAsyncEventArgs();
            this.sendEventArgs.UserToken = this.clientSocket;
            this.sendEventArgs.RemoteEndPoint = this.hostEndPoint;
            this.sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

            this.receiveEventArgs = new SocketAsyncEventArgs();
            this.receiveEventArgs.UserToken = new AsyncUserToken(clientSocket);
            this.receiveEventArgs.RemoteEndPoint = this.hostEndPoint;
            this.receiveEventArgs.SetBuffer(new Byte[bufferSize], 0, bufferSize);
            this.receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
        }

        public void Init(PlayerUser _clientPlayerUser)
        {
            this.clientPlayerUser = _clientPlayerUser.Clone() as PlayerUser;

            gameUserInfoDictionaryWithServer = new Dictionary<int, GameUserInfo>();

            //日志初始化
            //日志配置
            log4net.GlobalContext.Properties["LogName"] = clientPlayerUser.userName + "-" + GetTimeStr();
            log = log4net.LogManager.GetLogger(typeof(Program));
        }

        public void Connect()
        {
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
            connectArgs.UserToken = this.clientSocket;
            connectArgs.RemoteEndPoint = this.hostEndPoint;
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            clientSocket.ConnectAsync(connectArgs);
            autoConnectEvent.WaitOne();

            SocketError errorCode = connectArgs.SocketError;
            if (errorCode != SocketError.Success)
            {
                throw new SocketException((Int32)errorCode);
            }
            sendMessageWorker.Start();
            // processReceivedMessageWorker.Start();

            if (!clientSocket.ReceiveAsync(receiveEventArgs))
            {
                ProcessReceive(receiveEventArgs);
            }
        }

        public void Disconnect()
        {
            clientSocket.Disconnect(false);
        }

        public void Send(string message)
        {
            byte[] str_data = Encoding.UTF8.GetBytes(message);
            sendingQueue.Add(str_data);
            log.Info("向服务端" + this.serverPlayerUser + "发送数据：" + message);
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoConnectEvent.Set();
            connected = (e.SocketError == SocketError.Success);
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            autoSendEvent.Set();
        }

        private void SendQueueMessage()
        {
            while (true)
            {
                var message = sendingQueue.Take();
                if (message != null)
                {
                    sendEventArgs.SetBuffer(message, 0, message.Length);
                    clientSocket.SendAsync(sendEventArgs);
                    autoSendEvent.WaitOne();
                }
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        // 处理从服务端发送过来的数据
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                AsyncUserToken token = e.UserToken as AsyncUserToken;
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);

                byte[] data = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);

                string message = System.Text.Encoding.UTF8.GetString(data);
                Console.WriteLine("客户端接收到的信息是：" + message);

                if (message.Contains("我的玩家信息是"))
                {
                    serverPlayerUser = new PlayerUser(userSplitMessage(message));
                    Send("我是客户端，我的玩家信息是：" + this.clientPlayerUser);
                    log.Info("向服务端发送信息：" + this.clientPlayerUser);
                }
                else if (message.Contains("game loop and press and timeDeff and startTime and predicted with server is"))
                {
                    // 解析服务端端数据
                    int gameLoop = int.Parse(message.Split('!')[1]);
                    string gameKJ = message.Split('!')[2];
                    string gameDiffTime = message.Split('!')[3];
                    string gameStrTime = message.Split('!')[4];
                    string gamePredict = message.Split('!')[5];

                    GameUserInfo userinfo = new GameUserInfo(gameLoop, serverPlayerUser, gameKJ, gamePredict, gameDiffTime, gameStrTime);
                    gameUserInfoDictionaryWithServer.Add(gameLoop, userinfo);

                    log.Info("**********         接收服务端" + serverPlayerUser.userName + "第" + (gameLoop + 1).ToString() + "轮次游戏，游戏信息是："
                       + message + "            *********");
                    // log.Info("接收到服务端当前游戏状态是："+message);
                }
                else
                {
                    log.Info("接收到：" + clientPlayerUser + "\r\n" + "发送来的数据：：  " + message);
                }

                //接收后续的数据
                if (!token.Socket.ReceiveAsync(e))
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                ProcessError(e);
            }
        }

        private void ProcessReceivedData(int dataStartOffset, int totalReceivedDataSize, int alreadyProcessedDataSize, AsyncUserToken token, SocketAsyncEventArgs e)
        {
            if (alreadyProcessedDataSize >= totalReceivedDataSize)
            {
                return;
            }

            if (token.MessageSize == null)
            {
                //如果之前接收到到数据加上当前接收到的数据大于消息头的大小，则可以解析消息头
                if (totalReceivedDataSize > MessageHeaderSize)
                {
                    //解析消息长度
                    var headerData = new byte[MessageHeaderSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, headerData, 0, MessageHeaderSize);
                    var messageSize = BitConverter.ToInt32(headerData, 0);

                    token.MessageSize = messageSize;
                    token.DataStartOffset = dataStartOffset + MessageHeaderSize;

                    //递归处理
                    ProcessReceivedData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + MessageHeaderSize, token, e);
                }
                //如果之前接收到到数据加上当前接收到的数据仍然没有大于消息头的大小，则需要继续接收后续的字节
                else
                {
                    //这里不需要做什么事情
                }
            }
            else
            {
                var messageSize = token.MessageSize.Value;
                //判断当前累计接收到的字节数减去已经处理的字节数是否大于消息的长度，如果大于，则说明可以解析消息了
                if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize)
                {
                    var messageData = new byte[messageSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, messageSize);
                    ProcessMessage(messageData);

                    //消息处理完后，需要清理token，以便接收下一个消息
                    token.DataStartOffset = dataStartOffset + messageSize;
                    token.MessageSize = null;

                    //递归处理
                    ProcessReceivedData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + messageSize, token, e);
                }
                //说明剩下的字节数还不够转化为消息，则需要继续接收后续的字节
                else
                {
                    //这里不需要做什么事情
                }
            }
        }

        private void ProcessMessage(byte[] messageData)
        {
            receivedMessageQueue.Add(messageData);
        }

        //private void ProcessReceivedMessage()
        //{
        //    while (true)
        //    {
        //        var message = receivedMessageQueue.Take();
        //        if (message != null)
        //        {
        //            var current = Interlocked.Increment(ref Program._receivedMessageCount);
        //            if (current == 1)
        //            {
        //                Program._watch = Stopwatch.StartNew();
        //            }
        //            if (current % 1000 == 0)
        //            {
        //                Console.WriteLine("received reply message, length:{0}, count:{1}, timeSpent:{2}", message.Length, current, Program._watch.ElapsedMilliseconds);
        //            }
        //        }
        //    }
        //}

        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                // close the socket associated with the client
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // throws if client process has already closed
                }
                finally
                {
                    if (s.Connected)
                    {
                        s.Close();
                    }
                }
            }

            // Throw the SocketException
            throw new SocketException((Int32)e.SocketError);
        }

        public bool isConnected()
        {
            if (!connected) return false;
            else return true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            autoConnectEvent.Close();
            if (this.clientSocket.Connected)
            {
                this.clientSocket.Close();
            }
        }

        #endregion IDisposable Members

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

        public string GetTimeStr()
        {
            /* 获取当前时间字符串函数 */
            string tempTimeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0')
                        + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0'); /* 以年-月-日时分秒的格式命名文件 */
            return tempTimeStr;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}