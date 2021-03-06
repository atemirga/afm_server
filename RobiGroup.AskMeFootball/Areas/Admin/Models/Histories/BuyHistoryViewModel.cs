using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Histories
{
    public class BuyHistoryViewModel
    {
        [Required]
        [DisplayName("Ник")]
        public string NickName { get; set; }

        [Required]
        [DisplayName("Приз")]
        public string Prize { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Цена")]
        public int Price { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Дата покупки")]
        public DateTime BuyDate { get; set; }
    }
}
