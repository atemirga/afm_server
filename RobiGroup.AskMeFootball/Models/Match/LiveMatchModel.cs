using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Questions;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class LiveMatchModel
    {
        public int MatchId { get; set; }
        public DateTime StartTime { get; set; }
        public List<QuestionModel> Questions { get; set; }
        public CardTeams Teams { get; set; }
    }
}
