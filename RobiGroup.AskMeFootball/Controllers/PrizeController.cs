using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models;
using RobiGroup.AskMeFootball.Services;
using RobiGroup.AskMeFootball.Models.Prize;
using RobiGroup.Web.Common;
using RobiGroup.Web.Common.Identity;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Призы
    /// </summary>
    [Route("api/prize")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PrizeController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly GamersHandler _gamersHandler;
        private readonly ICardService _cardService;

        public PrizeController(ApplicationDbContext dbContext, GamersHandler gamersHandler, ICardService cardService)
        {
            _dbContext = dbContext;
            _gamersHandler = gamersHandler;
            _cardService = cardService;
        }

        /// <summary>
        /// Список призов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<PrizeModel>), 200)]
        public IActionResult GetAll()
        {
            var prizes = _dbContext.Prizes.Select(p => new PrizeModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                FirstPhotoUrl = p.FirstPhotoUrl,
                SecondPhotoUrl = p.SecondPhotoUrl,
                ThirdPhotoUrl = p.ThirdPhotoUrl,
                InStock = p.InStock,
                Address = p.Address,
                Date = p.Date.AddHours(6),
                Site = p.Site,
                Facebook = p.Facebook,
                Instagram = p.Instagram,
                Twitter = p.Twitter,
                Vkontakte = p.Vkontakte,
                FirstPhoneNumber = p.FirstPhoneNumber,
                //SecondPhoneNumber = p.SecondPhoneNumber
            }).ToList();

            return Ok(prizes);
        }


        /// <summary>
        /// Приз по ID
        /// </summary>
        /// <param name="id">ID приза</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PrizeModel), 200)]
        public IActionResult GetById(int id)
        {
            return Ok(_dbContext.Prizes.Find(id));
        }


        /// <summary>
        /// Профиль приза по ID
        /// </summary>
        /// <param name="id">ID приза</param>
        /// <returns></returns>
        [HttpGet("{id}/profile")]
        [ProducesResponseType(typeof(PrizeModel), 200)]
        public IActionResult GetProfile(int id)
        {
            var prize = _dbContext.Prizes.FirstOrDefault(p => p.Id == id);
            return Ok(new PrizeModel {
                Id = id,
                Name = prize.Name,
                Description = prize.Description,
                FirstPhotoUrl = prize.FirstPhotoUrl,
                SecondPhotoUrl = prize.SecondPhotoUrl,
                ThirdPhotoUrl = prize.ThirdPhotoUrl,
                Price = prize.Price,
                InStock = prize.InStock,
                Address = prize.Address,
                Date = prize.Date.AddHours(6),
                FirstPhoneNumber = prize.FirstPhoneNumber,
                //SecondPhoneNumber = prize.SecondPhoneNumber,
                Site = prize.Site,
                Facebook = prize.Facebook,
                Instagram = prize.Instagram,
                Twitter = prize.Twitter,
                Vkontakte = prize.Vkontakte,
            });
        }


        /// <summary>
        /// Покупка приза
        /// </summary>
        /// <param name="id">ID приза</param>
        /// <returns></returns>
        [HttpGet("{id}/buy")]
        [ProducesResponseType(typeof(PrizeModel), 200)]
        public IActionResult BuyPrize(int id)
        {
            var prize = _dbContext.Prizes.FirstOrDefault(p => p.Id == id);
            var gamerId = User.GetUserId();
            var lang = _dbContext.Users.FirstOrDefault(u => u.Id == gamerId).Lang;
            if (prize != null)
            {
                if (prize.InStock > 0)
                {
                    var userCoins = 0;
                    
                    var user = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == gamerId);
                    if (user != null)
                    {
                        userCoins = user.Coins;
                    }

                    var price = _dbContext.Prizes.FirstOrDefault(p => p.Id == id).Price;

                    if (price < userCoins)
                    {
                        user.Coins -= price;
                        prize.InStock -= 1;

                        Random random = new Random();
                        
                        var code = random.Next(10000000, 99999999);

                        _dbContext.PrizeBuyHistories.Add(new PrizeBuyHistory
                        {
                            GamerId = user.GamerId,
                            PrizeId = prize.Id,
                            Price = price,
                            BuyDate = DateTime.Now,
                            Code = code,
                            IsActive = true,
                        });
                        
                        _dbContext.SaveChanges();

                        return Ok(_dbContext.Prizes.Find(id));
                    }

                    var coinError = string.Empty;
                    switch (lang)
                    {
                        case "en":
                            coinError = "Not enough coins to buy";
                            break;
                        case "ru":
                            coinError = "Недостаточно монет чтобы купить";
                            break;
                    }
                    ModelState.AddModelError("error", coinError);
                    return BadRequest(ModelState);
                }

                var prizeError = string.Empty;
                switch (lang)
                {
                    case "en":
                        prizeError = "Prize not available";
                        break;
                    case "ru":
                        prizeError = "Приза нет в наличии";
                        break;
                }
                ModelState.AddModelError("error", prizeError);
                return BadRequest(ModelState);
            }

            var prizeExistError = string.Empty;
            switch (lang)
            {
                case "en":
                    prizeExistError = "Prize doesn't exist";
                    break;
                case "ru":
                    prizeExistError = "Такого приза не существует";
                    break;
            }

            ModelState.AddModelError("error", prizeExistError);

            return BadRequest(ModelState);
        }


        /// <summary>
        /// Забрать приз
        /// </summary>
        /// <param name="code">Код заказа</param>
        /// <returns></returns>
        [HttpGet("activate/{code}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public IActionResult ActivateCode(int code)
        {
            var message = "Код неправильный или не существует!";

            var error = new ErrorMessage();

            var order = _dbContext.PrizeBuyHistories.FirstOrDefault(pbh => pbh.Code == code);
            if (order != null)
            {
                if (order.IsActive)
                {
                    order.IsActive = false;
                    _dbContext.SaveChanges();
                    return Ok();
                }
                message = "Уже активирован";
                error.error = message;
                return BadRequest(error);
            }
            error.error = message;
            return BadRequest(error);
        }

        /// <summary>
        ///Заказы
        /// </summary>
        /// <returns></returns>
        [HttpGet("orders")]
        [ProducesResponseType(typeof(List<PrizeModel>), 200)]
        public IActionResult Orders()
        {
            var error = String.Empty;
            var gamerId = User.GetUserId();
            var orders = new List<PrizeModel>();
            var histories = _dbContext.PrizeBuyHistories.Where(o => o.GamerId == gamerId);

            if (histories.ToList().Count > 0)
            {
                foreach (var h in histories)
                {
                    var prize = _dbContext.Prizes.FirstOrDefault(p => p.Id == h.PrizeId);

                    var pm = new PrizeModel();
                    pm.Id = prize.Id;
                    pm.Address = prize.Address;
                    pm.Date = prize.Date.AddHours(6);
                    pm.Description = prize.Description;
                    pm.Facebook = prize.Facebook;
                    pm.FirstPhoneNumber = prize.FirstPhoneNumber;
                    pm.FirstPhotoUrl = prize.FirstPhotoUrl;
                    pm.Instagram = prize.Instagram;
                    pm.InStock = prize.InStock;
                    pm.Name = prize.Name;
                    pm.Price = h.Price;
                    pm.SecondPhotoUrl = prize.SecondPhotoUrl;
                    pm.Site = prize.Site;
                    pm.ThirdPhotoUrl = prize.ThirdPhotoUrl;
                    pm.Twitter = prize.Twitter;
                    pm.Vkontakte = prize.Vkontakte;
                    pm.Code = h.Code;
                    pm.IsActive = h.IsActive;

                    orders.Add(pm);
                }
                return Ok(orders);
            }

            error = "Нет покупок!";
            ModelState.AddModelError("error", error);
            return BadRequest(ModelState);
        }

    }
}