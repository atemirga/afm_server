using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class MatchBid
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int Bid { get; set; }
        public string Winner { get; set; }
        public bool Status { get; set;  }
    }
}
