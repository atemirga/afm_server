using System;

namespace RobiGroup.AskMeFootball.Data
{
    public class GamerCard
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int CardId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsActive { get; set; }

        public int Score { get; set; }

        public ApplicationUser Gamer { get; set; }

        public Card Card { get; set; }
    }
}