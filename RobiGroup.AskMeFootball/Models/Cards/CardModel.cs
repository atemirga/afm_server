using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Models.Cards
{
    public class CardModel
    {
        private DateTime _resetTime;
        public int Id { get; set; }

        public string Name { get; set; }

        public string Prize { get; set; }

        public List<CardInfo> Info { get; set; }

        public string ImageUrlCard { get; set; }
        public string ImageUrlDetail { get; set; }
        public string ImageUrlSelect { get; set; }

        public int InterestedCount { get; set; }


        public string[] InterestedTopPhotoUrls { get; set; }

        public DateTime ResetTime
        {
            get => _resetTime;//.AddHours(24);
            set
            {
                _resetTime = value;
                RemainingTime = DateTime.Now - _resetTime;
            }
        }

        public bool IsTwoH { get; set; }

        public bool IsHalfH { get; set; }

        public DateTime ServerTime => DateTime.Now;

        public TimeSpan RemainingTime { get; set; }

        public string Type { get; set; }

        public DateTime StartTime { get; set; }


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

        public string GamerPhotoUrl { get; set; }
    }

    public class UserCoinsModel
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int Coins { get; set; }

        public DateTime LastUpdate { get; set; }
    }

    public class PointHistoriesModel
    {
        public int Id { get; set; }

        public string GamerId { get; set; }

        public int Point { get; set; }

        public DateTime TimeAdded { get; set; }
    }
}