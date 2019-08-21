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
using RobiGroup.AskMeFootball.Areas.Admin.Models.InfoCards;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Questions;
using RobiGroup.AskMeFootball.Common.Files; 
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class InfoCardsController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public InfoCardsController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult _Filter(InfoCardsFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterCards);
        }

        private IEnumerable<InfoCardViewModel> FilterCards(InfoCardsFilterModel filter)
        {
            var cardsQuery = _dbContext.InfoCards.AsQueryable();

            filter.Total = cardsQuery.Count();
            return cardsQuery
                .Select(r => new InfoCardViewModel()
                {
                    Id = r.Id,
                    Title = r.Title,
                    Subtitle = r.SubTitle,
                    ButtonTitle = r.ButtonTitle,
                    Images = _dbContext.InfoCardImages.Where(ici => ici.InfoCardId == r.Id).Select(s => s.Url).ToList(),
                    ImageUrl = r.ImageUrl,
                    VideoUrl = r.VideoUrl,
                    EndDate = r.EndTime,
                    IsActive = r.IsActive ? 1 : 0
                });
        }

        #endregion

        #region Create/Edit

        public IActionResult Create(int? id)
        {
            InfoCardCreateModel createModel;

            if (id.HasValue)
            {
                createModel = _dbContext.InfoCards.Where(c => c.Id == id.Value).Select(r => new InfoCardCreateModel()
                {
                    Id = r.Id,
                    Title = r.Title,
                    Subtitle = r.SubTitle,
                    ButtonTitle = r.ButtonTitle,
                    Images = _dbContext.InfoCardImages.Where(ici => ici.InfoCardId == r.Id).Select(s => s.Url).ToList(),
                    ImageUrl = r.ImageUrl,
                    VideoUrl = r.VideoUrl,
                    EndDate = r.EndTime,
                    IsActive = r.IsActive ? 1 : 0
                }).Single();
            }
            else
            {
                createModel = new InfoCardCreateModel()
                {
                    IsActive = 0,
                };
            }

            return View(createModel);
        }
       

        [HttpPost]
        public IActionResult Create(InfoCardCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            InfoCard card;

            if (model.Id.HasValue)
            {
                card = _dbContext.InfoCards.Find(model.Id.Value);
            }
            else
            {
                card = new InfoCard();
                _dbContext.InfoCards.Add(card);
            }

            card.Title = model.Title;
            card.SubTitle = model.Subtitle;
            card.ButtonTitle = model.ButtonTitle;
            card.EndTime = model.EndDate;
            if (model.IsActive == 0)
            {
                card.IsActive = false;
            }
            else if (model.IsActive == 1)
            {
                card.IsActive = true;
            }
            else { card.IsActive = false; }
           
            _dbContext.SaveChanges();

            if (model.ImageUrl != null)
            {
                if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrl)))
                {
                    var photosDir = _hostingEnvironment.GetInfoCardPhotosFolder(card.Id);
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
            }

            //upload images 
            if (model.ImageFiles != null)
            {

                var imagesDir = _hostingEnvironment.GetInfoCardImagesFolder(card.Id);
                if (Directory.Exists(imagesDir))
                {
                    Directory.Delete(imagesDir, true);
                }

                Directory.CreateDirectory(imagesDir);

                foreach (var image in model.ImageFiles)
                {
                    var path = Path.Combine(imagesDir, Path.GetFileName(image.FileName));

                    image.CopyTo(new FileStream(path, FileMode.Create));

                    var url = Path.GetRelativePath(_hostingEnvironment.WebRootPath, path);

                    _dbContext.InfoCardImages.Add(new InfoCardImage {
                        InfoCardId = card.Id,
                        Url = url
                    });
                }

                
                _dbContext.SaveChanges();

            }

            //upload video file
            if (model.VideoFile != null)
            {

                var videosDir = _hostingEnvironment.GetInfoCardVideosFolder(card.Id);
                if (Directory.Exists(videosDir))
                {
                    Directory.Delete(videosDir, true);
                }

                Directory.CreateDirectory(videosDir);

                if (model.VideoFile == null || model.VideoFile.Length == 0)
                    return Content("file not selected");

                var path = Path.Combine(videosDir, Path.GetFileName(model.VideoFile.FileName));

                model.VideoFile.CopyTo(new FileStream(path, FileMode.Create));

                card.VideoUrl = Path.GetRelativePath(_hostingEnvironment.WebRootPath, path);
                _dbContext.SaveChanges();
                
            }


            return RedirectToAction("Index");
        }

        #endregion



        [HttpPost]
        public IActionResult Delete(int id)
        {
            
            _dbContext.InfoCards.Remove(_dbContext.InfoCards.FirstOrDefault(c => c.Id == id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}