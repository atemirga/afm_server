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
        /// Ник
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Очки
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// Онлайн
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public int Raiting { get; set; }

        /// <summary>
        ///  Играет в игру
        /// </summary>
        public bool IsPlaying { get; set; }
    }
}