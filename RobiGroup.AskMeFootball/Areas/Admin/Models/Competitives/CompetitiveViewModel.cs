using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;


namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Competitives
{
    public class CompetitiveViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [Required]
        [DisplayName("Названия")]
        public string Name { get; set; }

        [DisplayName("Приз")]
        public string Prize { get; set; }

        [DisplayName("Кол-во игроков в команде")]
        public int Gamers { get; set; }


        [DisplayName("Начало матча")]
        public DateTime StartTime { get; set; }

        [DisplayName("Конец матча")]
        public DateTime EndTime { get; set; }

        [DataTableColumn(Visible = false)]
        public string ImageUrlCard { get; set; }

        [DataTableColumn(Visible = false)]
        public string ImageUrlDetail { get; set; }

        [DataTableColumn(Visible = false)]
        public string ImageUrlSelect { get; set; }

        [DataTableColumn(Visible = false)]
        public int QuestionsTotal { get; set; }

        [DisplayName("Кол-во вопросов в игре")]
        public int MatchQuestions { get; set; }

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
