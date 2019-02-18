using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Models.Account
{
    public class TokenRequestModel
    {
        /// <summary>
        /// Имя пользователя / номер телефона
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}