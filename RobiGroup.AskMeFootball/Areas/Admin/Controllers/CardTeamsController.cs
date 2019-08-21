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
using RobiGroup.AskMeFootball.Common.Files;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Cards;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class CardTeamsController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CardTeamsController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(int id)
        {
            LoadIndexViewData(id);
            var infos = _dbContext.CardTeams.Where(q => q.CardId == id)
                .AsQueryable();

            var _infos = new List<TeamsViewModel>();
            foreach (var i in infos)
            {
                var _i = new TeamsViewModel();
                _i.Id = i.Id;
                _i.CardId = i.CardId;
                _i.FirstTeam = i.FirstTeam;
                _i.SecondTeam = i.SecondTeam;
                _i.FirstTeamScore = i.FirstTeamScore;
                _i.SecondTeamScore = i.SecondTeamScore;

                _infos.Add(_i);
            }

            return View(_infos);
        }

        private void LoadIndexViewData(int id)
        {
            ViewBag.CardId = id;
        }

        public IActionResult _Filter(TeamsFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterInfos);
        }

        private IEnumerable<TeamsViewModel> FilterInfos(TeamsFilterModel filter)
        {
            var infos = _dbContext.CardTeams.Where(q => q.CardId == filter.CardId)
                .AsQueryable();

            filter.Total = infos.Count();
            var _infos = new List<TeamsViewModel>();
            foreach (var i in infos)
            {
                var _i = new TeamsViewModel();
                _i.CardId = i.CardId;
                _i.FirstTeam = i.FirstTeam;
                _i.SecondTeam = i.SecondTeam;
                _i.FirstTeamScore = i.FirstTeamScore;
                _i.SecondTeamScore = i.FirstTeamScore;
                _infos.Add(_i);
            }

            return _infos;
        }


        #region Create/Edit

        public IActionResult Create(int cardId, int? id)
        {
            TeamsCreateModel createModel;

            if (id.HasValue)
            {
                createModel = _dbContext.CardTeams.Where(c => c.Id == id).Select(r => new TeamsCreateModel()
                {
                    Id = r.Id,
                    CardId = r.CardId,
                    FirstTeam = r.FirstTeam,
                    SecondTeam = r.SecondTeam,
                    FirstTeamScore = r.FirstTeamScore,
                    SecondTeamScore = r.SecondTeamScore
                }).Single();
            }
            else
            {
                createModel = new  TeamsCreateModel()
                {
                    CardId = cardId,
                };
            }
            return View(createModel);
            //return View(new InfoCreateModel()
            //{
            //    CardId = cardId,
            //});
        }

        [HttpPost]
        public IActionResult Create(TeamsCreateModel model)
        {

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            CardTeams cardTeam;

            if (model.Id.HasValue)
            {
                cardTeam = _dbContext.CardTeams.Find(model.Id.Value);
            }
            else
            {
                cardTeam = new CardTeams();
                _dbContext.CardTeams.Add(cardTeam);
            }

            cardTeam.CardId = model.CardId;
            cardTeam.FirstTeam = model.FirstTeam;
            cardTeam.SecondTeam = model.SecondTeam;
            cardTeam.FirstTeamScore = model.FirstTeamScore;
            cardTeam.SecondTeamScore = model.SecondTeamScore;

            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.FirstTeamLogo)) ||
                !System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.SecondTeamLogo)))
            {
                var photosDir = _hostingEnvironment.GetCardTeamLogosFolder(cardTeam.Id);
                if (Directory.Exists(photosDir))
                {
                    Directory.Delete(photosDir, true);
                }

                Directory.CreateDirectory(photosDir);

                if (!string.IsNullOrEmpty(model.FirstTeamLogo))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.FirstTeamLogo);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        cardTeam.FirstTeamLogo = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(model.SecondTeamLogo))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.SecondTeamLogo);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        cardTeam.SecondTeamLogo = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }
            }

                _dbContext.SaveChanges();

            return RedirectToAction("Index", new { id = model.CardId });
        }

        #endregion

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var CardId = _dbContext.CardTeams.FirstOrDefault(c => c.Id == id).CardId;
            _dbContext.CardTeams.Remove(_dbContext.CardTeams.FirstOrDefault(c => c.Id == id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index", new { id = CardId });
        }
    }
}