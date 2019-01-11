using System;

namespace RobiGroup.AskMeFootball.Common.Options
{
    public class MatchOptions
    {
        public int CorrectAnswerScore { get; set; }

        public int IncorrectAnswerScore { get; set; }

        public int BonusForAnswer { get; set; }

        public TimeSpan MatchPauseDuration { get; set; }

        public TimeSpan TimeForOneQuestion { get; set; }

        public int MissedQuestionsCount { get; set; }
    }
}