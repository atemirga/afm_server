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
        public int MatchScore { get; set; }

        public int CardScore { get; set; }
    }
}