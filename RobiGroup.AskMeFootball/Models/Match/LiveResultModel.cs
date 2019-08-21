using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class LiveResultModel
    {
        public int UserCoins { get; set; }
        public int Coins { get; set; }
        public bool IsWinner { get; set; }
        public List<MatchWinner> Winners { get; set; }
        public int Prize { get; set; }
        public double WinnerPrize { get; set; }
        public int Lifes { get; set; }
    }

    public class MatchWinner
    {
        public string NickName { get; set; }
        public string PhotoUrl { get; set; }
        
    }
}
