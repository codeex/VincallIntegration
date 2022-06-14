namespace VincallIntegration.Application.Oauth
{
    public class Comm100Oauth
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
        public string scope { get; set; }
        public string redirect_uri { get; set; }

        public string redirect_logon { get; set; }
    }
}