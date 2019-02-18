using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchQuestionAnswerModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int QuestionId { get; set; }

        public int? AnswerId { get; set; }
    }
}