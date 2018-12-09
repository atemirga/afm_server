using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchModel
    {
        public MatchModel(int matchId, ApplicationUser gamer)
        {
            MatchId = matchId;
            GamerFullName = gamer.FullName;
            GamerName = gamer.NickName;
            GamerPhotoUrl = gamer.PhotoUrl;
            GamerId = gamer.Id;
        }

        public int MatchId { get; set; }

        public string GamerName { get; set; }

        public string GamerId { get; set; }

        public string GamerFullName { get; set; }

        public string GamerPhotoUrl { get; set; }

        public int GamerRaiting { get; set; }

        public int GamerCardScore { get; set; }

        public int CorrectAnswerScore { get; set; }

        public int IncorrectAnswerScore { get; set; }
    }
}