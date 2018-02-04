using System.Configuration;

namespace BMIDocuSignApi {

    public static class SystemConfiguration {
        public static readonly string DocuSignRestApiBasePath = ConfigurationManager.AppSettings["DocuSignRestApiBasePath"];
        public static readonly string DocuSignIntegratorKey = ConfigurationManager.AppSettings["DocuSignIntegratorKey"];
        public static readonly string DocuSignAccountId = ConfigurationManager.AppSettings["DocuSignAccountId"];
        public static readonly string DocuSignUsername = ConfigurationManager.AppSettings["DocuSignUsername"];
        public static readonly string DocuSignPassword = ConfigurationManager.AppSettings["DocuSignPassword"];
    }
}