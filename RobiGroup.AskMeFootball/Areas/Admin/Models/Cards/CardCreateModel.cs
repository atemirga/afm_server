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
        [DisplayName("Картинка Карты")]
        public string ImageUrlCard { get; set; }

        [Required]
        [DisplayName("Картинка Деталь")]
        public string ImageUrlDetail { get; set; }

        [Required]
        [DisplayName("Картинка Выбор")]
        public string ImageUrlSelect { get; set; }

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
        [DisplayName("Начало Матча(Live-Half)")]
        public DateTime StartTime { get; set; }

        [Required]
        [DisplayName("Период сброса")]
        public int ResetPeriod { get; set; }

        [Required]
        [DisplayName("Максимальная ставка")]
        public int MaxBid { get; set; }

        [Required]
        [DisplayName("Входная цена(мячи)")]
        public int EntryPoint { get; set; }

        [Required]
        [DisplayName("Осталось 2 часа")]
        public int IsTwoH { get; set; }

        [Required]
        [DisplayName("Лимит Жизней")]
        public int Lifes { get; set; }

        [Required]
        [DisplayName("Лимит Подсказок")]
        public int Hints { get; set; }
    }
}