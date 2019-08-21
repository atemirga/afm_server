using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Prizes
{
    public class PrizeCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("Названия")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Определение")]
        public string Description { get; set; }

        [Required]
        [DisplayName("Первое фото")]
        public string FirstPhotoUrl { get; set; }

        [Required]
        [DisplayName("Второе фото")]
        public string SecondPhotoUrl { get; set; }

        [Required]
        [DisplayName("Третье фото")]
        public string ThirdPhotoUrl { get; set; }

        [Required]
        [DisplayName("Адрес")]
        public string Address { get; set; }

        [Required]
        [DisplayName("Номер телефона")]
        public string FirstPhoneNumber { get; set; }

        /*
        [Required]
        [DisplayName("Второй номер")]
        public string SecondPhoneNumber { get; set; }
        */

        [Required]
        [DisplayName("Дата")]
        public DateTime Date { get; set; }

        [Required]
        [DisplayName("Веб-сайт")]
        public string Site { get; set; }

        [Required]
        [DisplayName("Facebook")]
        public string Facebook { get; set; }

        [Required]
        [DisplayName("Instagram")]
        public string Instagram { get; set; }

        [Required]
        [DisplayName("Twitter")]
        public string Twitter { get; set; }

        [Required]
        [DisplayName("Vkontakte")]
        public string Vkontakte { get; set; }

        [Required]
        [DisplayName("Цена")]
        public int Price { get; set; }

        [Required]
        [DisplayName("Наличие")]
        public int InStock { get; set; }
    }
}
