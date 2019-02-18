using System;

namespace RobiGroup.AskMeFootball.Core.Handlers.Models
{
    public class PausedMatch
    {
        public int MatchId { get; set; }

        public DateTime PausedTime { get; set; }
    }
}