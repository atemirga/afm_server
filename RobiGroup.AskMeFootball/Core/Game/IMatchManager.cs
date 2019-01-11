using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Match;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public interface IMatchManager
    {
        Task<MatchSearchResultModel> SearchMatch(string gamerId, int cardId);

        Task<MatchSearchResultModel> RequestMatch(string gamerId, string rivalId, int cardId);

        Task<ConfirmResponseModel> Confirm(string gamerId, int matchId);
        
        Task<MatchResultModel> GetMatchResult(int id, string userId);

        Task<bool> GetQuestionAnswerStatus(int id, string userId, int questionId);
    }
}