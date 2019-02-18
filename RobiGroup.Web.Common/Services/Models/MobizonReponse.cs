namespace RobiGroup.Web.Common.Services.Models
{
    public class MobizonReponse
    {
        public int Code { get; set; }

        public object Data { get; set; }

        public string Message { get; set; }
    }

    public class MobizonRecipientData
    {
        public string Recipient { get; set; }
    }
}