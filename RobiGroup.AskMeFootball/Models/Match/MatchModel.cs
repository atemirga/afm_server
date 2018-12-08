namespace RobiGroup.AskMeFootball.Models.Match
{
    public class MatchSearchResultModel
    {
        public bool Found { get; set; }

        public MatchModel Match { get; set; }
    }

    public class MatchHistoryModel
    {
        public int Id { get; set; }

        public string CardName { get; set; }

        public string GamerName { get; set; }

        public string PhotoUrl { get; set; }

        public int Score { get; set; }

        public bool IsWon { get; set; }
    }
}