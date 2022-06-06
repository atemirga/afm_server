using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Models
{
    public class CloudpaymentPayment
    {
        public decimal Amount  { get; set; }
        public string Currency { get; set; }
        public int InvoiceId { get; set; }
        public string Description { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string CardCryptogramPacket { get; set; }
    }
}
