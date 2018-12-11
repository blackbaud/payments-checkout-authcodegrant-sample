namespace CheckoutAuthCodeGrant
{
    /// <summary>
    /// Provide application configuration.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// The application's Sky API callback URI.
        /// </summary>
        public static string ApplicationCallbackUri = "https://localhost:44300/auth/callback";

        /// <summary>
        /// The application's Sky API identifier.
        /// </summary>
        public static string ApplicationId = "";

        /// <summary>
        /// The application's Sky API secret.
        /// </summary>
        public static string ApplicationSecret = "";

        /// <summary>
        /// The Sky API subscription key to use when making Payments API requests.
        /// </summary>
        public static string SubscriptionKey = "";
    }
}