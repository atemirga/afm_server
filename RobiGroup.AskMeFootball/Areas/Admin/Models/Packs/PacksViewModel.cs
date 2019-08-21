using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Packs
{
    public class PacksViewModel
    {
        [DataTableColumn(Visible = false)]
        public int? Id { get; set; }

        [DataTableColumn(Visible = false)]
        public string Type { get; set; }

        [Required]
        [DisplayName("Count")]
        public int Count { get; set; }

        [Required]
        [DisplayName("Price")]
        public double Price { get; set; }
       

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
