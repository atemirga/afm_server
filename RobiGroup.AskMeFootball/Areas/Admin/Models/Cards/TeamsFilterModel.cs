using System.ComponentModel;
using JQuery.DataTables.Extensions;


namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class TeamsFilterModel: DataTablesFilterModel
    {
        public int CardId { get; set; }
    }
}
