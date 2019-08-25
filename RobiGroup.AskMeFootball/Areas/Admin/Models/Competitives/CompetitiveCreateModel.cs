using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.Competitives
{
    public class CompetitiveCreateModel
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
        [DisplayName("Кол-во игроков в команде")]
        public int Gamers { get; set; }

        [Required]
        [DisplayName("Кол-во вопросов в игре")]
        public int MatchQuestions { get; set; }

        [Required]
        [DisplayName("Начало Матча")]
        public DateTime StartTime { get; set; }

        [Required]
        [DisplayName("Конец Матча")]
        public DateTime EndTime { get; set; }

        [Required]
        [DisplayName("Входная цена(мячи)")]
        public int EntryPoint { get; set; }
    }
}
