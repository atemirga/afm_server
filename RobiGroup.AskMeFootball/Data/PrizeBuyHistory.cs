using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class PrizeBuyHistory
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int PrizeId { get; set; }

        public int Price { get; set; }

        public int Code { get; set; }

        public bool IsActive { get; set; }

        public DateTime BuyDate { get; set; }
    }
}
