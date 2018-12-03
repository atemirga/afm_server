using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
            LoadIndexViewData(id);

            if (TempData["Errors"] is List<string> errors)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(String.Empty, error);
                }
            }

            ViewBag.Message = TempData["Message"];

            return View(new QuestionsFilterModel{CardId = id });
        }

        private void LoadIndexViewData(int id)
        {
            ViewBag.CardId = id;
            ViewBag.CardName = _dbContext.Cards.Find(id).Name;
        }

        public IActionResult _Filter(QuestionsFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterQuestions);
        }

        private IEnumerable<QuestionViewModel> FilterQuestions(QuestionsFilterModel filter)
        {
            var questions = _dbContext.Questions.Where(q => q.CardId == filter.CardId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                questions = questions.Where(q => q.Text.Contains(filter.Search));
            }

            filter.Total = questions.Count();
            return questions.Include(q => q.Answers)
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

        public async Task<IActionResult> FileUpload(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return RedirectToAction("Index", new{id});
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream).ConfigureAwait(false);

                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var errors = readExcelPackageQuestions(package, worksheet, id);

                    if (errors.Any())
                    {
                        TempData["Errors"] = errors;
                    }

                    return RedirectToAction("Index",new{id});
                }
            }
        }

        private List<string> readExcelPackageQuestions(ExcelPackage package, ExcelWorksheet worksheet, int cardId)
        {
            var errors = new List<string>();

            var rowCount = worksheet.Dimension?.Rows;
            var colCount = worksheet.Dimension?.Columns;

            int questionsCount = 0;
            int insertedQuestionsCount = 0;
            int updatedQuestionCount = 0;
            int errorQuestionsCount = 0;

            if (rowCount.HasValue && colCount.HasValue)
            {
                for (int row = 2; row <= rowCount.Value; row++)
                {
                    try
                    {
                        var questionText = worksheet.Cells[row, 2].Value?.ToString();
                        if (string.IsNullOrEmpty(questionText))
                        {
                            break;
                        }

                        questionsCount++;
                        bool? insertUpdate = null;

                        var id = int.Parse(worksheet.Cells[row, 1].Value.ToString());

                        Question question = _dbContext.Questions.SingleOrDefault(q => q.CardId == cardId && q.Id == id);

                        if (question == null)
                        {
                            question = new Question
                            {
                                Id = id,
                                CardId = cardId
                            };
                            _dbContext.Questions.Add(question);
                            insertUpdate = true;
                        }

                        if (question.Text != questionText)
                        {
                            if (!string.IsNullOrEmpty(question.Text))
                            {
                                insertUpdate = false;
                            }
                            question.Text = questionText;

                            _dbContext.Database.OpenConnection();
                            try
                            {
                                var questionMapping = _dbContext.Model.FindEntityType(typeof(Question)).Relational();
                                _dbContext.Database.ExecuteSqlCommand(
                                    (string)$"SET IDENTITY_INSERT dbo.{questionMapping.TableName} ON");
                                _dbContext.SaveChanges();
                                _dbContext.Database.ExecuteSqlCommand(
                                    (string)$"SET IDENTITY_INSERT dbo.{questionMapping.TableName} OFF");
                            }
                            finally
                            {
                                _dbContext.Database.CloseConnection();
                            }
                        }

                        var questionAnswers = _dbContext.QuestionAnswers.Where(a => a.QuestionId == question.Id).OrderBy(a => a.Id).ToList();
                        QuestionAnswer questionAnswerA = questionAnswers.FirstOrDefault();

                        if (questionAnswerA == null)
                        {
                            questionAnswerA = new QuestionAnswer
                            {
                                QuestionId = question.Id
                            };
                            _dbContext.QuestionAnswers.Add(questionAnswerA);
                        }

                        var questionAText = worksheet.Cells[row, 3].Value.ToString();

                        if ((insertUpdate == null || !insertUpdate.Value) 
                            && questionAnswerA.Text != questionAText)
                        {
                            insertUpdate = false;
                        }
                        questionAnswerA.Text = questionAText;

                        QuestionAnswer questionAnswerB = questionAnswers.Skip(1).FirstOrDefault();
                        if (questionAnswerB == null)
                        {
                            questionAnswerB = new QuestionAnswer
                            {
                                QuestionId = question.Id
                            };
                            _dbContext.QuestionAnswers.Add(questionAnswerB);
                        }

                        var questionBText = worksheet.Cells[row, 4].Value.ToString();
                        if ((insertUpdate == null || !insertUpdate.Value)
                            && questionAnswerB.Text != questionBText)
                        {
                            insertUpdate = false;
                        }
                        questionAnswerB.Text = questionBText;

                        QuestionAnswer questionAnswerC = questionAnswers.Skip(2).FirstOrDefault();
                        if (questionAnswerC == null)
                        {
                            questionAnswerC = new QuestionAnswer
                            {
                                QuestionId = question.Id
                            };
                            _dbContext.QuestionAnswers.Add(questionAnswerC);
                        }

                        var questionCText = worksheet.Cells[row, 5].Value.ToString();

                        if ((insertUpdate == null || !insertUpdate.Value)
                            && questionAnswerC.Text != questionCText)
                        {
                            insertUpdate = false;
                        }
                        questionAnswerC.Text = questionCText;

                        if (insertUpdate.HasValue)
                        {
                            _dbContext.SaveChanges();
                            if (insertUpdate.Value)
                            {
                                insertedQuestionsCount++;
                            }
                            else
                            {
                                updatedQuestionCount++;
                            }

                            if (worksheet.Cells[row, 3].Style.Font.Bold)
                            {
                                question.CorrectAnswerId = questionAnswerA.Id;
                            }

                            if (worksheet.Cells[row, 4].Style.Font.Bold)
                            {
                                question.CorrectAnswerId = questionAnswerB.Id;
                            }

                            if (worksheet.Cells[row, 5].Style.Font.Bold)
                            {
                                question.CorrectAnswerId = questionAnswerC.Id;
                            }
                            _dbContext.SaveChanges();
                        }

                    }  
                    catch (Exception e)
                    {
                        errors.Add($"Ошибка при обработке строки {row}. " + e.Message);
                        errorQuestionsCount++;
                    }
                }
            }

            TempData["Message"] =
                $"Всего вопросов: {questionsCount}, добавлено {insertedQuestionsCount}, обновлено: {updatedQuestionCount}, с ошибками: {errorQuestionsCount}.";

            return errors;
        }
    }
}