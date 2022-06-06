using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Prizes;
using RobiGroup.AskMeFootball.Common.Files;
using RobiGroup.AskMeFootball.Controllers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class PrizesController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public PrizesController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult _Filter(PrizesFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterPrizes);
        }

        private IEnumerable<PrizeViewModel> FilterPrizes(PrizesFilterModel filter)
        {
            var prizesQuery = _dbContext.Prizes.AsQueryable();

            filter.Total = prizesQuery.Count();
            return prizesQuery.Select(r => new PrizeViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Price = r.Price,
                    InStock = r.InStock,
                    Code = r.Code,
                });
        }

        #endregion

        #region Create/Edit

        public IActionResult Create(int? id)
        {
            //LoadViewData();
            PrizeCreateModel createModel;

            if (id.HasValue)
            {
                createModel = _dbContext.Prizes.Where(c => c.Id == id.Value).Select(r => new PrizeCreateModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    FirstPhotoUrl = r.FirstPhotoUrl,
                    SecondPhotoUrl = r.SecondPhotoUrl,
                    ThirdPhotoUrl = r.ThirdPhotoUrl,
                    InStock = r.InStock,
                    Price = r.Price,
                    Address = r.Address,
                    Date = r.Date,
                    Site = r.Site,
                    Facebook = r.Facebook,
                    FirstPhoneNumber = r.FirstPhoneNumber,
                    //SecondPhoneNumber = r.SecondPhoneNumber,
                    Instagram = r.Instagram,
                    Twitter = r.Twitter,
                    Vkontakte = r.Vkontakte
                }).Single();
            }
            else
            {
                createModel = new PrizeCreateModel()
                {
                    InStock = 0,
                    Price = 0
                };
            }

            return View(createModel);
        }

        /*
        private void LoadViewData()
        {
            ViewBag.Types = new SelectList(_dbContext.CardTypes, "Id", "Name");
        }
        */

        [HttpPost]
        public IActionResult Create(PrizeCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                //LoadViewData();
                return View("Create", model);
            }

            Prize prize;

            if (model.Id.HasValue)
            {
                prize = _dbContext.Prizes.Find(model.Id.Value);
            }
            else
            {
                prize = new Prize();
                _dbContext.Prizes.Add(prize);
            }

            prize.Name = model.Name;
            prize.Description = model.Description;
            prize.Price = model.Price;
            prize.InStock = model.InStock;
            prize.Address = model.Address;
            prize.Date = model.Date;
            prize.FirstPhoneNumber = model.FirstPhoneNumber;
            //prize.SecondPhoneNumber = model.SecondPhoneNumber;
            prize.Site = model.Site;
            prize.Facebook = model.Facebook;
            prize.Instagram = model.Instagram;
            prize.Twitter = model.Twitter;
            prize.Vkontakte = model.Vkontakte;
            Random random = new Random();
            prize.Code = random.Next(10000000, 99999999);
            _dbContext.SaveChanges();

            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.FirstPhotoUrl)) ||
                !System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.SecondPhotoUrl)) ||
                !System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ThirdPhotoUrl)))
            {
                var photosDir = _hostingEnvironment.GetPrizePhotosFolder(prize.Id);
                if (Directory.Exists(photosDir))
                {
                    Directory.Delete(photosDir, true);
                }

                Directory.CreateDirectory(photosDir);

                if (!string.IsNullOrEmpty(model.FirstPhotoUrl))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.FirstPhotoUrl);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        prize.FirstPhotoUrl = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.SecondPhotoUrl))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.SecondPhotoUrl);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        prize.SecondPhotoUrl = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.ThirdPhotoUrl))
                {
                    var photoFile = Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), model.ThirdPhotoUrl);
                    if (System.IO.File.Exists(photoFile))
                    {
                        var destFileName = Path.Combine(photosDir, Path.GetFileName(photoFile));
                        System.IO.File.Move(photoFile, destFileName);

                        prize.ThirdPhotoUrl = Path.GetRelativePath(_hostingEnvironment.WebRootPath, destFileName);
                        _dbContext.SaveChanges();

                        var dir = Path.GetDirectoryName(photoFile);
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }
            }

            #region IMAGES
            /*
            //Deatail
            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrlDetail)))
            {
                var photosDir = _hostingEnvironment.GetCardPhotosFolder(card.Id);
                if (Directory.Exists(photosDir))
                {
                    Directory.Delete(photosDir, true);
                }

                Directory.CreateDirectory(photosDir);

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
            }

            //Select
            if (!System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, model.ImageUrlSelect)))
            {
                var photosDir = _hostingEnvironment.GetCardPhotosFolder(card.Id);
                if (Directory.Exists(photosDir))
                {
                    Directory.Delete(photosDir, true);
                }

                Directory.CreateDirectory(photosDir);

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
            */
            #endregion
            return RedirectToAction("Index");
        }

        #endregion


        [HttpPost]
        public IActionResult Delete(int id)
        {
            _dbContext.Prizes.Remove(_dbContext.Prizes.Find(id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}