using System.Collections.Generic;

namespace RobiGroup.AskMeFootball.Data
{
    public class Question
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public int Point { get; set; }

        public int Duration { get; set; }

        public int Order { get; set; }

        public int CardId { get; set; }

        public int CorrectAnswerId { get; set; }

        public QuestionAnswer CorrectAnswer { get; set; }

        public Card Card { get; set; }

        public List<QuestionAnswer> Answers { get; set; }
    }
}