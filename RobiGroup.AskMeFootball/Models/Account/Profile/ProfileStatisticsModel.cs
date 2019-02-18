namespace RobiGroup.AskMeFootball.Models.Account.Profile
{
    public class ProfileStatisticsModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Проигрыши
        /// </summary>
        public int Losses { get; set; }

        /// <summary>
        /// Выигрыши
        /// </summary>
        public int Wins { get; set; }
    }
}