using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RobiGroup.AskMeFootball.Data
{
    public class Card
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Prize { get; set; }

        //public string ImageUrl { get; set; }
        public string ImageUrlCard { get; set; }

        public string ImageUrlDetail { get; set; }

        public string ImageUrlSelect { get; set; }

        public int MatchQuestions { get; set; }

        public int TypeId { get; set; }

        public DateTime ResetTime { get; set; }

        public int ResetPeriod { get; set; }

        public int MaxBid { get; set; }

        public int EntryPoint { get; set; }

        public bool IsTwoH { get; set; }

        public bool IsHalfH { get; set; }

        public bool IsActive { get; set; }

        public List<Question> Questions { get; set; }

        public CardType Type { get; set; }

        public List<Match> Matches { get; set; }

        public List<GamerCard> GamerCards { get; set; }
    }

    public class CardWinner
    {
        public int Id { get; set; }

        public string Prize { get; set; }

        public DateTime CardStartTime { get; set; }

        public DateTime CardEndTime { get; set; }

        public int GamerCardScore { get; set; }

        public int CardId { get; set; }

        public string GamerId { get; set; }

        public Card Card { get; set; }

        public ApplicationUser Gamer { get; set; }
    }

    public class UserCoins
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int Coins { get; set; }

        public DateTime LastUpdate { get; set; }

        public ApplicationUser Gamer { get; set; }
    }

    public class PointHistories
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int Point { get; set; }

        public DateTime TimeAdded { get; set; }
    }
}