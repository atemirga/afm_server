using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Histories
{
    public class PlayHistoryViewModel
    {
        [Required]
        [DisplayName("Ник")]
        public string NickName { get; set; }

        [Required]
        [DisplayName("Оппонент")]
        public string Opponent { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Счет")]
        public int Score { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Счет Оппонента")]
        public int OpponentScore { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Результат")]
        public string Result { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Время")]
        public DateTime PlayDate { get; set; }
    }
}
