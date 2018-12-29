using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Game;
using RobiGroup.AskMeFootball.Core.Handlers.Models;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Match;
using RobiGroup.Web.Common.Identity;
using WebSocketManager;
using WebSocketManager.Common;

namespace RobiGroup.AskMeFootball.Core.Handlers
{
    public class GamersHandler : WebSocketHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GamersHandler> _logger;
        private readonly MatchOptions _matchOptions;

        private readonly IServiceProvider _serviceProvider;

        private ConcurrentDictionary<string, PausedMatch> _pausedMatches;

        private DateTime _handlerCreatedTime;

        public GamersHandler(WebSocketConnectionManager webSocketConnectionManager,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MatchOptions> matchOptions,
            ILogger<GamersHandler> logger,
            IServiceProvider serviceProvider)
            : base(webSocketConnectionManager, new ControllerMethodInvocationStrategy(), logger)
        {
            _pausedMatches = new ConcurrentDictionary<string, PausedMatch>();
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _matchOptions = matchOptions.Value;
            _handlerCreatedTime = DateTime.Now;
            ((ControllerMethodInvocationStrategy)MethodInvocationStrategy).Controller = this;
        }

        public async Task PauseMatch(WebSocket socket, int id)
        {
            var userId =  WebSocketConnectionManager.GetSocketGroup(WebSocketConnectionManager.GetId(socket));
            _logger.LogWarning($"PauseMatch called by {userId} for match {id}");
            await PauseResumeMatch(socket, id, true);
        }

        public async Task ResumeMatch(WebSocket socket, int id)
        {
            var userId = WebSocketConnectionManager.GetSocketGroup(WebSocketConnectionManager.GetId(socket));
            _logger.LogWarning($"ResumeMatch called by {userId} for match {id}");
            await PauseResumeMatch(socket, id, false);
        }

        private async Task PauseResumeMatch(WebSocket socket, int id, bool pause)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var socketId = WebSocketConnectionManager.GetId(socket);
                var gamerId = WebSocketConnectionManager.GetSocketGroup(socketId);

                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var matchGamer = dbContext.MatchGamers.SingleOrDefault(g => g.GamerId == gamerId && g.MatchId == id);

                if (matchGamer != null)
                {
                    var gamers = dbContext.MatchGamers.Where(g => g.GamerId != gamerId && !g.Cancelled && g.IsPlay).ToList();
                    if (gamers.Any())
                    {
                        gamers.Add(matchGamer);
                        if (pause)
                        {
                            _pausedMatches[gamerId] = new PausedMatch() {MatchId = id, PausedTime = DateTime.Now};
                        }
                        else if (_pausedMatches.TryRemove(gamerId, out var pausedMatch))
                        {
                            if (DateTime.Now - pausedMatch.PausedTime > _matchOptions.MatchPauseDuration)
                            {
                                matchGamer.Cancelled = true;
                                matchGamer.IsPlay = false;
                                await dbContext.SaveChangesAsync();

                                foreach (var game in gamers)
                                {
                                    await InvokeClientMethodToGroupAsync(game.GamerId, "matchStoped",
                                        new { id = game.MatchId, gamerId });
                                }
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }

                        foreach (var game in gamers)
                        {
                            await InvokeClientMethodToGroupAsync(game.GamerId, pause ? "matchPaused" : "matchResumed",
                                new {id = game.MatchId, gamerId});
                        }
                    }
                }
            }
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            WebSocketConnectionManager.AddToGroup(socketId, userId);
            WebSocketConnectionManager.Connections[socketId].UserId = userId;

            await ResumeMatchIfExists(userId, true);
        }

        public async Task ResumeMatchForAll()
        {
            foreach (var pausedMatch in _pausedMatches)
            {
                if (DateTime.Now - pausedMatch.Value.PausedTime > _matchOptions.MatchPauseDuration)
                {
                    await ResumeMatchIfExists(pausedMatch.Key);
                }
            }
        }

        private async Task ResumeMatchIfExists(string gamerId, bool fromDb = false)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchController = scope.ServiceProvider.GetService<MatchController>();

                if (_pausedMatches.TryRemove(gamerId, out var pausedMatch))
                {
                    var resumeMatch = DateTime.Now - pausedMatch.PausedTime <= _matchOptions.MatchPauseDuration;

                    var gamers = dbContext.MatchGamers.Include(g => g.Gamer).Where(g => g.MatchId == pausedMatch.MatchId && !g.Cancelled && g.IsPlay).ToList();

                    foreach (var gamer in gamers)
                    {
                        if (resumeMatch)
                        {
                            _logger.LogWarning($"matchResumed for: {gamer.GamerId}, match: {gamer.MatchId}");
                            await InvokeClientMethodToGroupAsync(gamer.GamerId, "matchResumed",
                                new {id = gamer.MatchId, gamerId});
                        }
                        else
                        {
                            if (gamer.GamerId == gamerId)
                            {

                                var lastQuestionId = (from a in dbContext.MatchAnswers
                                    join g in dbContext.MatchGamers on a.MatchGamerId equals g.Id
                                    where g.MatchId == gamer.MatchId
                                    orderby a.CreatedAt descending 
                                    select a.QuestionId).FirstOrDefault();

                                if (!dbContext.MatchAnswers.Any(a => a.MatchGamerId == gamer.Id && a.QuestionId == lastQuestionId))
                                {
                                    await matchController.AnswerToQuestions(gamer.MatchId, new MatchQuestionAnswerModel()
                                        {
                                            QuestionId = lastQuestionId
                                        },
                                        gamer.GamerId);
                                }

                                gamer.IsPlay = false;
                                gamer.Cancelled = true;
                            }
                            else if (gamer.Gamer.Bot > 0)
                            {
                                gamer.IsPlay = false;
                                gamer.Cancelled = true;
                            }

                            _logger.LogWarning($"matchStoped for: {gamer.GamerId}, match: {gamer.MatchId}");
                            await InvokeClientMethodToGroupAsync(gamer.GamerId, "matchStoped",
                                new {id = gamer.MatchId, gamerId});
                        }
                    }

                    if (!resumeMatch)
                    {
                        dbContext.SaveChanges();
                    }
                }
                else if (fromDb)
                {
                    var isStopMatch = (DateTime.Now - _handlerCreatedTime) > _matchOptions.MatchPauseDuration;
                    var matchGamers = dbContext.MatchGamers.Where(g => g.GamerId == gamerId && g.IsPlay).ToList();
                    
                    foreach (var gamer in matchGamers)
                    {
                        if (isStopMatch)
                        {
                            var matchBots = (from mg in dbContext.MatchGamers
                                join u in dbContext.Users on mg.GamerId equals u.Id
                                where mg.MatchId == gamer.MatchId && u.Bot > 0
                                select mg).ToList();
                            foreach (var matchBot in matchBots)
                            {
                                matchBot.IsPlay = false;
                                matchBot.Cancelled = true;
                            }

                            gamer.IsPlay = false;
                            gamer.Cancelled = true;

                            _logger.LogWarning("Match {0} cancelled for game {1} by server.", gamer.MatchId,
                                gamer.GamerId);
                            _logger.LogWarning($"matchStoped for: {gamer.GamerId}, match: {gamer.MatchId}");

                            await InvokeClientMethodToGroupAsync(gamer.GamerId, "matchStoped",
                                new {id = gamer.MatchId, gamer.GamerId});
                        }
                        else
                        {
                            _logger.LogWarning($"matchResumed for: {gamer.GamerId}, match: {gamer.MatchId}");
                            await InvokeClientMethodToGroupAsync(gamer.GamerId, "matchResumed",
                                new { id = gamer.MatchId, gamerId });
                        }
                    }

                    dbContext.SaveChanges();
                }
            }
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            if (!string.IsNullOrEmpty(socketId))
            {
                string gamerId = WebSocketConnectionManager.GetSocketGroup(socketId);

                await base.OnDisconnected(socket);

                await PauseMatchIfExists(gamerId);
            }
        }

        private async Task PauseMatchIfExists(string gamerId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var gamers = dbContext.MatchGamers.Where(g => g.GamerId != gamerId && !g.Cancelled && g.IsPlay).ToList();

                if (gamers.Any())
                {
                    _pausedMatches[gamerId] =
                        new PausedMatch() {MatchId = gamers.First().MatchId, PausedTime = DateTime.Now};

                    foreach (var game in gamers)
                    {
                        await InvokeClientMethodToGroupAsync(game.GamerId, "matchPaused",
                            new {id = game.MatchId, gamerId});
                    }
                }

                //matchManager.SearchMatch( )
            }
        }
    }
}