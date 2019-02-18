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
        /// ID игрока
        /// </summary>
        public string Id { get; set; }
    }
}