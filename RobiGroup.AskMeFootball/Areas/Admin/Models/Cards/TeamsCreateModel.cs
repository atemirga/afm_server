using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class TeamsCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("CardId")]
        public int CardId { get; set; }

        [Required]
        [DisplayName("First Team")]
        public string FirstTeam { get; set; }

        [Required]
        [DisplayName("Second Team")]
        public string SecondTeam { get; set; }

        [Required]
        [DisplayName("First Team score")]
        public int FirstTeamScore { get; set; }

        [Required]
        [DisplayName("Second Team score")]
        public int SecondTeamScore { get; set; }

        [Required]
        [DisplayName("First Team logo")]
        public string FirstTeamLogo { get; set; }

        [Required]
        [DisplayName("Second Team logo")]
        public string SecondTeamLogo { get; set; }
    }
}
