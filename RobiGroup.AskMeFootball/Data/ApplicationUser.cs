using Microsoft.AspNetCore.Identity;

namespace RobiGroup.AskMeFootball.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string NickName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string PhotoUrl { get; set; }

        /// <summary>
        /// Количество очков, которые можно использовать для поиска игры
        /// </summary>
        /// <value>The points to play.</value>
        public int PointsToPlay { get; set; }

        /// <summary>
        /// Итоговые очки
        /// </summary>
        public int TotalScore { get; set; }
        
        /// <summary>
        /// Текущие очки (раз в день начисляется 1000)
        /// </summary>
        public int Score { get; set; }

        public int? RankId { get; set; }

        public GamerRank Rank { get; set; }

        public int Bot { get; set; }
    }
}