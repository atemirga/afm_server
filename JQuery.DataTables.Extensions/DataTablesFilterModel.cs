using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;

namespace JQuery.DataTables.Extensions
{
    public class DataTablesFilterModel
    {
        [IgnoreSqlParam]
        [ModelBinder(BinderType = typeof(ModelBinder))]
        public IDataTablesRequest DataTablesRequest { get; set; }

        [IgnoreSqlParam]
        public bool IsReport { get; set; }

        [IgnoreSqlParam]
        public int Total { get; set; }
    }
}