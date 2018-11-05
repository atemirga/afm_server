using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class PlayModel
    {
        [Required]
        public int CardId { get; set; }
    }
}