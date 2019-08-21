using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Core.Game;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Match;
using RobiGroup.AskMeFootball.Models.Questions;
using RobiGroup.Web.Common;
using RobiGroup.Web.Common.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OneSignal;
using OneSignal.CSharp;
using OneSignal.CSharp.SDK;
using OneSignal.CSharp.SDK.Resources;
using OneSignal.CSharp.SDK.Serializers;
using OneSignal.CSharp.SDK.Resources.Notifications;
using OneSignal.CSharp.SDK.Resources.Devices;
using System.Net.Http;
using System.Net.Http.Headers;


namespace RobiGroup.AskMeFootball.Controllers
{
    /// <summary>
    /// Игра
    /// </summary>
    [Route("api/match")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MatchController : ControllerBase
    {
        private readonly IMatchManager _matchManager;
        private readonly ILogger<MatchController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly GamersHandler _gamersHandler;

        public static List<String> _searchingGamers = new List<String>();
        public static List<SearchingGamers> searchingGamers = new List<SearchingGamers>();

        //public static List<MatchGamer> matchParticipants = new List<MatchGamer>();

        public MatchController(IMatchManager matchManager, ILogger<MatchController> logger,
            ApplicationDbContext dbContext, GamersHandler gamersHandler)
        {
            _matchManager = matchManager;
            _logger = logger;
            _dbContext = dbContext;
            _gamersHandler = gamersHandler;
        }

        /// <summary>
        /// История игр
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="count">Количество записей на странице</param>
        /// <returns></returns>
        [HttpGet("history")]
        public IActionResult History(int page = 1, int count = 10)
        {
            string gamerId = User.GetUserId();

            var myMatches = _dbContext.MatchGamers.Where(mg => mg.GamerId == gamerId && mg.Confirmed && mg.JoinTime.HasValue && !mg.IsPlay && mg.Ready).OrderByDescending(mg => mg.JoinTime);

            var history = new List<MatchHistoryModel>();

            foreach (var mm in myMatches)
            {
                var match = _dbContext.Matches.FirstOrDefault(m => m.Id == mm.MatchId && m.Status == Match.MatchStatus.Finished);//Match.MatchStatus.Delayed
                if (match != null)
                {
                    var matchOpponent = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == mm.MatchId && mg.GamerId != gamerId && mg.Ready);
                    var gamerExist = _dbContext.Users.Any(u => u.Id == matchOpponent.GamerId);
                    if (matchOpponent != null && gamerExist)
                    {
                        if (matchOpponent.JoinTime.HasValue && matchOpponent.Confirmed)
                        {
                            var opponentPhoto = String.Empty;
                            var opponentNickName = String.Empty;
                            var opponentUser = _dbContext.Users.FirstOrDefault(u => u.Id == matchOpponent.GamerId);
                            if (opponentUser != null)
                            {
                                opponentPhoto = opponentUser.PhotoUrl;
                                opponentNickName = opponentUser.NickName;
                            }

                            var card = _dbContext.Cards.First(c => c.Id == match.CardId);
                            var h = new MatchHistoryModel();
                            h.Id = mm.MatchId;
                            h.CardName = card.Name;
                            h.Score = mm.Score;
                            h.PhotoUrl = opponentPhoto;
                            h.GamerName = opponentNickName;//_dbContext.Users.FirstOrDefault(u => u.Id == matchOpponent.GamerId).NickName;//mm.Gamer.NickName;
                            h.IsWon = mm.IsWinner;
                            h.Time = mm.JoinTime.Value.AddHours(6);
                            h.RivalIsWon = matchOpponent.IsWinner;
                            history.Add(h);
                        }
                    }
                }
            }
            /*
            return Ok((from g in _dbContext.MatchGamers
                       join m in _dbContext.Matches on g.MatchId equals m.Id
                       join c in _dbContext.Cards on m.CardId equals c.Id
                       join rg in _dbContext.MatchGamers on m.Id equals rg.MatchId
                       join ru in _dbContext.Users on rg.GamerId equals ru.Id
                       where g.Confirmed && g.JoinTime.HasValue && !g.IsPlay && g.GamerId == gamerId && rg.GamerId != gamerId && rg.Confirmed && rg.JoinTime.HasValue && m.Status != Match.MatchStatus.Delayed
                       orderby g.JoinTime descending 
                       select new MatchHistoryModel
                       {
                           Id = m.Id,
                           CardName = c.Name,
                           Score = g.Score,
                           PhotoUrl = ru.PhotoUrl,
                           GamerName = ru.NickName,
                           IsWon = g.IsWinner,
                           Time = g.JoinTime.Value,
                           RivalIsWon = rg.IsWinner
                       }).Skip((page - 1) * count).Take(count).ToList());

            */
            return Ok(history.Skip((page - 1) * count).Take(count).ToList());
        }

        /// <summary>
        /// Запрос на матч
        /// </summary>
        /// <param name="id">ID карты</param>
        /// <param name="gamerId">ID игрока</param>
        /// <param name="bid">Ставка</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/request/{gamerId}/{bid}")]
        [ProducesResponseType(typeof(MatchSearchResultModel), 200)]
        public async Task<IActionResult> Search([FromRoute]int id, [FromRoute]string gamerId, [FromRoute]int bid)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == userId).Lang;
                var gamer = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
                var opponent = _dbContext.Users.FirstOrDefault(u => u.Id == gamerId);
                var entryPoint = _dbContext.Cards.First(c => c.Id == id).EntryPoint;
                if (gamer.PointsToPlay < entryPoint)
                {
                    var errorPoints = string.Empty;
                    switch (lang)
                    {
                        case "en":
                            errorPoints = "Not enough points to play.";
                            break;
                        case "ru":
                            errorPoints = "Недостаточно мячей для игры.";
                            break;

                    }
                    //throw new Exception(errorPoints);
                    ModelState.AddModelError(string.Empty, errorPoints);
                    return BadRequest(ModelState);
                }
                if (opponent.PointsToPlay < entryPoint)
                {
                    var errorPoints = string.Empty;
                    switch (lang)
                    {
                        case "en":
                            errorPoints = "Opponent has not enough points to play.";
                            break;
                        case "ru":
                            errorPoints = "У оппонента недостаточно мячей для игры.";
                            break;

                    }
                    //throw new Exception(errorPoints);
                    ModelState.AddModelError(string.Empty, errorPoints);
                    return BadRequest(ModelState);
                }
                try
                {
                    var match = await _matchManager.RequestMatch(User.GetUserId(), bid, gamerId, id);

                    return Ok(match);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Поиск соперника
        /// </summary>
        /// <param name="id">ID карты</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/search")]
        [ProducesResponseType(typeof(MatchSearchResultModel), 200)]
        public async Task<IActionResult> Search(int id)
        {

            if (ModelState.IsValid)
            {
                var bid = 0;
                var userId = User.GetUserId();
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == userId).Lang;
                var gamer = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

                //card type code
                var card = _dbContext.Cards.FirstOrDefault(c => c.Id == id);
                var type = _dbContext.CardTypes.FirstOrDefault(ct => ct.Id == card.TypeId).Code;

                var entryPoint = _dbContext.Cards.First(c => c.Id == id).EntryPoint;

                if (gamer.PointsToPlay < entryPoint)
                {
                    var errorPoints = string.Empty;
                    switch (lang)
                    {
                        case "en":
                            errorPoints = "Not enough points to play.";
                            break;
                        case "ru":
                            errorPoints = "Недостаточно мячей для игры.";
                            break;

                    }
                    //throw new Exception(errorPoints);
                    ModelState.AddModelError(string.Empty, errorPoints);
                    return BadRequest(ModelState);
                }

                try
                {

                    if (type == "HalfTime")
                    {
                        if (!card.IsActive)
                        {
                            ModelState.AddModelError("error", "Card is inactive");
                            return BadRequest(ModelState);
                        }
                        var match = await _matchManager.HTMatch(User.GetUserId(), id);
                        return Ok(match);
                    }

                    if (type == "Live")
                    {
                        if (!card.IsActive)
                        {
                            
                            ModelState.AddModelError("error", "Card is inactive");
                            return BadRequest(ModelState);
                        }
                        /*
                        if (gamer.Lifes == 0)
                        {
                            ModelState.AddModelError("error", "You have not Lifes");
                            return BadRequest(ModelState);
                        }
                        */
                        var match = await _matchManager.LiveMatch(User.GetUserId(), id);
                        return Ok(match);
                    }

                    bool _addBot = false;

                    if (!searchingGamers.Any(sg => sg.UserId == User.GetUserId() && sg.CardId == id))
                    {
                        var _sg = new SearchingGamers();
                        _sg.CardId = id;
                        _sg.UserId = User.GetUserId();
                        searchingGamers.Add(_sg);
                    }
                    else
                    {
                        _addBot = true;
                    }

                    var cardUsersCount = searchingGamers.Count(sg => sg.CardId == id);
                    if (cardUsersCount > 1 || _addBot)
                    {
                        var match = await _matchManager.SearchMatch(User.GetUserId(), id, bid);
                        return Ok(match);
                    }
                    else
                    {
                        MatchWaiting mw = new MatchWaiting();
                        mw.found = false;
                        mw.matchWaiting = true;
                        string result = Newtonsoft.Json.JsonConvert.SerializeObject(mw);

                        return Ok(result);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Принять матч
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/accept")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Accept(int id)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                var matchParticipant = _dbContext.MatchGamers.FirstOrDefault(m =>
                    m.GamerId == userId && m.MatchId == id);
                matchParticipant.JoinTime = DateTime.Now;

                var matchOpponent = _dbContext.MatchGamers.FirstOrDefault(m =>
                   m.GamerId != userId && m.MatchId == id);
                _dbContext.SaveChanges();

                if (matchOpponent.Cancelled)

                {
                    _logger.LogInformation($"Match {id} canceled by {matchOpponent.GamerId}");
                    await _gamersHandler.InvokeClientMethodToGroupAsync(userId, "matchCanceled", new { id, gamerId = matchOpponent.GamerId });
                    return Ok();
                }



                if (matchParticipant != null
                    && !matchParticipant.Cancelled
                    )//&& !matchParticipant.Confirmed
                {
                    _logger.LogInformation($"Match {id} accepted by {userId}");

                    var matchParticipants = _dbContext.MatchGamers
                        .Where(p => p.MatchId == id && p.GamerId != userId)
                        .Select(p => p.GamerId).ToList();

                    foreach (var participant in matchParticipants)
                    {
                        await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchAccepted", new { id, gamerId = userId });
                        _logger.LogInformation($"matchAccepted {id} by {userId} for {participant}");
                    }

                    return Ok();
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Начать сейчас
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/start")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> StartNow(int id)
        {
            if (ModelState.IsValid)
            {
                
                var userId = User.GetUserId();
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == userId).Lang;
                //Игрок который начал матч
                var matchParticipant = _dbContext.MatchGamers.Include(mg => mg.Match).FirstOrDefault(m =>
                    m.GamerId == userId && m.MatchId == id);
                //Оппонент которого игрок позвал
                var opponent = _dbContext.MatchGamers.Include(mg => mg.Match).FirstOrDefault(m =>
                    m.GamerId != userId && m.MatchId == id);

                if (opponent != null
                    && !opponent.Cancelled
                    && !opponent.Confirmed)
                {
                    opponent.Delayed = true;
                    opponent.Match.Status = Match.MatchStatus.Delayed;
                    _dbContext.SaveChanges();

                    var matchParticipants = _dbContext.MatchGamers
                        .Where(p => p.MatchId == id)
                        .Select(p => p.GamerId).ToList();

                    await _gamersHandler.InvokeClientMethodToGroupAsync(opponent.GamerId, "matchDelayed", new { id, gamerId = userId });
                    _logger.LogInformation($"matchDelayed {id} by {userId} for {opponent}");
                }

                if (matchParticipant != null
                    && !matchParticipant.Cancelled)
                {
                    //matchParticipant.Confirmed = true;
                    matchParticipant.Match.Status = Match.MatchStatus.Confirmed;
                    matchParticipant.Ready = true;
                    matchParticipant.IsPlay = true;
                    matchParticipant.JoinTime = DateTime.Now;
                    var match = matchParticipant.Match;
                    match.Status = Match.MatchStatus.Delayed;
                    _dbContext.SaveChanges();

                    var matchParticipants = _dbContext.MatchGamers
                        .Where(p => p.MatchId == id)
                        .Select(p => p.GamerId).ToList();

                    //QUESTIONS
                    //var match = _dbContext.Matches.Find(id);

                    _dbContext.SaveChanges();



                    //await _gamersHandler.InvokeClientMethodToGroupAsync(userId, "matchStarted", new { id });
                    _logger.LogInformation($"Match {id} started by {userId} for {opponent.GamerId} match is delayed ");

                    var matchQuestions = match.Questions.SplitToIntArray();

                    Random rnd = new Random();

                    switch (lang) {
                        case "ru":
                            var questionsRu = _dbContext.QuestionAnswers
                                .Where(a => matchQuestions.Contains(a.QuestionId))
                                .OrderBy(qa => rnd.Next())
                                .GroupBy(a => a.Question)
                               .Select(qa => new QuestionModel
                               {
                                   Id = qa.Key.Id,
                                   Text = qa.Key.TextRu,
                                   Answers = qa.Select(a => new QuestionAnswerModel
                                   {
                                       Id = a.Id,
                                       Text = a.TextRu
                                   }).ToList()
                               }).ToList().OrderBy(q => matchQuestions.IndexOf(q.Id));
                            return Ok(questionsRu);
                        case "kz":
                            var questionsKz = _dbContext.QuestionAnswers
                                .Where(a => matchQuestions.Contains(a.QuestionId))
                                .OrderBy(qa => rnd.Next())
                                .GroupBy(a => a.Question)
                               .Select(qa => new QuestionModel
                               {
                                   Id = qa.Key.Id,
                                   Text = qa.Key.TextKz,
                                   Answers = qa.Select(a => new QuestionAnswerModel
                                   {
                                       Id = a.Id,
                                       Text = a.TextKz
                                   }).ToList()
                               }).ToList().OrderBy(q => matchQuestions.IndexOf(q.Id));
                            return Ok(questionsKz);
                    }
                    
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Отложить матч
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/delay")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delay(int id)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                var matchParticipant = _dbContext.MatchGamers.Include(mg => mg.Match).FirstOrDefault(m =>
                    m.GamerId == userId && m.MatchId == id);

                if (matchParticipant != null
                    && !matchParticipant.Cancelled
                    && !matchParticipant.Confirmed)
                {
                    matchParticipant.Delayed = true;
                    matchParticipant.Match.Status = Match.MatchStatus.Delayed;
                    _dbContext.SaveChanges();

                    var matchParticipants = _dbContext.MatchGamers
                        .Where(p => p.MatchId == id)
                        .Select(p => p.GamerId).ToList();

                    foreach (var participant in matchParticipants)
                    {
                        await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchDelayed", new { id, gamerId = userId });
                        _logger.LogInformation($"matchDelayed {id} by {userId} for {participant}");
                    }

                    return Ok();
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Потвердить матч
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/confirm")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Confirm(int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return Ok(await _matchManager.Confirm(User.GetUserId(), id));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Готов к игре
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/ready")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Ready(int id)
        {
            if (ModelState.IsValid)
            {
                string gamerId = User.GetUserId();
                bool isMatchDelayed = false;

                var isBot = false;
                List<MatchGamer> matchParticipants = null;
                using (var tran = _dbContext.Database.BeginTransaction())
                {
                    try
                    {

                        _logger.LogInformation($"Match ready from {gamerId}");
                        var matchParticipant = _dbContext.MatchGamers.Include(mg => mg.Match).FirstOrDefault(m =>
                            m.GamerId == gamerId && m.MatchId == id);

                        if (_dbContext.Matches.First(m => m.Id == id).Status == Match.MatchStatus.Delayed)
                        {
                            isMatchDelayed = true;
                            
                        }
                            

                        var matchGamers = _dbContext.MatchGamers.Where(p => p.MatchId == id && !p.Delayed);
                        foreach (var _mg in matchGamers)
                        {
                            isBot = _dbContext.Users.Any(g => g.Id == _mg.GamerId && g.Bot > 0);
                        }
                        if (matchParticipant != null)
                        {
                            //isMatchDelayed = matchParticipant.Delayed;

                            if (!matchParticipant.Confirmed)
                            {
                                matchParticipant.Confirmed = true;
                                await _dbContext.SaveChangesAsync();
                                //tran.Rollback();
                                //ModelState.AddModelError(String.Empty, "Матч не подтвержден!");
                                //return BadRequest(ModelState);

                            }

                            if (!matchParticipant.Ready)
                            {
                                matchParticipant.Ready = true;
                                await _dbContext.SaveChangesAsync();
                            }

                            /*
                            var _matchParticipant = _dbContext.MatchGamers.FirstOrDefault(p => p.MatchId == id && p.GamerId == gamerId);//&& (!p.Delayed || isMatchDelayed)
                            if (!matchParticipants.Contains(_matchParticipant))
                            {
                                matchParticipants.Add(_matchParticipant);

                                if (!matchParticipant.Ready)
                                {
                                    matchParticipant.Ready = true;
                                    await _dbContext.SaveChangesAsync();
                                }
                                _logger.LogInformation(string.Join(',', matchParticipants.Select(p => p.GamerId + ':' + p.Ready)));
                            }
                            */
                            matchParticipants = _dbContext.MatchGamers.Where(mg => mg.MatchId == id && mg.Ready).ToList();
                            if (!isMatchDelayed)//!matchParticipant.Delayed
                            {
                                
                                if (matchParticipants.Count == 2 && matchParticipants.All(p => p.Ready))
                                {
                                    foreach (var participant in matchParticipants)
                                    {
                                        participant.IsPlay = true;
                                        participant.JoinTime = DateTime.Now;
                                    }
                                    matchParticipant.Match.Status = Match.MatchStatus.Started;
                                    await _dbContext.SaveChangesAsync();
                                }
                            }
                            else {
                                matchParticipant.Delayed = false;
                                matchParticipant.JoinTime = DateTime.Now;
                                matchParticipant.IsPlay = true;
                                matchParticipant.Match.Status = Match.MatchStatus.Started;
                                await _dbContext.SaveChangesAsync();
                            }
                            tran.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error {gamerId}");
                        tran.Rollback();
                    }
                }

                if (isMatchDelayed)
                {
                    await _gamersHandler.InvokeClientMethodToGroupAsync(gamerId, "matchStarted", new { id });
                    //_logger.LogInformation($"matchStarted for {matchParticipants.First(p => p.GamerId == gamerId)}");
                }
                else {

                    if (matchParticipants.Count == 2 && isBot == false)
                    {
                        foreach (var participant in matchParticipants)
                        {
                            await _gamersHandler.InvokeClientMethodToGroupAsync(participant.GamerId, "matchStarted", new { id });
                            _logger.LogInformation($"matchStarted for {participant}");
                        }

                        matchParticipants.Clear();
                    }
                    else if (matchParticipants.Count > 1 && isBot)
                    {
                        await _gamersHandler.InvokeClientMethodToGroupAsync(gamerId, "matchStarted", new { id });
                        _logger.LogInformation($"matchStarted for {matchParticipants.First(p => p.GamerId == gamerId)}");
                        matchParticipants.Clear();
                    }
                }

                return Ok();
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Отменить поиск
        /// </summary>
        /// <param name="type">отмена при поиске</param>
        /// <returns></returns>
        [HttpPost]
        [Route("cancel/{type}")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public IActionResult CancelSearch(string type)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();

                var gamerIndex = MatchController.searchingGamers.FindIndex(g => g.UserId == userId);
                searchingGamers.RemoveAt(gamerIndex);

                var _type = JsonConvert.SerializeObject(type,
                                    new JsonSerializerSettings());

                return Ok(_type);
            }
            return BadRequest(ModelState);
        }


        /// <summary>
        /// Отменить матч
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <param name="type">тип - свернул или закончил</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/cancel/{type}")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Cancel(int id, string type)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();


                var matchParticipant = _dbContext.MatchGamers.Include(mp => mp.Match).FirstOrDefault(m =>
                    m.GamerId == userId && m.MatchId == id);

                if (matchParticipant != null && !matchParticipant.Cancelled
                    && matchParticipant.Match.Status != Match.MatchStatus.Cancelled
                    && !_dbContext.MatchGamers.Any(mg => mg.MatchId == id && mg.Cancelled))
                {
                    _logger.LogInformation($"Match {id} canceled from {userId}");
                    matchParticipant.IsPlay = false;
                    matchParticipant.Cancelled = true;
                    switch (matchParticipant.Match.Status)
                    {
                        case Match.MatchStatus.Started:
                            matchParticipant.Match.Status = Match.MatchStatus.CancelledAferStart;
                            break;
                        default:
                            matchParticipant.Match.Status = Match.MatchStatus.Cancelled;
                            break;
                    }

                    var matchGamers = _dbContext.MatchGamers.Include(g => g.Gamer)
                        .Where(p => p.MatchId == id && p.GamerId != userId);


                    foreach (var matchGamer in matchGamers)
                    {
                        if (!matchParticipant.Ready || matchGamer.Gamer.Bot > 0)
                        {
                            matchGamer.IsPlay = false;
                        }
                    }
                    _dbContext.SaveChanges();

                    if (type == "invite")
                    {
                        var matchParticipants = matchGamers.Select(p => p.GamerId).ToList();

                        foreach (var participant in matchParticipants)
                        {
                            await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchCanceled", new { id, gamerId = userId });
                            _logger.LogInformation($"matchCanceled {id} by {userId} for {participant}");
                        }
                    }
                    else
                    {
                        var matchRival = _dbContext.MatchGamers.First(g => g.MatchId == id && g.GamerId != userId);

                        switch (type)
                        {
                            case "background":
                                matchRival.Bonus = 20;
                                await _gamersHandler.InvokeClientMethodToGroupAsync(matchRival.GamerId, "matchCanceled", new { id, gamerId = userId, type });
                                _logger.LogInformation($"matchCanceled {id} by {userId} for {matchRival.GamerId}");
                                break;
                            case "match":
                                matchRival.Bonus = 20;
                                await _gamersHandler.InvokeClientMethodToGroupAsync(matchRival.GamerId, "matchCanceled", new { id, gamerId = userId, type });
                                _logger.LogInformation($"matchCanceled {id} by {userId} for {matchRival.GamerId}");
                                break;
                            case "start":
                                var gamerIndex = MatchController._searchingGamers.FindIndex(g => g.StartsWith(userId));
                                _searchingGamers.RemoveAt(gamerIndex);
                                break;
                        }
                        
                        _dbContext.SaveChanges();
                    }
                }
                else {
                    MatchCancelled mc = new MatchCancelled();
                    mc.cancelled = true;
                    mc.type = type;
                    string result = Newtonsoft.Json.JsonConvert.SerializeObject(mc);
                    return Ok(result);
                }
                return Ok();
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Получить вопросы
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/questions")]
        [ProducesResponseType(typeof(IEnumerable<QuestionModel>), 200)]
        [ProducesResponseType(400)]
        public IActionResult Questions(int id)
        {
            if (ModelState.IsValid)
            {
                var match = _dbContext.Matches.Find(id);
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == User.GetUserId()).Lang;

                _dbContext.SaveChanges();

                var matchQuestions = match.Questions.SplitToIntArray();

                Random rnd = new Random();

                //var questions = new List<QuestionModel>();

                switch (lang) {
                    case "ru":
                        var questionsRu = _dbContext.QuestionAnswers
                             .Where(a => matchQuestions.Contains(a.QuestionId))
                             .OrderBy(qa => rnd.Next())
                             .GroupBy(a => a.Question)
                            .Select(qa => new QuestionModel
                            {
                                Id = qa.Key.Id,
                                Text = qa.Key.TextRu,
                                Answers = qa.Select(a => new QuestionAnswerModel
                                {
                                    Id = a.Id,
                                    Text = a.TextRu
                                }).ToList()//.OrderBy(q => q.Id == rnd.Next())
                            }).ToList().OrderBy(q => matchQuestions.IndexOf(q.Id));
                        return Ok(questionsRu);
                        //break;
                    case "kz":
                         var questionsKz = _dbContext.QuestionAnswers
                             .Where(a => matchQuestions.Contains(a.QuestionId))
                             .OrderBy(qa => rnd.Next())
                             .GroupBy(a => a.Question)
                            .Select(qa => new QuestionModel
                            {
                                Id = qa.Key.Id,
                                Text = qa.Key.TextKz,
                                Answers = qa.Select(a => new QuestionAnswerModel
                                {
                                    Id = a.Id,
                                    Text = a.TextKz
                                }).ToList()//.OrderBy(q => q.Id == rnd.Next())
                            }).ToList().OrderBy(q => matchQuestions.IndexOf(q.Id));
                        return Ok(questionsKz);
                        //break;
                }

                
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Получить статус ответа на вопрос
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <param name="questionId">ID вопроса</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/answers/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetQuestionAnswerStatus([FromRoute]int id, [FromQuery]int questionId)
        {
            string userId = User.GetUserId();

            try
            {
                await _matchManager.GetQuestionAnswerStatus(id, userId, questionId);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return BadRequest(ModelState);
            }

            return Ok();
        }


        /// <summary>
        /// Получить статус ответа на вопрос
        /// </summary>
        /// <param name="id">ID Вопроса</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/answer/false/{count}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFalseAnswer([FromRoute]int id, [FromRoute]int count)
        {
            string userId = User.GetUserId();

            var user = _dbContext.Users.Find(userId);

            var question = _dbContext.Questions.Find(id);

            var card = _dbContext.Cards.FirstOrDefault(c => c.Id == question.CardId);

            var hintLimit = _dbContext.CardLimits.FirstOrDefault(cl => cl.CardId == card.Id).Hints;

            var myHints = user.Hints + 1;

            if(count <= hintLimit)
            {
                if (myHints != 0)
                {
                    var answerId = _dbContext.QuestionAnswers.First(qa => qa.QuestionId == id && qa.Id != question.CorrectAnswerId).Id;

                    user.Hints -= 1;

                    _dbContext.SaveChanges();

                    return Ok(answerId);
                }
                ModelState.AddModelError("error", "You have not hints");
                return BadRequest(ModelState);
            }

            ModelState.AddModelError("error", "Limit of hints is " + hintLimit);
            return BadRequest(ModelState);

        }

        /// <summary>
        /// Ответить на вопрос
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/answers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AnswerToQuestions([FromRoute]int id, [FromBody]MatchQuestionAnswerModel answer, [BindNever]string gamerId = null)
        {
            if (ModelState.IsValid)
            {
                string userId = gamerId ?? User.GetUserId();
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == userId).Lang;
                var matchGamer = _dbContext.MatchGamers.Single(g => g.MatchId == id && g.GamerId == userId);

                if (matchGamer.Cancelled)
                {
                    var _error = string.Empty;
                    switch (lang)
                    {
                        case "en":
                            _error = "Match cancelled";
                            break;
                        case "ru":
                            _error = "Матч отменен.";
                            break;
                    }
                    ModelState.AddModelError(string.Empty, _error);
                }
                else if (!matchGamer.IsPlay)
                {
                    var _error = string.Empty;
                    switch (lang)
                    {
                        case "en":
                            _error = "Match ended";
                            break;
                        case "ru":
                            _error = "Матч закончен.";
                            break;
                    }
                    ModelState.AddModelError(string.Empty, _error);
                }
                else
                {
                    var match = _dbContext.Matches.Find(id);
                    var matchQuestions = match.Questions.SplitToIntArray();

                    var correctAnswerId = _dbContext.Questions.Where(q => q.Id == answer.QuestionId)
                                                                    .Select(q => q.CorrectAnswerId).Single();
                    if (matchQuestions.Contains(answer.QuestionId))
                    {
                        var isCorrectAnswer = correctAnswerId == answer.AnswerId;
                        _dbContext.MatchAnswers.Add(new MatchAnswer
                        {
                            QuestionId = answer.QuestionId,
                            AnswerId = (answer.AnswerId ?? 0) > 0 ? answer.AnswerId : (int?)null,
                            CreatedAt = DateTime.Now,
                            MatchGamerId = matchGamer.Id,
                            IsCorrectAnswer = isCorrectAnswer
                        });

                        _logger.LogInformation($"Question answer from {userId}. Question: {answer.QuestionId}, answer: {answer.AnswerId}.");
                    }

                    var matchBots = (from mg in _dbContext.MatchGamers
                                     join u in _dbContext.Users on mg.GamerId equals u.Id
                                     where mg.MatchId == id && u.Bot > 0
                                     select new { mg.Id, u.Bot }).ToList();

                    if (matchBots.Any())
                    {
                        var random = new Random();
                        foreach (var botGamer in matchBots)
                        {
                            var botRand = random.Next(10);

                            bool isCorrectAnswer = botRand <= botGamer.Bot;

                            _dbContext.MatchAnswers.Add(new MatchAnswer
                            {
                                QuestionId = answer.QuestionId,
                                AnswerId = isCorrectAnswer ? correctAnswerId : (int?)null,
                                CreatedAt = DateTime.Now,
                                MatchGamerId = botGamer.Id,
                                IsCorrectAnswer = isCorrectAnswer
                            });
                        }
                    }

                    _dbContext.SaveChanges();

                    var matchParticipants = _dbContext.MatchGamers.Count(p => p.MatchId == id && !p.Cancelled && (!p.Delayed || matchGamer.Delayed));
                    
                    var answers = (from a in _dbContext.MatchAnswers
                                   join g in _dbContext.MatchGamers on a.MatchGamerId equals g.Id
                                   where g.MatchId == id && a.QuestionId == answer.QuestionId && !g.Cancelled
                                   select new MatchQuestionAnswerResponse
                                   {
                                       GamerId = g.GamerId,
                                       IsCorrect = a.IsCorrectAnswer,
                                       QuestionId = a.QuestionId,
                                       AnswerId = a.AnswerId ?? 0
                                   }).ToList();

                    if (matchParticipants == answers.Count())
                    {
                        foreach (var answerReponse in answers)
                        {
                            await _gamersHandler.InvokeClientMethodToGroupAsync(answerReponse.GamerId,
                                "questionAnswersResult", answers);
                            _logger.LogInformation($"questionAnswersResult for {answerReponse.GamerId}");
                        }
                    }

                    var gamer = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == id && mg.GamerId == gamerId);
                    var opponent = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == id && mg.GamerId != gamerId);

                    /*
                    var opponentDelayed = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == id && mg.GamerId != gamerId).Delayed;
                    if (opponentDelayed)
                    {
                        await _gamersHandler.InvokeClientMethodToGroupAsync(gamerId,
                                "questionAnswersResult", answers);
                    }
                    */

                    return Ok();
                }
            }

            return BadRequest(ModelState);
        }


        /// <summary>
        /// Ответить на вопрос Лайв
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("live/{id}/answers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AnswerToQuestionLive([FromRoute]int id, [FromBody]MatchQuestionAnswerModel answer)
        {
            if (ModelState.IsValid)
            {
                string userId = User.GetUserId();
                var user = _dbContext.Users.Find(userId);
                var lang = user.Lang;
                var matchGamer = _dbContext.MatchGamers.FirstOrDefault(g => g.MatchId == id && g.GamerId == userId);

                var matchAnswer = _dbContext.MatchAnswers.Count(ma => ma.QuestionId == answer.QuestionId && ma.MatchGamerId == matchGamer.Id);

                var incorrectAnswers = _dbContext.MatchAnswers.Count(ma => ma.MatchGamerId == matchGamer.Id && !ma.IsCorrectAnswer);

                if (matchAnswer == 0)
                {
                    var match = _dbContext.Matches.Find(id);
                    var matchQuestions = match.Questions.SplitToIntArray();

                    var correctAnswerId = _dbContext.Questions.Where(q => q.Id == answer.QuestionId)
                                                                    .Select(q => q.CorrectAnswerId).Single();
                    if (matchQuestions.Contains(answer.QuestionId))
                    {
                        var isCorrectAnswer = correctAnswerId == answer.AnswerId;
                        _dbContext.MatchAnswers.Add(new MatchAnswer
                        {
                            QuestionId = answer.QuestionId,
                            AnswerId = (answer.AnswerId ?? 0) > 0 ? answer.AnswerId : (int?)null,
                            CreatedAt = DateTime.Now,
                            MatchGamerId = matchGamer.Id,
                            IsCorrectAnswer = isCorrectAnswer
                        });

                        /*
                        if (user.Lifes != 0)
                        {
                            if (!isCorrectAnswer)
                            {
                                user.Lifes -= 1;
                            }
                        }
                        */

                        _dbContext.SaveChanges();

                        _logger.LogInformation($"Question answer from {userId}. Question: {answer.QuestionId}, answer: {answer.AnswerId}.");
                    }

                    return Ok();
                }
                ModelState.AddModelError("error", "You answered this question");
                return BadRequest(ModelState);
            }

            return BadRequest(ModelState);
        }


        /// <summary>
        /// Сколько людей на вриант ответили
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <param name="questionId">ID Вопроса</param>
        /// <returns></returns>
        [HttpPost]
        [Route("live/{id}/answers/{questionId}")]
        [ProducesResponseType(typeof(LiveAnswerModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AnswersCountLive([FromRoute]int id, [FromRoute]int questionId)
        {
            if (ModelState.IsValid)
            {
                string userId = User.GetUserId();
                var user = _dbContext.Users.Find(userId);
                var lang = _dbContext.Users.FirstOrDefault(u => u.Id == userId).Lang;
                var match = _dbContext.Matches.Find(id);
                var matchGamer = _dbContext.MatchGamers.Single(g => g.MatchId == id && g.GamerId == userId);
                var matchGamers = _dbContext.MatchGamers.Where(mg => mg.MatchId == id && !mg.Cancelled);

                var incorrectAnswers = _dbContext.MatchAnswers.Count(ma => ma.MatchGamerId == matchGamer.Id && !ma.IsCorrectAnswer);
                var lifeLimits = _dbContext.CardLimits.FirstOrDefault(cl => cl.CardId == match.CardId).Lifes;
                var lifes = user.Lifes + 1;

                var myAnswer = _dbContext.MatchAnswers.FirstOrDefault(ma => ma.MatchGamerId == matchGamer.Id && ma.QuestionId == questionId);

                var correctAnswerId = _dbContext.Questions.Where(q => q.Id == questionId)
                                                                .Select(q => q.CorrectAnswerId).Single();
                var isCorrect = false;
                if (myAnswer == null)
                {
                    isCorrect = false;
                }
                else {
                    isCorrect = myAnswer.IsCorrectAnswer;
                }

                //how many lifes you have
                lifes = lifes - incorrectAnswers;
                if (incorrectAnswers == lifeLimits)
                {
                    lifes = 0;
                }

                var answers = _dbContext.QuestionAnswers.Where(qa => qa.QuestionId == questionId);

                var answersCount = new List<AnswerModel>();

                foreach (var answer in answers) {
                    var count = 0;

                    //var gamerAnswers = _dbContext.MatchAnswers.Where(ma => ma.AnswerId == answer.Id && ma.QuestionId == questionId);

                    count = _dbContext.MatchAnswers.Count(ma => ma.AnswerId == answer.Id && ma.QuestionId == questionId);//gamerAnswers.Count();

                    var answerCount = new AnswerModel();
                    answerCount.AnswerId = answer.Id;
                    answerCount.Count = count;
                    answersCount.Add(answerCount);
                }

                return Ok(new LiveAnswerModel {
                    IsCorrectAnswer = isCorrect,
                    Lifes = lifes,
                    CorrectAnswerId = correctAnswerId,
                    AnswersCount = answersCount,
                    GamersCount = _dbContext.MatchAnswers.Count(ma => ma.QuestionId == questionId),
                });
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// отменить Лайв
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("live/{id}/cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CancelLive([FromRoute]int id)
        {
            if (ModelState.IsValid)
            {
                string userId = User.GetUserId();
                var user = _dbContext.Users.Find(userId);
                var lang = user.Lang;
                var matchGamer = _dbContext.MatchGamers.Single(g => g.MatchId == id && g.GamerId == userId);

                var match = _dbContext.Matches.Find(id);

                matchGamer.IsPlay = false;
                matchGamer.Cancelled = true;

                _dbContext.SaveChanges();

                return Ok();
            }

            return BadRequest(ModelState);
        }


        /// <summary>
        /// Cash Out
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("live/{id}/cash-out")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CashOut([FromRoute]int id, [FromBody]CashOutModel cashOut)
        {
            if (ModelState.IsValid)
            {
                var user = _dbContext.Users.Find(cashOut.UserId);
                var lang = user.Lang;

                var matchGamer = _dbContext.MatchGamers.FirstOrDefault(mg => mg.MatchId == id && mg.GamerId == cashOut.UserId);

                _dbContext.CashOutHistories.Add(new CashOutHistory {
                    UserId = cashOut.UserId,
                    Cash = cashOut.Cash,
                    OutDate = DateTime.Now
                });
                

                var userBalance = _dbContext.UserBalances.FirstOrDefault(ub => ub.UserId == cashOut.UserId);

                if (userBalance == null)
                {
                    _dbContext.UserBalances.Add(new UserBalance
                    {
                        UserId = cashOut.UserId,
                        Balance = cashOut.Cash
                    });
                }
                else {
                    userBalance.Balance += cashOut.Cash;
                }

                matchGamer.Cancelled = true;

                _dbContext.SaveChanges();
                
                return Ok();
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Получить результат матча  
        /// </summary> 
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/result")]
        [ProducesResponseType(typeof(MatchResultModel), 200)]
        public async Task<IActionResult> GetMatchResult([FromRoute]int id)
        {
            if (ModelState.IsValid)
            {
                string userId = User.GetUserId();

                return Ok(await _matchManager.GetMatchResult(id, userId));
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Получить результат матча  
        /// </summary> 
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpGet]
        [Route("live/{id}/result")]
        [ProducesResponseType(typeof(MatchResultModel), 200)]
        public async Task<IActionResult> LiveMatchResult([FromRoute]int id)
        {
            if (ModelState.IsValid)
            {
                string userId = User.GetUserId();

                return Ok(await _matchManager.LiveMatchResult(id, userId));
            }

            return BadRequest(ModelState);
        }


        /// <summary>
        /// Пуш игр
        /// </summary>
        /// <returns></returns>
        [HttpGet("push")]
        public async  Task<IActionResult> Push(int page = 1, int count = 10)
        {
            string gamerId = User.GetUserId();

            
            var _users = new List<string>();
            string _user = "45a65957-d9da-48db-a1ac-dedbc0424a23";
            

            _users.Add(_user);
            //_users.Add("068009c0-1368-4f5c-aa5b-6c650ec29b81");

            var obj = new
            {
                app_id = "4e799f1b-4965-4fa9-8919-e169ee157147",
                headings = new { en = "Check", ru = "Проверка" },
                contents = new { en = "Hello World!", ru = "Привет Мир!" },
                include_player_ids = _users
            };
            var json = JsonConvert.SerializeObject(obj);
            await CheckPush(json);

            return Ok();
        }

        private async Task CheckPush(string json)
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


        /// <summary>
        /// Отложенные игры
        /// </summary>
        /// <returns></returns>
        [HttpGet("delayed")]
        public IActionResult DelayedGames(int page = 1, int count = 10)
        {
            string gamerId = User.GetUserId();
            
            var delayed = new List<MatchModel>();
            var myMatches = _dbContext.MatchGamers.Where(mg => mg.GamerId == gamerId && !mg.Cancelled).OrderByDescending(d => d.Id).Select(m => m.MatchId).ToList().Take(100);
            foreach(var match in  myMatches)
            {
                if (_dbContext.Matches.Any(m => m.Id == match && m.Status != Match.MatchStatus.Finished))
                {
                    //var mGamer = _dbContext.MatchGamers.First(mg => mg.MatchId == match && mg.GamerId != gamerId && mg.Delayed);
                    var matchGamers = _dbContext.MatchGamers.Where(mg => mg.MatchId == match && mg.GamerId != gamerId && !mg.Cancelled && mg.Delayed);
                    foreach (var mGamer in matchGamers)
                    {
                        var gamerCard = _dbContext.GamerCards.FirstOrDefault(gc => gc.Id == mGamer.GamerCardId);
                        if (gamerCard != null)
                        {
                            var cardId = gamerCard.CardId;
                            var mm = new MatchModel();
                            mm.Id = match;
                            mm.CardName = _dbContext.Cards.First(c => c.Id == cardId).Name;
                            mm.GamerName = _dbContext.Users.First(u => u.Id == mGamer.GamerId).NickName;
                            mm.PhotoUrl = _dbContext.Users.First(u => u.Id == mGamer.GamerId).PhotoUrl;
                            mm.MatchStarted = Convert.ToDateTime(_dbContext.MatchGamers.First(mg => mg.MatchId == match && mg.GamerId != gamerId).JoinTime);
                            delayed.Add(mm);
                        }
                    }
                }
            }

            return Ok(delayed);
            
        }


        /// <summary>
        /// Приглашения на игру
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="count">Количество записей на странице</param>
        /// <returns></returns>
        [HttpGet("invites")]
        public IActionResult InvitedGames(int page = 1, int count = 10)
        {
            string gamerId = User.GetUserId();

            var invites = new List<MatchModel>();
            var myMatches = _dbContext.MatchGamers.Where(mg => mg.GamerId == gamerId && !mg.Confirmed && mg.JoinTime == null && !mg.Cancelled).OrderByDescending(d => d.Id).Select(m => m.MatchId).ToList().Take(100);
            foreach (var match in myMatches)
            {
                if (_dbContext.Matches.Any(m => m.Id == match && m.Status != Match.MatchStatus.Finished))
                {
                    var matchGamers = _dbContext.MatchGamers.Where(mg => mg.MatchId == match && !mg.Cancelled);
                    foreach (var mGamer in matchGamers)
                    {
                        var gamerCard = _dbContext.GamerCards.FirstOrDefault(gc => gc.Id == mGamer.GamerCardId);
                        if (mGamer.GamerId != gamerId && mGamer.Confirmed && gamerCard != null)
                        {
                            var cardId = gamerCard.CardId;

                            var mm = new MatchModel();
                            mm.Id = match;
                            mm.CardName = _dbContext.Cards.First(c => c.Id == cardId).Name;
                            mm.GamerName = _dbContext.Users.First(u => u.Id == mGamer.GamerId).NickName;
                            mm.PhotoUrl = _dbContext.Users.First(u => u.Id == mGamer.GamerId).PhotoUrl;
                            var started = _dbContext.MatchGamers.First(mg => mg.MatchId == match && mg.GamerId != gamerId).JoinTime;
                            mm.MatchStarted = Convert.ToDateTime(started);

                            invites.Add(mm);
                        }
                    }
                }
            }

            return Ok(invites);
        }
        
    }
}