using System;
using System.Collections.Generic;

namespace RobiGroup.AskMeFootball.Data
{
    public class Match
    {
        public enum MatchStatus : byte
        {
            Requested,
            Confirmed,
            Started,
            Finished,
            Cancelled,
            CancelledAferStart,
            Delayed
        }

        public int Id { get; set; }

        public DateTime CreateTime { get; set; }

        public string Questions { get; set; }

        public DateTime? StartTime { get; set; }

        public int CardId { get; set; }

        public MatchStatus Status { get; set; }

        public Card Card { get; set; }

        public List<MatchGamer> Gamers { get; set; }
    }
}