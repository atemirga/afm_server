namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchRequest
    {
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