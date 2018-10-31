using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Cards;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Карты
    /// </summary>
    [Route("api/cards")]
    [ApiController]
    [Authorize]
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


    }
}