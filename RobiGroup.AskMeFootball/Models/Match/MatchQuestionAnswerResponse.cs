namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchQuestionAnswerResponse : MatchQuestionAnswerModel
    {
        public string GamerId { get; set; }

        public bool IsCorrect { get; set; }
    }
}