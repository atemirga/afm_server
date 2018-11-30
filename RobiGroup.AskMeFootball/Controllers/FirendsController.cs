using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public FirendsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Список друзей
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(List<FriendModel>), 200)]
        public IActionResult GetAll(string[] phones)
        {
            return Ok(_dbContext.Users.Where(u => phones.Contains(u.PhoneNumber)).Select(u => new FriendModel
            {
                Id = u.Id,
                Nickname = u.NickName,
                PhoneNumber = u.PhoneNumber,
                TotalScore = u.TotalScore
            }).ToList());
        }
    }
}