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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Users;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class CompetitiveFriendsController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CompetitiveFriendsController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index( string userId, int cardId)
        {
            var usersQuery = _dbContext.Users.Where(u => u.UserName != "admin" && u.Bot == 0).AsQueryable();
            var users = new List<UserViewModel>();

            var phone = _dbContext.Users.Find(userId).PhoneNumber;

            foreach (var uq in usersQuery)
            {
                var inCard = _dbContext.GamerCards.Any(gc => gc.CardId == cardId && gc.GamerId == uq.Id);

                var isFriend = _dbContext.ReferralUsers.Any(ru => ru.UserId == uq.Id && ru.PhoneNumber == phone);

                if (inCard && isFriend)
                {
                    var coins = 0;
                    if (_dbContext.UserCoins.Any(uc => uc.GamerId == uq.Id))
                    {
                        coins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == uq.Id).Coins;
                    }
                    var startDate = uq.ResetTime.AddDays(-1);
                    var endDate = uq.ResetTime;
                    var coinsToday = 0;
                    var plays = 0;
                    var playsToday = 0;
                    var matchWithRandom = 0;
                    var matchWithFriend = 0;
                    var games = _dbContext.MatchGamers.Where(mg => mg.GamerId == uq.Id && mg.Confirmed && mg.Ready);

                    var multipliers = _dbContext.MultiplierHistories.Count(mh => mh.GamerId == uq.Id);
                    var hints = _dbContext.HintHistories.Count(mh => mh.GamerId == uq.Id);

                    foreach (var game in games)
                    {
                        if (game.JoinTime > startDate && game.JoinTime < endDate)
                        {
                            var matchBidToday = _dbContext.MatchBids.FirstOrDefault(mb => mb.MatchId == game.MatchId);
                            if (matchBidToday != null)
                            {
                                coinsToday += matchBidToday.Bid;
                            }
                            else
                            {
                                coinsToday += game.Score + game.Bonus;
                            }
                            playsToday += 1;
                        }
                        var matchBid = _dbContext.MatchBids.FirstOrDefault(mb => mb.MatchId == game.MatchId);
                        if (matchBid != null)
                        {
                            matchWithFriend += 1;
                        }
                        else
                        {
                            matchWithRandom += 1;
                        }
                        plays += 1;
                    }
                    var usersView = new UserViewModel();
                    usersView.Id = uq.Id;
                    usersView.Coins = coins;
                    usersView.CoinsToday = coinsToday;
                    usersView.NickName = uq.NickName;
                    usersView.Phone = "+" + uq.PhoneNumber;
                    usersView.Plays = plays;
                    usersView.PlaysToday = playsToday;
                    usersView.Hints = hints;
                    usersView.Multipliers = multipliers;
                    usersView.IsNotificationAllowed = _dbContext.UserNotifications.Any(un => un.GamerId == uq.Id) ?
                                                     _dbContext.UserNotifications.FirstOrDefault(un => un.GamerId == uq.Id).IsNotificationAllowed : false;

                    users.Add(usersView);
                }
            }
            return View(users);
        }

        #endregion
    }
}