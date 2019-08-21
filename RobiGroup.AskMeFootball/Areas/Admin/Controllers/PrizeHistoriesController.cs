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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Prizes;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class PrizeHistoriesController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public PrizeHistoriesController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table

        public IActionResult All()
        {
            var historyQuery = _dbContext.PrizeBuyHistories.AsQueryable();

            var histories = new List<PrizeHistoryModel>();

            foreach (var hq in historyQuery)
            {
                var nickname = _dbContext.Users.FirstOrDefault(u => u.Id == hq.GamerId).NickName;
                var prize = _dbContext.Prizes.FirstOrDefault(p => p.Id == hq.PrizeId).Name;


                var history = new PrizeHistoryModel();
                history.NickName = nickname;
                history.Prize = prize;
                history.Price = hq.Price;
                history.BuyDate = hq.BuyDate.AddHours(6);
                history.IsActive = hq.IsActive;
                histories.Add(history);
            }
            return View(histories);
        }

        public IActionResult Index(string id)
        {
            var historyQuery = _dbContext.PrizeBuyHistories.Where(pbh => pbh.GamerId == id);

            var histories = new List<PrizeHistoryModel>();

            foreach (var hq in historyQuery)
            {
                var nickname = _dbContext.Users.FirstOrDefault(u => u.Id == hq.GamerId).NickName;
                var prize = _dbContext.Prizes.FirstOrDefault(p => p.Id == hq.PrizeId).Name;


                var history = new PrizeHistoryModel();
                history.NickName = nickname;
                history.Prize = prize;
                history.Price = hq.Price;
                history.BuyDate = hq.BuyDate.AddHours(6);
                history.IsActive = hq.IsActive;
                histories.Add(history);
            }
            return View(histories);
        }

        public IActionResult _Filter(PrizeHistoryFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterHistories);
        }

        private IEnumerable<PrizeHistoryModel> FilterHistories(PrizeHistoryFilterModel filter)
        {
            var historyQuery = _dbContext.PrizeBuyHistories.AsQueryable();

            filter.Total = historyQuery.Count();

            var histories = new List<PrizeHistoryModel>();

            foreach (var hq in historyQuery)
            {
                var nickname = _dbContext.Users.FirstOrDefault(u => u.Id == hq.GamerId).NickName;
                var prize = _dbContext.Prizes.FirstOrDefault(p => p.Id == hq.PrizeId).Name;


                var history = new PrizeHistoryModel();
                history.NickName = nickname;
                history.Prize = prize;
                history.Price = hq.Price;
                history.BuyDate = hq.BuyDate.AddHours(6);
                history.IsActive = hq.IsActive;
                histories.Add(history);
            }
            
            return histories;
        }

        #endregion
    }
}