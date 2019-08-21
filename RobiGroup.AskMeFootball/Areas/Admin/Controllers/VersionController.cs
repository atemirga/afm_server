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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Version;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class VersionController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public VersionController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var versionQuery = _dbContext.Versions.AsQueryable();
            var versions = new List<VersionViewModel>();

            foreach (var version in versionQuery)
            {
                var v = new VersionViewModel();
                v.Id = version.Id;
                v.Version = version.Vers;
                v.LastUpdate = version.LastUpdate;

                versions.Add(v);
            }

            return View(versions);
        }

        [HttpPost]
        public IActionResult Update(string version)
        {
            var _version = _dbContext.Versions.FirstOrDefault();

            if (_version != null)
            {
                _version.Vers = version;
                _version.LastUpdate = DateTime.Now;
            }
            else {
                _dbContext.Versions.Add(new Data.Version{
                    Vers = version,
                    LastUpdate = DateTime.Now
            });
            }

            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}