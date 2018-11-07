﻿using Microsoft.AspNetCore.Identity;

namespace RobiGroup.AskMeFootball.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string PhotoUrl { get; set; }

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
    }

    public class GamerRank
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}