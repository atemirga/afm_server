using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Models.Games;

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

        public GameController(GamersHandler gamersHandler)
        {
            _gamersHandler = gamersHandler;
        }

        /// <summary>
        /// Начать игру
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("play")]
        [ProducesResponseType(typeof(GameModel), 200)]
        public IActionResult Play(PlayModel model)
        {
            return Ok();
        }
    }
}