using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using 囚徒困境2D.common;

/// <summary>
///  主要作用就是存储客户端的信息
/// </summary>
namespace 囚徒困境2D.AsyncSockUtil
{
    public class AsyncUserToken : IDisposable
    {
        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public IPAddress IPAddress { get; set; }

        /// <summary>
        /// 远程地址
        /// </summary>
        public EndPoint Remote { get; set; }

        /// <summary>
        /// 通信SOKET
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; }

        /// <summary>
        /// 所属用户信息
        /// </summary>
        public PlayerUser playerUser { get; set; }

        /// <summary>
        /// 数据缓存区
        /// </summary>
        public List<byte> Buffer { get; set; }

        public int? MessageSize { get; set; }
        public int DataStartOffset { get; set; }
        public int NextReceiveOffset { get; set; }

        public AsyncUserToken()
        {
            this.Buffer = new List<byte>();
        }

        public AsyncUserToken(Socket socket)
        {
            this.Socket = socket;
        }

        public override string ToString()
        {
            string tokenString = ",IPAddress：" + IPAddress + ",Remote：" + Remote + "Socket:" + Socket + ",ConnectTime:" + ConnectTime;
            return tokenString;
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                this.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            { }

            try
            {
                this.Socket.Close();
            }
            catch (Exception)
            { }
        }

        #endregion IDisposable Members
    }
}