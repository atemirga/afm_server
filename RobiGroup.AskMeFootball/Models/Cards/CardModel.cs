using System;
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

        public DateTime ResetTime
        {
            get => _resetTime;
            set
            {
                _resetTime = value;
                RemainingTime = _resetTime - DateTime.Now;
            }
        }

        public TimeSpan RemainingTime { get; set; }
    }
}