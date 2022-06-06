using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Friends;


namespace RobiGroup.AskMeFootball.Models
{
    public class DaylyReferrals
    {
        public List<FriendModel> friends { get; set; }
        public int day { get; set; }
    }
}
