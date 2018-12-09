using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Questions
{
    public class QuestionCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("Вопрос")]
        public string Text { get; set; }

        [Required]
        public int CorrectAnswerId { get; set; }

        [DisplayName("Ответы")]
        public List<string> Answers { get; set; }

        public int CardId { get; set; }
    }
}