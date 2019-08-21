using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class CashOutHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public double Cash { get; set; }
        public DateTime OutDate { get; set; }
    }
}
