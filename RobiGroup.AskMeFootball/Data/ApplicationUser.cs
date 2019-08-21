using Microsoft.AspNetCore.Identity;
using System;

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
        /// Жизни для Live игр
        /// </summary>
        /// <value>Lifes for Live Matches</value>
        public int Lifes { get; set; }

        /// <summary>
        /// купленные подсказки
        /// </summary>
        /// <value>My Hints</value>
        public int Hints { get; set; }

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

        public string OneSignalId { get; set; }
        public bool Sync { get; set; }

        public DateTime RegisteredDate { get; set; }

        public string Referral { get; set; }

        public bool ReferralUsed { get; set; }

        public string Lang { get; set; }

        public DateTime ResetTime { get; set; }
    }
}