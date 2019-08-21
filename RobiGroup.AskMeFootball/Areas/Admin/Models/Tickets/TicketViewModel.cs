using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Tickets
{
    public class TicketViewModel
    {
        [DataTableColumn(Visible = false)]
        public int Id { get; set; }

        [Required]
        [DisplayName("Ник")]
        public string NickName { get; set; }

        [Required]
        [DisplayName("Телефон")]
        public string Phone { get; set; }

        [Required]
        [DisplayName("Текст ")]
        public string Text { get; set; }

        [Required]
        [DisplayName("Дата ")]
        public DateTime CreatedDate { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Вложение")]
        public List<string> Attachment { get; set; }


        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
