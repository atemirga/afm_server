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
using RobiGroup.AskMeFootball.Areas.Admin.Models.Tickets;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class TicketsController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public TicketsController(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Table


        public IActionResult Index()
        {
            var ticketsQuery = _dbContext.Tickets.AsQueryable();
            var tickets = new List<TicketViewModel>();

            foreach (var tq in ticketsQuery)
            {
                
                var ticketsView = new TicketViewModel();
                ticketsView.Id = tq.Id;
                ticketsView.CreatedDate = tq.CreatedDate;
                ticketsView.Attachment = _dbContext.TicketAttachments.Where(ta => ta.TicketId == tq.Id).Select(s => s.Url).ToList();
                ticketsView.NickName = _dbContext.Users.Any(u => u.Id == tq.UserId) ? 
                    _dbContext.Users.First(u => u.Id == tq.UserId).NickName : "Вас нет в этой базе";
                ticketsView.Phone = _dbContext.Users.Any(u => u.Id == tq.UserId) ?
                    _dbContext.Users.First(u => u.Id == tq.UserId).NickName : "Вас нет в этой базе";
                ticketsView.Text = tq.Text;
                tickets.Add(ticketsView);
            }
            return View(tickets);
        }

        #endregion

        #region Show
        public IActionResult Show(int id)
        {
            //LoadViewData();
            TicketViewModel viewModel;

            viewModel = _dbContext.Tickets.Where(t => t.Id == id).Select(r => new TicketViewModel()
            {
                Id = r.Id,
                NickName = _dbContext.Users.First(u => u.Id == r.UserId).NickName,
                Phone = _dbContext.Users.First(u => u.Id == r.UserId).PhoneNumber,
                Attachment = _dbContext.TicketAttachments.Where(ta => ta.TicketId == id).Select(s => s.Url).ToList(),
                Text = r.Text

            }).Single();


            return View(viewModel);
        }
        #endregion
    }
}