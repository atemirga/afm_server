using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class VersionCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("Версия")]
        public string Version { get; set; }

        [Required]
        [DisplayName("Дата обновления")]
        public DateTime LastUpdate { get; set; }
    }
}