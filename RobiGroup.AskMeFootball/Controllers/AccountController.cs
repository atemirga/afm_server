using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RobiGroup.AskMeFootball.Common.Localization;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Account;
using RobiGroup.AskMeFootball.Models.Account.Profile;
using RobiGroup.Web.Common;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using RobiGroup.Web.Common.Services.Models;
using RobiGroup.Web.Common.Validation;

namespace RobiGroup.AskMeFootball.Controllers
{

    [Route("api/account")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private IStringLocalizer<Resources> _localizer;

        public AccountController(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IStringLocalizer<Resources> localizer)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
        }

        #region Auth

        /// <summary>
        /// Получить токен
        /// </summary>
        /// <param name="username">Имя пользователя / номер телефона</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/token")]
        [ProducesResponseType(typeof(UserTokenModel), 200)]
        public async Task<IActionResult> GetAuthTokenForm([Required]string username, [Required]string password)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser>>();
                    return Ok(await authService.AuthenticateAsync(username, password));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(String.Empty, e.Message);
                    return BadRequest(ModelState);
                }
            }

            return BadRequest(ModelState);
        }


        /// <summary>
        /// Получить токен
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("/token")]
        [ProducesResponseType(typeof(UserTokenModel), 200)]
        public async Task<IActionResult> GetAuthToken(TokenRequestModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser>>();
                    return Ok(await authService.AuthenticateAsync(model.Username, model.Password));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(String.Empty, e.Message);
                    return BadRequest(ModelState);
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Вход в систему через номер телефона
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [ProducesResponseType(typeof(LoginResponseModel), 200)]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            if (ModelState.IsValid)
            {
                if (!Regex.IsMatch(model.Phone, ValidationPatternConstants.PhoneNumber))
                {
                    ModelState.AddModelError(nameof(model.Phone).FirstCharToLower(), _localizer["Register_PhoneNumber_Invalid"]);
                    return BadRequest(ModelState);
                }

                model.Phone = FormatHelpers.NormalizePhoneNumber(model.Phone);

                var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.PhoneNumber == model.Phone);

                if (user != null)
                {
                    await SendConfirmationCode(user, model.Phone);

                    return Ok(new LoginResponseModel { Action = LoginAction.ConfirmPhone });
                    /*
                    if (!await _userManager.HasPasswordAsync(user))
                    {
                        await _userManager.DeleteAsync(user); 
                    }
                    else
                    {
                        var action = LoginAction.RequestToken;

                        if (!user.PhoneNumberConfirmed)
                        {
                            action = LoginAction.ConfirmPhone;
                            await SendConfirmationCode(user, model.Phone);
                        }

                        return Ok(new LoginResponseModel { Action = action });
                    }*/
                }

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    user = new ApplicationUser
                    {
                        NickName = "player" + (_dbContext.Users.Count() + 1),
                        UserName = model.Phone,
                        PhoneNumber = model.Phone
                    };
                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, ApplicationRoles.Gamer);

                        if (result.Succeeded)
                        {
                            transaction.Commit();
                            return await SendConfirmationCode(user, model.Phone);
                        }
                        else
                        {
                            transaction.Rollback();

                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(nameof(model.Phone).FirstCharToLower(), error.Description);
                            }
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(nameof(model.Phone).FirstCharToLower(), error.Description);
                        }
                    }
                }
            }

            return BadRequest(ModelState);
        }

        private async Task<IActionResult> SendConfirmationCode(ApplicationUser user, string phone)
        {
            var smsSender = HttpContext.RequestServices.GetService<ISmsSender>();
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phone);
            try
            {
                await smsSender.SendSmsAsync(phone,
                    string.Format(_localizer["Register_Phone_Confirmation_Code_Sms"], code));
                return Ok(new LoginResponseModel { Action = LoginAction.ConfirmPhone });
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(phone), e.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Подтверждение номера телефона 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("confirm")]
        [ProducesResponseType(typeof(UserTokenModel), 200)]
        public async Task<IActionResult> ConfirmPhone(ConfirmPhoneModel model)
        {
            if (ModelState.IsValid)
            {
                string normalizedPhone = FormatHelpers.NormalizePhoneNumber(model.Phone);
                var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);

                if (user != null)
                {
                    var result = await _userManager.ChangePhoneNumberAsync(user, normalizedPhone, model.Code);
                    if (result.Succeeded)
                    {
                        var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser>>();
                        return Ok(await authService.GenerateTokenAsync(user));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(nameof(model.Code).FirstCharToLower(), error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Phone).FirstCharToLower(), _localizer["Register_User_With_Phone_Not_Exists", normalizedPhone]);
                }
            }

            // If we got this far, something failed, redisplay the form
            // ModelState.AddModelError(nameof(model.Code), _localizer["Register_Confirmation_Code_Invalid"]);
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Создать пароль
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("password")]
        [Authorize]
        public async Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user != null)
                {
                    var result = await _userManager.AddPasswordAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser>>();
                        return Ok(await authService.GenerateTokenAsync(await _userManager.FindByIdAsync(user.Id)));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(nameof(model.Password).FirstCharToLower(), error.Description);
                    }
                }
            }

            return BadRequest(ModelState);
        }

        #endregion
    }
}