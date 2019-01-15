using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Cards;
using RobiGroup.AskMeFootball.Models.Leaderboard;
using RobiGroup.AskMeFootball.Services;

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
        private readonly GamersHandler _gamersHandler;
        private readonly ICardService _cardService;

        public CardsController(ApplicationDbContext dbContext, GamersHandler gamersHandler, ICardService cardService)
        {
            _dbContext = dbContext;
            _gamersHandler = gamersHandler;
            _cardService = cardService;
        }

        /// <summary>
        /// Список карточек
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CardModel>), 200)]
        public IActionResult GetAll()
        {
            var cards = _dbContext.Cards.Include(c => c.GamerCards).Select(c => new CardModel
            {
                Id = c.Id,
                Name = c.Name,
                Prize = c.Prize,
                ImageUrl = c.ImageUrl,
                ResetTime = c.ResetTime,
                InterestedCount = c.GamerCards.Count()
            }).ToList();

            foreach (var card in cards)
            {
                card.InterestedTopPhotoUrls = _cardService.GetLeaderboard(card.Id).Take(3).ToList().Select(m => m.PhotoUrl).ToArray();
            }

            return Ok(cards);
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
        /// <param name="id">ID карты</param>
        /// <param name="page">Страница</param>
        /// <param name="count">Количество записей на странице</param>
        /// <returns></returns>
        [HttpGet("{id}/leaderboard")]
        [ProducesResponseType(typeof(List<LeaderboardCardGamerModel>), 200)]
        public IActionResult GetLeaderboard(int id, int page = 1, int count = 10)
        {
           var gamers = _cardService.GetLeaderboard(id).Skip((page - 1) * count).Take(count).ToList();

            foreach (var gamer in gamers)
            {
                gamer.IsOnline = gamer.IsBot || _gamersHandler.WebSocketConnectionManager.Groups.Keys.Contains(gamer.Id);
            }

            return Ok(gamers);
        }

        /// <summary>
        /// Победители
        /// </summary>
        /// <param name="id">ID карты</param>
        /// <param name="page">Страница</param>
        /// <param name="count">Количество записей на странице</param>
        /// <returns></returns>
        [HttpGet("{id}/winners")]
        [ProducesResponseType(typeof(List<LeaderboardCardGamerModel>), 200)]
        public IActionResult GetWinners(int id, int page = 1, int count = 10)
        {
            var gamers = _cardService.GetWinners(id).Skip((page - 1) * count).Take(count).ToList();

            return Ok(gamers);
        }


        /// <summary>
        /// Последние победители
        /// </summary>
        /// <param name="id">ID карты</param>
        /// <returns></returns>
        [HttpGet("{id}/winners/last")]
        [ProducesResponseType(typeof(List<LeaderboardCardGamerModel>), 200)]
        public IActionResult GetLastWinners(int id)
        {
            var cardResetTime = _dbContext.CardWinners.Where(w => w.CardId == id).OrderByDescending(w => w.CardEndTime).Select(w => w.CardEndTime).FirstOrDefault();
            var gamers = _cardService.GetWinners(id).Where(w => w.CardEndTime == cardResetTime);

            return Ok(gamers);
        }
    }
}