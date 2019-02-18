using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Questions
{
    public class QuestionViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [DisplayName("Вопрос")]
        public string Text { get; set; }

        [DataTableColumn(Visible = false)]
        public int AnswerId { get; set; }

        [DataTableColumn(Render = "renderAnswers")]
        [DisplayName("Ответы")]
        public List<AnswerViewModel> Answers { get; set; }
    }
}