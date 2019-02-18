using System.Collections.Generic;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class ResumeMatchResultModel
    {
        public ResumeMatchResultModel()
        {
            MissedQuestions = new List<int>();
        }

        public List<int> MissedQuestions { get; set; }
    }
}