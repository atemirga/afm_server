using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Questions;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class CompetitiveMatchModel
    {
        public int MatchId { get; set; }
        public List<QuestionModel> Questions{ get; set; }
    }
}
