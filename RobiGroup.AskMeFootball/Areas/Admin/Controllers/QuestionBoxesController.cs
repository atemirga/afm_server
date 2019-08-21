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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Questions;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class QuestionBoxesController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public QuestionBoxesController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(int id)
        {
            LoadIndexViewData(id);
            var infos = _dbContext.QuestionBoxes.Where(q => q.QuestionId == id)
                .AsQueryable();

            var _infos = new List<QuestionBoxesViewModel>();
            foreach (var i in infos)
            {
                var _i = new QuestionBoxesViewModel();
                _i.Id = i.Id;
                _i.QuestionId = i.QuestionId;
                _i.Type = i.Type;
                _i.Text = i.Text;
                _i.ImageUrl = i.ImageUrl;

                _infos.Add(_i);
            }

            return View(_infos);
        }

        private void LoadIndexViewData(int id)
        {
            ViewBag.QuestionId = id;
        }

        public IActionResult _Filter(QuestionBoxesFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterInfos);
        }

        private IEnumerable<QuestionBoxesViewModel> FilterInfos(QuestionBoxesFilterModel filter)
        {
            var infos = _dbContext.QuestionBoxes.Where(q => q.QuestionId == filter.QuestionId)
                .AsQueryable();

            var _infos = new List<QuestionBoxesViewModel>();
            foreach (var i in infos)
            {
                var _i = new QuestionBoxesViewModel();
                _i.Id = i.Id;
                _i.QuestionId = i.QuestionId;
                _i.Type = i.Type;
                _i.Text = i.Text;
                _i.ImageUrl = i.ImageUrl;

                _infos.Add(_i);
            }

            return _infos;
        }

        #region Create/Edit

        public IActionResult Create(int questionId, int? id)
        {
            QuestionBoxesCreateModel createModel;

            if (id.HasValue)
            {
                createModel = _dbContext.QuestionBoxes.Where(c => c.Id == id).Select(r => new QuestionBoxesCreateModel()
                {
                    Id = r.Id,
                    QuestionId = r.QuestionId,
                    Type = r.Type,
                    Text = r.Text,
                    ImageUrl = r.ImageUrl
                }).Single();
            }
            else
            {
                createModel = new QuestionBoxesCreateModel()
                {
                    QuestionId = questionId,
                };
            }
            return View(createModel);
            //return View(new InfoCreateModel()
            //{
            //    CardId = cardId,
            //});
        }

        [HttpPost]
        public IActionResult Create(QuestionBoxesCreateModel model)
        {

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            QuestionBox box;

            if (model.Id.HasValue)
            {
                box = _dbContext.QuestionBoxes.Find(model.Id.Value);
            }
            else
            {
                box = new QuestionBox();
                _dbContext.QuestionBoxes.Add(box);
            }

            box.QuestionId = model.QuestionId;
            box.Type = model.Type;
            box.Text = model.Text;

            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrl)))
            {
                var photosDir = _hostingEnvironment.GetQuestionBoxFolder(box.Id);
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

                        box.ImageUrl = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
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

            return RedirectToAction("Index", new { id = model.QuestionId });
        }

        #endregion

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var QuestionId = _dbContext.QuestionBoxes.FirstOrDefault(c => c.Id == id).QuestionId;
            _dbContext.QuestionBoxes.Remove(_dbContext.QuestionBoxes.FirstOrDefault(c => c.Id == id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index", new { id = QuestionId });
        }
    }
}