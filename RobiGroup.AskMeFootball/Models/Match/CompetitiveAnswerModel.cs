using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Models.Match
{
    public class CompetitiveAnswerModel
    {
        public bool IsCorrectAnswer { get; set; }

        public int CorrectAnswers { get; set; }

        public int IncorrectAnswers { get; set; }

        public bool IsMultiplied { get; set; }

        public int Coins { get; set; }
    }
}
