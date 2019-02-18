using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RobiGroup.AskMeFootball.Data
{
    public class CardType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }
}