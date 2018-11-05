using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Match;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public interface IMatchManager
    {
        Task<MatchModel> SearchMatch(string gamerId, int cardId);

        Task<ConfirmResponseModel> Confirm(string gamerId, int matchId);
    }
}