namespace CheckoutAuthCodeGrant.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// View model for the checkout page.
    /// </summary>
    public class CheckoutViewModel
    {
        public CheckoutViewModel()
        {
            ErrorsViewModel = new ErrorsViewModel();
            PaymentConfigurations = new SelectList(Enumerable.Empty<SelectListItem>());
        }

        public ErrorsViewModel ErrorsViewModel { get; }

        public SelectList PaymentConfigurations { get; set; }

        [Display(Name = "Public key")]
        public string PublicKey { get; set; }

        [Display(Name = "Payment configuration")]
        public string SelectedPaymentConfigurationId { get; set; }
    }
}