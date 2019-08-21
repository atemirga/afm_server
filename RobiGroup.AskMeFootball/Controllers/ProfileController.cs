using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models;
using RobiGroup.AskMeFootball.Models.Account.Profile;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Профайл
    /// </summary>
    [Route("api/account/profile")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfileController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Профайл
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserProfileModel), 200)]
        public IActionResult Get()
        {
            var userId = User.GetUserId();

            var user = _dbContext.Users.Include(u => u.Rank).Single(u => u.Id == userId);
            var userCoins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == userId);
            var coins = 0;
            if (userCoins != null)
            {
                coins = userCoins.Coins;
            }

            var maxBid = _dbContext.Cards.First().MaxBid;

            return Ok(new UserProfileModel
            {
                Id = user.Id,
                Username = user.UserName,
                Nickname = user.NickName,
                Rank = user.Rank?.Name,
                Score = user.Score,
                PointsToPlay = user.PointsToPlay,
                TotalScore = user.TotalScore,
                PhotoUrl = user.PhotoUrl,
                Raiting = _dbContext.UserCoins.Count(u => _dbContext.Users.Any(_u => _u.Id == u.GamerId && _u.Bot == 0) && u.Coins > coins) + 1,
                Coins = coins,
                Hints = user.Hints,
                Lifes = user.Lifes,
                Balance = _dbContext.UserBalances.Any(ub => ub.UserId == userId) ?
                        _dbContext.UserBalances.FirstOrDefault(ub => ub.UserId == userId).Balance : 0,
                Referral = user.Referral,
                IsReferralUsed = user.ReferralUsed,
                MaxBid = maxBid,
                DailyResetTime = user.ResetTime,

            });
        }

        /// <summary>
        /// Изменить имя
        /// </summary>
        /// <param name="nickname">Имя</param>
        /// <returns></returns>
        [HttpPost("nickname/{nickname}")]
        [ProducesResponseType(200)]
        public IActionResult SetNickname([FromRoute]string nickname)
        {
            var userId = User.GetUserId();
            var user = _dbContext.Users.Include(u => u.Rank).Single(u => u.Id == userId);
            user.NickName = nickname;
            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpPost("photo")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SetPhoto()
        {
            var files = HttpContext.Request.Form.Files;

            if (files.Count == 0)
            {
                return BadRequest();
            }

            var userId = User.GetUserId();
            var user = _dbContext.Users.Find(userId);

            var fileService = HttpContext.RequestServices.GetService<IFileService>();
            var hostingEnvironment = HttpContext.RequestServices.GetService<IHostingEnvironment>();

           
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
            
            var photoPath = await fileService.Save(files[0], $"data/user/{user.Id}/profile");
            var rename = Path.Combine($"data/user/{user.Id}/profile", _name + Path.GetExtension(photoPath));
            
            var thumbnailPath = fileService.Thumbnail(photoPath, 150);

            user.PhotoUrl = Path.GetRelativePath(hostingEnvironment.WebRootPath, photoPath);

            _dbContext.SaveChanges();

            
            return Ok();
        }

        /// <summary>
        /// Статистика
        /// </summary>
        /// <returns></returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(UserProfileModel), 200)]
        public IActionResult GetStatistics()
        {
            var userId = User.GetUserId();

            var matches = (from m in _dbContext.Matches
                           join mg in _dbContext.MatchGamers on m.Id equals mg.MatchId
                           where mg.GamerId == userId && (m.Status == Match.MatchStatus.Finished && mg.JoinTime.HasValue
                           /*|| m.Status == Match.MatchStatus.CancelledAfterStart*/)
                           select mg.IsWinner);

            var wins = 0;
            var loses = 0;
            var totals = 0;
            var myMatches = _dbContext.MatchGamers.Where(mg => mg.GamerId == userId && mg.JoinTime.HasValue && mg.Confirmed && mg.Ready);
            foreach (var _mg in myMatches)
            {
                var match = _dbContext.Matches.FirstOrDefault(m => m.Id == _mg.MatchId && m.Status == Match.MatchStatus.Finished);
                if (match != null)
                {
                    var opponent = _dbContext.MatchGamers.FirstOrDefault(mg =>mg.MatchId == match.Id && mg.GamerId != userId);
                    if (opponent.IsWinner)
                    {
                        loses++;
                        totals++;
                    }
                    else if (_mg.IsWinner)
                    {
                        wins++;
                        totals++;
                    }
                    else if (!opponent.IsWinner && !_mg.IsWinner)
                    {
                        totals++;
                    }
                }

            }

            return Ok(new ProfileStatisticsModel()
            {
                Id = userId,
                Wins = wins,//matches.Count(m => m),
                Losses = loses,// matches.Count(m => !m),
                Totals = totals//matches.Count()
            });
        }

        /// <summary>
        /// Профиль юзера по ИД
        /// </summary>
        /// /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserProfileModel), 200)]
        public IActionResult GetUserProfile([FromRoute]string id)
        {
            var userId = id;//User.GetUserId();

            var user = _dbContext.Users.Include(u => u.Rank).Single(u => u.Id == userId);
            var userCoins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == userId);
            var coins = 0;
            if (userCoins != null)
            {
                coins = userCoins.Coins;
            }

            var isFriend = _dbContext.Friends.Any(f => ((f.FriendId == id && f.UserId == User.GetUserId()) || 
                                             (f.UserId == id && f.FriendId == User.GetUserId())) && f.IsAccepted);

            return Ok(new UserProfileModel
            {
                Id = user.Id,
                Username = user.UserName,
                Nickname = user.NickName,
                Rank = user.Rank?.Name,
                Score = user.Score,
                PointsToPlay = user.PointsToPlay,
                TotalScore = user.TotalScore,
                PhotoUrl = user.PhotoUrl,
                Raiting = _dbContext.UserCoins.Count(u => u.Coins > coins) + 1,
                Coins = coins,
                //Referral = user.Referral
                IsFriend = isFriend,
                DailyResetTime = user.ResetTime,
            });
        }

        /// <summary>
        /// Рефералка
        /// </summary>
        /// <param name="referral">Промо-код</param>
        /// <returns></returns>
        [HttpPost("referral/{referral}")]
        [ProducesResponseType(200)]
        public IActionResult UseReferral([FromRoute]string referral)
        {
            var balls = 0;
            var usedReferrals = 0;
            var userId = User.GetUserId();
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            var code = _dbContext.Users.FirstOrDefault(u => u.Referral == referral && u.Id != userId);
            usedReferrals = _dbContext.ReferralUsers.Where(ru => ru.Referral == referral).Count();
            var endDate = DateTime.Now.Date;
            var startDate = DateTime.Now.Date.AddDays(-7);
            var referrals = _dbContext.ReferralUsers.Where(ru => ru.PhoneNumber == code.PhoneNumber 
                                                            && ru.ActivatedDate > startDate && ru.ActivatedDate < endDate);

            if (referrals.Count() == 3)
            {
                ModelState.AddModelError("Referral", "Этот код был уже использован 3 раза за неделю. Попробуйте позже! ");
                return BadRequest(ModelState);
            }
            if (!user.ReferralUsed) 
            {
                
                if (code != null)
                {
                    user.ReferralUsed = true;
                    balls = (usedReferrals * 5) + 5;
                    if (balls > 20)
                    { balls = 20; }
                    code.PointsToPlay += balls;

                    _dbContext.PointHistories.Add(new PointHistories {
                        GamerId = code.Id,
                        Point = balls,
                        TimeAdded = DateTime.Now,
                    });

                    _dbContext.ReferralUsers.Add(new ReferralUser
                    {
                        UserId = userId,
                        PhoneNumber = code.PhoneNumber,
                        Referral = referral,
                        ActivatedDate = DateTime.Now
                    });
                    var thisCoin = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == userId);
                    if (thisCoin == null)
                    {
                        _dbContext.UserCoins.AddAsync(new UserCoins
                        {
                            GamerId = userId,
                            Coins = 100,
                            LastUpdate = DateTime.Now
                        });
                    }
                    else { thisCoin.Coins += 100; }
                    _dbContext.SaveChanges();
                    return Ok();
                }
                ModelState.AddModelError("Referral", "Код неправильный или не существует!");
                return BadRequest(ModelState);
            }
            
            ModelState.AddModelError("Referral", "Код уже использовался!");
            return BadRequest(ModelState);
        }


        /// Пакеты
        /// </summary>
        /// <returns></returns>
        [HttpGet("packs")]
        [ProducesResponseType(typeof(List<PackPrice>), 200)]
        public IActionResult GetPacks()
        {
            var userId = User.GetUserId();
            
            return Ok(_dbContext.PackPrices.AsQueryable());
        }

        /// <summary>
        /// Купить жизнь
        /// </summary>
        /// <param name="id">ID пакета</param>
        /// <param name="type">Способ покупки</param>
        /// <returns></returns>
        [HttpPost("buy/{id}/{type}")]
        [ProducesResponseType(200)]
        public IActionResult BuyLife([FromRoute]int id, [FromRoute]string type)
        {
            var userId = User.GetUserId();
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            var userBalance = _dbContext.UserBalances.FirstOrDefault(ub => ub.UserId == userId);
            var pack = _dbContext.PackPrices.Find(id);
            var price = pack.Price;

            if (type == "balance")
            {
                if (userBalance.Balance != 0)
                {
                    if (userBalance.Balance >= price) {
                        switch (pack.Type)
                        {
                            case "life":
                                user.Lifes += pack.Count;
                                break;
                            case "hint":
                                user.Hints += pack.Count;
                                break;
                            case "balls":
                                user.PointsToPlay += pack.Count;
                                break;
                        }
                        userBalance.Balance -= pack.Price;
                        _dbContext.SaveChanges();
                        return Ok();
                    }
                    ModelState.AddModelError("Referral", "Недостаточно средств!");
                    return BadRequest(ModelState);
                }
                ModelState.AddModelError("Referral", "Ваш баланс 0!");
                return BadRequest(ModelState);
            }

            switch (pack.Type)
            {
                case "life":
                    user.Lifes += pack.Count;
                    break;
                case "hint":
                    user.Hints += pack.Count;
                    break;
                case "balls":
                    user.PointsToPlay += pack.Count;
                    break;
            }
            _dbContext.SaveChanges();
            return Ok();
            
        }

        /// <summary>
        /// Всем по 100 монет :)
        /// </summary>
        /// <returns></returns>
        [HttpGet("add-coins")]
        public async Task<IActionResult> AddCoins()
        {
            var users = _dbContext.Users.Where(u => u.Bot == 0);
            var thisCoins = 100;
            foreach (var u in users)
            {
                
                var userCoins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == u.Id);
                if (userCoins == null)
                {
                    _dbContext.UserCoins.Add(new UserCoins
                    {
                        GamerId = u.Id,
                        Coins = thisCoins,
                        LastUpdate = DateTime.Now
                    });
                }
                else
                {
                    userCoins.Coins += thisCoins;
                    userCoins.LastUpdate = DateTime.Now;
                }

                
            }

            _dbContext.SaveChanges();

            return Ok();
        }


        /// <summary>
        /// Язык системы
        /// </summary>
        /// <param name="lang">язык</param>
        /// <returns></returns>
        [HttpPost("lang/{lang}")]
        [ProducesResponseType(200)]
        public IActionResult Language([FromRoute]string lang)
        {
            var userId = User.GetUserId();
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                user.Lang = lang;
                _dbContext.SaveChanges();

                return Ok();
            }

            ModelState.AddModelError("Lang", "какая-то ошибка");
            return BadRequest(ModelState);
        }


        //isNotificationAllowed
        /// <summary>
        /// Статус пуша
        /// </summary>
        /// <returns></returns>
        [HttpPost("notification")]
        [ProducesResponseType(200)]
        public IActionResult Notification([FromBody]NotificationModel notification)
        {
            var userNotification = _dbContext.UserNotifications.FirstOrDefault(u => u.GamerId == User.GetUserId());
            if (userNotification != null)
            {
                userNotification.IsNotificationAllowed = notification.IsNotificationAllowed;
            }
            else
            {
                _dbContext.UserNotifications.Add(new UserNotification {
                    GamerId = User.GetUserId(),
                    IsNotificationAllowed = notification.IsNotificationAllowed,
                });
            }
            _dbContext.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Версия 
        /// </summary>
        /// <returns></returns>
        [HttpGet("version")]
        [ProducesResponseType(typeof(VersionModel), 200)]
        public IActionResult Version()
        {
            var version = _dbContext.Versions.FirstOrDefault();

            var versionModel = new VersionModel();
            versionModel.version = version.Vers;
            versionModel.LastUpdate = version.LastUpdate;

            return Ok(versionModel);
        }
    }
}