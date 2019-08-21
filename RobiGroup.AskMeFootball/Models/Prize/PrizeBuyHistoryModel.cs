using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Models.Prize
{
    public class PrizeBuyHistoryModel
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int PrizeId { get; set; }

        public int Price { get; set; }

        public DateTime BuyDate { get; set; }
    }
}
