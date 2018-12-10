using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Core.Game;
using RobiGroup.AskMeFootball.Core.Handlers.Models;
using RobiGroup.AskMeFootball.Data;
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

        public GamersHandler(WebSocketConnectionManager webSocketConnectionManager,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MatchOptions> matchOptions,
            ILogger<GamersHandler> logger,
            IServiceProvider serviceProvider)
            : base(webSocketConnectionManager, new ControllerMethodInvocationStrategy())
        {
            _pausedMatches = new ConcurrentDictionary<string, PausedMatch>();
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _matchOptions = matchOptions.Value;
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
                if (_pausedMatches.TryRemove(gamerId, out var pausedMatch))
                {
                    var resumeMatch = DateTime.Now - pausedMatch.PausedTime < _matchOptions.MatchPauseDuration;

                    var gamers = dbContext.MatchGamers.Where(g => !g.Cancelled && g.IsPlay);

                    foreach (var game in gamers)
                    {
                        if (resumeMatch)
                        {

                            _logger.LogWarning($"matchResumed for: {game.GamerId}, match: {pausedMatch.MatchId}");
                            await InvokeClientMethodToGroupAsync(game.GamerId, "matchResumed",
                                new {id = game.MatchId, gamerId});
                        }
                        else
                        {
                            if (game.GamerId == gamerId)
                            {
                                game.Cancelled = true;
                              }


                            _logger.LogWarning($"matchStoped for: {game.GamerId}, match: {pausedMatch.MatchId}");
                            await InvokeClientMethodToGroupAsync(game.GamerId, "matchStoped",
                                new {id = game.MatchId, gamerId});
                        }
                    }

                    if (!resumeMatch)
                    {
                        dbContext.SaveChanges();
                    }
                }
                else if (fromDb)
                {
                    var matchGamers = dbContext.MatchGamers.Where(g => g.GamerId == gamerId && g.IsPlay);
                    foreach (var matchGamer in matchGamers)
                    {
                        matchGamer.IsPlay = false;
                        matchGamer.Cancelled = true;
                        _logger.LogWarning("Match {0} cancelled for game {1} by server.", matchGamer.MatchId, matchGamer.GamerId);
                    }

                    dbContext.SaveChanges();

                    foreach (var gamer in matchGamers)
                    {
                        _logger.LogWarning($"matchStoped for: {gamer.GamerId}, match: {pausedMatch.MatchId}");
                        await InvokeClientMethodToGroupAsync(gamer.GamerId, "matchStoped", new { id = gamer.MatchId, gamer.GamerId });
                    }
                }
            }
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            string gamerId = WebSocketConnectionManager.GetSocketGroup(socketId);

            await base.OnDisconnected(socket);

            await PauseMatchIfExists(gamerId);
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