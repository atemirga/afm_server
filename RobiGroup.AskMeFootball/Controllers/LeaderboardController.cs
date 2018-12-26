using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Cards;
using RobiGroup.AskMeFootball.Models.Leaderboard;

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
        public IActionResult GetGlobal(int page = 1, int count = 10)
        {
            var gamers = (from u in _dbContext.Users
                select new LeaderboardGamerModel
                {
                    Id = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    Nickname = u.NickName,
                    CurrentScore = u.Score,
                    TotalScore = u.TotalScore,
                    IsBot = u.Bot > 0,
                    Raiting = _dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,
                })
                .OrderBy(r => r.Raiting)
                .Skip((page - 1) * count)
                .Take(count).ToList();

            foreach (var gamer in gamers)
            {
                gamer.IsOnline = gamer.IsBot || _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
            }

            return Ok(gamers);
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
                    Raiting = _dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,
                })
                .OrderBy(r => r.Raiting)
                .ToList();

            foreach (var gamer in gamers)
            {
                gamer.IsOnline = gamer.IsBot || _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
            }

            return Ok(gamers);
        }
    }
}