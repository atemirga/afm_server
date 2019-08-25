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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Competitives;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Questions;
using RobiGroup.AskMeFootball.Common.Files;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class CompetitivesController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CompetitivesController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult _Filter(CompetitiveFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterCards);
        }

        private IEnumerable<CompetitiveViewModel> FilterCards(CompetitiveFilterModel filter)
        {
            var cardsQuery = _dbContext.Cards.Where(c => c.TypeId == _dbContext.CardTypes.FirstOrDefault(ct => ct.Code == "Competitive").Id).AsQueryable();

            filter.Total = cardsQuery.Count();
            return cardsQuery.Include(r => r.Type).Include(r => r.Questions)
                .OrderBy(t => t.ResetTime)
                .Skip(filter.DataTablesRequest.Start)
                .Take(filter.DataTablesRequest.Length)
                .Select(r => new CompetitiveViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ImageUrlCard = r.ImageUrlCard,
                    ImageUrlDetail = r.ImageUrlDetail,
                    ImageUrlSelect = r.ImageUrlSelect,
                    MatchQuestions = r.MatchQuestions,
                    QuestionsTotal = r.Questions.Count,
                    Prize = r.Prize,
                    Gamers = _dbContext.CompetitiveInfos.Any(ci => ci.CardId == r.Id) ?
                             _dbContext.CompetitiveInfos.FirstOrDefault(ci => ci.CardId == r.Id).Gamers : 0,
                    StartTime = _dbContext.CompetitiveInfos.Any(ci => ci.CardId == r.Id) ?
                             _dbContext.CompetitiveInfos.FirstOrDefault(ci => ci.CardId == r.Id).StartTime : DateTime.Now,

                    EndTime = _dbContext.CompetitiveInfos.Any(ci => ci.CardId == r.Id) ?
                             _dbContext.CompetitiveInfos.FirstOrDefault(ci => ci.CardId == r.Id).EndTime : DateTime.Now,
                });
        }

        #endregion

        #region Create/Edit

        public IActionResult Create(int? id)
        {
            LoadViewData();
            CompetitiveCreateModel createModel;
           

            if (id.HasValue)
            {
                createModel = _dbContext.Cards.Where(c => c.Id == id.Value).Select(r => new CompetitiveCreateModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ImageUrlCard = r.ImageUrlCard,
                    ImageUrlDetail = r.ImageUrlDetail,
                    ImageUrlSelect = r.ImageUrlSelect,
                    MatchQuestions = r.MatchQuestions,
                    Prize = r.Prize,
                    EntryPoint = r.EntryPoint,
                    Gamers = _dbContext.CompetitiveInfos.Any(ci => ci.CardId == r.Id) ?
                             _dbContext.CompetitiveInfos.FirstOrDefault(ci => ci.CardId == r.Id).Gamers : 0,
                    StartTime = _dbContext.CompetitiveInfos.Any(ci => ci.CardId == r.Id) ?
                             _dbContext.CompetitiveInfos.FirstOrDefault(ci => ci.CardId == r.Id).StartTime : DateTime.Now,

                    EndTime = _dbContext.CompetitiveInfos.Any(ci => ci.CardId == r.Id) ?
                             _dbContext.CompetitiveInfos.FirstOrDefault(ci => ci.CardId == r.Id).EndTime : DateTime.Now,
                }).Single();
            }
            else
            {
                createModel = new CompetitiveCreateModel()
                {
                    MatchQuestions = 10,
                };
            }

            return View(createModel);
        }

        private void LoadViewData()
        {
            ViewBag.Types = new SelectList(_dbContext.CardTypes, "Id", "Name");
        }

        [HttpPost]
        public IActionResult Create(CompetitiveCreateModel model)
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

            
            var typeId = _dbContext.CardTypes.FirstOrDefault(ct => ct.Code == "Competitive").Id;
            var resetTime = DateTime.Now.AddDays(30);

            card.Name = model.Name;
            card.TypeId = typeId;
            card.ResetTime = resetTime;
            card.ResetPeriod = 1;
            card.EntryPoint = model.EntryPoint;
            card.MaxBid = 0;
            card.IsTwoH = false;
            card.Prize = model.Prize;
            card.MatchQuestions = model.MatchQuestions;
            card.IsActive = true;
            _dbContext.SaveChanges();

            if (model.Id.HasValue)
            {
                var competitiveInfo = _dbContext.CompetitiveInfos.FirstOrDefault(ci => ci.CardId == model.Id.Value);
                if (competitiveInfo != null)
                {

                    competitiveInfo.StartTime = model.StartTime;
                    competitiveInfo.EndTime = model.EndTime;
                    competitiveInfo.Gamers = model.Gamers;
                    _dbContext.SaveChanges();
                }
                else
                {
                    _dbContext.CompetitiveInfos.Add(new CompetitiveInfo
                    {
                        CardId = model.Id.Value,
                        Gamers = model.Gamers,
                        StartTime = model.StartTime,
                        EndTime = model.EndTime
                    });
                }
                _dbContext.SaveChanges();

            }
            else
            {
                _dbContext.CompetitiveInfos.Add(new CompetitiveInfo
                {
                    CardId = card.Id,
                    Gamers = model.Gamers,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime
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
            _dbContext.CompetitiveInfos.RemoveRange(_dbContext.CompetitiveInfos.Where(cl => cl.CardId == id));
            _dbContext.Cards.Remove(_dbContext.Cards.FirstOrDefault(c => c.Id == id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}