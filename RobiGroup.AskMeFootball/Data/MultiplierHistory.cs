using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class MultiplierHistory
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int MatchId { get; set; }

        public int QuestionId { get; set; }

        public DateTime UsedAt { get; set; }

        public bool IsMultiplied { get; set; }
    }
}
