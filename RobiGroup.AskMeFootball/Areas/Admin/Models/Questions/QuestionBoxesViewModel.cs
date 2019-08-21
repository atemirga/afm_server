using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Questions
{
    public class QuestionBoxesViewModel
    {
        [DataTableColumn(Visible = false)]
        public int? Id { get; set; }

        [DataTableColumn(Visible = false)]
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

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
