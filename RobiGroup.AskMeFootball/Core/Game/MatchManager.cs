using System;
using System.Collections.Generic;
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
        private object _locker = new object();

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

        public async Task<MatchSearchResultModel> RequestMatch(string gamerId, string rivalId, int cardId)
        {
            return await RequestMatch(gamerId, cardId, rivalId);
        }

        public async Task<MatchSearchResultModel> SearchMatch(string gamerId, int cardId)
        {
            return await RequestMatch(gamerId, cardId);
        }

        public async Task<MatchSearchResultModel> RequestMatch(string gamerId, int cardId, string rivalCandidateId = null)
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
                        var rivalCandidates = _gamersHandler.WebSocketConnectionManager.Connections.Values
                            .Where(c => !c.Away && c.UserId != gamerId && (string.IsNullOrEmpty(rivalCandidateId) || c.UserId == rivalCandidateId))
                            .OrderByDescending(c => c.ConnectedTime)
                            .ToList();

                        CommonWebSocketConnection rival = null;

                        //var rivalIds = rivalCandidates.Select(r => r.UserId).ToArray();
                        Random rng = new Random();
                        // _dbContext.MatchGamers.Where(g => rivalIds.Contains(g.GamerId) && !g.IsPlay).OrderBy(m => m.Id).Select(r => r.GamerId).Distinct();
                        int n = rivalCandidates.Count;
                        while (n > 1)
                        {
                            n--;
                            int k = rng.Next(n + 1);
                            var value = rivalCandidates[k];
                            rivalCandidates[k] = rivalCandidates[n];
                            rivalCandidates[n] = value;
                        }

                        foreach (var candidate in rivalCandidates)
                        {
                            if (!_dbContext.MatchGamers.Any(g => g.GamerId == candidate.UserId && (g.IsPlay)))
                            {
                                rival = candidate;
                                break;
                            }
                        }

                        bool isBot = false;
                        var rivalId = rival?.UserId;
                        if (rival == null)
                        {
                            rivalId = (from u in _dbContext.Users
                                where u.Bot > 0 && !_dbContext.MatchGamers.Any(g => g.GamerId == u.Id && g.IsPlay)
                                select u.Id).Distinct().OrderBy(u => Guid.NewGuid()).FirstOrDefault();
                            isBot = true;
                        }

                        if (!string.IsNullOrEmpty(rivalId))
                        {
                            var match = new Match()
                            {
                                CardId = cardId,
                                CreateTime = DateTime.Now,
                            };
                            _dbContext.Matches.Add(match);
                            _dbContext.SaveChanges();

                            var gamerCard = GetOrAddGamerCard(gamerId, cardId, _dbContext);

                            _dbContext.MatchGamers.Add(new MatchGamer
                            {
                                MatchId = match.Id,
                                GamerId = gamerId,
                                GamerCardId = gamerCard.Id
                            });
                            _dbContext.SaveChanges();

                            var rivalCard = GetOrAddGamerCard(rivalId, cardId, _dbContext);

                            var entity = new MatchGamer
                            {
                                MatchId = match.Id,
                                GamerId = rivalId,
                                GamerCardId = rivalCard.Id
                            };

                            if (isBot)
                            {
                                entity.JoinTime = DateTime.Now;
                                entity.Ready = true;
                                entity.Confirmed = true;
                            }

                            _dbContext.MatchGamers.Add(entity);
                            _dbContext.SaveChanges();

                            var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();

                            if (!isBot)
                            {
                                var rivalMatchModel = new MatchRequestModel(match.Id, _dbContext.Users.Find(gamerId));
                                rivalMatchModel.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                                rivalMatchModel.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                                var gamerCardScore = _dbContext.GamerCards
                                    .Where(gc => gc.CardId == cardId && gc.GamerId == gamerId && gc.IsActive).Select(gc => gc.Score)
                                    .SingleOrDefault();
                                rivalMatchModel.GamerRaiting =
                                    _dbContext.GamerCards.Where(gcr => gcr.CardId == cardId && gcr.IsActive)
                                        .Count(gr => gr.Score > gamerCardScore) + 1;
                                rivalMatchModel.GamerCardScore = gamerCardScore;
                                await _gamersHandler.InvokeClientMethodToGroupAsync(rivalId, "matchRequest",
                                    rivalMatchModel);
                            }

                            model.Match = new MatchRequestModel(match.Id, _dbContext.Users.Find(rivalId));
                            model.Found = true;

                            model.Match.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                            model.Match.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                            var rivaCardScore = _dbContext.GamerCards
                                .Where(gc => gc.CardId == cardId && gc.GamerId == rivalId && gc.IsActive).Select(gc => gc.Score)
                                .SingleOrDefault();
                            model.Match.GamerRaiting =
                                _dbContext.GamerCards.Where(gcr => gcr.CardId == cardId && gcr.IsActive)
                                    .Count(gr => gr.Score > rivaCardScore) + 1;
                            model.Match.GamerCardScore = rivaCardScore;

                            model.Match.IsBot = isBot;
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

        private static GamerCard GetOrAddGamerCard(string gamerId, int cardId, ApplicationDbContext _dbContext)
        {
            var gamerCard =
                _dbContext.GamerCards.SingleOrDefault(gc => gc.CardId == cardId && gc.GamerId == gamerId && gc.IsActive);
            if (gamerCard == null)
            {
                // Создаем новую карточку для игрока
                gamerCard = new GamerCard
                {
                    CardId = cardId,
                    GamerId = gamerId,
                    StartTime = DateTime.Now,
                    IsActive = true
                };
                _dbContext.GamerCards.Add(gamerCard);
                _dbContext.SaveChanges();
            }
            return gamerCard;
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
                            if (matchParticipant.Cancelled)
                            {
                                throw new Exception("Матч отменен!");
                            }

                            if (!matchParticipant.Confirmed)
                            {
                                matchParticipant.Confirmed = true;
                                matchParticipant.JoinTime = DateTime.Now;

                                _dbContext.SaveChanges();
                            }

                            tran.Commit();

                            var matchParticipants = _dbContext.MatchGamers.Where(p => p.MatchId == matchId && !p.Delayed);
                            var confirm = new ConfirmResponseModel
                            {
                                MatchId = matchId,
                                Confirmed = matchParticipant.Delayed || matchParticipants.All(p => p.Confirmed)
                            };

                            var participants = matchParticipants.Where(p => p.GamerId != gamerId).Select(p => p.GamerId)
                                .ToList();

                            var match = _dbContext.Matches.Include(m => m.Card).ThenInclude(c => c.Questions)
                                    .Single(m => m.Id == matchId);
                            if (confirm.Confirmed && string.IsNullOrEmpty(match.Questions))
                            {
                                match.StartTime = DateTime.Now;
                                match.Questions = string.Join(',', match.Card.Questions
                                    .OrderBy(o => Guid.NewGuid())
                                    .Take(match.Card.MatchQuestions)
                                    .Select(q => q.Id));
                                _dbContext.SaveChanges();
                            }

                            if (!matchParticipant.Delayed)
                            {
                                foreach (var participant in participants)
                                {
                                    await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchConfirmed", confirm);
                                    _logger.LogInformation($"matchConfirmed for {participant}");
                                }
                            }

                            return confirm;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error {gamerId}");
                        tran.Rollback();
                        throw e;
                    }
                }

                throw new Exception("Match not found.");
            }
        }

        public async Task<MatchResultModel> GetMatchResult(int id, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>(); var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();

                lock (_locker)
                {
                    MatchGamer currentMatchGamer = null;
                    ApplicationUser currentGamer = null;

                    var match = _dbContext.Matches.Find(id);
                    var matchGamers = _dbContext.MatchGamers.Include(m => m.Match).Include(m => m.Answers).Where(g => g.MatchId == id);
                    /*
                    if (matchGamers.Any(g => g.Delayed && (!g.Ready || g.IsPlay)))
                    {
                        var matchGamer = matchGamers.Single(g => g.GamerId == userId);
                        matchGamer.IsPlay = false;
                        _dbContext.SaveChanges();
                        transaction.Commit();
                        ModelState.AddModelError(String.Empty, "Результат будет готов когда другие игроки доиграют свои матчи!");
                        return BadRequest(ModelState);
                    }*/

                    var resultModel = new MatchResultModel();

                    int winnerScore = 0;

                    var matchGamerBonuses = new List<Tuple<MatchGamer, GamerCard, int>>();

                    foreach (var matchGamer in matchGamers)
                    {
                        var gamer = _dbContext.Users.Find(matchGamer.GamerId);
                        if (matchGamer.IsPlay && matchGamer.JoinTime.HasValue)
                        {
                            var questionsCount = match.Questions.SplitToIntArray().Length;
                            var answersCount = matchGamer.Answers.Count();
                            var correctAnswersCount = matchGamer.Answers.Count(a => a.IsCorrectAnswer);
                            var incorrectAnswersCount = questionsCount - correctAnswersCount;

                            var pointsForMacth = correctAnswersCount * matchOptions.Value.CorrectAnswerScore +
                                                 incorrectAnswersCount * matchOptions.Value.IncorrectAnswerScore;
                            matchGamer.Score = pointsForMacth;
                            matchGamer.IsPlay = false;

                            var gamerCard = _dbContext.GamerCards.Single(gc =>
                                gc.CardId == matchGamer.Match.CardId && gc.GamerId == matchGamer.GamerId &&
                                gc.IsActive);

                            matchGamerBonuses.Add(new Tuple<MatchGamer, GamerCard, int>(matchGamer, gamerCard,
                                answersCount * matchOptions.Value.BonusForAnswer));

                            gamerCard.Score += matchGamer.Score; // Добавляем (или отнимаем) очки к карте игрока 

                            gamer.Score += matchGamer.Score; // Прибавляем текущие очки игроку

                            if (matchGamer.Score > winnerScore)
                            {
                                winnerScore = matchGamer.Score;
                            }
                        }

                        if (matchGamer.GamerId == userId)
                        {
                            currentMatchGamer = matchGamer;
                            currentGamer = gamer;
                        }
                        else
                        {
                            resultModel.RivalMatchScore = matchGamer.Score;
                        }
                    }

                    foreach (var gamerBonus in matchGamerBonuses)
                    {
                        gamerBonus.Item1.IsWinner = gamerBonus.Item1.Score == winnerScore;

                        int bonus = gamerBonus.Item1.IsWinner ? +gamerBonus.Item3 : -gamerBonus.Item3;
                        gamerBonus.Item1.Score += bonus;
                        gamerBonus.Item2.Score += bonus;

                        if (gamerBonus.Item1 != currentMatchGamer)
                        {
                            resultModel.RivalMatchScore = gamerBonus.Item1.Score;
                        }
                        gamerBonus.Item1.Bonus = bonus;
                    }

                    _dbContext.SaveChanges();

                    resultModel.CardScore = _dbContext.GamerCards.Where(gc => gc.CardId == currentMatchGamer.Match.CardId && gc.GamerId == userId && gc.IsActive).Select(c => c.Score).FirstOrDefault();
                    resultModel.MatchScore = currentMatchGamer.Score;
                    resultModel.IsWinner = currentMatchGamer.IsWinner;
                    resultModel.CurrentGamerScore = currentGamer.Score;
                    resultModel.MatchBonus = currentMatchGamer.Bonus;

                    return resultModel;
                }
            }
        }
    }
}