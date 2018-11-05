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

        public async Task<MatchModel> SearchMatch(string gamerId, int cardId)
        {
            var enemy = _gamersHandler.WebSocketConnectionManager.Connections.Values.Where(c => !c.Away && !c.IsBusy && c.UserId != gamerId).OrderByDescending(c => c.ConnectedTime).FirstOrDefault();
            var model = new MatchModel();

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

                var enemyUser = _dbContext.Users.Find(enemy.UserId);

                await _gamersHandler.InvokeClientMethodToGroupAsync(enemy.UserId, "matchRequest", new MatchRequest
                {
                    MatchId = match.Id,
                    GamerFullName = enemyUser.FullName,
                    GamerName = enemyUser.UserName,
                    GamerPhotoUrl = enemyUser.PhotoUrl
                });

                model.Id = match.Id;
                model.Found = true;
            }

            return model;
        }

        public async Task<ConfirmResponseModel> Confirm(string gamerId, int matchId)
        {
            var matchParticipant = _dbContext.MatchGamers.FirstOrDefault(m =>
                m.GamerId == gamerId && m.MatchId == matchId && !m.Confirmed && !m.JoinTime.HasValue);

            if (matchParticipant != null)
            {
                matchParticipant.Confirmed = true;
                matchParticipant.JoinTime = DateTime.Now;
                matchParticipant.IsPlay = true;

                _dbContext.SaveChanges();

                var matchParticipants = _dbContext.MatchGamers.Where(p => p.MatchId == matchId);
                var confirm = new ConfirmResponseModel
                {
                    MatchId = matchId,
                    Confirmed = matchParticipants.All(p => p.Confirmed)
                };

                if (confirm.Confirmed)
                {
                    var match = _dbContext.Matches.Include(m => m.Card).ThenInclude(c => c.Questions).Single(m => m.Id == matchId);
                    match.StartTime = DateTime.Now;
                    match.Questions = string.Join(',',  match.Card.Questions
                                                        .OrderBy(o => Guid.NewGuid())
                                                        .Take(match.Card.MatchQuestions)
                                                        .Select(q => q.Id));
                    _dbContext.SaveChanges();

                    var participants = matchParticipants.Where(p => p.GamerId != gamerId).Select(p => p.GamerId).ToList();

                    foreach (var participant in participants)
                    {
                        await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchConfirmed", confirm);
                    }
                }

                return confirm;
            }

            throw new Exception("Match not found.");
        }
    }
}