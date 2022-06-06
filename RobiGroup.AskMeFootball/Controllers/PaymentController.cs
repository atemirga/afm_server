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
using System.Net;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;


namespace RobiGroup.AskMeFootball.Controllers
{
    [Route("api/payment")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PaymentController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Отправить
        /// </summary>
        /// <returns></returns>

        [HttpPost("send")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Send([FromBody]CloudpaymentPayment payment)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.cloudpayments.kz/payments/cards/charge");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            String username = "pk_9761453d649bbd16e67249554980c";
            String password = "0cfad7c55aa695b9f8f1fe38faa553f9";
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(payment);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var result = "";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return Ok(result);
        }
    }
}