﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Match;
using RobiGroup.Web.Common;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public class MatchManager : IMatchManager
    {
        private GamersHandler _gamersHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public MatchManager(GamersHandler gamersHandler, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _gamersHandler = gamersHandler;
            _httpContextAccessor = httpContextAccessor;
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
                        var enemy = _gamersHandler.WebSocketConnectionManager.Connections.Values
                            .Where(c => !c.Away && !c.IsBusy && c.UserId != gamerId).OrderByDescending(c => c.ConnectedTime)
                            .FirstOrDefault();
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

                            var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();

                            var rivalMatchModel = new MatchModel(match.Id, _dbContext.Users.Find(gamerId));
                            rivalMatchModel.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                            rivalMatchModel.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                            await _gamersHandler.InvokeClientMethodToGroupAsync(enemy.UserId, "matchRequest",
                                rivalMatchModel);

                            model.Match = new MatchModel(match.Id, _dbContext.Users.Find(enemy.UserId));
                            model.Found = true;

                            model.Match.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                            model.Match.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;
                        }
                        else
                        {
                            throw new Exception("No enemy error.");
                        }
                    }
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
                _dbContext = _httpContextAccessor.HttpContext.GetService<ApplicationDbContext>();
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

                        var match = _dbContext.Matches.Include(m => m.Card).ThenInclude(c => c.Questions)
                            .Single(m => m.Id == matchId);
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

                    var participants = matchParticipants.Where(p => p.GamerId != gamerId).Select(p => p.GamerId)
                        .ToList();

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
}