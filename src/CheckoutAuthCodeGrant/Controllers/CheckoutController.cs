namespace CheckoutAuthCodeGrant.Controllers
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    /// <summary>
    /// A controller to handle the checkout page for this application.
    /// </summary>
    /// <remarks>
    /// The checkout page is intended to be an example of a page that would be provided to a donor of an organization.
    /// The donor would visit the checkout page in order to submit a donation to the organization using the Payments API checkout functionality.
    /// </remarks>
    public class CheckoutController : Controller
    {
        /// <summary>
        /// Charges a Checkout transaction.
        /// </summary>
        [Route("paymentsapi/chargetransaction")]
        public async Task<ActionResult> CheckoutPaymentsApi(decimal amount, string token)
        {
            var response = await PaymentsApiClient.ChargeCheckoutTransaction(amount, token);
            var content = await response.Content.ReadAsStringAsync();

            // Return any error to display to the user.
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = TryGetErrorMessage(content);

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "The attempt to retrieve payment configurations from Payments API did not succeed and has produced an unexpected error.";
                }

                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return new JsonResult()
                {
                    Data = new
                    {
                        Error = errorMessage
                    }
                };
            }

            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Endpoint to support the checkout page.
        /// </summary>
        [Route("checkout")]
        public async Task<ActionResult> Index()
        {
            var model = new CheckoutViewModel();
            await SetPublicKey(model);
            await SetPaymentConfigurations(model);
            return View(model);
        }

        /// <summary>
        /// Sets the merchant accounts on the view model or any errors that occurred.
        /// </summary>
        private async Task SetPaymentConfigurations(CheckoutViewModel model)
        {
            var response = await PaymentsApiClient.GetPaymentConfigurations();
            var content = await response.Content.ReadAsStringAsync();

            // Check that the request to get payment configurations was successful.
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = TryGetErrorMessage(content);

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "The attempt to retrieve payment configurations from Payments API did not succeed and has produced an unexpected error.";
                }

                model.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Payment configurations request error",
                    Description = errorMessage
                });

                return;
            }

            var paymentConfigurationsData = JsonConvert.DeserializeObject<Models.PaymentsApi.PaymentConfigurationsData>(content);

            // Check if there is at least one payment configuration available.
            if (!paymentConfigurationsData.Value.Any())
            {
                model.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Payment configurations not found",
                    Description = "There were no payment configurations returned from Payments API.  " +
                                  "Ensure that there are payment configurations available to the current organization user.  " +
                                  "For the purposes of this sample application you may not use payment configurations that are currently marked with a 'Live' process mode."
                });

                return;
            }

            var validPaymentConfigurations = paymentConfigurationsData.Value.Where(pc => pc.ProcessMode != "Live");

            // Check that at least one payment configuration is valid.
            if (!validPaymentConfigurations.Any())
            {
                model.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Payment configurations not valid ('Live' process mode)",
                    Description = "All payment configurations returned from Payments API are currently marked with a 'Live' process mode.  " +
                                  "For the purposes of this sample application you may not use payment configurations that are currently marked with a 'Live' process mode.  "
                });

                return;
            }

            model.PaymentConfigurations = new SelectList(validPaymentConfigurations, "Id", "Name");
        }

        /// <summary>
        /// Sets the public key on the view model or any errors that occurred.
        /// </summary>
        private async Task SetPublicKey(CheckoutViewModel model)
        {
            var response = await PaymentsApiClient.GetPublicKey();
            var content = await response.Content.ReadAsStringAsync();

            // Check that the request to get the public key was successful.
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = TryGetErrorMessage(content);

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "The attempt to retrieve the public key from Payments API did not succeed and has produced an unexpected error.";
                }

                model.ErrorsViewModel.Errors.Add(new ErrorViewModel
                {
                    Error = "Public key request error",
                    Description = errorMessage
                });

                return;
            }

            var publicKeyData = JsonConvert.DeserializeObject<Models.PaymentsApi.PublicKeyData>(content);

            model.PublicKey = publicKeyData.Value;
        }

        /// <summary>
        /// Attempt to retreive an error message from a Payments API response.
        /// </summary>
        private string TryGetErrorMessage(string content)
        {
            Models.PaymentsApi.ErrorData error = null;

            try
            {
                error = JsonConvert.DeserializeObject<Models.PaymentsApi.ErrorData>(content);
            }
            catch (Exception)
            {
            }

            return error?.Message;
        }
    }
}