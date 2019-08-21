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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Packs;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class PacksController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public PacksController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var packs = _dbContext.PackPrices.AsQueryable();

            var _packs = new List<PacksViewModel>();
            foreach (var i in packs)
            {
                var _i = new PacksViewModel();
                _i.Id = i.Id;
                _i.Type = i.Type;
                _i.Count = i.Count;
                _i.Price = i.Price;

                _packs.Add(_i);
            }

            return View(_packs);
        }
       

        #region Create/Edit

        public IActionResult Create(int? id)
        {
            PacksCreateModel createModel;

            if (id.HasValue)
            {
                createModel = _dbContext.PackPrices.Where(c => c.Id == id).Select(r => new PacksCreateModel()
                {
                    Id = r.Id,
                    Count  = r.Count,
                    Type = r.Type,
                    Price = r.Price
                }).Single();
            }
            else
            {
                createModel = new PacksCreateModel()
                {
                    Type = "life",
                };
            }
            return View(createModel);
        }

        [HttpPost]
        public IActionResult Create(PacksCreateModel model)
        {

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            PackPrice pack;

            if (model.Id.HasValue)
            {
                pack = _dbContext.PackPrices.Find(model.Id.Value);
            }
            else
            {
                pack = new PackPrice();
                _dbContext.PackPrices.Add(pack);
            }

            pack.Count = model.Count;
            pack.Type = model.Type;
            pack.Price = model.Price;
           

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        #endregion

        [HttpPost]
        public IActionResult Delete(int id)
        {
            
            _dbContext.PackPrices.Remove(_dbContext.PackPrices.FirstOrDefault(c => c.Id == id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}