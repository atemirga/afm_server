namespace RobiGroup.Web.Common.Configuration
{
    public class VidyoOptions
    {
        public string ApplicationId { get; set; }

        public string DeveloperKey { get; set; }

        public int ExpiresInSecs { get; set; } = 3 * 60 * 60;
    }
}