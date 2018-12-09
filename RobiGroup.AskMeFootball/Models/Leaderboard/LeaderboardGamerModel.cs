namespace RobiGroup.AskMeFootball.Models.Leaderboard
{
    public class LeaderboardGamerModel
    {
        /// <summary>
        /// ID игрока
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Ссылка на фото
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Текущие очки
        /// </summary>
        public int CurrentScore { get; set; }

        /// <summary>
        /// Итоговые очки игрока
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public int Raiting { get; set; }

        /// <summary>
        /// Онлайн
        /// </summary>
        public bool IsOnline { get; set; }
    }
}