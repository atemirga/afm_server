using System;

namespace RobiGroup.AskMeFootball.Data
{
    public class MatchAnswer
    {
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int MatchGamerId { get; set; }

        public int QuestionId { get; set; }

        public int? AnswerId { get; set; }

        public bool IsCorrectAnswer { get; set; }

        public QuestionAnswer Answer { get; set; }

        public Question Question { get; set; }

        public MatchGamer MatchGamer { get; set; }
    }
}