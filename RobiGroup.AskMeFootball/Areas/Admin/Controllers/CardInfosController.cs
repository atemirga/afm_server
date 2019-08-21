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
    public class CardInfosController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CardInfosController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(int id)
        {
            LoadIndexViewData(id);
            var infos = _dbContext.CardInfos.Where(q => q.CardId == id)
                .AsQueryable();
            
            var _infos = new List<InfoViewModel>();
            foreach (var i in infos)
            {
                var _i = new InfoViewModel();
                _i.Id = i.Id;
                _i.CardId = i.CardId;
                _i.Text = i.Text;
                _infos.Add(_i);
            }

            return View(_infos);
        }

        private void LoadIndexViewData(int id)
        {
            ViewBag.CardId = id;
        }

        public IActionResult _Filter(InfoFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterInfos);
        }

        private IEnumerable<InfoViewModel> FilterInfos(InfoFilterModel filter)
        {
            var infos = _dbContext.CardInfos.Where(q => q.CardId == filter.CardId)
                .AsQueryable();

            filter.Total = infos.Count();
            var _infos = new List<InfoViewModel>();
            foreach (var i in infos)
            {
                var _i = new InfoViewModel();
                _i.CardId = i.CardId;
                _i.Text = i.Text;
                _infos.Add(_i);
            }

            return _infos;
        }


        #region Create/Edit

        public IActionResult Create(int cardId, int? id)
        {
            InfoCreateModel createModel;

            if (id.HasValue)
            {
                createModel = _dbContext.CardInfos.Where(c => c.Id == id).Select(r => new InfoCreateModel()
                {
                    Id = r.Id,
                    CardId = r.CardId,
                    Text = r.Text
                }).Single();
            }
            else
            {
                createModel = new InfoCreateModel()
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
        public IActionResult Create(InfoCreateModel model)
        {

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            CardInfo cardInfo;

            if (model.Id.HasValue)
            {
                cardInfo = _dbContext.CardInfos.Find(model.Id.Value);
            }
            else
            {
                cardInfo = new CardInfo();
                _dbContext.CardInfos.Add(cardInfo);
            }

            cardInfo.CardId = model.CardId;
            cardInfo.Text = model.Text;
            _dbContext.SaveChanges();

            return RedirectToAction("Index", new { id = model.CardId });
        }

        #endregion

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var CardId = _dbContext.CardInfos.FirstOrDefault(c => c.Id == id).CardId;
            _dbContext.CardInfos.Remove(_dbContext.CardInfos.FirstOrDefault(c => c.Id == id));
            _dbContext.SaveChanges();
            return RedirectToAction("Index", new { id = CardId });
        }
    }
}