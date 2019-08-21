using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Packs
{
    public class PacksCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("Type")]
        public string Type { get; set; }

        [Required]
        [DisplayName("Count")]
        public int Count { get; set; }

        [Required]
        [DisplayName("Price")]
        public double Price { get; set; }
    }
}
