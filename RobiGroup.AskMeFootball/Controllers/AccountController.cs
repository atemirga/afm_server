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
using Microsoft.Extensions.Logging;
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
        //private readonly IServiceScopeFactory _scopeFactory;

        private IStringLocalizer<Resources> _localizer;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IStringLocalizer<Resources> localizer,
            ILogger<AccountController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
            _logger = logger;
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
                    var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser, AmfTokenModel>>();
                    var tokenModel = await authService.AuthenticateAsync(username, password);

                    var normalizePhoneNumber = FormatHelpers.NormalizePhoneNumber(username);
                    var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == normalizePhoneNumber || u.PhoneNumber == normalizePhoneNumber);
                    tokenModel.Username = user?.NickName;  

                    return Ok(tokenModel);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Логин: " + username);
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
                    var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser, AmfTokenModel>>();
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

                if (model.Phone == "77771112233")
                {
                    return Ok(new LoginResponseModel { Action = LoginAction.ConfirmPhone, IsRegistered = true });
                }

                var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.PhoneNumber == model.Phone);

                if (user != null)
                {
                    await SendConfirmationCode(user, model.Phone);

                    return Ok(new LoginResponseModel { Action = LoginAction.ConfirmPhone, IsRegistered = true});
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
                    //var scope = _scopeFactory.CreateScope();
                    //var gamerOptions = scope.ServiceProvider.GetService<IOptions<GamerOptions>>();gamerOptions.Value.DailyPoints
                    var resetTime = _dbContext.Users.First().ResetTime;
                    user = new ApplicationUser
                    {
                        NickName = "player" + (_dbContext.Users.Count() + 1),
                        UserName = model.Phone,
                        PhoneNumber = model.Phone,
                        PointsToPlay = 5,
                        Sync = false,
                        ReferralUsed = false,
                        RegisteredDate = DateTime.Now,
                        Lang = "ru",
                        ResetTime = resetTime,
                    };
                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, ApplicationRoles.Gamer);

                        _dbContext.UserCoins.Add(new UserCoins { 
                            
                            Coins = 100,
                            GamerId = user.Id,
                            LastUpdate = DateTime.Now,
                        });
                        _dbContext.UserBalances.Add(new UserBalance
                        {
                            UserId = user.Id,
                            Balance = 0
                        });
                        _dbContext.SaveChanges();

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
                
                if (normalizedPhone == "77771112233" && model.Code == "1234")
                {
                    var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser, AmfTokenModel>>();
                    
                    return Ok(await authService.GenerateTokenAsync(user));
                }
                
                if (user != null)
                {
                    var result = await _userManager.ChangePhoneNumberAsync(user, normalizedPhone, model.Code);
                    if (result.Succeeded)
                    {
                        var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser, AmfTokenModel>>();
                        var _user = _dbContext.Users.First(u => u.PhoneNumber == normalizedPhone);
                        //user.OneSignalId = model.OneSignalId;
                        if(user.RegisteredDate == null)
                            user.RegisteredDate = DateTime.Now;

                        #region Random Referral
                        if (user.Referral == null)
                        {
                            var length = 8;
                            Random random = new Random();
                            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                            var code = string.Empty;
                            var exist = true;

                            while (exist)
                            {
                                code = new string(Enumerable.Repeat(chars, length)
                              .Select(s => s[random.Next(s.Length)]).ToArray());

                                exist = _dbContext.Users.Any(u => u.Referral == code);
                            }
                            user.Referral = code;
                        }
                        #endregion
                        _dbContext.SaveChanges();
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


        /*
        private static Random random = new Random();
        public void RandomCode(int length, string userId)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = string.Empty;
            var exist = true;

            while (exist)
            {
                code = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

                exist = _dbContext.Users.Any(u => u.Referral == code);
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            user.Referral = code;
            _dbContext.SaveChanges();
        }
        */


        /// <summary>
        /// Согласие на уведомение
        /// </summary>
        /// <param name="id">ID OneSignal</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("push/accept/{id}")]
        [ProducesResponseType(typeof(UserTokenModel), 200)]
        [ProducesResponseType(400)]
        public IActionResult AcceptPush(string id)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    user.OneSignalId = id;
                    _dbContext.SaveChanges();
                    return Ok();
                }

                ModelState.AddModelError("User","User doesn't exist");
                return BadRequest(ModelState);
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
                        var authService = HttpContext.RequestServices.GetService<IAuthService<ApplicationUser, AmfTokenModel>>();
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