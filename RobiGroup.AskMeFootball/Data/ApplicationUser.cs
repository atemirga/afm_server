using Microsoft.AspNetCore.Identity;

namespace RobiGroup.AskMeFootball.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhotoUrl { get; set; }

        public int TotalScore { get; set; }

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