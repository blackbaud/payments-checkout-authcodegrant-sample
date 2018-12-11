// View model for client side interaction with the checkout page.
function CheckoutViewModel(data) {
    var self = this;

    // Observe any Checkout states so we may display them to the user.
    self.checkoutError = ko.observable();
    self.checkoutFailure = ko.observable(false);
    self.checkoutSuccess = ko.observable(false);

    // Observe if Checkout has been invoked so we can display a success or error state appropriately.
    self.checkoutLaunched = ko.observable(false);

    // Observe the selected amount, payment configuration, and any page model data we need for Checkout.
    self.amount = ko.observable();
    self.paymentConfigurationId = ko.observable();
    self.publicKey = ko.observable(data.PublicKey);

    // Create properties to store values returned from Checkout.
    self.checkoutToken = '';

    // Launch the Checkout form.
    self.launchCheckout = function () {
        // Clear any display state for previous launches.
        self.checkoutSuccess(false);
        self.checkoutFailure(false);

        self.checkoutToken = '';

        // See https://developer.blackbaud.com/skyapi/beta/payments/checkout/supported-transactions#card-not-present-payment for Blackbaud_OpenPaymentForm documentation.
        var data = {
            'amount': self.amount(),
            'client_app_name': 'CheckoutAuthCodeGrant Sample Application',
            'key': self.publicKey(),
            'merchant_account_id': self.paymentConfigurationId()
        };

        Blackbaud_OpenPaymentForm(data);
    };

    // Charge the Checkout transaction.
    self.chargeDonation = function (transactionToken) {
        var amount = self.amount();

        var chargeTransactionUrl = "/paymentsapi/chargetransaction?amount=" + amount + "&token=" + transactionToken;

        $.ajax({
            url: chargeTransactionUrl,
            async: true,
            type: 'POST',
            error: function (xhr, status, error) {
                self.checkoutError(xhr.responseJSON.Error);
                self.checkoutFailure(true);
            },
            success: function () {
                self.checkoutSuccess(true);
            }
        });
    }

    // When checkout completes, finialize the donation by charging the transaction.
    // See https://developer.blackbaud.com/skyapi/beta/payments/checkout/integration-guide/standard-workflow#events for checkoutComplete documentation.
    document.addEventListener('checkoutComplete', function (event) {
        self.chargeDonation(event.detail.transactionToken);
    });

    // When checkout returns an error display it to the user.
    // See https://developer.blackbaud.com/skyapi/beta/payments/checkout/integration-guide/standard-workflow#error-handling for checkoutError documentation.
    document.addEventListener('checkoutError', function (event) {
        self.checkoutError(event.detail.errorText);
        self.checkoutFailure(true);
    });

    return self;
}