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
using RobiGroup.Web.Common.Identity;

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

        /// <summary>
        /// Список друзей
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(List<FriendModel>), 200)]
        public IActionResult GetAll(string[] phones)
        {
            var friends = _dbContext.Users.Where(u => phones.Contains(u.PhoneNumber)).Select(u => new FriendModel
            {
                Id = u.Id,
                Nickname = u.NickName,
                PhoneNumber = u.PhoneNumber,
                TotalScore = u.TotalScore,
                PhotoUrl = u.PhotoUrl,
                IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == u.Id && mg.IsPlay),
                Raiting = _dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,
            }).ToList();

            foreach (var gamer in friends)
            {
                gamer.IsOnline = _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
            }

            return Ok(friends);
        }
    }
}