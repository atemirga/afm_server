using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Cards;
using RobiGroup.AskMeFootball.Models.Friends;
using RobiGroup.AskMeFootball.Models.Leaderboard;
using RobiGroup.Web.Common;
using RobiGroup.Web.Common.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Карты
    /// </summary>
    [Route("api/friends")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FirendsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly GamersHandler _gamersHandler;

        public FirendsController(ApplicationDbContext dbContext, GamersHandler gamersHandler)
        {
            _dbContext = dbContext;
            _gamersHandler = gamersHandler;
        }
        
    
        
        /*
        [HttpPost]
        [ProducesResponseType(typeof(List<FriendModel>), 200)]
        public IActionResult GetAll()
        {
            var userId = User.GetUserId();
            var friendId = string.Empty;
            var friendList = _dbContext.Friends.Where(f => (f.FriendId == userId || f.UserId == userId) && f.IsAccepted);
            var friends = new List<FriendModel>();
            foreach (var f in friendList)
            {
                if (userId == f.UserId)
                {
                    friendId = f.FriendId;
                } else
                {
                    friendId = f.UserId;
                }
                var friend = new FriendModel();
                friend.Id = friendId;
                friend.Nickname = _dbContext.Users.FirstOrDefault(u => u.Id == friendId).NickName;
                friend.PhoneNumber = _dbContext.Users.FirstOrDefault(u => u.Id == friendId).PhoneNumber;
                friend.TotalScore = _dbContext.Users.FirstOrDefault(u => u.Id == friendId).TotalScore;
                friend.PhotoUrl = _dbContext.Users.FirstOrDefault(u => u.Id == friendId).PhotoUrl;
                friend.IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == friendId && mg.IsPlay);
                friend.Raiting = _dbContext.Users.Count(ru => ru.TotalScore >
                            _dbContext.Users.FirstOrDefault(u => u.Id == friendId).TotalScore) + 1;
                friend.OneSignalId = _dbContext.Users.FirstOrDefault(u => u.Id == friendId).OneSignalId;
                friend.Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == friendId) ?
                      _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == friendId).Coins : 0;

                friends.Add(friend);
            }
            return Ok(friends);
        }
        */

        
        /// <summary>
        /// Список друзей
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(List<FriendModel>), 200)]
        public async Task<IActionResult> GetAll([FromBody]SyncModel phones)
        {

            var myPhoneNumber = _dbContext.Users.First(u => u.Id == User.GetUserId()).PhoneNumber;
            var myName = _dbContext.Users.First(u => u.Id == User.GetUserId()).NickName;
            var sync = _dbContext.Users.First(u => u.Id == User.GetUserId()).Sync;

            //var getContacts = new List<GetContact>();

            
            
            var _users = new List<string>();
            string _user = String.Empty;
            //&& u.PhoneNumber != myPhoneNumber
            var friends = _dbContext.Users.Where(u => phones.Phones.Contains(u.PhoneNumber)).Select(u => new FriendModel
            {
                Id = u.Id,
                Nickname = u.NickName,
                PhoneNumber = u.PhoneNumber,
                TotalScore = u.TotalScore,
                PhotoUrl = u.PhotoUrl,
                IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == u.Id && mg.IsPlay),
                Raiting = _dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,
                OneSignalId = u.OneSignalId,
                Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == u.Id) ?
                       _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == u.Id).Coins : 0
            }).ToList();


            foreach (var gamer in friends)
            {
                gamer.IsOnline = _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
                _user = gamer.OneSignalId;
                if (myPhoneNumber != gamer.PhoneNumber)
                {
                    _users.Add(_user);
                }
            }

            if (!sync)
            {
                //foreach (var gamer in friends)
                //{
                //    var _gamer = _dbContext.Users.FirstOrDefault(u => u.Id == gamer.Id);
                //    _gamer.PointsToPlay += 1;
                //    _dbContext.PointHistories.Add(new PointHistories
                //    {
                //        GamerId = gamer.Id,
                //        Point = 1,
                //        TimeAdded = DateTime.Now
                //    });
                //    _dbContext.SaveChanges();
                //}


                var user = _dbContext.Users.First(u => u.Id == User.GetUserId());
                user.Sync = true;
                #region
                using (var client = new HttpClient())
                {
                    var url = new Uri("https://onesignal.com/api/v1/notifications");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");
                    var obj = new
                    {
                        app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                        headings = new { en = "New Friend", ru = "Новый друг" },
                        contents = new { en = "Your contact " + myName + " added to game", ru = "Ваш контакт " + myName + " добавился в игру" },
                        data = new { number = myPhoneNumber },
                        include_player_ids = _users
                    };
                    var json = JsonConvert.SerializeObject(obj);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                }
                #endregion

                _dbContext.SaveChanges();
            }

            return Ok(friends);
        }
        
        

        /*    
            ////////////////////////////////////////////////////////////////////
            /// <summary>
            /// Список Контактов
            /// </summary>
            /// <returns></returns>
        [HttpPost("contact/sync")]
        [ProducesResponseType(typeof(List<FriendModel>), 200)]
        public async Task<IActionResult> SyncContacts([FromBody]SyncModel contacts)
        {

            var myPhoneNumber = _dbContext.Users.First(u => u.Id == User.GetUserId()).PhoneNumber;
            var myName = _dbContext.Users.First(u => u.Id == User.GetUserId()).NickName;
            var sync = _dbContext.Users.First(u => u.Id == User.GetUserId()).Sync;

            //var getContacts = new List<GetContact>();

            foreach (var p in contacts.contacts)
            {
                foreach (var _p in p.Phones)
                {
                    var phoneExist = _dbContext.GetContacts.Any(gc => gc.PhoneNumber == _p);
                    if (phoneExist)
                    {
                        var getContact = _dbContext.GetContacts.FirstOrDefault(gc => gc.PhoneNumber == _p);
                        var names = getContact.Names.SplitToArray().ToList();
                        if (!names.Contains(p.Name))
                        {
                            names.Add(p.Name);
                            getContact.Names = string.Join(',', names);
                        }
                        _dbContext.SaveChanges();

                    }
                    else
                    {
                        _dbContext.GetContacts.Add(new GetContact
                        {
                            PhoneNumber = _p,
                            LastUpdate = DateTime.Now,
                            Names = p.Name
                        });
                    }
                    _dbContext.SaveChanges();
                }


            }

            var _users = new List<string>();
            string _user = String.Empty;
            //&& u.PhoneNumber != myPhoneNumber
            var friends = _dbContext.Users.Where(u => contacts.contacts.Exists(p => p.Phones.Contains(u.PhoneNumber))).Select(u => new FriendModel
            {
                Id = u.Id,
                Nickname = u.NickName,
                PhoneNumber = u.PhoneNumber,
                TotalScore = u.TotalScore,
                PhotoUrl = u.PhotoUrl,
                IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == u.Id && mg.IsPlay),
                Raiting = _dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,
                OneSignalId = u.OneSignalId,
                Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == u.Id) ?
                       _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == u.Id).Coins : 0
            }).ToList();

            
            foreach (var gamer in friends)
            {
                gamer.IsOnline = _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
                _user = gamer.OneSignalId;
                if (myPhoneNumber != gamer.PhoneNumber)
                {
                    _users.Add(_user);
                }
            }

            if (!sync)
            {
                foreach (var gamer in friends)
                {
                    //var _gamer = _dbContext.Users.FirstOrDefault(u => u.Id == gamer.Id);
                    //_gamer.PointsToPlay += 1;
                    _dbContext.PointHistories.Add(new PointHistories
                    {
                        GamerId = gamer.Id,
                        Point = 1,
                        TimeAdded = DateTime.Now
                    });
                    _dbContext.SaveChanges();
                }


                var user = _dbContext.Users.First(u => u.Id == User.GetUserId());
                user.Sync = true;

                #region
                using (var client = new HttpClient())
                {
                    var url = new Uri("https://onesignal.com/api/v1/notifications");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");
                    var obj = new
                    {
                        app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                        headings = new { en = "Новый друг", ru = "Новый друг" },
                        contents = new { en = "Ваш контакт " + myName + " добавился в игру", ru = "Ваш контакт " + myName + " добавился в игру" },
                        data = new { number = myPhoneNumber },
                        include_player_ids = _users
                    };
                    var json = JsonConvert.SerializeObject(obj);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                }
                #endregion

                _dbContext.SaveChanges();
            }

            return Ok(friends);
        }*/
        

        /// <summary>
        /// Запрос в друзья по ИД
        /// </summary>
        /// /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpPost("request/{id}")]
        [ProducesResponseType(200)]
        public IActionResult FriendRequest([FromRoute]string id)
        {
            var friendId = id;
            var userId = User.GetUserId();

            var isExist = _dbContext.Friends.Any(f => (f.FriendId == friendId && f.UserId == userId) 
                                                    || (f.UserId == friendId && f.FriendId == userId));

            if (!isExist)
            {
                _dbContext.Friends.Add(new Friend {
                    UserId = userId,
                    FriendId = friendId,
                    IsAccepted = false
                });
                _dbContext.SaveChanges();
                return Ok();
            }

            var friend = _dbContext.Friends.FirstOrDefault(f => (f.FriendId == friendId && f.UserId == userId)
                                                    || (f.UserId == friendId && f.FriendId == userId));

            if (friend.IsAccepted)
            {
                ModelState.AddModelError("exist", "You are already friends");
            }
            else {
                ModelState.AddModelError("exist", "You are already requested");
            }
            
            return BadRequest(ModelState);
            
        }

        /// <summary>
        /// Запросы в друзья мне
        /// </summary>
        /// /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpGet("requests")]
        [ProducesResponseType(typeof(FriendModel),200)]
        public IActionResult FriendRequests()
        {
            
            var userId = User.GetUserId();
            var friends = _dbContext.Friends.Where(f => f.FriendId == userId && !f.IsAccepted).Select(f => new FriendModel
            {
                Id = f.UserId,
                Nickname = _dbContext.Users.FirstOrDefault(u => u.Id == f.UserId).NickName,
                PhoneNumber = _dbContext.Users.FirstOrDefault(u => u.Id == f.UserId).PhoneNumber,
                TotalScore = _dbContext.Users.FirstOrDefault(u => u.Id == f.UserId).TotalScore,
                PhotoUrl = _dbContext.Users.FirstOrDefault(u => u.Id == f.UserId).PhotoUrl,
                IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == f.UserId && mg.IsPlay),
                Raiting = _dbContext.UserCoins.Count(uc => uc.Coins > _dbContext.UserCoins.First(_uc => _uc.GamerId == f.FriendId).Coins) + 1,
                //_dbContext.Users.Count(ru => ru.TotalScore > 
                //_dbContext.Users.FirstOrDefault(u => u.Id == f.UserId).TotalScore) + 1,
                OneSignalId = _dbContext.Users.FirstOrDefault(u => u.Id == f.UserId).OneSignalId
            }).ToList();

            return Ok(friends);

        }


        /// <summary>
        /// Подтверждение запроса по ID
        /// </summary>
        /// /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpPost("accept/request/{id}")]
        [ProducesResponseType(200)]
        public IActionResult AcceptRequest([FromRoute]string id)
        {
            var userId = id;//it's not this user
            var thisUser = User.GetUserId();

            var request = _dbContext.Friends.FirstOrDefault(f => f.FriendId == thisUser && f.UserId == userId);

            request.IsAccepted = true;
            request.LoveStartedDate = DateTime.Now;
            _dbContext.SaveChanges();
            return Ok();
        }

    }
}