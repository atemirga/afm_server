using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Users
{
    public class UserCreateModel
    {
        public string Id { get; set; }

        
        [DisplayName("Ник")]
        public string NickName { get; set; }

        
        [DisplayName("Телефон")]
        public string PhoneNumber { get; set; }

        
        [DisplayName("Мячи")]
        public int PointsToPlay { get; set; }

        [DisplayName("Жизни")]
        public int Lifes { get; set; }


        [DisplayName("Общий Счет")]
        public int TotalScore { get; set; }

        
        [DisplayName("Фото")]
        public string PhotoUrl { get; set; }
    }
}
