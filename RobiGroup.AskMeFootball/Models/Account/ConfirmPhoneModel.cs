using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Models.Account
{
    public class ConfirmPhoneModel
    {
        /// <summary>
        /// Номер телефона
        /// </summary>
        [Required]
        public string Phone { get; set; }

        /// <summary>
        /// Код подтверждение
        /// </summary>
        [Required]
        public string Code { get; set; }

        /*
        /// <summary>
        /// One Signal ID
        /// </summary>
        [Required]
        public string OneSignalId { get; set; }
        */
    }
}