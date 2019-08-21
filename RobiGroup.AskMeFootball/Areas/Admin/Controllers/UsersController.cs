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
    public class UsersController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public UsersController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index()
        {
            var usersQuery = _dbContext.Users.AsQueryable();
            var users = new List<UserViewModel>();

            foreach (var uq in usersQuery)
            {
                if (uq.UserName != "admin" && uq.Bot == 0)
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
                    usersView.DailyScore = uq.Score;
                    usersView.TotalScore = uq.TotalScore;
                    usersView.Plays = plays;
                    usersView.PlaysToday = playsToday;
                    usersView.MatchWithRandom = matchWithRandom;
                    usersView.MatchWithFriend = matchWithFriend;
                    usersView.RegisteredDate = uq.RegisteredDate.AddHours(6);
                    usersView.IsNotificationAllowed = _dbContext.UserNotifications.Any(un => un.GamerId == uq.Id) ?
                                                     _dbContext.UserNotifications.FirstOrDefault(un => un.GamerId == uq.Id).IsNotificationAllowed : false;

                    users.Add(usersView);
                }
            }
            return View(users);
        }

        [HttpGet]
        public IActionResult FilterDate(string startDateF, string endDateF)
        {
            var usersQuery = _dbContext.Users.AsQueryable();
            var users = new List<UserViewModel>();

            foreach (var uq in usersQuery)
            {
                if (uq.Bot > 0)
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
                    var games = _dbContext.MatchGamers.Where(mg => mg.GamerId == uq.Id);
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
                    usersView.DailyScore = uq.Score;
                    usersView.TotalScore = uq.TotalScore;
                    usersView.Plays = plays;
                    usersView.PlaysToday = playsToday;
                    usersView.MatchWithRandom = matchWithRandom;
                    usersView.MatchWithFriend = matchWithFriend;
                    usersView.RegisteredDate = uq.RegisteredDate.AddHours(6);
                    usersView.IsNotificationAllowed = _dbContext.UserNotifications.Any(un => un.GamerId == uq.Id) ?
                                                     _dbContext.UserNotifications.FirstOrDefault(un => un.GamerId == uq.Id).IsNotificationAllowed : false;

                    users.Add(usersView);
                }
            }
            return View(users);
        }

        public IActionResult _Filter(UserFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterUsers);
        }

        private IEnumerable<UserViewModel> FilterUsers(UserFilterModel filter)
        {
            var usersQuery = _dbContext.Users.AsQueryable();

            filter.Total = usersQuery.Count();

            var users = new List<UserViewModel>();

            foreach (var uq in usersQuery)
            {
                if(uq.UserName != "admin")
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
                    var games = _dbContext.MatchGamers.Where(mg => mg.GamerId == uq.Id && mg.Ready);
                    foreach (var game in games)
                    {
                        if (game.JoinTime > startDate && game.JoinTime < endDate)
                        {
                            var matchBid = _dbContext.MatchBids.FirstOrDefault(mb => mb.MatchId == game.MatchId);
                            if (matchBid != null)
                            {
                                coinsToday += matchBid.Bid;
                            }
                            else
                            {
                                coinsToday += game.Score + game.Bonus;
                            }
                            playsToday += 1;
                        }
                        plays += 1;
                    }

                    var usersView = new UserViewModel();
                    usersView.Id = uq.Id;
                    usersView.Coins = coins;
                    usersView.CoinsToday = coinsToday;
                    usersView.NickName = uq.NickName;
                    usersView.Phone = "+" + uq.PhoneNumber;
                    usersView.DailyScore = uq.Score;
                    usersView.TotalScore = uq.TotalScore;
                    usersView.Plays = plays;
                    usersView.PlaysToday = playsToday;
                    usersView.RegisteredDate = uq.RegisteredDate.AddHours(6);

                    users.Add(usersView);
                }
            }

            
            return users;
        }

        #endregion


        

        #region Create/Edit

        public IActionResult Create(string id)
        {
            //LoadViewData();
            UserCreateModel createModel;

            if (id != null)
            {
                createModel = _dbContext.Users.Where(c => c.Id == id).Select(r => new UserCreateModel()
                {
                    Id = r.Id,
                    NickName = r.NickName,
                    PhoneNumber = r.PhoneNumber,
                    PointsToPlay = r.PointsToPlay,
                    Lifes = r.Lifes,
                    TotalScore = r.TotalScore,
                    PhotoUrl = r.PhotoUrl
                    
                }).Single();
            }
            else
            {
                createModel = new UserCreateModel()
                {
                    NickName = String.Empty,
                    PointsToPlay = 0
                };
            }

            return View(createModel);
        }
        

        [HttpPost]
        public IActionResult Create(UserCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                //LoadViewData();
                return View("Create", model);
            }

            ApplicationUser user;

            user = _dbContext.Users.Find(model.Id);
            if (model.PhoneNumber != "" || model.PhoneNumber != null)
            {
                user.PhoneNumber = model.PhoneNumber;
                user.UserName = model.PhoneNumber;
                user.NormalizedUserName = model.PhoneNumber;
            }
            
            user.NickName = model.NickName;
            user.PointsToPlay = model.PointsToPlay;
            user.Lifes = model.Lifes;
            user.TotalScore = model.TotalScore;
            _dbContext.SaveChanges();

            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.PhotoUrl)))
            {

                var photosDir = "data/user/" + user.Id + "/profile";
                if (!string.IsNullOrEmpty(model.PhotoUrl))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.PhotoUrl);
                    if (System.IO.File.Exists(photoFile))
                    {
                        if (!Directory.Exists(photosDir))
                        {
                            Directory.CreateDirectory(photosDir);
                        }
                        else
                        {
                            Directory.Delete(photosDir, true);
                            Directory.CreateDirectory(photosDir);
                        }

                        if (System.IO.File.Exists(user.PhotoUrl))
                        {
                            System.IO.File.Delete(user.PhotoUrl);
                        }

                        var length = 8;
                        Random random = new Random();
                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                        var code = string.Empty;
                        code = new string(Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)]).ToArray());

                        var _name = code;

                        var destFileName = Path.Combine(photosDir, _name + Path.GetExtension(photoFile));
                        if (System.IO.File.Exists(destFileName))
                        { 
                            System.IO.File.Delete(destFileName);
                        }
                        

                        
                        System.IO.File.Move(photoFile, destFileName);

                        user.PhotoUrl = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        /*
                        var filename = Path.GetFileName(user.PhotoUrl);
                        var _path = "data/user/" + user.Id + "/profile";
                        Image image = Image.FromFile(_path);//Path.Combine(_path, filename)
                        //Image image = Image.FromFile(@user.PhotoUrl);
                        Image thumbnail = image.GetThumbnailImage(80, 80, null, IntPtr.Zero);
                        var thumb = Path.Combine(_path, "thumb" + Path.GetExtension(filename));
                        if (System.IO.File.Exists(thumb))
                        {
                            System.IO.File.Delete(thumb);
                        }
                        thumbnail.Save(thumb);
                        */
                    }
                }
            }
            return RedirectToAction("Index");
        }

        #endregion
    }
}