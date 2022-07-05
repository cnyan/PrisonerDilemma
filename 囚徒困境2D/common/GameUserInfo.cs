using System;

namespace 囚徒困境2D.common
{
    public class GameUserInfo : ICloneable
    {
        public int gameLoop { get; set; }
        public PlayerUser user { get; set; }
        public string gamesPressKJ { get; set; } //玩家按下的 K J键盘
        public string gamesPartTime { get; set; } //玩家游戏消耗时间
        public int gamesGrade { get; set; } // 玩家当前得分
        public string gamePredictRival { get; set; } //玩家预测值 1 2
        public string startTime { get; set; }  //玩家每一轮游戏开始时间

        public GameUserInfo()
        { }

        public GameUserInfo(int gameLoop, PlayerUser user, string gamesKJ, string gamesPtime)
        {
            this.gameLoop = gameLoop;
            this.user = user;
            this.gamesPressKJ = gamesKJ;
            this.gamesPartTime = gamesPtime;
            this.gamesGrade = -1;
            this.gamePredictRival = "-1";
        }

        public GameUserInfo(int gameLoop, PlayerUser user, string gamesKJ, string gamesNum, string gamesPtime, string gamestrTime)
        {
            this.gameLoop = gameLoop;
            this.user = user;
            this.gamesPressKJ = gamesKJ;
            this.gamesPartTime = gamesPtime;
            this.gamesGrade = -1;
            this.gamePredictRival = gamesNum;
            this.startTime = gamestrTime;
        }

        public override string ToString()
        {
            if (this.user.userCategory == 1) //服务端
            {
                return "$$$$$$$$   当前玩家是服务端，玩家名字叫做" + this.user.userName + " ，当前时间是" + this.startTime + " ,该玩家在第 " + (this.gameLoop + 1) + " 轮游戏中，按下了 "
              + this.gamesPressKJ + " 按键,消耗时间是 " + this.gamesPartTime + " 秒, 玩家该局游戏得分是 " + this.gamesGrade + " ,预测的对手玩家选项是 " + this.gamePredictRival + " 。\r\n";
            }
            else if (this.user.userCategory == 2)
            {
                return "$$$$$$$$   当前玩家是客户端，玩家名字叫做" + this.user.userName + " ，当前时间是" + this.startTime + " ,该玩家在第 " + (this.gameLoop + 1) + " 轮游戏中，按下了 "
             + this.gamesPressKJ + " 按键,消耗时间是 " + this.gamesPartTime + " 秒, 玩家该局游戏得分是 " + this.gamesGrade + " ,预测的对手玩家选项是 " + this.gamePredictRival + " 。\r\n";
            }

            return base.ToString();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}