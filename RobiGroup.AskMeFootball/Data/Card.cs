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

        public string ImageUrl { get; set; }

        public int MatchQuestions { get; set; }

        public int TypeId { get; set; }

        public DateTime ResetTime { get; set; }

        public int ResetPeriod { get; set; }

        public List<Question> Questions { get; set; }

        public CardType Type { get; set; }

        public List<Match> Matches { get; set; }

        public List<GamerCard> GamerCards { get; set; }
    }
}