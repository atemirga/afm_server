using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class UserNotification
    {
        public int Id { get; set; }
        public string GamerId { get; set; }

        public bool IsNotificationAllowed { get; set; }
    }
}
