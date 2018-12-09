using System.ComponentModel;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Questions
{
    public class QuestionsFilterModel : DataTablesFilterModel
    {
        [DisplayName("Поиск")]
        public string Search { get; set; }

        public int CardId { get; set; }
    }
}