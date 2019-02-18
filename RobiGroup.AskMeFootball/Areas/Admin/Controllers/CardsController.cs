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
                    ResetTime = r.ResetTime,
                    Type = r.Type.Name + "(" + r.ResetPeriod + ")",
                    ImageUrl = r.ImageUrl,
                    MatchQuestions = r.MatchQuestions,
                    QuestionsTotal = r.Questions.Count,
                    Prize = r.Prize
                });
        }

        #endregion

        #region Create/Edit

        public IActionResult Create(int? id)
        {
            LoadViewData();
            CardCreateModel createModel;

            if (id.HasValue)
            {
                createModel = _dbContext.Cards.Where(c => c.Id == id.Value).Select(r => new CardCreateModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ResetTime = r.ResetTime,
                    TypeId = r.TypeId,
                    ImageUrl = r.ImageUrl,
                    MatchQuestions = r.MatchQuestions,
                    Prize = r.Prize,
                    ResetPeriod = r.ResetPeriod
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
            card.ResetTime = model.ResetTime;
            card.ResetPeriod = model.ResetPeriod;
            card.Prize = model.Prize;
            card.MatchQuestions = model.MatchQuestions;
            _dbContext.SaveChanges();

            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrl)))
            {
                var photosDir = _hostingEnvironment.GetCardPhotosFolder(card.Id);
                if (Directory.Exists(photosDir))
                {
                    Directory.Delete(photosDir, true);
                }

                Directory.CreateDirectory(photosDir);

                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.ImageUrl);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        card.ImageUrl = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
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
    }
}