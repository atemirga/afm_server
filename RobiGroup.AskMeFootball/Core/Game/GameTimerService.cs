using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Core.Game
{
    internal class GameTimerService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private Timer _timer;

        private DateTime _gamersDailyScoresLastResetTime = DateTime.MinValue;

        public GameTimerService(IServiceProvider services, ILogger<GameTimerService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Game Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Timed Game Service is working.");

            ResetCardGamers();

            ResetGamerDailyScores().Wait();
        }

        private async Task ResetGamerDailyScores()
        {
            if (_gamersDailyScoresLastResetTime.Date < DateTime.Today)
            {
                var gamerOptions = _services.GetService<IOptions<GamerOptions>>();
                _gamersDailyScoresLastResetTime = DateTime.Now;

                var userManager = _services.GetService<UserManager<ApplicationUser>>();
                foreach (var gamer in await userManager.GetUsersInRoleAsync(ApplicationRoles.Gamer))
                {
                    gamer.Score = gamerOptions.Value.DailyPoints;
                    await userManager.UpdateAsync(gamer);
                }
            }
        }

        private void ResetCardGamers()
        {
            var dbContext = _services.GetService<ApplicationDbContext>();

            var cards = dbContext.Cards.Include(c => c.Type).Where(c => c.ResetTime > DateTime.Now).ToList();

            foreach (var card in cards)
            {
                using (var tran = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (card.Type.Name == CardTypes.Daily.ToString()) card.ResetTime = card.ResetTime.AddDays(card.ResetPeriod);
                        else if (card.Type.Name == CardTypes.Weekly.ToString()) card.ResetTime = card.ResetTime.AddDays(card.ResetPeriod * 7);
                        else if (card.Type.Name == CardTypes.Monthly.ToString()) card.ResetTime = card.ResetTime.AddMonths(card.ResetPeriod);
                        else throw new Exception($"Unknown CardType {card.Type.Name}");

                        foreach (var gamerCard in dbContext.GamerCards.Include(c => c.Gamer).Where(g => g.CardId == card.Id))
                        {
                            gamerCard.Gamer.TotalScore += gamerCard.Score; // Добавляем текушие очки игрока к итоговому
                            gamerCard.Score = 0; // Обнуляем очки игрока в карточке
                        }

                        dbContext.SaveChanges();

                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Reset card: {0}", card.Id);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Game Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}