using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Friends;

namespace RobiGroup.AskMeFootball.Models
{
    public class MonthlyReferrals
    {
        public List<DaylyReferrals> days { get; set; }
        public int month { get; set; }
    }
}
