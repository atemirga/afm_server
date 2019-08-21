using System.Collections.Generic;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Models.Questions
{
    public class QuestionModel
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public int Delay { get; set; }

        public List<QuestionAnswerModel> Answers { get; set; }

        public QuestionBox Box { get; set; }
    }
}