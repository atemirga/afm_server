namespace RobiGroup.Web.Common.Configuration
{
    public class GoogleAnalyticsOptions
    {
        public string BaseUri = "https://www.google-analytics.com";

        public string CollectPath = "/collect";

        public string TrackingId { get; set; }
    }
}