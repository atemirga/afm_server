using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobiGroup.AskMeFootball.Data
{
    public class CardTeams
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public string FirstTeam { get; set; }
        public string SecondTeam { get; set; }
        public int FirstTeamScore { get; set; }
        public int SecondTeamScore { get; set; }
        public string FirstTeamLogo { get; set; }
        public string SecondTeamLogo { get; set; }
    }
}
