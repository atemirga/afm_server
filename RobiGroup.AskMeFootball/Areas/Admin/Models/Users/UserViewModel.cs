using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Users
{
    public class UserViewModel
    {
        [DataTableColumn(Visible = false)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Ник")]
        public string NickName { get; set; }

        [Required]
        [DisplayName("Телефон")]
        public string Phone { get; set; }

        [Required]
        [DisplayName("Общий счет")]
        public int TotalScore { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Сегодняшний Счет")]
        public int DailyScore { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Монеты")]
        public int Coins { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Монеты за сегодня")]
        public int CoinsToday { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Игры")]
        public int Plays { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Игры сегодня")]
        public int PlaysToday { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Подсказки")]
        public int Hints { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Удвоители")]
        public int Multipliers { get; set; }


        [DataTableColumn(Visible = true)]
        [DisplayName("Рандом")]
        public int MatchWithRandom { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("С другом")]
        public int MatchWithFriend { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Пуш")]
        public bool IsNotificationAllowed { get; set; }

        [DataTableColumn(Visible = true)]
        [DisplayName("Registered Date")]
        public DateTime RegisteredDate { get; set; }

        [DataTableColumn(Render = "renderActions")]
        public string Action { get; set; }
    }
}
