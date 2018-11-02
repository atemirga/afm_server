using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Models.Games
{
    public class PlayModel
    {
        [Required]
        public int CardId { get; set; }
    }
}