using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Common.Files;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Histories;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class BuyHistoriesController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public BuyHistoriesController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index(string id)
        {
            LoadIndexViewData(id);

            if (TempData["Errors"] is List<string> errors)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(String.Empty, error);
                }
            }

            ViewBag.Message = TempData["Message"];

            return View(new BuyHistoryFilterModel { UserId = id });
        }

        private void LoadIndexViewData(string id)
        {
            ViewBag.UserId = id;
        }

        public IActionResult _Filter(BuyHistoryFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterBuyHistory);
        }

        private IEnumerable<BuyHistoryViewModel> FilterBuyHistory(BuyHistoryFilterModel filter)
        {
            var userHistory = _dbContext.PrizeBuyHistories.Where(pbh => pbh.GamerId == filter.UserId).ToList();

            var histories = new List<BuyHistoryViewModel>();
            foreach (var uh in userHistory)
            {
                var nickname = _dbContext.Users.FirstOrDefault(u => u.Id == filter.UserId).NickName;
                var prize = _dbContext.Prizes.FirstOrDefault(p => p.Id == uh.PrizeId).Name;
                var history = new BuyHistoryViewModel();
                history.BuyDate = uh.BuyDate;
                history.NickName = nickname;
                history.Price = uh.Price;
                history.Prize = prize;
                histories.Add(history);
            }
            

            return histories;
        }
        #endregion
    }
}