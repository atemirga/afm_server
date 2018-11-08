using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Match;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public class MatchManager : IMatchManager
    {
        private GamersHandler _gamersHandler;
        private readonly ApplicationDbContext _dbContext;

        public MatchManager(GamersHandler gamersHandler, ApplicationDbContext dbContext)
        {
            _gamersHandler = gamersHandler;
            _dbContext = dbContext;
        }

        public async Task<MatchSearchResultModel> SearchMatch(string gamerId, int cardId)
        {
            var enemy = _gamersHandler.WebSocketConnectionManager.Connections.Values.Where(c => !c.Away && !c.IsBusy && c.UserId != gamerId).OrderByDescending(c => c.ConnectedTime).FirstOrDefault();
            var model = new MatchSearchResultModel();

            if (enemy != null)
            {
                var match = new Match()
                {
                    CardId = cardId,
                    CreateTime = DateTime.Now,
                };
                _dbContext.Matches.Add(match);
                _dbContext.SaveChanges();
                
                _dbContext.MatchGamers.Add(new MatchGamer
                {
                    MatchId = match.Id,
                    GamerId = gamerId
                });
                _dbContext.MatchGamers.Add(new MatchGamer
                {
                    MatchId = match.Id,
                    GamerId = enemy.UserId
                });
                _dbContext.SaveChanges();

                await _gamersHandler.InvokeClientMethodToGroupAsync(enemy.UserId, "matchRequest", new MatchModel(match.Id, _dbContext.Users.Find(gamerId)));

                model.Match = new MatchModel(match.Id, _dbContext.Users.Find(enemy.UserId));
                model.Found = true;
            }

            return model;
        }

        public async Task<ConfirmResponseModel> Confirm(string gamerId, int matchId)
        {
            var matchParticipant = _dbContext.MatchGamers.FirstOrDefault(m =>
                m.GamerId == gamerId && m.MatchId == matchId && !m.JoinTime.HasValue);

            if (matchParticipant != null)
            {
                var matchParticipants = _dbContext.MatchGamers.Where(p => p.MatchId == matchId);

                if (!matchParticipant.Confirmed)
                {
                    matchParticipant.Confirmed = true;
                    matchParticipant.JoinTime = DateTime.Now;
                    matchParticipant.IsPlay = true;

                    _dbContext.SaveChanges();

                    var match = _dbContext.Matches.Include(m => m.Card).ThenInclude(c => c.Questions).Single(m => m.Id == matchId);
                    match.StartTime = DateTime.Now;
                    match.Questions = string.Join(',', match.Card.Questions
                        .OrderBy(o => Guid.NewGuid())
                        .Take(match.Card.MatchQuestions)
                        .Select(q => q.Id));
                    _dbContext.SaveChanges();
                }

                var confirm = new ConfirmResponseModel
                {
                    MatchId = matchId,
                    Confirmed = matchParticipants.All(p => p.Confirmed)
                };

                var participants = matchParticipants.Where(p => p.GamerId != gamerId).Select(p => p.GamerId).ToList();

                foreach (var participant in participants)
                {
                    await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchConfirmed", confirm);
                }

                return confirm;
            }

            throw new Exception("Match not found.");
        }
    }
}