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
    public class PlayHistoriesController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public PlayHistoriesController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index(string id)
        {
            var userHistory = _dbContext.MatchGamers.Where(mg => mg.GamerId == id && mg.Ready).ToList();

            var histories = new List<PlayHistoryViewModel>();
            foreach (var uh in userHistory)
            {
                var opponentMatch = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == uh.MatchId && mg.GamerId != id);
                if (opponentMatch != null)
                {
                    var opponent = _dbContext.Users.FirstOrDefault(u => u.Id == opponentMatch.GamerId);
                    if (opponent != null)
                    {
                        var nickname = _dbContext.Users.FirstOrDefault(u => u.Id == id).NickName;

                        var history = new PlayHistoryViewModel();
                        history.NickName = nickname;
                        history.Opponent = opponent.NickName;
                        history.Score = uh.Score + opponentMatch.Bonus;
                        history.OpponentScore = opponentMatch.Score + opponentMatch.Bonus;
                        history.Result = uh.IsWinner ? "Выигрыш" : "Проигрыш";
                        history.PlayDate = Convert.ToDateTime(uh.JoinTime).AddHours(6);
                        histories.Add(history);
                    }
                }

            }
            return View(histories);
            //LoadIndexViewData(id);

            //if (TempData["Errors"] is List<string> errors)
            //{
            //    foreach (var error in errors)
            //    {
            //        ModelState.AddModelError(String.Empty, error);
            //    }
            //}

            //ViewBag.Message = TempData["Message"];

            //return View(new PlayHistoryFilterModel { UserId = id });
        }

        private void LoadIndexViewData(string id)
        {
            ViewBag.UserId = id;
        }

        public IActionResult _Filter(PlayHistoryFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterPlayHistory);
        }

        private IEnumerable<PlayHistoryViewModel> FilterPlayHistory(PlayHistoryFilterModel filter)
        {
            var userHistory = _dbContext.MatchGamers.Where(mg => mg.GamerId == filter.UserId).ToList();

            var histories = new List<PlayHistoryViewModel>();
            foreach (var uh in userHistory)
            {
                var opponentMatch = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == uh.MatchId && mg.GamerId != filter.UserId);
                if (opponentMatch != null)
                {
                    var opponent = _dbContext.Users.FirstOrDefault(u => u.Id == opponentMatch.GamerId);
                    if (opponent != null)
                    {
                        var nickname = _dbContext.Users.FirstOrDefault(u => u.Id == filter.UserId).NickName;

                        var history = new PlayHistoryViewModel();
                        history.NickName = nickname;
                        history.Opponent = opponent.NickName;
                        history.Score = uh.Score + opponentMatch.Bonus;
                        history.OpponentScore = opponentMatch.Score + opponentMatch.Bonus;
                        history.Result = uh.IsWinner ? "Выигрыш" : "Проигрыш";
                        history.PlayDate = Convert.ToDateTime(uh.JoinTime).AddHours(6);
                        histories.Add(history);
                    }
                }
                
            }


            return histories;
        }
        #endregion
    }
}