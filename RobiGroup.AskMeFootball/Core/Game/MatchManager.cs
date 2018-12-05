using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Match;
using RobiGroup.Web.Common;
using WebSocketManager;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public class MatchManager : IMatchManager
    {
        private GamersHandler _gamersHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MatchController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MatchManager(GamersHandler gamersHandler,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MatchController> logger,
            IServiceProvider serviceProvider)
        {
            _gamersHandler = gamersHandler;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<MatchSearchResultModel> SearchMatch(string gamerId, int cardId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var model = new MatchSearchResultModel();
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var gamer = _dbContext.Users.Find(gamerId);

                if (gamer.Score > 100)
                {
                    var card = _dbContext.Cards.Find(cardId);
                    if ((card.ResetTime - DateTime.Now) > TimeSpan.FromMinutes(10))
                    {
                        var enemyCandidates = _gamersHandler.WebSocketConnectionManager.Connections.Values
                            .Where(c => !c.Away && !c.IsBusy && c.UserId != gamerId)
                            .OrderByDescending(c => c.ConnectedTime)
                            .ToList();

                        CommonWebSocketConnection enemy = null;

                        foreach (var candidate in enemyCandidates)
                        {
                            if (!_dbContext.MatchGamers.Any(g => g.GamerId == candidate.UserId && (g.IsPlay)))
                            {
                                enemy = candidate;
                                break;
                            }
                        }

                        if (enemy != null && !enemy.IsBusy)
                        {
                            enemy.IsBusy = true;
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

                            var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();

                            var rivalMatchModel = new MatchModel(match.Id, _dbContext.Users.Find(gamerId));
                            rivalMatchModel.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                            rivalMatchModel.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                            var gamerCardScore = _dbContext.GamerCards
                                .Where(gc => gc.CardId == cardId && gc.GamerId == gamerId).Select(gc => gc.Score)
                                .SingleOrDefault();
                            rivalMatchModel.GamerRaiting =
                                _dbContext.GamerCards.Where(gcr => gcr.CardId == cardId)
                                    .Count(gr => gr.Score > gamerCardScore) + 1;
                            rivalMatchModel.GamerCardScore = gamerCardScore;

                            await _gamersHandler.InvokeClientMethodToGroupAsync(enemy.UserId, "matchRequest",
                                rivalMatchModel);

                            model.Match = new MatchModel(match.Id, _dbContext.Users.Find(enemy.UserId));
                            model.Found = true;

                            model.Match.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                            model.Match.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                            var rivaCardScore = _dbContext.GamerCards
                                .Where(gc => gc.CardId == cardId && gc.GamerId == enemy.UserId).Select(gc => gc.Score)
                                .SingleOrDefault();
                            model.Match.GamerRaiting =
                                _dbContext.GamerCards.Where(gcr => gcr.CardId == cardId)
                                    .Count(gr => gr.Score > rivaCardScore) + 1;
                            model.Match.GamerCardScore = rivaCardScore;
                        }
                        else
                        {
                            throw new Exception("Никого не найдено!");
                        }
                    }
                }
                else
                {
                    throw new Exception($"Недостаточно очков для игры. У вас {gamer.Score} очков.");
                }

                return model;
            }
        }

        public async Task<ConfirmResponseModel> Confirm(string gamerId, int matchId)
        {
            //  await semaphoreSlim.WaitAsync();
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                using (var tran = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _logger.LogInformation($"Match confirm from {gamerId}");
                        var matchParticipant = _dbContext.MatchGamers.FirstOrDefault(m =>
                            m.GamerId == gamerId && m.MatchId == matchId && !m.JoinTime.HasValue);

                        if (matchParticipant != null)
                        {
                            if (!matchParticipant.Confirmed)
                            {
                                matchParticipant.Confirmed = true;
                                matchParticipant.JoinTime = DateTime.Now;
                                matchParticipant.IsPlay = true;

                                _dbContext.SaveChanges();

                                var match = _dbContext.Matches.Include(m => m.Card).ThenInclude(c => c.Questions)
                                    .Single(m => m.Id == matchId);
                                match.StartTime = DateTime.Now;
                                match.Questions = string.Join(',', match.Card.Questions
                                    .OrderBy(o => Guid.NewGuid())
                                    .Take(match.Card.MatchQuestions)
                                    .Select(q => q.Id));
                                _dbContext.SaveChanges();
                            }

                            var matchParticipants = _dbContext.MatchGamers.Where(p => p.MatchId == matchId);
                            var confirm = new ConfirmResponseModel
                            {
                                MatchId = matchId,
                                Confirmed = matchParticipants.All(p => p.Confirmed)
                            };

                            var participants = matchParticipants.Where(p => p.GamerId != gamerId).Select(p => p.GamerId)
                                .ToList();

                            tran.Commit();

                            foreach (var participant in participants)
                            {
                                await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchConfirmed", confirm);
                                _logger.LogInformation($"matchConfirmed for {participant}");
                            }

                            return confirm;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error {gamerId}");
                        tran.Rollback();
                    }
                }

                throw new Exception("Match not found.");
            }
        }
    }
}