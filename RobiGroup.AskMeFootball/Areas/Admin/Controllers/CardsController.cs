using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Cards;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Questions;
using RobiGroup.AskMeFootball.Common.Files;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class CardsController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CardsController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult _Filter(CardsFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterCards);
        }

        private IEnumerable<CardViewModel> FilterCards(CardsFilterModel filter)
        {
            var cardsQuery = _dbContext.Cards.AsQueryable();

            filter.Total = cardsQuery.Count();
            return cardsQuery.Include(r => r.Type).Include(r => r.Questions)
                .OrderBy(t => t.ResetTime)
                .Skip(filter.DataTablesRequest.Start)
                .Take(filter.DataTablesRequest.Length)
                .Select(r => new CardViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ResetTime = r.ResetTime.AddHours(6),
                    Type = r.Type.Name + "(" + r.ResetPeriod + ")",
                    ImageUrlCard = r.ImageUrlCard,
                    ImageUrlDetail = r.ImageUrlDetail,
                    ImageUrlSelect = r.ImageUrlSelect,
                    MatchQuestions = r.MatchQuestions,
                    QuestionsTotal = r.Questions.Count,
                    Prize = r.Prize,
                    Matches = _dbContext.Matches.Any(m => m.CardId == r.Id) ? 
                                _dbContext.Matches.Where(m => m.CardId == r.Id).Count() : 0,
                    IsTwoH = r.IsTwoH ? 1 : 0//Convert.ToInt32(r.IsTwoH)
                });
        }

        #endregion

        #region Create/Edit

        public IActionResult Create(int? id)
        {
            LoadViewData();
            CardCreateModel createModel;

            //StartTime = (r.Type.Code == "Live") ? (_dbContext.Matches.Any(m => m.CardId == r.Id) ? 
            //                   Convert.ToDateTime(_dbContext.Matches.LastOrDefault(m => m.CardId == r.Id).StartTime) : Convert.ToDateTime("01/01/0001"))
            //                   : Convert.ToDateTime("01/01/0001"),

            if (id.HasValue)
            {
                createModel = _dbContext.Cards.Where(c => c.Id == id.Value).Select(r => new CardCreateModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ResetTime = r.ResetTime.AddHours(6),
                    TypeId = r.TypeId,
                    ImageUrlCard = r.ImageUrlCard,
                    ImageUrlDetail = r.ImageUrlDetail,
                    ImageUrlSelect = r.ImageUrlSelect,
                    MatchQuestions = r.MatchQuestions,
                    Prize = r.Prize,
                    EntryPoint = r.EntryPoint,
                    ResetPeriod = r.ResetPeriod,
                    MaxBid = r.MaxBid,
                    IsTwoH = r.IsTwoH ? 1 : 0,
                    Lifes = _dbContext.CardLimits.Any(cl =>cl.CardId == r.Id) ? 
                            _dbContext.CardLimits.FirstOrDefault(cl => cl.CardId == r.Id).Lifes : 0,
                    Hints = _dbContext.CardLimits.Any(cl => cl.CardId == r.Id) ?
                            _dbContext.CardLimits.FirstOrDefault(cl => cl.CardId == r.Id).Hints : 0
                }).Single();
            }
            else
            {
                createModel = new CardCreateModel()
                {
                    ResetTime = DateTime.Today.AddDays(1),
                    MatchQuestions = 10,
                    ResetPeriod = 1,
                };
            }

            return View(createModel);
        }

        private void LoadViewData()
        {
            ViewBag.Types = new SelectList(_dbContext.CardTypes, "Id", "Name");
        }

        [HttpPost]
        public IActionResult Create(CardCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadViewData();
                return View("Create", model);
            }

            Card card;

            if (model.Id.HasValue)
            {
                card = _dbContext.Cards.Find(model.Id.Value);
            }
            else
            {
                card = new Card();
                _dbContext.Cards.Add(card);
            }

            card.Name = model.Name;
            card.TypeId = model.TypeId;
            card.ResetTime = model.ResetTime.AddHours(-6);
            card.ResetPeriod = model.ResetPeriod;
            card.EntryPoint = model.EntryPoint;
            card.MaxBid = model.MaxBid;
            card.IsTwoH = Convert.ToBoolean(model.IsTwoH);
            card.Prize = model.Prize;
            card.MatchQuestions = model.MatchQuestions;
            _dbContext.SaveChanges();

            if (model.Id.HasValue)
            {
                var limit = _dbContext.CardLimits.FirstOrDefault(cl => cl.CardId == model.Id.Value);
                if (limit != null)
                {
                   
                    limit.Lifes = model.Lifes;
                    limit.Hints = model.Hints;
                    _dbContext.SaveChanges();
                }
                else
                {
                    _dbContext.CardLimits.Add(new CardLimits
                    {
                        CardId = model.Id.Value,
                        Lifes = model.Lifes,
                        Hints = model.Hints,
                    });
                }
                _dbContext.SaveChanges();

            }
            else {
                _dbContext.CardLimits.Add(new CardLimits
                {
                    CardId = card.Id,
                    Lifes = model.Lifes,
                    Hints = model.Hints,
                });
                _dbContext.SaveChanges();
            }
            
           

            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrlCard)) ||
                !System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrlDetail)) ||
                !System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrlSelect)))
            {
                var photosDir = _hostingEnvironment.GetCardPhotosFolder(card.Id);
                if (Directory.Exists(photosDir))
                {
                    Directory.Delete(photosDir, true);
                }

                Directory.CreateDirectory(photosDir);

                if (!string.IsNullOrEmpty(model.ImageUrlCard))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.ImageUrlCard);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        card.ImageUrlCard = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.ImageUrlDetail))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.ImageUrlDetail);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        card.ImageUrlDetail = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.ImageUrlSelect))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.ImageUrlSelect);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        card.ImageUrlSelect = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }
            }

          

            if (_dbContext.CardTypes.FirstOrDefault(c => c.Id == model.TypeId).Code == "HalfTime" ||
                _dbContext.CardTypes.FirstOrDefault(c => c.Id == model.TypeId).Code == "Live")
            {
                _dbContext.Matches.Add(new Match
                {
                    CardId = card.Id,
                    CreateTime = DateTime.Now,
                    StartTime = model.StartTime,
                    Status = Match.MatchStatus.Requested
                });

                _dbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion

        

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var questions = _dbContext.Questions.Where(c => c.CardId == id);
            foreach (var q in questions)
            {
                var matchAnswers = _dbContext.MatchAnswers.Where(ma => ma.QuestionId == q.Id);
                if (matchAnswers != null)
                {
                    _dbContext.MatchAnswers.RemoveRange(matchAnswers);
                }
                var answers = _dbContext.QuestionAnswers.Where(qa => qa.QuestionId == q.Id);
                if (answers != null)
                {
                    _dbContext.QuestionAnswers.RemoveRange(answers);
                }
            }
            _dbContext.Questions.RemoveRange(questions);
            _dbContext.CardLimits.RemoveRange(_dbContext.CardLimits.Where(cl => cl.CardId == id));
            _dbContext.Cards.Remove(_dbContext.Cards.FirstOrDefault(c => c.Id == id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}