using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RobiGroup.AskMeFootball.Data
{
    public class Question
    {
        public int Id { get; set; }

        public string TextRu { get; set; }

        public string TextKz { get; set; }

        public int CardId { get; set; }

        public int CorrectAnswerId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime StartTime { get; set; }

        public int ExpirationTime { get; set; }

        /// <summary>
        /// Секунд между вопросами
        /// </summary>
        /// <returns></returns>
        public int Delay { get; set; }

        public Card Card { get; set; }
        
        public List<QuestionAnswer> Answers { get; set; }
    }
}