using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class HTMatchModel
    {
        public int MatchId { get; set; }
        public int QuestionId { get; set; }
        public DateTime QuestionStartTime { get; set; }
        public DateTime QuestionExpirationTime { get; set; }
    }
}
