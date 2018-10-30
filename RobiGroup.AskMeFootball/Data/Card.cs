using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RobiGroup.AskMeFootball.Data
{
    public class Card
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int PrizeAmount { get; set; }

        public int PrizeCurrency { get; set; }

        public string ImageUrl { get; set; }

        public int TypeId { get; set; }

        [ForeignKey("TypeId")]
        public CardType Type { get; set; }
    }

    public class CardType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public int Point { get; set; }

        public int Duration { get; set; }

        public int Order { get; set; }
    }

    public class QuestionAnswer
    {
        
    }
}