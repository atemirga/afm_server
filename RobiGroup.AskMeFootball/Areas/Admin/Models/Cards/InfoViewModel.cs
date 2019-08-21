using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class InfoViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [DataTableColumn(Visible = false)]
        public int CardId { get; set; }

        [Required]
        [DisplayName("Текст")]
        public string Text { get; set; }

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }

    }
}
