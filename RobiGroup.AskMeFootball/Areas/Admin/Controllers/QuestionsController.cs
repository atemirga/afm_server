using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Questions;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class QuestionsController : BaseController
    {
        public QuestionsController(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public IActionResult Index(int id)
        {
            ViewBag.CardId = id;
            ViewBag.CardName = _dbContext.Cards.Find(id).Name;
            return View();
        }

        public IActionResult _Filter(QuestionsFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterQuestions);
        }

        private IEnumerable<QuestionViewModel> FilterQuestions(QuestionsFilterModel filter)
        {
            var questions = _dbContext.Questions.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                questions = questions.Where(q => q.Text.Contains(filter.Search));
            }

            filter.Total = questions.Count();
            return questions.Include(q => q.Answers)
                .OrderByDescending(t => t.Id)
                .Skip(filter.DataTablesRequest.Start)
                .Take(filter.DataTablesRequest.Length)
                .Select(r => new QuestionViewModel()
                {
                    Id = r.Id,
                    Text = r.Text,
                    AnswerId = r.CorrectAnswerId,
                    Answers = r.Answers.Select(a => new AnswerViewModel
                    {
                        Id = a.Id,
                        Text = a.Text
                    }).ToList()
                });
        }


        #region Create/Edit

        public IActionResult Create(int cardId)
        {
            return View(new QuestionCreateModel()
            {
                CardId = cardId,
                Answers = new List<string>() { "", "", ""}
            });
        }

        [HttpPost]
        public IActionResult Create(QuestionCreateModel model)
        {
            var filledAnswersCount = model.Answers.Count(a => !string.IsNullOrEmpty(a));
            if (model.Answers == null || model.Answers.Count < 2 || filledAnswersCount < 2)
            {
                ModelState.AddModelError(nameof(model.Answers), "Введите хотябы 2 варианта ответов на вопрос.");
            }

            if (model.CorrectAnswerId < 0 || model.CorrectAnswerId >= filledAnswersCount)
            {
                ModelState.AddModelError(nameof(model.Answers), "Выберите правельный ответ.");
            }

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            var question = new Question
            {
                Text = model.Text,
                CardId = model.CardId
            };
            question.Answers = model.Answers.Where(a => !string.IsNullOrEmpty(a)).Select(a => new QuestionAnswer{ Text = a }).ToList();
            _dbContext.Questions.Add(question);
            _dbContext.SaveChanges();

            question.CorrectAnswerId = question.Answers[model.CorrectAnswerId].Id;
            _dbContext.SaveChanges();

            return RedirectToAction("Index", new { id = model.CardId });
        }

        public IActionResult Delete(int id)
        {
            var question = _dbContext.Questions.Find(id);
            question.IsDeleted = true;
            _dbContext.SaveChanges();

            return Ok();
        }

        public IActionResult DeleteAnswer(int id)
        {
            var answer = _dbContext.QuestionAnswers.Find(id);
            answer.IsDeleted = true;
            _dbContext.SaveChanges();

            return Ok();
        }

        #endregion
    }
}