using System;

namespace 囚徒困境2D.common
{
    public class PlayerUser : ICloneable
    {
        public string userName { get; set; }
        public string userDate { get; set; }

        /// <summary>
        /// 玩家属性,1:玩家一，作为服务端， 2：玩家二，作为客户端
        /// </summary>
        public int userCategory { get; set; }

        //生成用户日志文件名称
        public string userLogName
        {
            get { return userName + "-" + userDate; }
            set { userLogName = value; }
        }

        public string userIp { get; set; }

        public PlayerUser()
        { }

        public PlayerUser(PlayerUser p)
        {
            this.userName = p.userName;
            this.userDate = p.userDate;
            this.userCategory = p.userCategory;
            this.userIp = p.userIp;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override string ToString()
        {
            return "userNmae:" + userName + " ,userCategory:" + userCategory.ToString() + " ,userDate:" + userDate + " ,user IP:" + userIp;
        }
    }
}