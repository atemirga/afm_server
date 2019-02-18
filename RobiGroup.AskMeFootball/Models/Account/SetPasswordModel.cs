using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Models.Account
{
    public class SetPasswordModel
    {
        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        [Compare("Password", ErrorMessage = "Пароль и подтверждение пароля не совпадают")]
        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}