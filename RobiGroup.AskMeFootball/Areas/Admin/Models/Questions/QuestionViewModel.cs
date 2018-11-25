using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Questions
{
    public class QuestionsFilterModel : DataTablesFilterModel
    {
        [DisplayName("Поиск")]
        public string Search { get; set; }
    }

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

    public class AnswerViewModel
    {
        public int Id { get; set; }

        public string Text { get; set; }
    }
}