using System;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchModel
    {
        public int Id { get; set; }

        public string CardName { get; set; }

        public string GamerName { get; set; }

        public string PhotoUrl { get; set; }

        public DateTime MatchStarted { get; set; }
    }

    public class MatchHistoryModel : MatchModel
    {
        public int Score { get; set; }

        public bool IsWon { get; set; }

        public DateTime Time { get; set; }
        public bool RivalIsWon { get; set; }
    }
}