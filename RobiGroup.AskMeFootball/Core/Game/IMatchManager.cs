using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Match;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public interface IMatchManager
    {
        Task<MatchSearchResultModel> SearchMatch(string gamerId, int cardId);

        Task<ConfirmResponseModel> Confirm(string gamerId, int matchId);
    }
}