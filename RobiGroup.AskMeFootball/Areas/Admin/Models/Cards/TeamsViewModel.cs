using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;


namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class TeamsViewModel
    {
        [DataTableColumn(Visible = false)]
        public int? Id { get; set; }

        [DataTableColumn(Visible = false)]
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

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
