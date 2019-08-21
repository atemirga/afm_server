using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Users
{
    public class FriendViewModel
    {

        [DisplayName("Ник")]
        public string NickName { get; set; }
        
        [DataTableColumn(Visible = true)]
        [DisplayName("Телефон")]
        public int Phone { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Счет")]
        public int Score { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Монеты")]
        public int Coins { get; set; }
    }
}
