using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ApplicationDbContext _dbContext;

        public MatchController(IMatchManager matchManager, ApplicationDbContext dbContext)
        {
            _matchManager = matchManager;
            _dbContext = dbContext;
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
                var match = await _matchManager.SearchMatch(User.GetUserId(), id);

                return Ok(match);
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
                return Ok(await _matchManager.Confirm(User.GetUserId(), id));
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
        public IActionResult AnswerToQuestions([FromRoute]int id, [FromBody]MatchQuestionAnswerModel answer)
        {
            if (ModelState.IsValid)
            {
                string userId = User.GetUserId();
                var matchGamer = _dbContext.MatchGamers.Single(g => g.MatchId == id && g.GamerId == userId);
                if (matchGamer.IsPlay)
                {
                    var match = _dbContext.Matches.Find(id);
                    var matchQuestions = match.Questions.SplitToIntArray();

                    if (matchQuestions.Contains(answer.QuestionId))
                    {
                        var correctAnswerId = _dbContext.Questions.Where(q => q.Id == answer.QuestionId)
                                                                    .Select(q => q.CorrectAnswerId).Single();
                        var isCorrectAnswer = correctAnswerId == answer.AnswerId;
                        _dbContext.MatchAnswers.Add(new MatchAnswer
                        {
                            QuestionId = answer.QuestionId,
                            AnswerId = answer.AnswerId,
                            CreatedAt = DateTime.Now,
                            MatchGamerId = matchGamer.Id,
                            IsCorrectAnswer = isCorrectAnswer
                        });

                        _dbContext.SaveChanges();

                        return Ok(new MatchQuestionAnswerResponse { IsCorrect = isCorrectAnswer, });
                    }    
                }

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
        public IActionResult GetMatchResult([FromRoute]int id)
        {
            if (ModelState.IsValid)
            {
                string userId = User.GetUserId();
                var matchOptions = HttpContext.RequestServices.GetService<IOptions<MatchOptions>>();

                MatchGamer currentMatchGamer = null;

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    var match = _dbContext.Matches.Find(id);
                    var matchGamers = _dbContext.MatchGamers.Include(m => m.Match).Include(m => m.Answers).Include(m => m.Gamer).Where(g => g.MatchId == id);

                    var resultModel = new MatchResultModel();

                    int winnerScore = 0;

                    var matchGamerBonuses = new List<Tuple<MatchGamer, GamerCard, int>>();

                    foreach (var matchGamer in matchGamers)
                    {
                        if (matchGamer.IsPlay && matchGamer.JoinTime.HasValue)
                        {
                            var questionsCount = match.Questions.SplitToIntArray().Length;
                            var answersCount = matchGamer.Answers.Count();
                            var correctAnswersCount = matchGamer.Answers.Count(a => a.IsCorrectAnswer);
                            var incorrectAnswersCount = questionsCount - correctAnswersCount;

                            var pointsForMacth = correctAnswersCount * matchOptions.Value.CorrectAnswerScore + incorrectAnswersCount * matchOptions.Value.IncorrectAnswerScore;
                            matchGamer.Score = pointsForMacth;
                            matchGamer.IsPlay = false;

                            var gamerCard = _dbContext.GamerCards.SingleOrDefault(gc => gc.CardId == matchGamer.Match.CardId && gc.GamerId == matchGamer.GamerId);
                            if (gamerCard == null)
                            {
                                // Создаем новую карточку для игрока
                                gamerCard = new GamerCard
                                {
                                    CardId = matchGamer.Match.CardId,
                                    GamerId = matchGamer.GamerId,
                                    StartTime = DateTime.Now,
                                };
                                _dbContext.GamerCards.Add(gamerCard);
                            }

                            matchGamerBonuses.Add(new Tuple<MatchGamer, GamerCard, int>(matchGamer, gamerCard, answersCount * matchOptions.Value.BonusForAnswer));

                            gamerCard.Score += matchGamer.Score; // Добавляем (или отнимаем) очки к карте игрока 
                            matchGamer.Gamer.Score -= Math.Abs(matchGamer.Score); // Отнимаем из текущих очков у игрока

                            if (matchGamer.Score > winnerScore)
                            {
                                winnerScore = matchGamer.Score;
                            }
                        }

                        if (matchGamer.GamerId == userId)
                        {
                            currentMatchGamer = matchGamer;
                        }
                    }

                    foreach (var gamerBonus in matchGamerBonuses)
                    {
                        if (gamerBonus.Item1.Score == winnerScore)
                        {
                            gamerBonus.Item1.IsWinner = true;
                        }

                        int bonus = gamerBonus.Item1.IsWinner ? +gamerBonus.Item3 : -gamerBonus.Item3;
                        gamerBonus.Item1.Score += bonus;
                        gamerBonus.Item2.Score -= bonus;

                        if (gamerBonus.Item1 != currentMatchGamer)
                        {
                            resultModel.RivalMatchScore = gamerBonus.Item1.Score;
                        }
                    }

                    _dbContext.SaveChanges();

                    transaction.Commit();

                    resultModel.CardScore = _dbContext.GamerCards.Where(gc => gc.CardId == currentMatchGamer.Match.CardId && gc.GamerId == userId).Select(c => c.Score).FirstOrDefault();
                    resultModel.MatchScore = currentMatchGamer.Score;
                    resultModel.IsWinner = currentMatchGamer.IsWinner;
                    resultModel.CurrentGamerScore = currentMatchGamer.Gamer.Score;

                    return Ok(resultModel);
                }
            }

            return BadRequest(ModelState);
        }


    }
}