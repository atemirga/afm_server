using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobiGroup.AskMeFootball.Core.Game;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Models.Games;
using RobiGroup.Web.Common.Identity;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Игра
    /// </summary>
    [Route("api/game")]
    [ApiController]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly GamersHandler _gamersHandler;

        private readonly IGameManager _gameManager;

        public GameController(GamersHandler gamersHandler, IGameManager gameManager)
        {
            _gamersHandler = gamersHandler;
            _gameManager = gameManager;
        }

        /// <summary>
        /// Поиск соперника
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [ProducesResponseType(typeof(GameModel), 200)]
        public IActionResult Play(PlayModel model)
        {
            if (ModelState.IsValid)
            {
                var gameModel = _gameManager.TryStartGame(User.GetUserId(), model.CardId);

                return Ok(gameModel);
            }

            return BadRequest(ModelState);
        }
    }
}