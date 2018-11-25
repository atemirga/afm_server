using System;
using System.Collections;
using DataTables.AspNet.AspNetCore;
using JQuery.DataTables.Extensions;
using Microsoft.AspNetCore.Mvc;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Controllers
{
    public class BaseController : Controller
    {
        protected ApplicationDbContext _dbContext;

        public BaseController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected IActionResult DataTableResponse<TFilterModel>(TFilterModel filterModel, Func<TFilterModel, IEnumerable> filterExpression) where TFilterModel : DataTablesFilterModel
        {
            var data = filterExpression.Invoke(filterModel);
            return new DataTablesJsonResult(DataTablesResponse.Create(filterModel.DataTablesRequest, filterModel.Total, filterModel.Total, data), true);
        }
    }
}