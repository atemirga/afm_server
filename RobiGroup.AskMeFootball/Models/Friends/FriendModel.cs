using System;

namespace RobiGroup.AskMeFootball.Models.Friends
{
    /// <summary>
    /// Друг
    /// </summary>
    public class FriendModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Ник
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Очки
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// Время создания
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}