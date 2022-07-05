namespace 囚徒困境2D.common
{
    /// <summary>
    /// 自定义数据包,用来发送或接收数据
    /// </summary>
    public class DataPackage
    {
        /// <summary>
        /// 功能码
        /// </summary>
        public int FCode { get; set; } = -1;

        public string userName { get; set; }
        public int userCategory { get; set; }

        /// <summary>
        /// 发送或接收到的数据
        /// </summary>
        public int sendOrRec { get; set; }

        // 用户按下的键盘按钮，1：F， 2：J
        public int pushKeyboard { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Datas { get; set; }

        public override string ToString()
        {
            var uCate = "null";
            if (this.userCategory == 1)
            {
                uCate = "服务端，玩家一:";
            }
            else if (this.userCategory == 2)
            {
                uCate = "客户端，玩家二:";
            }

            var sOr = "null";
            if (this.sendOrRec == 1)
            {
                sOr = "发送：";
            }
            else if (this.sendOrRec == 2)
            {
                sOr = "接收：";
            }

            var pKey = "null";
            if (this.pushKeyboard == 1)
            {
                pKey = "F";
            }
            else if (this.pushKeyboard == 2)
            {
                pKey = "K";
            }

            return uCate + this.userName + "," + sOr + pKey;
        }
    }
}