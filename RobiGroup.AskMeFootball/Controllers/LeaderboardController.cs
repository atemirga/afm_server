using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using RobiGroup.Web.Common.Identity;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Leaderboard;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Leaderboard
    /// </summary>
    [Route("api/leaderboard")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LeaderboardController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly GamersHandler _gamersHandler;

        public LeaderboardController(ApplicationDbContext dbContext, GamersHandler gamersHandler)
        {
            _dbContext = dbContext;
            _gamersHandler = gamersHandler;
        }

        /// <summary>
        /// Глобальный
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="count">Количество записей на странице</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<LeaderboardGamerModel>), 200)]
        public async Task<IActionResult> GetGlobal(int page = 1, int count = 10)
        {
            var userId = User.GetUserId();
            var _gamer = _dbContext.MatchGamers.LastOrDefault(mg => mg.GamerId == userId && mg.IsPlay);
            if (_gamer != null)
            {
                _gamer.IsPlay = false;
                _dbContext.SaveChanges();
            }

            var gamers = (from u in _dbContext.Users
                          where u.Bot == 0 && u.UserName != "admin" && u.UserName != "77771112233"
                          select new LeaderboardGamerModel
                          {
                              Id = u.Id,
                              PhotoUrl = u.PhotoUrl,
                              Nickname = u.NickName,
                              CurrentScore = u.Score,
                              TotalScore = u.TotalScore,
                              IsBot = u.Bot > 0,
                              Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == u.Id) ?
                                       _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == u.Id).Coins : 0,
                              IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == u.Id && mg.IsPlay),
                              Raiting = _dbContext.UserCoins.Count(uc =>  _dbContext.Users.Any(_u => _u.Id == uc.GamerId && _u.Bot == 0)
                                        && uc.Coins > _dbContext.UserCoins.FirstOrDefault(_uc => _uc.GamerId == u.Id).Coins) + 1,
                              //_dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,

                          })
                .OrderBy(r => r.Raiting)
                .ThenBy(r => r.TotalScore)
                .ThenBy(r => r.CurrentScore)
                .Skip((page - 1) * count)
                .Take(count).ToList();


            //var sockets = _gamersHandler.WebSocketConnectionManager.GetAll();
            //foreach (var s in sockets)
            //{
            //    if (s.Value.WebSocket.State != WebSocketState.Open)
            //    {
            //        await _gamersHandler.OnDisconnected(s.Value.WebSocket);
            //    }
            //}
            
            


            foreach (var gamer in gamers)
            {
                if (gamer.IsPlaying)
                {
                    var matchGamer = _dbContext.MatchGamers.FirstOrDefault(mg => mg.GamerId == gamer.Id);
                    if (matchGamer != null)
                    {
                        var diffInSeconds = Math.Abs((DateTime.Now - Convert.ToDateTime(matchGamer.JoinTime)).TotalSeconds);
                        if (diffInSeconds > 120)
                        {
                            matchGamer.IsPlay = false;
                            _dbContext.SaveChanges();
                        }
                    }
                }
                
                gamer.IsOnline = gamer.IsBot || _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
            }

            return Ok(gamers.Take(100)); 
        }

        /// <summary>
        /// Друзья
        /// </summary>
        /// <returns></returns>
        [HttpGet("friends")]
        [ProducesResponseType(typeof(List<LeaderboardGamerModel>), 200)]
        public IActionResult GetFriends(string[] phones)
        {
            var gamers = (from u in _dbContext.Users
                where phones.Contains(u.PhoneNumber)
                select new LeaderboardGamerModel
                {
                    Id = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    Nickname = u.NickName, 
                    CurrentScore = u.Score,
                    TotalScore = u.TotalScore,
                    IsBot = u.Bot > 0,
                    IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == u.Id && mg.IsPlay),
                    Raiting = _dbContext.UserCoins.Count(uc => uc.Coins > _dbContext.UserCoins.First(_uc => _uc.GamerId == u.Id).Coins) + 1,//_dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,
                })
                .OrderBy(r => r.Raiting)
                .ThenBy(r => r.TotalScore)
                .ThenBy(r => r.CurrentScore)
                .ToList();

            foreach (var gamer in gamers)
            {

                //var webSocket = _gamersHandler.WebSocketConnectionManager.GetSocketById(gamer.Id);
                gamer.IsOnline = gamer.IsBot || _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
            }

            return Ok(gamers);
        }
    }
}