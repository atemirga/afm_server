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

            return Ok((from g in _dbContext.MatchGamers
                join m in _dbContext.Matches on g.MatchId equals m.Id
                join c in _dbContext.Cards on m.CardId equals c.Id
                join rg in _dbContext.MatchGamers on m.Id equals rg.MatchId
                join ru in _dbContext.Users on rg.GamerId equals ru.Id
                where g.JoinTime.HasValue && !g.IsPlay && !g.Cancelled && g.GamerId == gamerId && rg.GamerId != gamerId
                orderby g.JoinTime descending 
                select new MatchHistoryModel
                {
                    Id = m.Id,
                    CardName = c.Name,
                    Score = g.Score,
                    PhotoUrl = ru.PhotoUrl,
                    GamerName = ru.NickName,
                    IsWon = g.IsWinner,
                    Time = g.JoinTime.Value
                }).Skip((page - 1)*count).Take(count).ToList());
        }

        /// <summary>
        /// Отложенные игры
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="count">Количество записей на странице</param>
        /// <returns></returns>
        [HttpGet("delayed")]
        private IActionResult Delayed(int page = 1, int count = 10)
        {
            string gamerId = User.GetUserId();

            return Ok((from g in _dbContext.MatchGamers
                join m in _dbContext.Matches on g.MatchId equals m.Id
                join c in _dbContext.Cards on m.CardId equals c.Id
                join rg in _dbContext.MatchGamers on m.Id equals rg.MatchId
                join ru in _dbContext.Users on rg.GamerId equals ru.Id
                where !g.JoinTime.HasValue && !g.IsPlay && !g.Cancelled && g.Delayed && g.GamerId == gamerId && rg.GamerId != gamerId
                orderby g.JoinTime descending
                select new MatchModel
                {
                    Id = m.Id,
                    CardName = c.Name,
                    PhotoUrl = ru.PhotoUrl,
                    GamerName = ru.NickName,
                }).Skip((page - 1) * count).Take(count).ToList());
        }

        /// <summary>
        /// Запрос на матч
        /// </summary>
        /// <param name="id">ID карты</param>
        /// <param name="gamerId">ID игрока</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/request/{gamerId}")]
        [ProducesResponseType(typeof(MatchSearchResultModel), 200)]
        public async Task<IActionResult> Search([FromRoute]int id, [FromRoute]string gamerId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var match = await _matchManager.RequestMatch(User.GetUserId(), gamerId, id);

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
                try
                {
                    var match = await _matchManager.SearchMatch(User.GetUserId(), id);

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

                if (matchParticipant != null 
                    && !matchParticipant.Cancelled 
                    && !matchParticipant.Confirmed)
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
                var matchParticipant = _dbContext.MatchGamers.FirstOrDefault(m =>
                    m.GamerId == userId && m.MatchId == id);

                if (matchParticipant != null
                    && !matchParticipant.Cancelled
                    && !matchParticipant.Confirmed)
                {
                    matchParticipant.Delayed = true;
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
                using (var tran = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _logger.LogInformation($"Match ready from {gamerId}");
                        var matchParticipant = _dbContext.MatchGamers.FirstOrDefault(m =>
                            m.GamerId == gamerId && m.MatchId == id);

                        if (matchParticipant != null)
                        {
                            isMatchDelayed = matchParticipant.Delayed;

                            if (!matchParticipant.Confirmed)
                            {
                                tran.Rollback();
                                ModelState.AddModelError(String.Empty, "Матч не подтвержден!");
                                return BadRequest(ModelState);
                            }

                            if (!matchParticipant.Ready)
                            {
                                matchParticipant.Ready = true;
                                _dbContext.SaveChanges();
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

                var matchParticipants = _dbContext.MatchGamers.Where(p => p.MatchId == id && (!p.Delayed || isMatchDelayed)).ToList();
                _logger.LogInformation(string.Join(',', matchParticipants.Select(p => p.GamerId + ':' + p.Ready)));
                if (matchParticipants.All(p => p.Ready))
                {
                    foreach (var participant in matchParticipants)
                    {
                        participant.IsPlay = true;
                    }
                    await _dbContext.SaveChangesAsync();

                    foreach (var participant in matchParticipants)
                    {
                        await _gamersHandler.InvokeClientMethodToGroupAsync(participant.GamerId, "matchStarted", new { id });
                        _logger.LogInformation($"matchStarted for {participant}");
                    }
                }

                return Ok();
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Отменить матч
        /// </summary>
        /// <param name="id">ID матча</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/cancel")]
        [ProducesResponseType(typeof(ConfirmResponseModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Cancel(int id)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                var matchParticipant = _dbContext.MatchGamers.FirstOrDefault(m =>
                    m.GamerId == userId && m.MatchId == id);

                if (matchParticipant != null && !matchParticipant.Cancelled)
                {
                    _logger.LogInformation($"Match {id} canceled from {userId}");
                    matchParticipant.IsPlay = false;
                    matchParticipant.Cancelled = true;

                    var matchGamers = _dbContext.MatchGamers.Include(g => g.Gamer)
                        .Where(p => p.MatchId == id && p.GamerId != userId);

                    
                    foreach (var matchGamer in matchGamers)
                    {
                        if (!matchParticipant.Ready || matchGamer.Gamer.Bot > 0)
                        {
                            matchGamer.IsPlay = false;
                            matchGamer.Cancelled = true;
                        }
                    }
                    _dbContext.SaveChanges();

                    var matchParticipants = matchGamers
                                                .Select(p => p.GamerId).ToList();

                    foreach (var participant in matchParticipants)
                    {
                        await _gamersHandler.InvokeClientMethodToGroupAsync(participant, "matchCanceled", new { id, gamerId = userId });
                        _logger.LogInformation($"matchCanceled {id} by {userId} for {participant}");
                    }
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
                var matchQuestions = match.Questions.SplitToIntArray();

                var questions = _dbContext.QuestionAnswers
                     .Where(a => matchQuestions.Contains(a.QuestionId))
                    .GroupBy(a => a.Question)
                    .Select(qa => new QuestionModel
                    {
                        Id = qa.Key.Id,
                        Text = qa.Key.Text,
                        Answers = qa.Select(a => new QuestionAnswerModel
                        {
                            Id = a.Id,
                            Text = a.Text
                        }).ToList()
                    });

                return Ok(questions);
            }

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
                var matchGamer = _dbContext.MatchGamers.Single(g => g.MatchId == id && g.GamerId == userId);

                if (matchGamer.Cancelled)
                {
                    ModelState.AddModelError(string.Empty, "Матч отменен.");
                }
                else if (!matchGamer.IsPlay)
                {   
                    ModelState.AddModelError(string.Empty, "Матч закончен.");
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
                        select new {mg.Id, u.Bot}).ToList();

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

                    if (matchQuestions[matchQuestions.Length - 1] == answer.QuestionId)
                    {
                        
                    }

                    return Ok();
                }
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


    }
}