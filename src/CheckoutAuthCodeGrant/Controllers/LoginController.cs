namespace CheckoutAuthCodeGrant.Controllers
{
    using Models;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    /// <summary>
    /// A controller to handle the login page for this application.
    /// </summary>
    /// <remarks>
    /// The login page is intended to be an example of a page that would be provided to a user within an organization that uses a Blackbaud product.
    /// The organization user would visit the login page in order to authenticate with Sky API so that this application can consume Sky API functionality
    /// and the organization's data on behalf of the user.
    /// </remarks>
    public class LoginController : Controller
    {
        // Cache Payments API request test result.
        private static ErrorViewModel PaymentsApiRequestResult;
        private static string PaymentsApiRequestAccessToken = "";

        /// <summary>
        /// Endpoint to support the login page.
        /// </summary>
        [Route("login")]
        public async Task<ActionResult> Index()
        {
            var model = new LoginViewModel
            {
                AccessToken = SkyApiAuthenticator.AccessToken,
                RefreshToken = SkyApiAuthenticator.RefreshToken
            };

            await FindPotentialApplicationErrors(model);
            return View(model);
        }

        /// <summary>
        /// Endpoint to support the ApplicationCallbackUri for Sky API authentication.
        /// </summary>
        /// <remarks>
        /// This endpoint will be called by Sky API after the user authenticates.  Sky API will pass an authorization code as a
        /// query string parameter 'code' which this application will use to retreive tokens from Sky API.
        ///
        /// Refer to https://developer.blackbaud.com/skyapi/docs/authorization/auth-code-flow#user-is-redirected-back for callback documentation.
        /// </remarks>
        [Route("auth/callback")]
        public async Task<ActionResult> LoginCallbackSkyApi(string code)
        {
            await SkyApiAuthenticator.RequestTokens(code);

            return Redirect("/login");
        }

        /// <summary>
        /// Endpoint to redirect the organization user to authenticate with Sky API.
        /// </summary>
        [Route("skyapi/login")]
        public ActionResult LoginSkyApi()
        {
            // Refer to https://developer.blackbaud.com/skyapi/docs/authorization/auth-code-flow#request-authorization for endpoint documentation.
            var skyApiAuthUrl = $"https://oauth2.sky.blackbaud.com/authorization?client_id={Configuration.ApplicationId}&response_type=code&redirect_uri={Configuration.ApplicationCallbackUri}";

            return Redirect(skyApiAuthUrl);
        }

        /// <summary>
        /// Endpoint to simulate logging the user out of this application.
        /// </summary>
        [Route("skyapi/logout")]
        public ActionResult LogoutSkyApi()
        {
            SkyApiAuthenticator.Logout();

            return Redirect("/login");
        }

        /// <summary>
        /// Endpoint to simulate refreshing the Sky API access token.
        /// </summary>
        [Route("skyapi/refresh")]
        public async Task<ActionResult> RefreshSkyApi()
        {
            await SkyApiAuthenticator.RefreshTokens();

            return Redirect("/login");
        }

        /// <summary>
        /// Validate the current application state so we can present any potential problems to the user of this application.
        /// </summary>
        private async Task FindPotentialApplicationErrors(LoginViewModel loginViewModel)
        {
            // Check that a user is currently logged in.
            if (!loginViewModel.IsLoggedIn)
            {
                loginViewModel.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Organization user not logged in"
                });
            }

            // Any additional validation requires the previous validation to pass, so if the previous validation did not pass don't attempt to do any more validation.
            if (loginViewModel.ErrorsViewModel.Errors.Count > 0)
            {
                return;
            }

            // If the user navigates to the login page more than once, don't spam Payment API's endpoint.  Any change to the Payment API request that we are testing here
            // would require the Sky API authorization to change.
            if (string.IsNullOrWhiteSpace(PaymentsApiRequestAccessToken) || PaymentsApiRequestAccessToken != SkyApiAuthenticator.AccessToken)
            {
                PaymentsApiRequestResult = await TestPaymentsApiAuthorization();
                PaymentsApiRequestAccessToken = SkyApiAuthenticator.AccessToken;
            }

            if (PaymentsApiRequestResult != null)
            {
                loginViewModel.ErrorsViewModel.Errors.Add(PaymentsApiRequestResult);
            }
        }

        /// <summary>
        /// Validate that the authenticated organization user can make a request to Payments API.
        /// </summary>
        private async Task<ErrorViewModel> TestPaymentsApiAuthorization()
        {
            var response = await PaymentsApiClient.GetPublicKey();

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return new ErrorViewModel
            {
                Error = "Public key request error",
                Description = "The attempt to test Payments API did not succeed and has produced an unexpected error."
            };
        }
    }
}