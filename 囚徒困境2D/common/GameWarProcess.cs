namespace 囚徒困境2D.common
{
    public class GameWarProcess
    {
        public PlayerUser userWithServer { get; set; }
        public PlayerUser userWithClient { get; set; }

        public int gameLoop { set; get; }
        public string gameUserNameWithServer { set; get; } //服务端
        public string gameUserNameWithClient { set; get; } //客户端
        public string gameUserCategory { set; get; } //当前状态是 服务端或者客户端

        public string pressKeyValueWithServer { set; get; } //玩家按下的键 FJ
        public string pressKeyValueWithClient { set; get; }
        public string pressKeyTimeWithServer { set; get; } //玩家该轮游戏消耗时间
        public string pressKeyTimeWithClient { set; get; }
        public int nowGradeWithServer { set; get; } //当前状态得分
        public int nowGradeWithClient { set; get; } //当前状态得分

        public GameWarProcess()
        { }

        public GameWarProcess(PlayerUser _userWithServer, PlayerUser _userWithClient, string _gameUserCategory)
        {
            this.userWithServer = _userWithServer;
            this.userWithClient = _userWithClient;
            this.gameLoop = 0;
            this.gameUserNameWithServer = this.userWithServer.userName;
            this.gameUserNameWithClient = this.userWithClient.userName;
            this.gameUserCategory = _gameUserCategory;

            this.pressKeyValueWithServer = null;
            this.pressKeyValueWithClient = null;
            this.pressKeyTimeWithServer = null;
            this.pressKeyTimeWithClient = null;
            this.nowGradeWithServer = 0;
            this.nowGradeWithServer = 0;
        }

        public void SetGameWarProcessParameters(PlayerUser _userWithServer, PlayerUser _userWithClient, int _gameLoop, string _userCategory, string _pressKeyValueWithServer, string _pressKeyValueWithClient,
            string _pressKeyTimeWithServer, string _pressKeyTimeWithClient, int _lastGradeWithServer, int _lastGradeWithClient)
        {
            this.userWithServer = _userWithServer;
            this.userWithClient = _userWithClient;
            this.gameLoop = _gameLoop;
            this.gameUserCategory = _userCategory;
            this.pressKeyValueWithServer = _pressKeyValueWithServer;
            this.pressKeyValueWithClient = _pressKeyValueWithClient;
            this.pressKeyTimeWithServer = _pressKeyTimeWithServer;
            this.pressKeyTimeWithClient = _pressKeyTimeWithClient;
            this.nowGradeWithServer = setNowGradeWithServer(_lastGradeWithServer);
            this.nowGradeWithServer = setNowGradeWithClient(_lastGradeWithClient);
        }

        //判断当前游戏状态是否完毕
        public bool IsGameRunning()
        {
            if (!(this.pressKeyValueWithServer == null && this.pressKeyValueWithClient == null
                && this.pressKeyTimeWithServer == null && this.pressKeyTimeWithClient == null
                && this.nowGradeWithServer == 0 && this.nowGradeWithClient == 0))
            {
                return true;
            }
            return false;
        }

        // 配置得分,并返回当前得分
        public int setNowGradeWithServer(int lastGradeWithServer)
        {
            return 0;
        }

        //配置玩家得分，并返回当前得分
        public int setNowGradeWithClient(int lastGradeWithClient)
        {
            return 0;
        }

        public int getNowGradeWithCategory(string _gameUserCategory)
        {
            if (_gameUserCategory == "服务端")
            {
                return this.nowGradeWithServer;
            }
            else if (_gameUserCategory == "客户端")
            {
                return this.nowGradeWithClient;
            }
            return 0;
        }

        public override string ToString()
        {
            if (this.gameUserCategory == "服务端")
            {
                return "$$$$$$$$   当前玩家是服务端，玩家名字叫做" + this.gameUserNameWithServer + ",该玩家在第 " + this.gameLoop + " 轮游戏中，按下了 "
                + this.pressKeyValueWithServer + " 按键,消耗时间是 " + this.pressKeyTimeWithServer + " 秒,游戏得分是 " + this.nowGradeWithServer + " 。\r\n"
                + "对方玩家是客户端，玩家名字叫做" + this.gameUserNameWithClient + ",该玩家在第 " + this.gameLoop + " 轮游戏中，按下了 "
                + this.pressKeyValueWithClient + " 按键,消耗时间是 " + this.pressKeyTimeWithClient + " 秒,游戏得分是 " + this.nowGradeWithClient + " 。    $$$$$$$$$$$";
            }
            else if (this.gameUserCategory == "客户端")
            {
                return "$$$$$$$$   当前玩家是客户端，玩家名字叫做" + this.gameUserNameWithClient + ",该玩家在第 " + this.gameLoop + " 轮游戏中，按下了 "
               + this.pressKeyValueWithClient + " 按键,消耗时间是 " + this.pressKeyTimeWithClient + " 秒,游戏得分是 " + this.nowGradeWithClient + " 。\r\n"
               + "对方玩家是服务端，玩家名字叫做" + this.gameUserNameWithServer + ",该玩家在第 " + this.gameLoop + " 轮游戏中，按下了 "
               + this.pressKeyValueWithServer + " 按键,消耗时间是 " + this.pressKeyTimeWithServer + " 秒,游戏得分是 " + this.nowGradeWithServer + " 。    $$$$$$$$$$$";
            }
            return base.ToString();
        }
    }
}