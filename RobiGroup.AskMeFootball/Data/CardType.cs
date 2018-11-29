using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RobiGroup.AskMeFootball.Data
{
    public class CardType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }

    public enum CardTypes
    {
        Daily, Weekly, Monthly
    }

    public class Friend
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public string FriendId { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("GamerId")]
        public ApplicationUser Gamer { get; set; }

        [ForeignKey("FriendId")]
        public ApplicationUser FriendUser { get; set; }
    }
}