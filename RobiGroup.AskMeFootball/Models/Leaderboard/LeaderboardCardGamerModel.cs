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
    }

    public class LeaderboardCardGamerModel : LeaderboardGamerModel
    {
        /// <summary>
        /// Очки на карточке
        /// </summary>
        public int CardScore { get; set; }
    }
}