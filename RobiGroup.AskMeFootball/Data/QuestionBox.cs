using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class QuestionBox
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
    }
}
