using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public LeaderboardController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Глобальный
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<LeaderboardGamerModel>), 200)]
        public IActionResult GetGlobal()
        {
            var gamers = (from u in _dbContext.Users
                select new LeaderboardGamerModel
                {
                    Id = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    Nickname = u.NickName,
                    CurrentScore = u.Score,
                    TotalScore = u.TotalScore,
                    Raiting = _dbContext.Users.Count(ru => ru.TotalScore > u.TotalScore) + 1,
                });

            return Ok(gamers);
        }
    }
}