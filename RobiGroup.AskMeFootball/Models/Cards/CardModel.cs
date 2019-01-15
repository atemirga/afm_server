using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RobiGroup.AskMeFootball.Models.Cards
{
    public class CardModel
    {
        private DateTime _resetTime;
        public int Id { get; set; }

        public string Name { get; set; }

        public string Prize { get; set; }

        public string ImageUrl { get; set; }

        public int InterestedCount { get; set; }

        public string[] InterestedTopPhotoUrls { get; set; }

        public DateTime ResetTime
        {
            get => _resetTime;
            set
            {
                _resetTime = value;
                RemainingTime = _resetTime - DateTime.Now;
            }
        }

        public DateTime ServerTime => DateTime.Now;

        public TimeSpan RemainingTime { get; set; }
    }

    public class CardWinnerModel
    {
        public int Id { get; set; }

        public string Prize { get; set; }

        public string GamerId { get; set; }

        public string GamerNickName { get; set; }

        public int GamerCardScore { get; set; }

        public DateTime CardStartTime { get; set; }

        public DateTime CardEndTime { get; set; }
    }
}