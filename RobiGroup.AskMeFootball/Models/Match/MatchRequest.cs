using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchModel
    {
        public MatchModel(int matchId, ApplicationUser gamer)
        {
            MatchId = matchId;
            GamerFullName = gamer.FullName;
            GamerName = gamer.UserName;
            GamerPhotoUrl = gamer.PhotoUrl;
        }

        public int MatchId { get; set; }

        public string GamerName { get; set; }

        public string GamerFullName { get; set; }

        public string GamerPhotoUrl { get; set; }
    }

    public class MatchResultModel
    {
        /// <summary>
        /// Очки за матч
        /// </summary>
        public int MatchScore { get; set; }

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