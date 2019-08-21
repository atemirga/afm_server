using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class Friend
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FriendId { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime LoveStartedDate { get; set; }
    }
}
