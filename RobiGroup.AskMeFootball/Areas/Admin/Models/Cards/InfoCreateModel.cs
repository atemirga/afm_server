using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class InfoCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("CardId")]
        public int CardId { get; set; }

        [Required]
        [DisplayName("Text")]
        public string Text { get; set; }
    }
}
