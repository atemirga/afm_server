using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Match;
using RobiGroup.AskMeFootball.Models.Questions;
using RobiGroup.Web.Common;
using WebSocketManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;


namespace RobiGroup.AskMeFootball.Core.Game
{
    public class MatchManager : IMatchManager
    {
        private GamersHandler _gamersHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MatchController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private object _locker = new object();

        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

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

        public async Task<MatchSearchResultModel> RequestMatch(string gamerId, int bid, string rivalId, int cardId)
        {
            return await RequestMatch(gamerId, cardId, bid, rivalId);
        }

        public async Task<MatchSearchResultModel> SearchMatch(string gamerId, int cardId, int bid)
        {
            return await RequestMatch(gamerId, cardId, bid);
        }

        public async Task<HTMatchModel> HTMatch(string gamerId, int cardId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var model = new HTMatchModel();
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();
                var gamers = _dbContext.Users.All(g => g.Bot > 0);
                var gamer = _dbContext.Users.Find(gamerId);
                var lang = gamer.Lang;
                var _card = _dbContext.Cards.FirstOrDefault(c => c.Id == cardId).Name;

                var questionId = 0;

                string rivalCandidateId = null;

                var card = _dbContext.Cards.Find(cardId);

                if (_dbContext.CardTypes.FirstOrDefault(ct => ct.Id == card.TypeId).Code == "HalfTime")
                {
                    var htMatch = _dbContext.Matches.LastOrDefault(m => m.CardId == cardId && m.Status == Match.MatchStatus.Started);
                    //if match not started yet
                    if (htMatch.StartTime > DateTime.Now)
                    {
                        switch (lang)
                        {
                            case "en":
                                throw new Exception("Match will start at " + htMatch.StartTime);
                            case "ru":
                                throw new Exception("Матч начнется в " + htMatch.StartTime);
                            case "kz":
                                throw new Exception("Ойын " + htMatch.StartTime + " басталады");
                        }

                    }

                    var questions = _dbContext.Matches.FirstOrDefault(m => m.Id == htMatch.Id).Questions.SplitToIntArray();
                    if (questions != null)
                    {
                        for (int i = 0; i < questions.Count(); i++)
                        {
                            if (_dbContext.Questions.FirstOrDefault(q => q.Id == questions[i])
                                .StartTime.AddMinutes(_dbContext.Questions.FirstOrDefault(q => q.Id == questions[i]).ExpirationTime) < DateTime.Now)
                            {
                                switch (lang)
                                {
                                    case "en":
                                        throw new Exception("Question Expired");
                                    case "ru":
                                        throw new Exception("Время вопроса вышло");
                                    case "kz":
                                        throw new Exception("Сурактын уакыты бытты");
                                }
                            }

                            var matchGamer = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == htMatch.Id);
                            if (matchGamer != null)
                            {
                                var gamerAnswer = _dbContext.MatchAnswers.FirstOrDefault(ma => ma.MatchGamerId == matchGamer.Id);
                                if (gamerAnswer == null)
                                {
                                    questionId = questions[i];
                                    break;
                                }
                                else
                                {
                                    switch (lang)
                                    {
                                        case "en":
                                            throw new Exception("You've answered to this question. Wait for next");
                                        case "ru":
                                            throw new Exception("Вы уже ответили на вопрос. Ждите следующий вопрос");
                                        case "kz":
                                            throw new Exception("Бул суракка жауап берып койдыныз келесы суракты кутыныз");
                                    }
                                }
                            }
                            else
                            {
                                questionId = questions[i];
                                break;
                            }
                        }
                    }

                    var htGamerCard = GetOrAddGamerCard(gamerId, cardId, _dbContext, rivalCandidateId);

                    _dbContext.MatchGamers.Add(new MatchGamer
                    {
                        MatchId = htMatch.Id,
                        GamerId = gamerId,
                        GamerCardId = htGamerCard.Id,
                        Confirmed = true,
                        JoinTime = DateTime.Now
                    });
                    _dbContext.SaveChanges();

                    //get last question of match
                    model.MatchId = htMatch.Id;
                    model.QuestionId = questionId;
                    model.QuestionExpirationTime = _dbContext.Questions.FirstOrDefault(q => q.Id == questionId)
                                                .StartTime.AddMinutes(_dbContext.Questions.FirstOrDefault(q => q.Id == questionId).ExpirationTime);
                    model.QuestionStartTime = _dbContext.Questions.FirstOrDefault(q => q.Id == questionId)
                                                .StartTime;

                }
                return model;
            }
        }

        public async Task<LiveMatchModel> LiveMatch(string gamerId, int cardId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var model = new LiveMatchModel();
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();
                var gamers = _dbContext.Users.All(g => g.Bot > 0);
                var gamer = _dbContext.Users.Find(gamerId);
                var lang = gamer.Lang;
                var _card = _dbContext.Cards.FirstOrDefault(c => c.Id == cardId).Name;

                var questionId = 0;

                string rivalCandidateId = null;

                var card = _dbContext.Cards.Find(cardId);

                //if (_dbContext.CardTypes.FirstOrDefault(ct => ct.Id == card.TypeId).Code == "Live")
                //{
                var htMatch = _dbContext.Matches.LastOrDefault(m => m.CardId == cardId && m.Status == Match.MatchStatus.Started);
                if (htMatch != null)
                {
                    if (htMatch.StartTime < DateTime.Now)
                    {
                        switch (lang)
                        {
                            case "en":
                                throw new Exception("Match already started. You can't join ");
                            case "ru":
                                throw new Exception("Матч уже начался вы не можете играть");
                            case "kz":
                                throw new Exception("Ойын басталып кетты, косыла алмайсыз");
                        }
                    }
                    var matchQuestions = htMatch.Questions.SplitToIntArray();

                    var htGamerCard = GetOrAddGamerCard(gamerId, cardId, _dbContext, rivalCandidateId);

                    var matchGamer = _dbContext.MatchGamers.Count(mg => mg.GamerId == gamerId && mg.MatchId == htMatch.Id);

                    if (matchGamer == 0)
                    {
                        _dbContext.MatchGamers.Add(new MatchGamer
                        {
                            MatchId = htMatch.Id,
                            GamerId = gamerId,
                            GamerCardId = htGamerCard.Id,
                            Confirmed = true,
                            Ready = true,
                            JoinTime = DateTime.Now
                        });
                    }


                    _dbContext.SaveChanges();

                    model.MatchId = htMatch.Id;
                    model.StartTime = Convert.ToDateTime(htMatch.StartTime);
                    
                    if (_dbContext.CardTeams.Any(ct => ct.CardId == cardId))
                    {
                        model.Teams = _dbContext.CardTeams.First(ct => ct.CardId == cardId);
                    }

                    switch (lang)
                    {
                        case "ru":
                            var questionsRu = _dbContext.QuestionAnswers
                                 .Where(a => matchQuestions.Contains(a.QuestionId))
                                 .GroupBy(a => a.Question)
                                .Select(qa => new QuestionModel
                                {
                                    Id = qa.Key.Id,
                                    Text = qa.Key.TextRu,
                                    Delay = qa.Key.Delay,
                                    Answers = qa.Select(a => new QuestionAnswerModel
                                    {
                                        Id = a.Id,
                                        Text = a.TextRu
                                    }).ToList()
                                }).ToList();
                            foreach (var qRu in questionsRu)
                            {
                                if (_dbContext.QuestionBoxes.Any(qb => qb.QuestionId == qRu.Id))
                                {
                                    qRu.Box = _dbContext.QuestionBoxes.First(qb => qb.QuestionId == qRu.Id);
                                }
                            }
                            model.Questions = questionsRu;
                            break;
                        case "kz":
                            var questionsKz = _dbContext.QuestionAnswers
                                .Where(a => matchQuestions.Contains(a.QuestionId))
                                .GroupBy(a => a.Question)
                               .Select(qa => new QuestionModel
                               {
                                   Id = qa.Key.Id,
                                   Text = qa.Key.TextKz,
                                   Delay = qa.Key.Delay,
                                   Answers = qa.Select(a => new QuestionAnswerModel
                                   {
                                       Id = a.Id,
                                       Text = a.TextKz
                                   }).ToList()
                               }).ToList();
                            foreach (var qKz in questionsKz)
                            {
                                if (_dbContext.QuestionBoxes.Any(qb => qb.QuestionId == qKz.Id))
                                {
                                    qKz.Box = _dbContext.QuestionBoxes.First(qb => qb.QuestionId == qKz.Id);
                                }
                            }
                            model.Questions = questionsKz;
                            break;
                    }
                }
                //gamer.Hints += 1;
                //_dbContext.SaveChanges();
               
                //}
                return model;
            }
        }

        public async Task<MatchSearchResultModel> RequestMatch(string gamerId, int cardId, int bid, string rivalCandidateId = null)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                bool autoConfirm = false;
                var model = new MatchSearchResultModel();
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();
                var gamers = _dbContext.Users.All(g => g.Bot > 0);
                var gamer = _dbContext.Users.Find(gamerId);
                var lang = gamer.Lang;
                var _card = _dbContext.Cards.FirstOrDefault(c => c.Id == cardId).Name;


                var card = _dbContext.Cards.Find(cardId);

                if (Math.Abs((card.ResetTime - DateTime.Now).TotalMinutes) <= 10)//TimeSpan.FromMinutes(10)
                {
                    var errorCardTime = string.Empty;
                    switch (lang)
                    {
                        case "en":
                            errorCardTime = "Card time is up";
                            break;
                        case "ru":
                            errorCardTime = "Время карты истекло.";
                            break;

                    }
                    throw new Exception(errorCardTime);
                }

                // retrieve all users connected to web socket except of self
                var rivalCandidates = _gamersHandler.WebSocketConnectionManager.Connections.Values
                    .Where(c => !c.Away
                        && c.UserId != gamerId
                        && (string.IsNullOrEmpty(rivalCandidateId)
                            || c.UserId == rivalCandidateId)
                        )
                    .OrderByDescending(c => c.ConnectedTime)
                    .ToList();

                var rivalIds = rivalCandidates.Select(r => r.UserId).ToArray();

                string rivalId;
                bool isBot = false;

                var _participants = new List<String>();

                if (string.IsNullOrEmpty(rivalCandidateId)) // if rival is not defined by request author
                {

                    _participants.Add(gamerId);
                    var foundRival = String.Empty;
                    var gamerIndex = MatchController.searchingGamers.FindIndex(g => g.UserId == gamerId);
                    //MatchController._searchingGamers.Count > 1
                    if (MatchController.searchingGamers.Count(sg => sg.CardId == cardId) > 1)
                    {
                        int rivalIndex = 0;

                        for (int i = 0; i < MatchController.searchingGamers.Count(sg => sg.CardId == cardId); i++)//MatchController._searchingGamers.Count
                        {
                            if (MatchController.searchingGamers[i].UserId != gamerId)//_searchingGamers
                            {
                                foundRival = MatchController.searchingGamers[i].UserId;//MatchController._searchingGamers[i];
                                rivalIndex = i;
                                _participants.Add(foundRival);
                            }

                        }
                        //удалить геймера из списка ...
                        MatchController.searchingGamers.RemoveAt(gamerIndex);//_searchingGamers
                        MatchController.searchingGamers.RemoveAt(rivalIndex);//_searchingGamers
                    }

                    //if (foundRival == null)
                    if (String.IsNullOrEmpty(foundRival))
                    {

                        rivalId = (from u in _dbContext.Users
                                   where u.Bot > 0 && !_dbContext.MatchGamers.Any(g => g.GamerId == u.Id && g.IsPlay)
                                   select u.Id).Distinct().OrderBy(u => Guid.NewGuid()).FirstOrDefault();

                        isBot = true;
                        MatchController.searchingGamers.RemoveAt(gamerIndex);//_searchingGamers
                    }
                    else
                    {
                        rivalId = foundRival;//foundRival.Id;
                        isBot = false;// foundRival.Bot > 0;
                    }

                    autoConfirm = true;
                }
                else // user given rivalCandidateId
                {
                    rivalId = rivalCandidateId;
                    //autoConfirm = true;
                }

                if (string.IsNullOrEmpty(rivalId))
                {

                    switch (lang)
                    {
                        case "en":
                            throw new Exception("Opponent not found!");
                        case "ru":
                            throw new Exception("Соперник не найден!");

                    }
                }

                var match = new Match()
                {
                    CardId = cardId,
                    CreateTime = DateTime.Now,
                };
                _dbContext.Matches.Add(match);
                _dbContext.SaveChanges();

                var gamerCard = GetOrAddGamerCard(gamerId, cardId, _dbContext, rivalCandidateId);

                if (rivalCandidateId != null)
                {
                    _dbContext.MatchGamers.Add(new MatchGamer
                    {
                        MatchId = match.Id,
                        GamerId = gamerId,
                        GamerCardId = gamerCard.Id,
                        Confirmed = true,
                        JoinTime = DateTime.Now
                    });
                }
                else
                {
                    _dbContext.MatchGamers.Add(new MatchGamer
                    {
                        MatchId = match.Id,
                        GamerId = gamerId,
                        GamerCardId = gamerCard.Id,
                        Confirmed = autoConfirm
                    });
                }
                _dbContext.SaveChanges();

                var rivalCard = GetOrAddGamerCard(rivalId, cardId, _dbContext, rivalCandidateId);

                var entity = new MatchGamer
                {
                    MatchId = match.Id,
                    GamerId = rivalId,
                    GamerCardId = rivalCard.Id,
                    Confirmed = autoConfirm
                };

                if (isBot)
                {
                    entity.JoinTime = DateTime.Now;
                    entity.Ready = true;
                    entity.Confirmed = true;
                }
                _dbContext.MatchGamers.Add(entity);
                _dbContext.SaveChanges();

                //socket to rival
                if (!isBot)
                {
                    if (string.IsNullOrEmpty(rivalCandidateId))
                    {
                        var rivalMatchModel = new MatchRequestModel(match.Id, _card, _dbContext.Users.Find(gamerId));
                        rivalMatchModel.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                        rivalMatchModel.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                        var gamerCardScore = _dbContext.GamerCards
                            .Where(gc => gc.CardId == cardId
                                && gc.GamerId == gamerId
                                && gc.IsActive).Select(gc => gc.Score).SingleOrDefault();
                        rivalMatchModel.GamerRaiting = _dbContext.GamerCards
                            .Where(gcr => gcr.CardId == cardId && gcr.IsActive)
                            .Count(gr => gr.Score > gamerCardScore) + 1;
                        rivalMatchModel.GamerCardScore = gamerCardScore;
                        var myCardScore = _dbContext.GamerCards
                            .Where(gc => gc.CardId == cardId
                                && gc.GamerId == rivalId
                                && gc.IsActive).Select(gc => gc.Score).SingleOrDefault(); ;
                        rivalMatchModel.MyCardScore = myCardScore;
                        rivalMatchModel.Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == rivalId) ?
                               _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == rivalId).Coins : 0;
                        rivalMatchModel.RivalCoins = _dbContext.UserCoins.Any(uc => uc.GamerId == gamerId) ?
                       _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == gamerId).Coins : 0;

                        await _gamersHandler.InvokeClientMethodToGroupAsync(rivalId, "searchPoolResult", rivalMatchModel);
                    }
                    else
                    {
                        var _opponent = _dbContext.Users.First(o => o.Id == gamerId).NickName;

                        var rivalMatchModel = new MatchRequestModel(match.Id, _card, _dbContext.Users.Find(gamerId));
                        rivalMatchModel.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                        rivalMatchModel.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                        var gamerCardScore = 0;

                        rivalMatchModel.GamerRaiting = 0;

                        rivalMatchModel.GamerCardScore = gamerCardScore;
                        var myCardScore = 0;

                        rivalMatchModel.MyCardScore = myCardScore;
                        rivalMatchModel.Bid = bid;
                        rivalMatchModel.Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == rivalId) ?
                                                _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == rivalId).Coins : 0;
                        rivalMatchModel.RivalCoins = _dbContext.UserCoins.Any(uc => uc.GamerId == gamerId) ?
                                                    _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == gamerId).Coins : 0;

                        _dbContext.MatchBids.Add(new MatchBid
                        {
                            MatchId = match.Id,
                            Bid = bid,
                            Status = false
                        });

                        _dbContext.SaveChanges();

                        var _users = new List<string>();
                        string _user = _dbContext.Users.First(u => u.Id == rivalId).OneSignalId;
                        _users.Add(_user);

                        var _buttonA = new OneSignalButtons();
                        _buttonA.id = "id1";
                        _buttonA.text = "Accept";
                        _buttonA.icon = "";
                        var _buttonC = new OneSignalButtons();
                        _buttonC.id = "id2";
                        _buttonC.text = "Cancel";
                        _buttonC.icon = "";
                        var my_buttons = new OneSignalButtons[] {
                            _buttonA, _buttonC
                        };


                        using (var client = new HttpClient())
                        {
                            var url = new Uri("https://onesignal.com/api/v1/notifications");
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NmMzNDMwMWQtYzlhNC00ZDdhLWFlODYtZjBhMWU1MzhhMWU4");
                            var obj = new
                            {
                                app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                                headings = new { en = "Приглашение на матч", ru = "Приглашение на матч" },
                                contents = new
                                {
                                    en = _opponent + " зовет вас на матч в карте " + _card,
                                    ru = _opponent + " зовет вас на матч в карте " + _card
                                },
                                data = new { matchRequest = rivalMatchModel },
                                buttons = my_buttons,
                                include_player_ids = _users,
                                ios_category = "cancel"
                            };

                            var seralizeSettings = new JsonSerializerSettings();
                            seralizeSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                            var json = JsonConvert.SerializeObject(obj, seralizeSettings);
                            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                            var response = await client.PostAsync(url, content);

                        }
                    }
                }


                //create questions  
                if (autoConfirm || rivalCandidateId != null)
                {
                    AutoConfirm(_dbContext, gamerId, match.Id);
                }


                model.Match = new MatchRequestModel(match.Id, _card, _dbContext.Users.Find(rivalId));
                model.Found = true;

                model.Match.CorrectAnswerScore = matchOptions.Value.CorrectAnswerScore;
                model.Match.IncorrectAnswerScore = matchOptions.Value.IncorrectAnswerScore;

                var rivalCardScore = 0;
                model.Match.GamerRaiting = 0;
                if (String.IsNullOrEmpty(rivalCandidateId))
                {
                    rivalCardScore = _dbContext.GamerCards
                    .Where(gc =>
                        gc.CardId == cardId
                        && gc.GamerId == rivalId && gc.IsActive)
                    .Select(gc => gc.Score)
                    .Last();//SingleOrDefault
                    model.Match.GamerRaiting = _dbContext.GamerCards
                    .Where(gcr => gcr.CardId == cardId && gcr.IsActive)
                    .Count(gr => gr.Score > rivalCardScore) + 1;
                }

                model.Match.GamerCardScore = rivalCardScore;
                model.Match.IsBot = isBot;
                model.Match.MyCardScore = gamerCard.Score;
                model.Match.Bid = bid;
                model.Match.Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == gamerId) ?
                       _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == gamerId).Coins : 0;
                model.Match.RivalCoins = _dbContext.UserCoins.Any(uc => uc.GamerId == rivalId) ?
                       _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == rivalId).Coins : 0;
                return model;
            }
        }

        private static GamerCard GetOrAddGamerCard(string gamerId, int cardId, ApplicationDbContext _dbContext, string rivalCandidateId)
        {
            var gamerCard =
                _dbContext.GamerCards.FirstOrDefault(gc => gc.CardId == cardId && gc.GamerId == gamerId && gc.IsActive);// 
            var IsActive = true;

            if (!String.IsNullOrEmpty(rivalCandidateId))
            { IsActive = false; }

            if (gamerCard == null)//
            {
                // Создаем новую карточку для игрока
                gamerCard = new GamerCard
                {
                    CardId = cardId,
                    GamerId = gamerId,
                    StartTime = DateTime.Now,
                    IsActive = IsActive
                };
                _dbContext.GamerCards.Add(gamerCard);
                _dbContext.SaveChanges();
            }
            return gamerCard;
        }

        public void AutoConfirm(ApplicationDbContext _dbContext, string gamerId, int matchId)
        {
            using (var tran = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    _logger.LogInformation($"Match confirm from {gamerId}");
                    var matchParticipant = _dbContext.MatchGamers.Include(mg => mg.Match)
                        .ThenInclude(m => m.Card)
                        .ThenInclude(c => c.Questions)
                        .FirstOrDefault(m => m.GamerId == gamerId && m.MatchId == matchId); //&& !m.JoinTime.HasValue

                    if (matchParticipant != null)
                    {
                        var match = matchParticipant.Match;
                        var cardId = match.CardId;

                        var matchParticipants = _dbContext.MatchGamers.Where(p => p.MatchId == matchId && !p.Delayed);

                        var participants = matchParticipants.Where(p => p.GamerId != gamerId).Select(p => p.GamerId).ToList();
                        if (string.IsNullOrEmpty(match.Questions))
                        {
                            bool isBot = false;
                            var firstGamerAnswers = new List<GamerAnswers>();
                            var secondGamerAnswers = new List<GamerAnswers>();

                            var opponentId = matchParticipants.First(p => p.GamerId != gamerId).GamerId;
                            if (_dbContext.Users.Any(u => u.Id == opponentId && u.Bot != 0))
                            { isBot = true; }

                            var _matchGamersFirst = _dbContext.MatchGamers.Where(mg => mg.GamerId == gamerId);
                            if (_matchGamersFirst.Count() > 0)
                            {
                                foreach (var _matchGamer in _matchGamersFirst)
                                {
                                    var _matchAnswers = _dbContext.MatchAnswers.Where(ma => ma.MatchGamerId == _matchGamer.Id);//.ToList();
                                    if (_matchAnswers.Count() > 0)
                                    {
                                        foreach (var _matchAnswer in _matchAnswers.ToList())
                                        {
                                            var _card = _dbContext.Questions.FirstOrDefault(q => q.Id == _matchAnswer.QuestionId);
                                            if (_card != null && _card.CardId == cardId)
                                            //if (_matchAnswer.Question.CardId == cardId)
                                            {
                                                if (firstGamerAnswers.Exists(fga => fga.QuestionId == _matchAnswer.QuestionId))
                                                {
                                                    var index = firstGamerAnswers.IndexOf(firstGamerAnswers.FirstOrDefault(fga => fga.QuestionId == _matchAnswer.QuestionId));
                                                    firstGamerAnswers[index].Count += 1;
                                                    if (_matchAnswer.IsCorrectAnswer)
                                                    {
                                                        firstGamerAnswers[index].IsCorrect += 1;
                                                    }

                                                }
                                                else
                                                {
                                                    var _ga = new GamerAnswers();
                                                    _ga.QuestionId = _matchAnswer.QuestionId;
                                                    _ga.Count = 1;
                                                    if (_matchAnswer.IsCorrectAnswer)
                                                    {
                                                        _ga.IsCorrect = 1;
                                                    }
                                                    else { _ga.IsCorrect = 0; }
                                                    firstGamerAnswers.Add(_ga);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!isBot)
                            {
                                var _matchGamersSecond = _dbContext.MatchGamers.Where(mg => mg.GamerId == opponentId);
                                if (_matchGamersSecond.Count() > 0)
                                {
                                    foreach (var _matchGamer in _matchGamersSecond)
                                    {
                                        var _matchAnswers = _dbContext.MatchAnswers.Where(ma => ma.MatchGamerId == _matchGamer.Id);//.ToList();
                                        if (_matchAnswers.Count() > 0)
                                        {
                                            foreach (var _matchAnswer in _matchAnswers.ToList())
                                            {
                                                var _card = _dbContext.Questions.FirstOrDefault(q => q.Id == _matchAnswer.QuestionId);
                                                if (_card != null && _card.CardId == cardId)
                                                //if (_matchAnswer.Question.CardId == cardId)
                                                {
                                                    if (secondGamerAnswers.Exists(fga => fga.QuestionId == _matchAnswer.QuestionId))
                                                    {
                                                        var index = secondGamerAnswers.IndexOf(secondGamerAnswers.FirstOrDefault(fga => fga.QuestionId == _matchAnswer.QuestionId));
                                                        secondGamerAnswers[index].Count += 1;
                                                        if (_matchAnswer.IsCorrectAnswer)
                                                        {
                                                            secondGamerAnswers[index].IsCorrect += 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var _ga = new GamerAnswers();
                                                        _ga.QuestionId = _matchAnswer.QuestionId;
                                                        _ga.Count = 1;
                                                        if (_matchAnswer.IsCorrectAnswer)
                                                        {
                                                            _ga.IsCorrect = 1;
                                                        }
                                                        else { _ga.IsCorrect = 0; }
                                                        secondGamerAnswers.Add(_ga);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }



                            var _questions = _dbContext.Questions.Where(q => q.CardId == cardId).ToList();
                            var questionCount = _dbContext.Cards.FirstOrDefault(c => c.Id == cardId).MatchQuestions;
                            Random rnd = new Random();
                            var randomQuestions = RandomQuestions(_questions, questionCount, firstGamerAnswers, secondGamerAnswers).OrderBy(qa => rnd.Next());
                            match.Questions = string.Join(',', randomQuestions);

                            match.StartTime = DateTime.Now;
                            //match.Questions = string.Join(',', match.Card.Questions
                            //                            .OrderBy(o => Guid.NewGuid())
                            //                            .Take(match.Card.MatchQuestions)
                            //                            .Select(q => q.Id));
                            _dbContext.SaveChanges();
                        }
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error {gamerId}");
                    tran.Rollback();
                    throw e;
                }
            }
        }

        public List<int> RandomQuestions(List<Question> questions, int questionCount, List<GamerAnswers> firstGamerAnswers, List<GamerAnswers> secondGamerAnswers)
        {
            var matchQuestions = new List<int>();


            foreach (var q in questions)
            {
                if (!firstGamerAnswers.Exists(ga => ga.QuestionId == q.Id)
                        && !secondGamerAnswers.Exists(ga => ga.QuestionId == q.Id)
                        && matchQuestions.Count < questionCount)
                {
                    matchQuestions.Add(q.Id);
                    if (matchQuestions.Count == questionCount)
                        return matchQuestions;
                }
            }

            var _adder = 0;
            //while (matchQuestions.Count < 10)
            do
            {
                foreach (var _fga in firstGamerAnswers.OrderBy(fga => fga.IsCorrect))
                {
                    if (secondGamerAnswers.Count != 0)
                    {
                        var second = secondGamerAnswers.FirstOrDefault(sga => sga.QuestionId == _fga.QuestionId);

                        if (second != null)
                        {
                            if (_fga.Count == second.Count ||
                                 _fga.Count == second.Count + _adder ||
                                _fga.Count + _adder == second.Count)
                            {
                                if (!matchQuestions.Contains(_fga.QuestionId))
                                    matchQuestions.Add(_fga.QuestionId);

                            }
                            /*
                            if (_fga.IsCorrect == second.IsCorrect || 
                                _fga.IsCorrect == second.IsCorrect + _adder || 
                                _fga.IsCorrect + _adder == second.IsCorrect)
                            {
                                if(!matchQuestions.Contains(_fga.QuestionId))
                                    matchQuestions.Add(_fga.QuestionId);
                            }
                            */

                        }
                        else
                        {
                            if (!matchQuestions.Contains(_fga.QuestionId))
                                matchQuestions.Add(_fga.QuestionId);
                        }

                        if (matchQuestions.Count == questionCount)
                            return matchQuestions;
                    }
                    else
                    {
                        if (!matchQuestions.Contains(_fga.QuestionId))
                            matchQuestions.Add(_fga.QuestionId);
                        if (matchQuestions.Count == questionCount)
                            return matchQuestions;
                    }

                }
                _adder++;
            } while (matchQuestions.Count < questionCount);

            return matchQuestions;
        }

        public async Task<ConfirmResponseModel> Confirm(string gamerId, int matchId)
        {
            //  await semaphoreSlim.WaitAsync();
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == gamerId).Lang;
                using (var tran = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _logger.LogInformation($"Match confirm from {gamerId}");
                        var matchParticipant = _dbContext.MatchGamers.Include(mg => mg.Match)
                            .ThenInclude(m => m.Card)
                            .ThenInclude(c => c.Questions)
                            .FirstOrDefault(m => m.GamerId == gamerId && m.MatchId == matchId && !m.JoinTime.HasValue);

                        if (matchParticipant != null)
                        {
                            var match = matchParticipant.Match;
                            var cardId = match.CardId;
                            if (match.Status == Match.MatchStatus.Cancelled)
                            {
                                switch (lang)
                                {
                                    case "en":
                                        throw new Exception("Match cancelled!");
                                    case "ru":
                                        throw new Exception("Матч отменен!");

                                }

                            }

                            if (!matchParticipant.Confirmed)
                            {
                                //matchParticipant.Delayed = false;
                                matchParticipant.Confirmed = true;
                                matchParticipant.JoinTime = DateTime.Now;
                                if (!matchParticipant.Delayed && match.Status == Match.MatchStatus.Requested)
                                {
                                    match.Status = Match.MatchStatus.Confirmed;
                                }
                                _dbContext.SaveChanges();
                            }

                            var matchParticipants = _dbContext.MatchGamers.Where(p => p.MatchId == matchId && !p.Delayed);
                            var confirm = new ConfirmResponseModel
                            {
                                MatchId = matchId,
                                Confirmed = matchParticipant.Delayed || matchParticipants.All(p => p.Confirmed)
                            };

                            var opponent = matchParticipants.First(p => p.GamerId != gamerId).GamerId;
                            if (confirm.Confirmed && string.IsNullOrEmpty(match.Questions))
                            {
                                bool isBot = false;
                                var firstGamerAnswers = new List<GamerAnswers>();
                                var secondGamerAnswers = new List<GamerAnswers>();

                                var opponentId = opponent;
                                if (_dbContext.Users.Any(u => u.Id == opponentId && u.Bot != 0))
                                { isBot = true; }

                                var _matchGamersFirst = _dbContext.MatchGamers.Where(mg => mg.GamerId == gamerId);
                                if (_matchGamersFirst.Count() > 0)
                                {
                                    foreach (var _matchGamer in _matchGamersFirst)
                                    {
                                        var _matchAnswers = _dbContext.MatchAnswers.Where(ma => ma.MatchGamerId == _matchGamer.Id);//.ToList();
                                        if (_matchAnswers.Count() > 0)
                                        {
                                            foreach (var _matchAnswer in _matchAnswers.ToList())
                                            {
                                                var _card = _dbContext.Questions.FirstOrDefault(q => q.Id == _matchAnswer.QuestionId);
                                                if (_card != null && _card.CardId == cardId)
                                                //if (_matchAnswer.Question.CardId == cardId)
                                                {
                                                    if (firstGamerAnswers.Exists(fga => fga.QuestionId == _matchAnswer.QuestionId))
                                                    {
                                                        var index = firstGamerAnswers.IndexOf(firstGamerAnswers.FirstOrDefault(fga => fga.QuestionId == _matchAnswer.QuestionId));
                                                        firstGamerAnswers[index].Count += 1;
                                                        if (_matchAnswer.IsCorrectAnswer)
                                                        {
                                                            firstGamerAnswers[index].IsCorrect += 1;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        var _ga = new GamerAnswers();
                                                        _ga.QuestionId = _matchAnswer.QuestionId;
                                                        _ga.Count = 1;
                                                        if (_matchAnswer.IsCorrectAnswer)
                                                        {
                                                            _ga.IsCorrect = 1;
                                                        }
                                                        else { _ga.IsCorrect = 0; }
                                                        firstGamerAnswers.Add(_ga);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!isBot)
                                {
                                    var _matchGamersSecond = _dbContext.MatchGamers.Where(mg => mg.GamerId == opponentId);
                                    if (_matchGamersSecond.Count() > 0)
                                    {
                                        foreach (var _matchGamer in _matchGamersSecond)
                                        {
                                            var _matchAnswers = _dbContext.MatchAnswers.Where(ma => ma.MatchGamerId == _matchGamer.Id).ToList();
                                            if (_matchAnswers.Count() > 0)
                                            {
                                                foreach (var _matchAnswer in _matchAnswers.ToList())
                                                {
                                                    var _card = _dbContext.Questions.FirstOrDefault(q => q.Id == _matchAnswer.QuestionId);
                                                    if (_card != null && _card.CardId == cardId)
                                                    //if (_matchAnswer.Question.CardId == cardId)
                                                    {
                                                        if (secondGamerAnswers.Exists(fga => fga.QuestionId == _matchAnswer.QuestionId))
                                                        {
                                                            var index = secondGamerAnswers.IndexOf(secondGamerAnswers.FirstOrDefault(fga => fga.QuestionId == _matchAnswer.QuestionId));
                                                            secondGamerAnswers[index].Count += 1;
                                                            if (_matchAnswer.IsCorrectAnswer)
                                                            {
                                                                secondGamerAnswers[index].IsCorrect += 1;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            var _ga = new GamerAnswers();
                                                            _ga.QuestionId = _matchAnswer.QuestionId;
                                                            _ga.Count = 1;
                                                            if (_matchAnswer.IsCorrectAnswer)
                                                            {
                                                                _ga.IsCorrect = 1;
                                                            }
                                                            else { _ga.IsCorrect = 0; }
                                                            secondGamerAnswers.Add(_ga);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }


                                var _questions = _dbContext.Questions.Where(q => q.CardId == match.CardId).ToList();
                                var questionCount = _dbContext.Cards.FirstOrDefault(c => c.Id == cardId).MatchQuestions;
                                Random rnd = new Random();
                                var randomQuestions = RandomQuestions(_questions, questionCount, firstGamerAnswers, secondGamerAnswers).OrderBy(qa => rnd.Next());
                                match.Questions = string.Join(',', randomQuestions);
                                //match.Questions = string.Join(',', RandomQuestions(_questions, firstGamerAnswers, secondGamerAnswers));

                                match.StartTime = DateTime.Now;
                                //match.Questions = string.Join(',', match.Card.Questions
                                //    .OrderBy(o => Guid.NewGuid())
                                //    .Take(match.Card.MatchQuestions)
                                //    .Select(q => q.Id));
                                _dbContext.SaveChanges();
                            }


                            if (!matchParticipant.Delayed)
                            {
                                //foreach (var participant in participants)
                                //{
                                await _gamersHandler.InvokeClientMethodToGroupAsync(opponent, "matchConfirmed", confirm);
                                _logger.LogInformation($"matchConfirmed for {opponent}");
                                //}
                            }
                            tran.Commit();
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

                var _error = string.Empty;
                switch (lang)
                {
                    case "en":
                        _error = "Match not found!";
                        break;
                    case "ru":
                        _error = "Матч не найден!";
                        break;

                }
                throw new Exception(_error);

            }
        }

        public async Task<MatchResultModel> GetMatchResult(int id, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();
                bool _winnerBonus = false;
                var bid = 0;
                lock (_locker)
                {
                    MatchGamer currentMatchGamer = null;
                    ApplicationUser currentGamer = null;

                    var match = _dbContext.Matches.Find(id);
                    var matchGamers = _dbContext.MatchGamers.Include(m => m.Answers).Include(m => m.Gamer).Where(g => g.MatchId == id);


                    var opponentMG = _dbContext.MatchGamers.First(mg => mg.MatchId == id && mg.GamerId != userId);
                    var IsBot = _dbContext.Users.Any(u => u.Id == opponentMG.GamerId && u.Bot > 0);

                    //foreach (var mg in matchGamers)
                    //{
                    //    IsBot = _dbContext.Users.Any(u => u.Id == mg.GamerId && u.Bot > 0);
                    //}
                    var delayed = matchGamers.Any(mg => mg.Delayed);
                    var matchWinners = new List<MatchGamer>();
                    /*
                    if (matchGamers.Any(g => g.Delayed && (!g.Ready || g.IsPlay)))
                    {
                        var matchGamer = matchGamers.Single(g => g.GamerId == userId);
                        matchGamer.IsPlay = false;
                        _dbContext.SaveChanges();
                        transaction.Commit();
                        ModelState.AddModelError(String.Empty, "Результат будет готов когда другие игроки доиграют свои матчи!");
                        return BadRequest(ModelState);
                    }
                    */

                    var resultModel = new MatchResultModel();

                    int winnerScore = 0;

                    var _gamer = matchGamers.First(_mg => _mg.GamerId == userId);
                    var entryPoint = _dbContext.Cards.First(c =>c.Id == match.CardId).EntryPoint;

                    if ((DateTime.Now - Convert.ToDateTime(_gamer.JoinTime)).TotalSeconds > 1)
                    {
                        _gamer.Gamer.PointsToPlay -= entryPoint;
                        _gamer.JoinTime = DateTime.Now;
                    }


                    #region Считает баллы  всегда без условии
                    foreach (var matchGamer in matchGamers)
                    {
                        var gamer = matchGamer.Gamer;
                        var questionsCount = match.Questions.SplitToIntArray().Length;
                        var answersCount = matchGamer.Answers.Count();
                        var correctAnswersCount = matchGamer.Answers.Count(a => a.IsCorrectAnswer);
                        var incorrectAnswersCount = questionsCount - correctAnswersCount;

                        if (questionsCount == answersCount)
                        { _winnerBonus = true; }

                        var pointsForMatch = correctAnswersCount * matchOptions.Value.CorrectAnswerScore +
                                             incorrectAnswersCount * matchOptions.Value.IncorrectAnswerScore;
                        matchGamer.Score = pointsForMatch;
                        matchGamer.IsPlay = false;

                        /*
                            var gamerCard = _dbContext.GamerCards.Last(gc =>
                                gc.CardId == match.CardId && gc.GamerId == matchGamer.GamerId &&
                                gc.IsActive);
                                */

                        //matchGamerBonuses.Add(new Tuple<MatchGamer, GamerCard, int>(matchGamer, gamerCard,
                        //answersCount * matchOptions.Value.BonusForAnswer));

                        //gamerCard.Score += matchGamer.Score + matchGamer.Bonus; // Добавляем (или отнимаем) очки к карте игрока 
                        //gamer.Score += matchGamer.Score + matchGamer.Bonus; // Прибавляем текущие очки игроку
                        //gamer.PointsToPlay--;


                        //if (matchGamer.Bonus > 0)
                        //{
                        //    resultModel.MatchScore = matchGamer.Score;
                        //    resultModel.RivalMatchScore = 0;
                        //}
                        //else {
                        //    resultModel.MatchScore = 0;
                        //    resultModel.RivalMatchScore = matchGamer.Score;
                        //}

                        if (_winnerBonus)
                        {
                            if (matchGamer.Score > winnerScore) // + matchGamer.Bonus
                            {
                                matchWinners.Clear();
                                winnerScore = matchGamer.Score + matchGamer.Bonus;
                                matchWinners.Add(matchGamer);
                            }
                            else if (matchGamer.Score == winnerScore)// + matchGamer.Bonus
                            {
                                matchWinners.Add(matchGamer);
                            }
                        }
                        else
                        {
                            if (!matchGamer.Cancelled)
                            {
                                matchWinners.Add(matchGamer);
                            }
                        }



                        if (matchGamer.GamerId == userId)
                        {
                            currentMatchGamer = matchGamer;
                            currentGamer = gamer;
                            resultModel.MatchScore = matchGamer.Score;//currentMatchGamer.Score
                        }
                        else
                        {
                            resultModel.RivalMatchScore = matchGamer.Score;
                        }
                    }

                    if (matchWinners.Count() == 1)//&& !matchWinners.Any(mw => mw.Delayed)
                    {
                        var winner = matchWinners.First();
                        winner.IsWinner = true;
                        if (!IsBot)
                        {

                            var matchBid = _dbContext.MatchBids.FirstOrDefault(mb => mb.MatchId == id);
                            if (matchBid != null)
                            {
                                matchBid.Status = true;
                                matchBid.Winner = winner.GamerId;
                                bid = matchBid.Bid;
                                _dbContext.SaveChanges();
                            }
                        }

                        if (_winnerBonus && !delayed && bid == 0)
                        {
                            winner.Bonus = matchOptions.Value.BonusForWin;
                        }
                    }

                    if (matchWinners.Count() == 2)
                    {
                        foreach (var mw in matchWinners)
                        {
                            mw.IsWinner = false;
                            /*
                            mw.Score += mw.Bonus;
                            */
                            var matchBid = _dbContext.MatchBids.FirstOrDefault(mb => mb.MatchId == id);
                            if (matchBid != null)
                            {
                                matchBid.Status = true;
                                matchBid.Winner = null;
                                bid = matchBid.Bid;
                                _dbContext.SaveChanges();
                            }
                        }
                    }

                    if (!delayed)
                    {
                        match.Status = Match.MatchStatus.Finished;
                    }

                    _dbContext.SaveChanges();
                    #endregion

                    #region считает баллы по условию
                    /*
                    if (match.Status == Match.MatchStatus.Started)
                    {
                    foreach (var matchGamer in matchGamers)
                        {
                            var gamer = matchGamer.Gamer;
                            if (matchGamer.IsPlay && matchGamer.JoinTime.HasValue)
                            {
                                var questionsCount = match.Questions.SplitToIntArray().Length;
                                var answersCount = matchGamer.Answers.Count();
                                var correctAnswersCount = matchGamer.Answers.Count(a => a.IsCorrectAnswer);
                                var incorrectAnswersCount = questionsCount - correctAnswersCount;

                                var pointsForMatch = correctAnswersCount * matchOptions.Value.CorrectAnswerScore +
                                                     incorrectAnswersCount * matchOptions.Value.IncorrectAnswerScore;
                                matchGamer.Score = pointsForMatch;
                                matchGamer.IsPlay = false;

                                var gamerCard = _dbContext.GamerCards.Single(gc =>
                                    gc.CardId == match.CardId && gc.GamerId == matchGamer.GamerId &&
                                    gc.IsActive);

                                //matchGamerBonuses.Add(new Tuple<MatchGamer, GamerCard, int>(matchGamer, gamerCard,
                                //answersCount * matchOptions.Value.BonusForAnswer));

                                gamerCard.Score += matchGamer.Score; // Добавляем (или отнимаем) очки к карте игрока 
                                gamer.Score += matchGamer.Score; // Прибавляем текущие очки игроку
                                gamer.PointsToPlay--;
                                if (matchGamer.Score > winnerScore)
                                {
                                    matchWinners.Clear();
                                    winnerScore = matchGamer.Score;
                                    matchWinners.Add(matchGamer);
                                }
                                else if (matchGamer.Score == winnerScore)
                                {
                                    matchWinners.Add(matchGamer);
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
                        // Тут добавляем бонусы
                        if (matchWinners.Count() == 1)
                        {
                            var winner = matchWinners.First();
                            winner.IsWinner = true;
                            winner.Bonus = matchOptions.Value.BonusForWin;
                            winner.Score += matchOptions.Value.BonusForWin;
                        }
                        else
                        {
                            matchWinners.ForEach((MatchGamer obj) => { obj.IsWinner = false; });
                        }
                        match.Status = Match.MatchStatus.Finished;
                    }
                    
                    else
                    {
                        currentMatchGamer = matchGamers.First(mg => mg.GamerId == userId);
                        currentGamer = currentMatchGamer.Gamer;
                        resultModel.RivalMatchScore = matchGamers.First(mg => mg.GamerId != userId).Score;
                    }
                    */
                    #endregion

                    var thisMG = _dbContext.MatchGamers.First(mg => mg.MatchId == id && mg.GamerId == userId);

                    if (bid == 0)
                    {
                        var gamerCard = _dbContext.GamerCards.Last(gc =>
                               gc.CardId == match.CardId && gc.GamerId == thisMG.GamerId &&
                               gc.IsActive);

                        if (!thisMG.Cancelled)
                        {
                            gamerCard.Score += thisMG.Score + thisMG.Bonus;
                            gamerCard.Gamer.TotalScore += thisMG.Score + thisMG.Bonus;
                            gamerCard.Gamer.Score += thisMG.Score + thisMG.Bonus;

                            var thisCoins = thisMG.Score + thisMG.Bonus;
                            var gamerCoins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == thisMG.GamerId);
                            if (gamerCoins == null)
                            {
                                _dbContext.UserCoins.Add(new UserCoins
                                {
                                    GamerId = thisMG.GamerId,
                                    Coins = thisCoins,
                                    LastUpdate = DateTime.Now
                                });
                            }
                            else
                            {
                                gamerCoins.Coins += thisCoins;
                                gamerCoins.LastUpdate = DateTime.Now;
                            }
                        }

                    }
                    /*
                    if (bid != 0)
                    {
                        var thisCoins = bid;//thisMG.Score + thisMG.Bonus;
                        var gamerCoins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == thisMG.GamerId);
                        if (gamerCoins == null)
                        {
                            _dbContext.UserCoins.Add(new UserCoins
                            {
                                GamerId = thisMG.GamerId,
                                Coins = thisCoins,
                                LastUpdate = DateTime.Now
                            });
                        }
                        else
                        {
                            gamerCoins.Coins += thisCoins;
                            gamerCoins.LastUpdate = DateTime.Now;
                        }
                    }
                    */




                    if (IsBot)
                    {
                        var opponentCard = _dbContext.GamerCards.Last(gc => gc.CardId == match.CardId &&
                                gc.GamerId == opponentMG.GamerId && gc.IsActive);

                        opponentCard.Score += opponentMG.Score + opponentMG.Bonus;
                        opponentCard.Gamer.TotalScore += opponentMG.Score + opponentMG.Bonus;
                        opponentCard.Gamer.Score += opponentMG.Score + opponentMG.Bonus;


                        var oppoCoins = opponentMG.Score + opponentMG.Bonus;
                        var opponentCoins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == opponentMG.GamerId);
                        if (opponentCoins == null)
                        {
                            _dbContext.UserCoins.Add(new UserCoins
                            {
                                GamerId = opponentMG.GamerId,
                                Coins = oppoCoins,
                                LastUpdate = DateTime.Now
                            });
                        }
                        else
                        {
                            opponentCoins.Coins += oppoCoins;
                            opponentCoins.LastUpdate = DateTime.Now;
                        }

                    }
                    _dbContext.SaveChanges();

                    resultModel.IsWinner = currentMatchGamer.IsWinner;
                    resultModel.RivalIsWinner = matchGamers.First(mg => mg.GamerId != userId).IsWinner;

                    if (!_winnerBonus)
                    {
                        if (currentMatchGamer.Bonus > 0)
                        {
                            resultModel.MatchScore = currentMatchGamer.Score;
                            resultModel.RivalMatchScore = 0;//matchGamers.First(mg => mg.GamerId != userId).Bonus;
                        }
                        else
                        {
                            resultModel.MatchScore = 0;// currentMatchGamer.Bonus;
                            resultModel.RivalMatchScore = matchGamers.First(mg => mg.GamerId != userId).Score;
                        }
                    }

                    resultModel.MatchBonus = currentMatchGamer.Bonus;
                    resultModel.RivalMatchBonus = matchGamers.First(mg => mg.GamerId != userId).Bonus;

                    var userCoins = _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == userId);
                    if (resultModel.IsWinner)
                    {
                        resultModel.PrizeCoins = bid * 2;
                        if (userCoins != null)
                        {
                            userCoins.Coins += bid;
                            userCoins.LastUpdate = DateTime.Now;
                        }
                        else
                        {
                            _dbContext.UserCoins.Add(new UserCoins
                            {
                                GamerId = userId,
                                Coins = bid,
                                LastUpdate = DateTime.Now
                            });

                        }
                        _dbContext.SaveChanges();
                    }
                    else if (resultModel.RivalIsWinner)
                    {
                        resultModel.PrizeCoins = 0;
                        if (userCoins != null)
                        {
                            userCoins.Coins -= bid;
                        }
                        else
                        {
                            _dbContext.UserCoins.Add(new UserCoins
                            {
                                GamerId = userId,
                                Coins = 0 - bid,
                                LastUpdate = DateTime.Now
                            });

                        }
                        _dbContext.SaveChanges();
                    }
                    else if (!resultModel.IsWinner && !resultModel.RivalIsWinner)
                    {
                        resultModel.PrizeCoins = bid;
                    }
                    return resultModel;
                }
            }
        }

        public async Task<LiveResultModel> LiveMatchResult(int id, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();

                lock (_locker)
                {
                    var user = _dbContext.Users.Find(userId);
                    var match = _dbContext.Matches.Find(id);
                    var questionsCount = match.Questions.SplitToIntArray().Length;

                    var matchGamer = _dbContext.MatchGamers.Include(m => m.Answers).Include(m => m.Gamer).First(g => g.MatchId == id && g.GamerId == userId);

                    var userCoins = _dbContext.UserCoins.First(uc => uc.GamerId == userId);
                    var entryPoint = _dbContext.Cards.First(c =>c.Id == match.CardId).EntryPoint;

                    user.PointsToPlay -= entryPoint;

                    var matchCoins = 0;
                    var isWinner = false;
                    var prize = Convert.ToInt32(_dbContext.Cards.First(c => c.Id == match.CardId).Prize);

                    //lifes
                    var incorrectAnswers = _dbContext.MatchAnswers.Count(ma => ma.MatchGamerId == matchGamer.Id && !ma.IsCorrectAnswer);
                    var lifeLimits = _dbContext.CardLimits.FirstOrDefault(cl => cl.CardId == match.CardId).Lifes;
                    var lifes = user.Lifes + 1;
                    user.Lifes = lifes - incorrectAnswers;

                    if (user.Lifes == 0)
                    {
                        isWinner = false;
                        var answersCount = matchGamer.Answers.Count();
                        var correctAnswersCount = matchGamer.Answers.Count(a => a.IsCorrectAnswer);
                        var incorrectAnswersCount = questionsCount - correctAnswersCount;

                        var pointsForMatch = correctAnswersCount * matchOptions.Value.CorrectAnswerScore +
                                             incorrectAnswersCount * matchOptions.Value.IncorrectAnswerScore;
                        matchCoins = pointsForMatch * correctAnswersCount;
                        userCoins.Coins += matchCoins;

                        matchGamer.Score = matchCoins;

                        _dbContext.SaveChanges();
                    }

                    var matchWinners = new List<MatchWinner>();

                    var matchGamers = _dbContext.MatchGamers.Include(m => m.Answers).Include(m => m.Gamer).Where(mg => mg.MatchId == id && !mg.Cancelled);

                    foreach (var _mg in matchGamers)
                    {
                        var correctAnswersCount = matchGamer.Answers.Count(a => a.IsCorrectAnswer);
                        var gamer = _dbContext.Users.Find(_mg.GamerId);
                        if (correctAnswersCount == questionsCount && gamer.Lifes != 0)
                        {
                            if (_mg.GamerId == userId)
                            {
                                isWinner = true;
                            }
                            var winner = new MatchWinner();
                            winner.NickName = gamer.NickName;
                            winner.PhotoUrl = gamer.PhotoUrl;
                            matchWinners.Add(winner);
                        }
                    }

                    _dbContext.SaveChanges();

                    var resultModel = new LiveResultModel();
                    resultModel.IsWinner = isWinner;
                    resultModel.Lifes = user.Lifes;
                    resultModel.Prize = prize;
                    resultModel.UserCoins = userCoins.Coins;
                    resultModel.Coins = matchCoins;
                    resultModel.Winners = matchWinners;
                    resultModel.WinnerPrize = prize / (matchWinners.Any() ? matchWinners.Count() : 1);

                    return resultModel;
                }
            }
        }

        public async Task<bool> GetQuestionAnswerStatus(int id, string userId, int questionId)
        {
            var result = false;
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchOptions = scope.ServiceProvider.GetService<IOptions<MatchOptions>>();
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == userId).Lang;
                try
                {
                    await _semaphoreSlim.WaitAsync();

                    var matchGamer = _dbContext.MatchGamers.Single(g => g.MatchId == id && g.GamerId == userId);

                    var lastActiveTime = DateTime.Now - matchGamer.JoinTime;

                    var lastQuestionAnswer = _dbContext.MatchAnswers
                        .Where(a => a.MatchGamerId == matchGamer.Id && a.QuestionId != questionId)
                        .OrderByDescending(a => a.CreatedAt).FirstOrDefault();

                    if (lastQuestionAnswer != null)
                    {
                        lastActiveTime = DateTime.Now - lastQuestionAnswer.CreatedAt;
                    }

                    if (!matchGamer.Cancelled && matchGamer.IsPlay
                        && lastActiveTime > matchOptions.Value.TimeForOneQuestion)
                    {
                        var match = _dbContext.Matches.Find(id);
                        var matchQuestions = match.Questions.SplitToIntArray();

                        var questionIndx = matchQuestions.IndexOf(questionId);
                        if ((lastQuestionAnswer == null && questionIndx == 0)
                            || (lastQuestionAnswer != null &&
                                questionIndx - matchQuestions.IndexOf(lastQuestionAnswer.QuestionId) == 1))
                        {
                            var answer = _dbContext.MatchAnswers.FirstOrDefault(a =>
                                a.QuestionId == questionId && a.MatchGamerId == matchGamer.Id);

                            var matchController = scope.ServiceProvider.GetService<MatchController>();

                            if (answer == null)
                            {
                                await matchController.AnswerToQuestions(id, new MatchQuestionAnswerModel()
                                {
                                    QuestionId = questionId
                                }, userId);

                                await GetMissedQuestionsForMatch(match.Id, matchGamer.GamerId);
                            }

                            var matchGamers = _dbContext.MatchGamers.Where(g => g.MatchId == id && g.GamerId != userId)
                                .ToList();
                            foreach (var gamer in matchGamers)
                            {
                                if (!_dbContext.MatchAnswers.Any(a =>
                                    a.MatchGamerId == gamer.Id && a.QuestionId == questionId))
                                {
                                    await matchController.AnswerToQuestions(id, new MatchQuestionAnswerModel()
                                    {
                                        QuestionId = questionId
                                    }, gamer.GamerId);
                                    await GetMissedQuestionsForMatch(match.Id, gamer.GamerId);
                                }
                            }
                            result = true;
                        }
                    }
                    else
                    {
                        switch (lang)
                        {
                            case "en":
                                throw new Exception("Match ended, or request timed out!");
                            case "ru":
                                throw new Exception("Матч окончен, либо не пришло время для ответа.");

                        }

                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while getting question status!");
                    throw e;
                }
                finally
                {
                    try
                    {
                        _semaphoreSlim.Release();
                    }
                    catch (Exception e)
                    {

                    }
                }
                return result;
            }
        }

        public async Task<List<int>> GetMissedQuestionsForMatch(int matchId, string gamerId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var matchOptions = 3;//scope.ServiceProvider.GetService<IOptions<MatchOptions>>();
                var match = dbContext.Matches.Find(matchId);
                var matchQuestions = match.Questions.SplitToIntArray();

                var matchAnswers = (from a in dbContext.MatchAnswers
                                    join g in dbContext.MatchGamers on a.MatchGamerId equals g.Id
                                    where g.MatchId == matchId && g.GamerId == gamerId
                                    orderby a.CreatedAt descending
                                    select a).ToList();

                List<int> missedQuestions = new List<int>();
                foreach (var answer in matchAnswers)
                {
                    if (!answer.AnswerId.HasValue)
                    {
                        missedQuestions.Add(answer.QuestionId);
                    }
                    else
                    {
                        break;
                    }
                }
                if (missedQuestions.Count > matchOptions)
                {
                    var gamers = dbContext.MatchGamers.Where(g => g.MatchId == matchId);
                    foreach (var game in gamers)
                    {
                        game.Cancelled = true;
                        game.IsPlay = false;

                        await _gamersHandler.InvokeClientMethodToGroupAsync(game.GamerId, "matchStoped",
                            new { id = game.MatchId, gamerId = game.GamerId, reason = "Матч был отменен. Соперник покинул игру." });
                    }
                    await dbContext.SaveChangesAsync();
                }

                /*
                var nextQuestionIndex = 0;

                if (missedQuestions.Any())
                {
                    nextQuestionIndex = matchQuestions.IndexOf(missedQuestions[0]) + 1;
                }

                missedQuestions.Insert(0, matchQuestions[nextQuestionIndex]);
                
                if (missedQuestions.Count > matchOptions.Value.MissedQuestionsCount)
                {
                    var gamers = dbContext.MatchGamers.Where(g => g.MatchId == matchId);
                    foreach (var game in gamers)
                    {
                        game.Cancelled = true;
                        game.IsPlay = false;

                        await _gamersHandler.InvokeClientMethodToGroupAsync(game.GamerId, "matchStoped",
                            new { id = game.MatchId, gamerId = game.GamerId, reason = "Матч был отменен. Соперник покинул игру." });
                    }
                    await dbContext.SaveChangesAsync();
                }
                */
                return missedQuestions;
            }
        }
    }
}