using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class ReferralUser
    {
        public int Id { get; set; }

        public string Referral { get; set; }

        public string UserId  { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime ActivatedDate { get; set; }
    }
}
