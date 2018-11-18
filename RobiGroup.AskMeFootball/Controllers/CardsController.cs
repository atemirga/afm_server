using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Cards;
using RobiGroup.AskMeFootball.Models.Leaderboard;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Карты
    /// </summary>
    [Route("api/cards")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CardsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CardsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Список карточек
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CardModel>), 200)]
        public IActionResult GetAll()
        {
            return Ok(_dbContext.Cards.Select(c => new CardModel
            {
                Id = c.Id,
                Name = c.Name,
                Prize = c.Prize,
                ImageUrl = c.ImageUrl,
                ResetTime = c.ResetTime
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

        /// <summary>
        /// Leaderboard
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/leaderboard")]
        [ProducesResponseType(typeof(List<LeaderboardCardGamerModel>), 200)]
        public IActionResult GetLeaderboard(int id)
        {
           var gamers = (from gc in _dbContext.GamerCards
                join u in _dbContext.Users on gc.GamerId equals u.Id
                where gc.CardId == id
                select new LeaderboardCardGamerModel
                {
                    Id = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    Nickname = u.NickName,
                    CardScore = gc.Score,
                    CurrentScore = u.Score,
                    TotalScore = u.TotalScore
                });

            return Ok(gamers);
        }

    }
}