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
                ImageUrlCard = c.ImageUrlCard,
                ImageUrlDetail = c.ImageUrlDetail,
                ImageUrlSelect = c.ImageUrlSelect,
                ResetTime = c.ResetTime,//.AddHours(24),
                InterestedCount = c.GamerCards.Count(gc => gc.IsActive && _dbContext.Users.Any(u => u.Id == gc.GamerId && u.Bot == 0)),
                Info = _dbContext.CardInfos.Where(ci => ci.CardId == c.Id).ToList(),
                Type = c.Type.Code,
               
            }).ToList();

            foreach (var card in cards)
            {
                if (card.Type == "Live")
                {
                    card.StartTime = _dbContext.Matches.Any(m => m.CardId == card.Id) ? Convert.ToDateTime(_dbContext.Matches.Last(m => m.CardId == card.Id).StartTime) : Convert.ToDateTime("01/01/0001");
                }
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
            var _card = _dbContext.Cards.FirstOrDefault(c => c.Id == id);
            var card = new CardModel();
            card.Id = id;
            card.Name = _card.Name;
            card.Prize = _card.Prize;
            card.ImageUrlCard = _card.ImageUrlCard;
            card.ImageUrlDetail = _card.ImageUrlDetail;
            card.ImageUrlSelect = _card.ImageUrlSelect;
            card.ResetTime = _card.ResetTime;//.AddHours(24),
            card.InterestedCount = _card.GamerCards.Any(gc => gc.IsActive && _dbContext.Users.Any(u => u.Id == gc.GamerId && u.Bot == 0)) ?
                                _card.GamerCards.Count(gc => gc.IsActive && _dbContext.Users.Any(u => u.Id == gc.GamerId && u.Bot == 0)) : 0;
            card.Info = _dbContext.CardInfos.Where(ci => ci.CardId == id).ToList();
            card.Type = _card.Type.Code;
            if (_card.Type.Code == "Live" || _card.Type.Code == "HalfTime")
            {
                card.StartTime = Convert.ToDateTime(_dbContext.Matches.Last(m => m.CardId == id).StartTime);
            }

            return Ok(card);
            //return Ok(_dbContext.Cards.Find(id));
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


        /// <summary>
        /// Инфо карты
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="count">Количество записей на странице</param>
        /// <returns></returns>
        [HttpGet("info-cards")]
        [ProducesResponseType(typeof(List<InfoCardModel>), 200)]
        public IActionResult InfoCards(int page = 1, int count = 10)
        {
            return Ok(from u in _dbContext.InfoCards
                      where u.IsActive
                      select new InfoCardModel
                      {
                          Id = u.Id,
                          ButtonTitle = u.ButtonTitle,
                          EndTime = u.EndTime,
                          Images = _dbContext.InfoCardImages.Where(ici => ici.InfoCardId == u.Id).Select(s => s.Url).ToList(),
                          ImageUrl = u.ImageUrl,
                          IsActive = u.IsActive,
                          SubTitle =u.SubTitle,
                          Title = u.Title,
                          VideoUrl = u.VideoUrl,

                      });
        }
    }
}