using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RobiGroup.Web.Common.Configuration;
using RobiGroup.Web.Common.Services.Models;

namespace RobiGroup.Web.Common.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class MobizonSmsSender : ISmsSender
    {
        private readonly MobizonOptions _options;
        private static readonly string SMS_SERVICE_URL = "https://api.mobizon.com/service/";

        public MobizonSmsSender(IOptions<MobizonOptions> options)
        {
            _options = options.Value;
        }
        
        public async Task SendSmsAsync(string number, string message)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            
            var response = await client.GetStringAsync($"{SMS_SERVICE_URL}message/sendsmsmessage?apiKey={_options.ApiKey}&recipient={number}&text={message}");
            var mobizonReponse = JsonConvert.DeserializeObject<MobizonReponse>(response);
            // var mobizonReponse = new MobizonReponse() {Code = 0, Data = "Test", Message = "Test message"};
               
            if (mobizonReponse.Code != 0 && mobizonReponse.Code != 100)
            {
                if (mobizonReponse.Code == 1)
                {
                    if (mobizonReponse.Data != null)
                    {
                        var data = JsonConvert.DeserializeObject<MobizonRecipientData>(mobizonReponse.Data.ToString());
                        throw new Exception("Mobizon: " + data.Recipient);
                    }
                    else
                    {
                        throw new Exception("Mobizon: " + mobizonReponse.Message);
                    }
                }

                throw new Exception($"Mobizon response code: {mobizonReponse.Code}");
            }
        }
    }
}
