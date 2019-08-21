using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchRequestModel
    {
        public MatchRequestModel(int matchId, string cardName, ApplicationUser gamer)
        {
            MatchId = matchId;
            GamerFullName = gamer.FullName;
            GamerName = gamer.NickName;
            GamerPhotoUrl = gamer.PhotoUrl;
            GamerId = gamer.Id;
            CardName = cardName;
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

        public int MyCardScore { get; set; }

        public bool IsBot { get; set; }
        public string CardName { get; set; }

        public int Bid { get; set; }

        public int Coins { get; set; }
        public int RivalCoins { get; set; }
    }
}