using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RobiGroup.Web.Common;
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
                questions = questions.Where(q => q.TextRu.Contains(filter.Search));
            }

            filter.Total = questions.Count();
            return questions.Include(q => q.Answers)
                .Skip(filter.DataTablesRequest.Start)
                .Take(filter.DataTablesRequest.Length)
                .Select(r => new QuestionViewModel()
                {
                    Id = r.Id,
                    Text = r.TextRu,
                    AnswerId = r.CorrectAnswerId,
                    Answers = r.Answers.Select(a => new AnswerViewModel
                    {
                        Id = a.Id,
                        Text = a.TextRu
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
                ModelState.AddModelError(nameof(model.Answers), "Введите хотябы 2 варианта ответа на вопрос.");
            }

            if (model.CorrectAnswerId < 0 || model.CorrectAnswerId >= filledAnswersCount)
            {
                ModelState.AddModelError(nameof(model.Answers), "Выберите правильный ответ.");
            }

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            var question = new Question
            {
                TextRu = model.Text,
                TextKz = model.Text,
                StartTime = model.StartTime,
                ExpirationTime = model.ExpirationTime,
                Delay = model.Delay,
                CardId = model.CardId
            };

            
            question.Answers = model.Answers.Where(a => !string.IsNullOrEmpty(a)).Select(a => new QuestionAnswer{ TextRu = a }).ToList();
            _dbContext.Questions.Add(question);
            _dbContext.SaveChanges();

            var card = _dbContext.Cards.Find(model.CardId);
            var cardType = _dbContext.CardTypes.FirstOrDefault(ct => ct.Id == _dbContext.Cards.FirstOrDefault(c => c.Id == model.CardId).TypeId);

            if (cardType.Code == "HalfTime")
            {
                var match = _dbContext.Matches.LastOrDefault(m => m.CardId == model.CardId && m.Status == Match.MatchStatus.Requested);
                var newQuestion = question.Id;
                if (match.Questions == null)
                {
                    var addQuestions = new List<int>();
                    addQuestions.Add(newQuestion);
                    match.Questions = string.Join(',', addQuestions);
                }
                else {
                    var matchQuestions = match.Questions.SplitToIntArray().ToList();
                    matchQuestions.Add(newQuestion);
                    match.Questions = string.Join(',', matchQuestions);
                }
                match.Status = Match.MatchStatus.Started;
                card.IsActive = true;
                _dbContext.SaveChanges();
            }

            if (cardType.Code == "Live")
            {
                var match = _dbContext.Matches.LastOrDefault(m => m.CardId == model.CardId && m.Status == Match.MatchStatus.Requested);
               
                var newQuestion = question.Id;
                if (match.Questions == null)
                {
                    var addQuestions = new List<int>();
                    addQuestions.Add(newQuestion);
                    match.Questions = string.Join(',', addQuestions);
                }
                else
                {
                    var matchQuestions = match.Questions.SplitToIntArray().ToList();
                    matchQuestions.Add(newQuestion);
                    match.Questions = string.Join(',', matchQuestions);
                }

                if (match.Questions.SplitToIntArray().Count() == card.MatchQuestions)
                {
                    match.Status = Match.MatchStatus.Started;
                    card.IsActive = true;
                }

                _dbContext.SaveChanges();
            }


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

            var last_id = 0;

            var questionsExist = _dbContext.Questions.OrderByDescending(q => q.Id).FirstOrDefault();
            if (questionsExist != null)
            {
                last_id = _dbContext.Questions.OrderByDescending(q => q.Id).FirstOrDefault().Id;
            }


            if (rowCount.HasValue && colCount.HasValue)
            {
                //deleting old questions and answers
                var oldQuestions = _dbContext.Questions.Where(q => q.CardId == cardId);
                foreach (var oq in oldQuestions)
                {
                    var oldQuestionAnswers = _dbContext.QuestionAnswers.Where(qa => qa.QuestionId == oq.Id);
                    foreach (var oqa in oldQuestionAnswers)
                    {
                        var matchQuestions = _dbContext.MatchAnswers.Where(ma => ma.QuestionId == oq.Id);
                        _dbContext.RemoveRange(matchQuestions);
                        var matchAnswers = _dbContext.MatchAnswers.Where(ma => ma.AnswerId == oqa.Id);
                        _dbContext.RemoveRange(matchAnswers);
                    }
                    _dbContext.RemoveRange(oldQuestionAnswers);
                    _dbContext.RemoveRange(oq);
                }
                _dbContext.SaveChanges();



                for (int row = 1; row <= rowCount.Value; row++)
                {
                    try
                    {
                        var questionTextRu = worksheet.Cells[row, 2].Value?.ToString();
                        var questionTextKz = worksheet.Cells[row, 3].Value?.ToString();
                        if (string.IsNullOrEmpty(questionTextRu) && string.IsNullOrEmpty(questionTextKz))
                        {
                            break;
                        }

                        questionsCount++;
                        bool? insertUpdate = null;

                        var id = last_id + int.Parse(worksheet.Cells[row, 1].Value.ToString());

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

                        if (question.TextRu != questionTextRu && question.TextKz != questionTextKz)
                        {
                            if (!string.IsNullOrEmpty(question.TextRu) && !string.IsNullOrEmpty(question.TextKz))
                            {
                                insertUpdate = false;
                            }
                            question.TextRu = questionTextRu;
                            question.TextKz = questionTextKz;

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

                        var questionATextRu = worksheet.Cells[row, 4].Value.ToString();
                        var questionATextKz = worksheet.Cells[row, 5].Value.ToString();

                        if ((insertUpdate == null || !insertUpdate.Value) 
                            && questionAnswerA.TextRu != questionATextRu && questionAnswerA.TextKz != questionATextKz)
                        {
                            insertUpdate = false;
                        }
                        questionAnswerA.TextRu = questionATextRu;
                        questionAnswerA.TextKz = questionATextKz;

                        QuestionAnswer questionAnswerB = questionAnswers.Skip(1).FirstOrDefault();
                        if (questionAnswerB == null)
                        {
                            questionAnswerB = new QuestionAnswer
                            {
                                QuestionId = question.Id
                            };
                            _dbContext.QuestionAnswers.Add(questionAnswerB);
                        }

                        var questionBTextRu = worksheet.Cells[row, 6].Value.ToString();
                        var questionBTextKz = worksheet.Cells[row, 7].Value.ToString();

                        if ((insertUpdate == null || !insertUpdate.Value)
                            && questionAnswerB.TextRu != questionBTextRu && questionAnswerB.TextKz != questionBTextKz)
                        {
                            insertUpdate = false;
                        }
                        questionAnswerB.TextRu = questionBTextRu;
                        questionAnswerB.TextKz = questionBTextKz;

                        QuestionAnswer questionAnswerC = questionAnswers.Skip(2).FirstOrDefault();
                        if (questionAnswerC == null)
                        {
                            questionAnswerC = new QuestionAnswer
                            {
                                QuestionId = question.Id
                            };
                            _dbContext.QuestionAnswers.Add(questionAnswerC);
                        }

                        var questionCTextRu = worksheet.Cells[row, 8].Value.ToString();
                        var questionCTextKz = worksheet.Cells[row, 9].Value.ToString();

                        if ((insertUpdate == null || !insertUpdate.Value)
                            && questionAnswerC.TextRu != questionCTextRu && questionAnswerC.TextKz != questionCTextKz)
                        {
                            insertUpdate = false;
                        }
                        questionAnswerC.TextRu = questionCTextRu;
                        questionAnswerC.TextKz = questionCTextKz;

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

                            if (worksheet.Cells[row, 4].Style.Font.Bold)
                            {
                                question.CorrectAnswerId = questionAnswerA.Id;
                            }

                            if (worksheet.Cells[row, 6].Style.Font.Bold)
                            {
                                question.CorrectAnswerId = questionAnswerB.Id;
                            }

                            if (worksheet.Cells[row, 8].Style.Font.Bold)
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