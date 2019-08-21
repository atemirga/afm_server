using System;
namespace RobiGroup.AskMeFootball.Models.Account.Profile
{
    public class UserProfileModel
    {
        /// <summary>
        /// Логин
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Ник
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Rank { get; set; }

        public string PhotoUrl { get; set; }

        public int PointsToPlay { get; set; }

        /// <summary>
        /// Итоговые очки
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// Текущие очки
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public int Raiting { get; set; }

        /// <summary>
        /// Очки
        /// </summary>
        public int Coins { get; set; }

        /// <summary>
        /// Hints
        /// </summary>
        public int Hints { get; set; }

        /// <summary>
        /// Lifes
        /// </summary>
        public int Lifes { get; set; }

        /// <summary>
        /// Balance
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// ID игрока
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Промо код
        /// </summary>
        public string  Referral{ get; set; }

        /// <summary>
        /// Использовал код
        /// </summary>
        public bool IsReferralUsed { get; set; }

        /// <summary>
        /// Максимальная ставка
        /// </summary>
        public int MaxBid { get; set; }


        /// <summary>
        /// Проверка на друга
        /// </summary>
        public bool IsFriend { get; set; }

        /// <summary>
        /// Время до сброса
        /// </summary>
        public DateTime DailyResetTime { get; set; }
    }
}