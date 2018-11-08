using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Account.Profile;
using RobiGroup.Web.Common.Identity;

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
                Username = user.UserName,
                Rank = user.Rank?.Name,
                Score = user.Score,
                TotalScore = user.TotalScore,
                PhotoUrl = user.PhotoUrl
            });
        }


    }
}