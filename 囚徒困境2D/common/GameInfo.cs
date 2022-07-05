using System;
using System.Collections.Generic;

namespace 囚徒困境2D.common
{
    public class GameInfo : ICloneable
    {
        public PlayerUser playerUser { get; set; } //当前玩家信息
        public int gameLoop { get; set; } // 当前玩家在第几局

        public IDictionary<int, string> gamesKJparametersWithServerGF { get; set; }//存储玩家不同轮次响应的 k f
        public IDictionary<int, int> gamesGradeparametersWithServerGF { get; set; }//存储玩家不同轮次的得分
        public IDictionary<int, string> gamesPartTimeParametersWithServerGF { get; set; }//存储玩家不同轮次的消耗时间
        public IDictionary<int, string> gamesKJparametersWithClientGF { get; set; }//存储玩家不同轮次响应的 k f
        public IDictionary<int, int> gamesGradeparametersWithClientGF { get; set; }//存储玩家不同轮次的得分
        public IDictionary<int, string> gamesPartTimeParametersWithClientGF { get; set; }//存储玩家不同轮次的消耗时间

        //public IDictionary<int,GameProcessParameters> gamesWarProcessInfo { get; set; } //储存玩家双方对战信息

        public int numberOfKJwithServer { set; get; }
        public int numberOfKJwithClient { set; get; }
        public int numberOfGradewithServer { set; get; }
        public int numberOfGradewithClient { set; get; }

        public GameInfo()
        {
        }

        public GameInfo(PlayerUser player)
        {
            this.playerUser = player;
            this.gameLoop = 0;
            this.numberOfKJwithServer = 0;
            this.numberOfGradewithServer = 0;
            this.numberOfKJwithClient = 0;
            this.numberOfGradewithClient = 0;
            gamesKJparametersWithServerGF = new Dictionary<int, string>();
            gamesGradeparametersWithServerGF = new Dictionary<int, int>();
            gamesPartTimeParametersWithServerGF = new Dictionary<int, string>();

            gamesKJparametersWithClientGF = new Dictionary<int, string>();
            gamesGradeparametersWithClientGF = new Dictionary<int, int>();
            gamesPartTimeParametersWithClientGF = new Dictionary<int, string>();

            //gamesWarProcessInfo = new Dictionary<int, GameProcessParameters>();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        //public void addElementToGamesKJparametersWithServerGF(int loop,string KJ)
        //{
        //    this.gamesKJparametersWithServerGF[loop] = KJ;
        //}
        //public void addElementToGamesGradeparametersWithServerGF(int loop, int grade)
        //{
        //    this.gamesGradeparametersWithServerGF[loop] = grade;
        //}
        //public void addElementToGamesKJparametersWithClientGF(int loop, string KJ)
        //{
        //    this.gamesKJparametersWithClientGF[loop] = KJ;
        //}
        //public void addElementToGamesGradeparametersWithClientGF(int loop, int grade)
        //{
        //    this.gamesGradeparametersWithClientGF[loop] = grade;
        //}
    }
}