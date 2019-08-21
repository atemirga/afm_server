using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class LiveAnswerModel
    {
        public bool IsCorrectAnswer { get; set; }
        public List<AnswerModel> AnswersCount { get; set; }
        public int CorrectAnswerId { get; set; }
        public int Lifes { get; set; }
        public int GamersCount { get; set; }
    }

    public class AnswerModel
    {
        public int AnswerId { get; set; }
        public int Count { get; set; }
    }
}
