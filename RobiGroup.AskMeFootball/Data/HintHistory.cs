using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class HintHistory
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int MatchId { get; set; }

        public int QuestionId { get; set; }

        public DateTime UsedAt { get; set; }
    }
}
