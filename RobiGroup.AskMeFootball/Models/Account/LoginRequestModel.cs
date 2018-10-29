using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Models.Account
{
    public class LoginRequestModel
    {
        /// <summary>
        /// Номер телефона
        /// </summary>
        [Required]
        public string Phone { get; set; }
    }
}