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
        [HttpGet]
        [ProducesResponseType(typeof(List<FriendModel>), 200)]
        public IActionResult GetAll()
        {
            string userId = User.GetUserId();
            return Ok(_dbContext.Friends.Include(f => f.FriendUser).Where(f => f.GamerId == userId).Select(f => new FriendModel
            {
                Id = f.FriendId,
                Nickname = f.FriendUser.NickName,
                PhoneNumber = f.FriendUser.PhoneNumber,
                CreatedAt = f.CreatedAt,
                TotalScore = f.FriendUser.TotalScore
            }).ToList());
        }

        /// <summary>
        /// Получить карточку по ID
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CardModel), 200)]
        public IActionResult GetById(int id)
        {
            return Ok(_dbContext.Cards.Find(id));
        }
    }
}