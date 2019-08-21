using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Questions
{
    public class QuestionBoxesCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("QuestionId")]
        public int QuestionId { get; set; }

        [Required]
        [DisplayName("Type")]
        public string Type { get; set; }

        [Required]
        [DisplayName("Text")]
        public string Text { get; set; }

        [Required]
        [DisplayName("Image")]
        public string ImageUrl { get; set; }
    }
}
