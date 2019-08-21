using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RobiGroup.AskMeFootball.Models
{
    public class TicketModel
    {
        //public int Id { get; set; }
        public int CategoryId { get; set; }
        public IFormFile Attachment { get; set; }
        public string Text { get; set; }
    }
}
