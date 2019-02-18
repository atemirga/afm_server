namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchResultModel
    {
        /// <summary>
        /// Очки за матч
        /// </summary>
        public int MatchScore { get; set; }

        /// <summary>
        /// Бонусные очки за матч
        /// </summary>
        public int MatchBonus { get; set; }

        /// <summary>
        /// Очки за матч соперника
        /// </summary>
        public int RivalMatchScore { get; set; }

        /// <summary>
        /// Итоговые очки на карточке
        /// </summary>
        public int CardScore { get; set; }

        /// <summary>
        /// Текущие очки
        /// </summary>
        public int CurrentGamerScore { get; set; }

        /// <summary>
        /// Победиль или нет
        /// </summary>
        public bool IsWinner { get; set; }
    }
}