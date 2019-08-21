using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Prizes
{
    public class PrizeViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [Required]
        [DisplayName("Названия")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Код")]
        public int Code { get; set; }

        [Required]
        [DisplayName("Определение")]
        public string Description { get; set; }

        [DataTableColumn(Visible = false)]
        [DisplayName("Первое фото")]
        public string FirstPhotoUrl { get; set; }

        [DataTableColumn(Visible = false)]
        [DisplayName("Второе фото")]
        public string SecondPhotoUrl { get; set; }

        [DataTableColumn(Visible = false)]
        [DisplayName("Третье фото")]
        public string ThirdPhotoUrl { get; set; }

        [Required]
        [DataTableColumn(Visible = false)]
        [DisplayName("Адресс")]
        public string Address { get; set; }

        [Required]
        [DataTableColumn(Visible = false)]
        [DisplayName("Номер телефона")]
        public string FirstPhoneNumber { get; set; }

        /*
        [Required]
        [DataTableColumn(Visible = false)]
        [DisplayName("Второй номер телефона")]
        public string SecondPhoneNumber { get; set; }
        */

        [Required]
        [DataTableColumn(Visible = false)]
        [DisplayName("Дата")]
        public DateTime Date { get; set; }

        [DataTableColumn(Visible = false)]
        [DisplayName("Facebook")]
        public string Facebook { get; set; }

        [DataTableColumn(Visible = false)]
        [DisplayName("Instagram")]
        public string Instagram { get; set; }

        [DataTableColumn(Visible = false)]
        [DisplayName("Twitter")]
        public string Twitter { get; set; }

        [DataTableColumn(Visible = false)]
        [DisplayName("Vkontakte")]
        public string Vkontakte { get; set; }

        [Required]
        [DisplayName("Цена")]
        public int Price { get; set; }

        [Required]
        [DisplayName("Наличие")]
        public int InStock { get; set; }

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
