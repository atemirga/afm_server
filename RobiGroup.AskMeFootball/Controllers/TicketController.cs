using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services;
using System.Drawing;


namespace RobiGroup.AskMeFootball.Controllers
{
    [Route("api/ticket")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public TicketController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Категории тикетов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(TicketCategory), 200)]
        public IActionResult Get()
        {

            return Ok(_dbContext.TicketCategories.AsQueryable());
        }

        /// <summary>
        /// Отправить
        /// </summary>
        /// <returns></returns>
        
        [HttpPost("send/{id}/{text}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Send([FromRoute]int id, [FromRoute]string text)
        {
            var files = HttpContext.Request.Form.Files;
            var userId = User.GetUserId();

            if (files.Count == 0)
            {
                _dbContext.Add(new Ticket
                {
                    UserId = userId,
                    CategoryId = id,
                    Text = text,
                    CreatedDate = DateTime.Now
                });
                _dbContext.SaveChanges();

                return Ok();
            }

            var fileService = HttpContext.RequestServices.GetService<IFileService>();
            var hostingEnvironment = HttpContext.RequestServices.GetService<IHostingEnvironment>();

            var ticketAttachments = new List<TicketAttachment>();

            var ticket = new Ticket();
            ticket.UserId = userId;
            ticket.Text = text;
            ticket.CreatedDate = DateTime.Now;
            ticket.CategoryId = id;
            _dbContext.Tickets.Add(ticket);
            _dbContext.SaveChanges();

            for (int i = 0; i < files.Count; i++)
            {
                var photoPath = await fileService.Save(files[i], $"data/user/{userId}/ticket");

                var ta = new TicketAttachment();
                ta.TicketId = ticket.Id;
                ta.Url = Path.GetRelativePath(hostingEnvironment.WebRootPath, photoPath);
                ticketAttachments.Add(ta);
               
            }
            _dbContext.TicketAttachments.AddRange(ticketAttachments);
            _dbContext.SaveChanges();

            return Ok();
        }
    }
}