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
    }

    public class Match
    {
        public int Id { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? StartTime { get; set; }

        public int CardId { get; set; }

        public Card Card { get; set; }

        public List<MatchParticipant> Participants { get; set; }
    }

    public class MatchParticipant
    {
        public int Id { get; set; }

        public int MacthId { get; set; }

        public string GamerId { get; set; }

        public bool Confirmed { get; set; }

        public DateTime? JoinTime { get; set; }

        public int Score { get; set; }

        public bool IsPlay { get; set; }

        public ApplicationUser Gamer { get; set; }

        public Match Match { get; set; }

        public List<MatchAnswer> MatchAnswers { get; set; }
    }

    public class MatchAnswer
    {
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ParticipantId { get; set; }

        public int AnswerId { get; set; }

        public bool IsCorrectAnswer { get; set; }

        public QuestionAnswer Answer { get; set; }

        public Question Question { get; set; }

        public ApplicationUser Gamer { get; set; }

        public Card Card { get; set; }

        public MatchParticipant Participant { get; set; }
    }
}