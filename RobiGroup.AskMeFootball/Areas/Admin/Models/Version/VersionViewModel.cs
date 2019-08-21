using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Version
{
    public class VersionViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [Required]
        [DisplayName("Версия")]
        public string Version { get; set; }

        [Required]
        [DisplayName("Дата")]
        public DateTime LastUpdate { get; set; }
    }
}
