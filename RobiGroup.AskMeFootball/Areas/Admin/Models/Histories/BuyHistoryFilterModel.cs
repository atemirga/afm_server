using System.ComponentModel;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Histories
{
    public class BuyHistoryFilterModel : DataTablesFilterModel
    {
        [DisplayName("Поиск")]
        public string Search { get; set; }
        public string UserId { get; set; }
    }
}
