﻿namespace RobiGroup.AskMeFootball.Data
{
    public class QuestionAnswer
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public bool IsDeleted { get; set; }

        public string Order { get; set; }

        public int QuestionId { get; set; }

        public Question Question { get; set; }
    }
}