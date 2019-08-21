using System;
using System.Collections.Generic;
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
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Services;
using RobiGroup.Web.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RobiGroup.AskMeFootball.Core.Game
{
    internal class GameTimerService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GamersHandler _gamersHandler;
        private readonly ILogger _logger;
        private readonly MatchOptions _matchOptions;
        private Timer _timer;

        private bool firstRun = true;

        private object _locker = new object();

        private DateTime _gamersDailyScoresLastResetTime = DateTime.MinValue;

        public GameTimerService(IServiceScopeFactory scopeFactory, GamersHandler gamersHandler, ILogger<GameTimerService> logger, IOptions<MatchOptions> matchOptions)
        {
            _scopeFactory = scopeFactory;
            _gamersHandler = gamersHandler;
            _logger = logger;
            _matchOptions = matchOptions.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Game Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Timed Game Service is working.");
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    _gamersHandler.ResumeMatchForAll().Wait();

                    //TimeLeft(scope.ServiceProvider);

                    ResetCardGamers(scope.ServiceProvider);

                    ResetGamerDailyScores(scope.ServiceProvider).Wait();

                    //StopDelayedMatch(scope.ServiceProvider).Wait();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ERROR");
            }
        }

        public void TimeLeft(IServiceProvider services)
        {
            var dbContext = services.GetService<ApplicationDbContext>();
            var cards = dbContext.Cards.Where(c => c.ResetTime > DateTime.Now).ToList();
            foreach (var card in cards)
            {
                var cardGamers = dbContext.GamerCards.Where(gc => gc.CardId == card.Id && gc.StartTime > card.ResetTime.AddDays(-1));
                var oneSignals = new List<string>();
                foreach (var cardGamer in cardGamers)
                {
                    var _user = dbContext.Users.FirstOrDefault(u => u.Id == cardGamer.GamerId && u.Bot == 0);
                    if (_user != null)
                    {
                        var oneSignal = _user.OneSignalId;
                        oneSignals.Add(oneSignal);
                    }

                }

                var leftTime = (card.ResetTime - DateTime.Now).TotalMinutes;
                if (leftTime < 121 && leftTime > 119 && !card.IsTwoH)
                {
                    try
                    {
                        card.IsTwoH = true;
                        dbContext.SaveChanges();
                        using (var client = new HttpClient())
                        {
                            var url = new Uri("https://onesignal.com/api/v1/notifications");
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");
                            var obj = new
                            {
                                app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                                headings = new { en = "Время карты истекает", ru = "Время карты истекает" },
                                contents = new { en = "Время карты " + card.Name + " истекает через 2 часа", ru = "Время карты " + card.Name + " истекает через 2 часа" },
                                data = new { cardTimeLeft = card.Id },
                                include_player_ids = oneSignals
                            };
                            var json = JsonConvert.SerializeObject(obj);
                            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                            var response = client.PostAsync(url, content).Result;
                            //var response = await client.PostAsync(url, content);

                        }
                    }
                    catch (Exception exe)
                    {
                        Console.WriteLine(exe);
                    }
                }
                if (leftTime < 31 && leftTime > 29 && !card.IsHalfH)
                {
                    try
                    {
                        card.IsHalfH = true;
                        dbContext.SaveChanges();
                        using (var client = new HttpClient())
                        {
                            var url = new Uri("https://onesignal.com/api/v1/notifications");
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");
                            var obj = new
                            {
                                app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                                headings = new { en = "Время карты истекает", ru = "Время карты истекает" },
                                contents = new { en = "Время карты " + card.Name + " истекает через 30 минут", ru = "Время карты " + card.Name + " истекает через 30 минут" },
                                data = new { cardTimeLeft = card.Id },
                                include_player_ids = oneSignals
                            };
                            var json = JsonConvert.SerializeObject(obj);
                            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                            var response = client.PostAsync(url, content).Result;
                            //var response = await client.PostAsync(url, content);
                            //var response = Task.Run(async () => await client.PostAsync(url, content));

                        }
                    }
                    catch (Exception exe)
                    {
                        Console.WriteLine(exe);
                    }

                }

            }
        }

        public async Task StopDelayedMatch(IServiceProvider services)
        {
            try
            {
                var dbContext = services.GetService<ApplicationDbContext>();
                var matches = dbContext.Matches.Where(m => m.Status == Match.MatchStatus.Delayed);
                var mCount = matches.Count();
                foreach (var match in matches)
                {
                    var matchParticipants = dbContext.MatchGamers.Where(mg => mg.MatchId == match.Id);//match.Gamers.ToList();
                    var delayedGamer = dbContext.MatchGamers.First(mg => mg.MatchId == match.Id && mg.Delayed);
                    var playedGamer = dbContext.MatchGamers.First(mg => mg.MatchId == match.Id && !mg.Delayed);

                    var playedGamerJoinTime = playedGamer.JoinTime;
                    if (DateTime.Now.Subtract(Convert.ToDateTime(playedGamerJoinTime)).TotalMinutes >= 120)
                    {
                        delayedGamer.Delayed = false;
                        delayedGamer.IsWinner = false;
                        match.Status = Match.MatchStatus.Finished;


                        #region send push to participants
                        foreach (var mp in matchParticipants)
                        {
                            mp.IsPlay = false;
                            var _users = new List<String>();
                            var _user = dbContext.Users.First(u => u.Id == mp.GamerId);
                            _users.Add(_user.OneSignalId);

                            var opponent = dbContext.Users.First(u => u.Id == matchParticipants.First(_mp => _mp.GamerId != mp.GamerId).GamerId);

                            using (var client = new HttpClient())
                            {
                                var url = new Uri("https://onesignal.com/api/v1/notifications");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");

                                var obj = new
                                {
                                    app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                                    headings = new { en = "Время ожидание матча закончилось", ru = "Время ожидание матча закончилось" },
                                    contents = new { en = "Ваш матч с игороком " + opponent.NickName + " окончен", ru = "Ваш матч с игороком " + opponent.NickName + " окончен" },
                                    include_player_ids = _users
                                };
                                var json = JsonConvert.SerializeObject(obj);
                                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                var response = await client.PostAsync(url, content);
                            }
                        }
                        #endregion
                    }
                    if (DateTime.Now.Subtract(Convert.ToDateTime(playedGamerJoinTime)).TotalMinutes > 29 && DateTime.Now.Subtract(Convert.ToDateTime(playedGamerJoinTime)).TotalMinutes < 31)
                    {
                        foreach (var mp in matchParticipants)
                        {
                            #region send push 30 min left
                            var _user = dbContext.Users.First(u => u.Id == mp.GamerId).OneSignalId;
                            var opponent = dbContext.Users.First(u => u.Id == dbContext.MatchGamers.First(_mg => _mg.MatchId == mp.MatchId && _mg.GamerId != mp.GamerId).GamerId);

                            using (var client = new HttpClient())
                            {
                                var url = new Uri("https://onesignal.com/api/v1/notifications");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");

                                var obj = new
                                {
                                    app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                                    headings = new { en = "Осталось 30 минут до окончания матча", ru = "Осталось 30 минут до оканчания матча" },
                                    contents = new { en = "Осталось 30 минут до окончания матча с игороком  " + opponent.NickName + "", ru = "Осталось 30 минут до окончания матча с игороком  " + opponent.NickName + "" },
                                    include_player_ids = _user
                                };
                                var json = JsonConvert.SerializeObject(obj);
                                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                var response = await client.PostAsync(url, content);
                            }
                            #endregion
                        }
                    }
                }
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ERROR");
                Console.WriteLine(e);
            }
        }

        private async Task ForceStopDelayed(IServiceProvider services, MatchGamer mg, Match match, IQueryable<MatchGamer> matchParticipants)
        {
            var dbContext = services.GetService<ApplicationDbContext>();
            mg.Delayed = false;
            mg.IsWinner = false;
            match.Status = Match.MatchStatus.Finished;
            foreach (var mp in matchParticipants)
            {
                mp.IsPlay = false;
                var _user = dbContext.Users.First(u => u.Id == mp.GamerId).OneSignalId;
                var opponent = dbContext.Users.First(u => u.Id == dbContext.MatchGamers.First(_mg => _mg.MatchId == mp.MatchId && _mg.GamerId != mp.GamerId).GamerId);
                var obj = new
                {
                    app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                    headings = new { en = "Delayed match time is over", ru = "Время ожидание матча закончилось" },
                    contents = new { en = "Your match with " + opponent.NickName + " ended", ru = "Ваш матч с игороком " + opponent.NickName + " окончен" },
                    include_player_ids = _user
                };
                var json = JsonConvert.SerializeObject(obj);
                await SendPush(json);
            }

            dbContext.SaveChanges();
        }

        public async Task SendPush(string json)
        {
            using (var client = new HttpClient())
            {
                var url = new Uri("https://onesignal.com/api/v1/notifications");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
            }
        }

        private async Task ResetGamerDailyScores(IServiceProvider services)
        {
            try
            {
                lock (_locker)
                {
                    var dbContext = services.GetService<ApplicationDbContext>();

                    var cards = dbContext.Cards.Where(c =>  c.TypeId == dbContext.CardTypes.First(ct => ct.Code == "HalfTime").Id);
                    //c.TypeId == dbContext.CardTypes.First(ct => ct.Code == "Live").Id 

                    foreach (var card in cards)
                    {
                        var matches = dbContext.Matches.Where(m => m.CardId == card.Id && m.Status == Match.MatchStatus.Started);

                        foreach (var match in matches)
                        {
                            var questions = match.Questions.SplitToIntArray();
                            if (questions.Count() == card.MatchQuestions)
                            {
                                var questionId = questions[questions.Count() - 1];
                                var question = dbContext.Questions.Find(questionId);

                                if (question.StartTime.AddMinutes(question.ExpirationTime) < DateTime.Now)
                                {
                                    match.Status = Match.MatchStatus.Finished;
                                }
                            }
                        }
                        
                    }
                    

                    var matchGamers = dbContext.MatchGamers.Where(mg => mg.IsPlay);
                    foreach (var matchGamer in matchGamers)
                    {
                        var diffInSeconds = Math.Abs((DateTime.Now - Convert.ToDateTime(matchGamer.JoinTime)).TotalSeconds);
                        if (diffInSeconds > 120)
                        {
                            matchGamer.IsPlay = false;
                        }
                    }

                    var infoCards = dbContext.InfoCards.Where(ic => ic.IsActive && ic.EndTime < DateTime.Now);
                    foreach (var infoCard in infoCards)
                    {
                        infoCard.IsActive = false;
                    }

                    dbContext.SaveChanges();
                    //Update scores and balls
                    var resetTime = dbContext.Users.First().ResetTime;
                    if (resetTime < DateTime.Now)
                    {
                        var gamerOptions = services.GetService<IOptions<GamerOptions>>();
                        var userManager = services.GetService<UserManager<ApplicationUser>>();

                        var _users = dbContext.Users.AsQueryable();

                        //foreach (var gamer in await userManager.GetUsersInRoleAsync(ApplicationRoles.Gamer))
                        //{
                        //    gamer.PointsToPlay = gamerOptions.Value.DailyPoints;
                        //    gamer.Score = 0;
                        //    await userManager.UpdateAsync(gamer);
                        //}

                        foreach (var gamer in _users)
                        {
                            gamer.PointsToPlay = gamerOptions.Value.DailyPoints;
                            gamer.Score = 0;
                            gamer.ResetTime = resetTime.AddDays(1);
                        }
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception e) {
                _logger.LogError(e, "ERROR");
            }
        }

        private void ResetCardGamers(IServiceProvider services)
        {
            try
            {
                lock (_locker)
                {
                    var dbContext = services.GetService<ApplicationDbContext>();

                    var cardService = services.GetService<ICardService>();

                    //var _cards = dbContext.Cards.First(c => c.Id == 1);

                    var cards = dbContext.Cards.Include(c => c.Type).Where(c => c.ResetTime < DateTime.Now && c.Type.Code != "Live" && c.Type.Code != "HalfTime").ToList();

                    foreach (var card in cards)
                    {
                        using (var tran = dbContext.Database.BeginTransaction())
                        {
                            var cardEndTime = DateTime.Now;
                            var cardLastResetTime = card.ResetTime;
                            if (card.Type.Code == CardTypes.Daily.ToString()) card.ResetTime = card.ResetTime.AddDays(card.ResetPeriod);
                            else if (card.Type.Code == CardTypes.Weekly.ToString()) card.ResetTime = card.ResetTime.AddDays(card.ResetPeriod * 7);
                            else if (card.Type.Code == CardTypes.Monthly.ToString()) card.ResetTime = card.ResetTime.AddMonths(card.ResetPeriod);
                            else throw new Exception($"Unknown CardType {card.Type.Name}");

                            card.IsTwoH = false;
                            card.IsHalfH = false;

                            foreach (var gamerCard in dbContext.GamerCards.Include(c => c.Gamer).Where(g => g.CardId == card.Id))
                            {
                                //gamerCard.Gamer.TotalScore += gamerCard.Score; // Добавляем текушие очки игрока к итоговому

                                gamerCard.EndTime = cardEndTime;
                                gamerCard.IsActive = false;// Обнуляем очки игрока в карточке
                            }

                            dbContext.SaveChanges();

                            foreach (var bot in dbContext.Users.Where(u => u.Bot > 0).ToList())
                            {
                                dbContext.GamerCards.Add(new GamerCard
                                {
                                    CardId = card.Id,
                                    GamerId = bot.Id,
                                    StartTime = DateTime.Now,
                                    IsActive = true
                                });
                            }

                            dbContext.SaveChanges();

                            var winner = cardService.GetLeaderboard(card.Id).FirstOrDefault(c => c.Raiting == 1);
                            //var winners = cardService.GetLeaderboard(card.Id).Where(w => w.Raiting == 1).OrderByDescending(w => w.CardScore).ToList();

                            if (winner != null)
                            {

                                //var _winner = winners.First();
                                dbContext.CardWinners.Add(new CardWinner
                                {
                                    GamerId = winner.Id,
                                    CardId = card.Id,
                                    Prize = card.Prize,
                                    CardEndTime = cardEndTime,
                                    CardStartTime = cardLastResetTime,
                                    GamerCardScore = winner.CardScore,
                                });

                                var userCoins = dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == winner.Id);
                                if (userCoins == null)
                                {
                                    dbContext.UserCoins.Add(new UserCoins
                                    {
                                        GamerId = winner.Id,
                                        Coins = Convert.ToInt32(card.Prize),
                                        LastUpdate = DateTime.Now
                                    });
                                }
                                else
                                {
                                    userCoins.Coins += Convert.ToInt32(card.Prize);
                                    userCoins.LastUpdate = DateTime.Now;
                                    
                                }

                                dbContext.SaveChanges();
                                /*
                                foreach (var winner in winners)
                                {
                                    dbContext.CardWinners.Add(new CardWinner
                                    {
                                        GamerId = winner.Id,
                                        CardId = card.Id,
                                        Prize = card.Prize,
                                        CardEndTime = cardEndTime,
                                        CardStartTime = cardLastResetTime,
                                        GamerCardScore = winner.CardScore,
                                    });
                                }
                                */
                                if (!winner.IsBot)
                                {
                                    var userOneSignal = dbContext.Users.FirstOrDefault(u => u.Id == winner.Id).OneSignalId;
                                    using (var client = new HttpClient())
                                    {
                                        var url = new Uri("https://onesignal.com/api/v1/notifications");
                                        client.DefaultRequestHeaders.Accept.Clear();
                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");
                                        var obj = new
                                        {
                                            app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                                            headings = new { en = "Вы выиграли", ru = "Вы выиграли" },
                                            contents = new { en = "Вы выиграли на карте " + card.Name, ru = "Вы выиграли на карте " + card.Name },
                                            include_player_ids = userOneSignal
                                        };
                                        var json = JsonConvert.SerializeObject(obj);
                                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                        var response = client.PostAsync(url, content).Result;
                                        //var response = Task.Run(async () => await client.PostAsync(url, content));

                                    }
                                }
                                /*
                                var json = JsonConvert.SerializeObject(obj);
                                //task send push async
                                Task.Run(async () => await SendPush(json));
                                */


                                //send to all users 
                                var _users = new List<string>();
                                var _user = String.Empty;
                                var allUsers = dbContext.Users.Where(u => u.Bot == 0 && u.Id != winner.Id);
                                foreach (var gamer in allUsers)
                                {
                                    _users.Add(_user);
                                }

                                using (var client = new HttpClient())
                                {
                                    var url = new Uri("https://onesignal.com/api/v1/notifications");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");
                                    var obj = new
                                    {
                                        app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                                        headings = new { en = "Победитель карты", ru = "Победитель карты" },
                                        contents = new { en = "Победитель карты " + card.Name + " : " + winner.Nickname, ru = "Победитель карты " + card.Name + " : " + winner.Nickname },
                                        include_player_ids = _users
                                    };
                                    var json = JsonConvert.SerializeObject(obj);
                                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                    var response = client.PostAsync(url, content).Result;
                                    //var response = Task.Run(async () => await client.PostAsync(url, content));

                                }

                                dbContext.SaveChanges();
                            }

                            tran.Commit();

                            _logger.LogInformation($"Reset {card.Id} success.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ERROR");
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