using System;

namespace RobiGroup.AskMeFootball.Data
{
    public class GamerCard
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int CardId { get; set; }

        public DateTime StartTime { get; set; }

        public int Points { get; set; }

        public ApplicationUser Gamer { get; set; }

        public Card Card { get; set; }
    }
}