using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JQuery.DataTables.Extensions;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Users
{
    public class FriendFilterModel: DataTablesFilterModel
    {
        public string UserId { get; set; }
    }
}
