namespace CheckoutAuthCodeGrant.Controllers
{
    using Models;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    /// <summary>
    /// A controller to handle the home page for this application.
    /// </summary>
    /// <remarks>
    /// The home page is intended to contain general information about the functionality specific to this application.
    /// </remarks>
    public class HomeController : Controller
    {
        // Cache Sky API authorization test result.
        private static ErrorViewModel SkyApiAuthResult;
        private static bool SkyApiAuthTested = false;

        /// <summary>
        /// Endpoint to support the home page.
        /// </summary>
        [Route("~/")]
        public async Task<ActionResult> Index()
        {
            var model = new HomeViewModel();
            await FindPotentialApplicationErrors(model);
            return View(model);
        }

        /// <summary>
        /// Validate the current application state so we can present any potential problems to the user of this application.
        /// </summary>
        private async Task FindPotentialApplicationErrors(HomeViewModel homeViewModel)
        {
            // Check that the subscription key has been set.
            if (string.IsNullOrWhiteSpace(Configuration.SubscriptionKey))
            {
                homeViewModel.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Subscription key not configured",
                    Description = "The subscription key represents Blackbaud's permission to you to use the SKY API functionality.  " +
                                  "Without a subscription key this sample application is unauthorized to use Sky API functionality. " +
                                  "You can find subscription key at <a href='https://developer.blackbaud.com/subscriptions/'>SKY API developer account</a> page."
                });
            }

            // Check that this application's Sky API application ID has been set.
            if (string.IsNullOrWhiteSpace(homeViewModel.ApplicationId))
            {
                homeViewModel.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Application ID not configured",
                    Description = "The application ID is used by Sky API to validate the calling application when making requests to Sky API.  " +
                                  "Without an application ID this sample application will not be able to authenticate a user with Sky API or use Sky API functionality.  " +
                                  "You can find application ID at <a href=' https://developer.blackbaud.com/apps/'>SKY API application</a> page."

                });
            }

            // Check that this application's Sky API secret token has been set.
            if (!homeViewModel.ApplicationSecretSet)
            {
                homeViewModel.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Application secret not configured",
                    Description = "The application secret is used by Sky API to validate the calling application when making requests to Sky API.  " +
                                  "Without an application secret this sample application will not be able to authenticate a user with Sky API nor use Sky API functionality.  " +
                                  "You can find application secret at <a href=' https://developer.blackbaud.com/apps/'>SKY API application</a> page."
                });
            }

            // Check that this application's Sky API callback URI has been set.
            if (string.IsNullOrWhiteSpace(homeViewModel.ApplicationCallbackUri))
            {
                homeViewModel.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Application callback URI not configured",
                    Description = "The application callback URI is used by Sky API to return the organization user to an application after the user logs in through Sky API.  " +
                                  "Without an application callback URI the organization user will not be able to authenticate with Sky API."
                });
            }

            // Any additional validation requires the previous validation to pass, so if the previous validation did not pass don't attempt to do any more validation.
            if (homeViewModel.ErrorsViewModel.Errors.Count > 0)
            {
                return;
            }

            // If the user navigates to the home page more than once, don't spam Sky API's authorization endpoint.  Any change to the Sky API authorization result
            // would require this application's configuration to change (the application would need to be restarted).
            if (!SkyApiAuthTested)
            {
                SkyApiAuthResult = await TestSkyApiAuthorization();
                SkyApiAuthTested = true;
            }

            if (SkyApiAuthResult != null)
            {
                homeViewModel.ErrorsViewModel.Errors.Add(SkyApiAuthResult);
            }
        }

        /// <summary>
        /// Validate that the application can make a request to authorize an organization user with Sky API.
        /// </summary>
        private async Task<ErrorViewModel> TestSkyApiAuthorization()
        {
            var response = await SkyApiAuthenticator.RequestSkyApiAuthorization();

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            var authContent = await response.Content.ReadAsStringAsync();
            var authErrorMessage = "";

            // The Sky API authorization endpoint returns a web page where a Blackbaud application user would provide authorization for this
            // application to access the Blackbaud application user's data.  We manually parse the HTML returned from the Sky API authorization endpoint
            // and look for any error messages that would denote configuration issues with this application.
            var errorDetailsIndex = authContent.IndexOf("bbapi-error-developer-details-message");

            if (errorDetailsIndex > -1)
            {
                authErrorMessage = authContent.Substring(authContent.IndexOf(">", errorDetailsIndex) + 1, 50).ToLower().Trim();
            }

            if (authErrorMessage.StartsWith("invalid client_id"))
            {
                return new ErrorViewModel
                {
                    Error = "Application ID invalid",
                    Description = "Sky API has indicated that the application ID is not valid for authorization.  " +
                                  "Ensure that the configured application ID is correct."
                };
            }

            if (authErrorMessage.StartsWith("invalid redirect_uri"))
            {
                return new ErrorViewModel
                {
                    Error = "Application callback URI invalid",
                    Description = "Sky API has indicated that the application callback URI is not valid for authorization.  " +
                                  "Ensure that the configured application callback URI is correct."
                };
            }

            return new ErrorViewModel
            {
                Error = "Sky API authorization request error",
                Description = "The attempt to test authorization against Sky API did not succeed and has produced an unexpected error."
            };
        }
    }
}