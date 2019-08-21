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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Users;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class FriendsController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public FriendsController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index(string id)
        {
            LoadIndexViewData(id);

            if (TempData["Errors"] is List<string> errors)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(String.Empty, error);
                }
            }

            ViewBag.Message = TempData["Message"];

            return View(new FriendFilterModel { UserId = id });
        }

        private void LoadIndexViewData(string id)
        {
            ViewBag.UserId = id;
        }

        public IActionResult _Filter(FriendFilterModel filterModel)
        {
            return DataTableResponse(filterModel, FilterFriends);
        }

        private IEnumerable<FriendViewModel> FilterFriends(FriendFilterModel filter)
        {
            var phone  = _dbContext.Users.FirstOrDefault(q => q.Id == filter.UserId).PhoneNumber;
            var friends = new List<FriendViewModel>();
            var friend = new FriendViewModel();
            friends.Add(friend);
            
            return friends;
        }
        #endregion
    }
}