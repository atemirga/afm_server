namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchQuestionAnswerModel
    {
        public int QuestionId { get; set; }

        public int AnswerId { get; set; }
    }

    public class MatchQuestionAnswerResponse
    {
        public string GamerId { get; set; }

        public bool IsCorrect { get; set; }
    }
}