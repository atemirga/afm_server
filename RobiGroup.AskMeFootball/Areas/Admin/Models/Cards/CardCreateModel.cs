using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Cards
{
    public class CardCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("Названия")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Приз")]
        public string Prize { get; set; }

        [Required]
        [DisplayName("Картинка")]
        public string ImageUrl { get; set; }

        [Required]
        [DisplayName("Кол-во вопросов в игре")]
        public int MatchQuestions { get; set; }

        [Required]
        [DisplayName("Тип")]
        public int TypeId { get; set; }

        [Required]
        [DisplayName("Время сброса")]
        public DateTime ResetTime { get; set; }

        [Required]
        [DisplayName("Период сброса")]
        public int ResetPeriod { get; set; }
    }
}