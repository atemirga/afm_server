using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Models.Prize
{
    public class PrizeModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string FirstPhotoUrl { get; set; }

        public string SecondPhotoUrl { get; set; }

        public string ThirdPhotoUrl { get; set; }

        public string Address { get; set; }

        public string FirstPhoneNumber { get; set; }

        //public string SecondPhoneNumber { get; set; }

        public DateTime Date { get; set; }

        public string Site { get; set; }

        public string Facebook { get; set; }

        public string Instagram { get; set; }

        public string Twitter { get; set; }

        public string Vkontakte { get; set; }

        public int Price { get; set; }

        public int InStock { get; set; }

        public int Code { get; set; }

        public bool IsActive { get; set; }
    }
}
