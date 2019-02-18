using System;
using System.Collections.Generic;

namespace RobiGroup.AskMeFootball.Data
{
    public class MatchGamer
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public int GamerCardId { get; set; }

        public string GamerId { get; set; }

        public bool Confirmed { get; set; }

        public bool Ready { get; set; }

        public bool Cancelled { get; set; }

        public bool Delayed { get; set; }

        public DateTime? JoinTime { get; set; }

        public int Score { get; set; }

        public bool IsPlay { get; set; }

        public bool IsWinner { get; set; }

        public ApplicationUser Gamer { get; set; }

        public Match Match { get; set; }

        public GamerCard GamerCard { get; set; }

        public List<MatchAnswer> Answers { get; set; }

        public int Bonus { get; set; }
    }
}