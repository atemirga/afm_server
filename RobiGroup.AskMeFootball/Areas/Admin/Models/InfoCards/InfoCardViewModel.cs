using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.InfoCards
{
    public class InfoCardViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [Required]
        [DisplayName("Названия")]
        public string Title { get; set; }

        [DisplayName("Второе название")]
        public string Subtitle { get; set; }

        [DisplayName("Текст Кнопки")]
        public string ButtonTitle { get; set; }

        [DataTableColumn(Visible = false)]
        public List<string> Images { get; set; }

        [DataTableColumn(Visible = false)]
        public string ImageUrl { get; set; }

        [DataTableColumn(Visible = false)]
        public string VideoUrl { get; set; }

        [DisplayName("Заканчивается")]
        public DateTime EndDate { get; set; }

        [DataTableColumn(Visible = false)]
        public int IsActive { get; set; }

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
