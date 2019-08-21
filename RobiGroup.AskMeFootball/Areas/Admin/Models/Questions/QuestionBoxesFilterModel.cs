using System;
using System.ComponentModel;
using JQuery.DataTables.Extensions;


namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Questions
{
    public class QuestionBoxesFilterModel : DataTablesFilterModel
    {
        public int QuestionId { get; set; }
    }
}
