using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class CardViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [Required]
        [DisplayName("Названия")]
        public string Name { get; set; }

        [DisplayName("Приз")]
        public string Prize { get; set; }

        [DataTableColumn(Visible = false)]
        public string ImageUrl { get; set; }

        [DisplayName("Кол-во вопросов в игре")]
        public int MatchQuestions { get; set; }

        [DisplayName("Время сброса")]
        public DateTime ResetTime { get; set; }

        [DisplayName("Время сброса")]
        public string Type { get; set; }

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}