using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RobiGroup.AskMeFootball.Areas.Admin.Models.InfoCards
{
    public class InfoCardCreateModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("Названия")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Второе название")]
        public string Subtitle { get; set; }

        [Required]
        [DisplayName("Текст Кнопки")]
        public string ButtonTitle { get; set; }

        [DisplayName("Фотки карусель")]
        public List<string> Images { get; set; }

        [DisplayName("Фотки карусель")]
        public List<IFormFile> ImageFiles { get; set; }

        [DisplayName("Фото")]
        public string ImageUrl { get; set; }

        public IFormFile VideoFile { get; set; }

        public string VideoUrl { get; set; }

        [Required]
        [DisplayName("Заканчивается")]
        public DateTime EndDate { get; set; }


        [Required]
        [DisplayName("Активный")]
        public int IsActive { get; set; }

        
    }
}
