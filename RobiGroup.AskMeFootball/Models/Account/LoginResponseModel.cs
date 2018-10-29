using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RobiGroup.AskMeFootball.Models.Account
{
    public class LoginResponseModel
    {
        /// <summary>
        /// Действие пользователя, 
        /// если <see cref="Action"/> равно <see cref="LoginAction.RequestToken"/> то пользователь существует в системе и можно запросить токен,
        /// если <see cref="Action"/> равно <see cref="LoginAction.ConfirmPhone"/> то пользователь не существует в системе и нужно подтвердить номер телефона.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LoginAction Action { get; set; }
    }

    public enum LoginAction
    {
        RequestToken, ConfirmPhone
    }
}