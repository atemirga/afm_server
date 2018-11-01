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

        public int GameQuestions { get; set; }

        public int TypeId { get; set; }

        public DateTime ResetTime { get; set; }

        public int ResetPeriod { get; set; }

        public List<Question> Questions { get; set; }

        public CardType Type { get; set; }

        public List<Game> Games { get; set; }
    }

    public class Game
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; }

        public int CardId { get; set; }

        public Card Card { get; set; }

        public List<GameParticipant> Participants { get; set; }
    }

    public class GameParticipant
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public int GamerId { get; set; }

        public int Score { get; set; }

        public bool IsPlay { get; set; }

        public ApplicationUser Gamer { get; set; }

        public Game Game { get; set; }

        public List<GamerAnswer> GamerAnswers { get; set; }
    }

    public class GamerAnswer
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

        public GameParticipant Participant { get; set; }
    }
}