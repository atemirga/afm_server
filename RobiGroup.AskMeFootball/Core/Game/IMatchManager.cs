using System.Collections.Generic;
using System.Threading.Tasks;
using RobiGroup.AskMeFootball.Models.Match;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public interface IMatchManager
    {
        Task<MatchSearchResultModel> SearchMatch(string gamerId, int cardId, int bid);

        Task<CompetitiveMatchModel> CompetitiveMatch(string gamerId, int cardId);

        Task<HTMatchModel> HTMatch(string gamerId, int cardId);

        Task<LiveMatchModel> LiveMatch(string gamerId, int cardId);

        Task<MatchSearchResultModel> RequestMatch( string gamerId, int bid, string rivalId, int cardId);

        Task<ConfirmResponseModel> Confirm(string gamerId, int matchId);
        
        Task<MatchResultModel> GetMatchResult(int id, string userId);

        Task<LiveResultModel> LiveMatchResult(int id, string userId);

        Task<CompetitiveResultModel> CompetitiveMatchResult(int id, string userId);

        Task<bool> GetQuestionAnswerStatus(int id, string userId, int questionId);

        Task<List<int>> GetMissedQuestionsForMatch(int matchId, string gamerId);
    }
}