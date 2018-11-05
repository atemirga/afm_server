using System.Collections.Generic;

namespace RobiGroup.AskMeFootball.Models.Questions
{
    public class QuestionModel
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public List<QuestionAnswerModel> Answers { get; set; }
    }
}