using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Account.Profile;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;

namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Профайл
    /// </summary>
    [Route("api/account/profile")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfileController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Профайл
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserProfileModel), 200)]
        public IActionResult Get()
        {
            var userId = User.GetUserId();

            var user = _dbContext.Users.Include(u => u.Rank).Single(u => u.Id == userId);

            return Ok(new UserProfileModel
            {
                Id = user.Id,
                Username = user.UserName,
                Nickname = user.NickName,
                Rank = user.Rank?.Name,
                Score = user.Score,
                TotalScore = user.TotalScore,
                PhotoUrl = user.PhotoUrl,
                Raiting = _dbContext.Users.Count(u => u.TotalScore > user.TotalScore) + 1,
            });
        }

        /// <summary>
        /// Изменить имя
        /// </summary>
        /// <param name="nickname">Имя</param>
        /// <returns></returns>
        [HttpPost("nickname/{nickname}")]
        [ProducesResponseType(200)]
        public IActionResult SetNickname([FromRoute]string nickname)
        {
            var userId = User.GetUserId();
            var user = _dbContext.Users.Include(u => u.Rank).Single(u => u.Id == userId);
            user.NickName = nickname;
            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpPost("photo")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SetPhoto()
        {
            var files = HttpContext.Request.Form.Files;

            if (files.Count == 0)
            {
                return BadRequest();
            }

            var userId = User.GetUserId();
            var user = _dbContext.Users.Find(userId);

            var fileService = HttpContext.RequestServices.GetService<IFileService>();
            var hostingEnvironment = HttpContext.RequestServices.GetService<IHostingEnvironment>();

            var photoPath = await fileService.Save(files[0], $"data/user/{user.Id}/profile");

            user.PhotoUrl = Path.GetRelativePath(hostingEnvironment.WebRootPath, photoPath);
            _dbContext.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Статистика
        /// </summary>
        /// <returns></returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(UserProfileModel), 200)]
        public IActionResult GetStatistics()
        {
            var userId = User.GetUserId();

            var matches = (from m in _dbContext.Matches
                           join mg in _dbContext.MatchGamers on m.Id equals mg.MatchId
                           where mg.GamerId == userId
                           select mg.IsWinner);

            return Ok(new ProfileStatisticsModel()
            {
                Id = userId,
                Wins = matches.Count(m => m),
                Losses = matches.Count(m => !m)
            });
        }
    }
}