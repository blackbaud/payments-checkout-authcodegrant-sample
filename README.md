# payments-checkout-authcodegrant-sample

This is a sample web application to demonstrate Checkout functionality using OAuth code grant flow with Sky API.

## Requirements
 - .NET framework 4.6.1
 - IIS Express
 - Sky API application
 - User account associated with an environment that is connected to Sky API application

## Configuration
 - Update Configuration.cs properties ApplicationId, ApplicationSecret, and SubscriptionKey to identify your application and subscription to Sky API.
 - Add the callback URI (https://localhost:44300/auth/callback) to your application in the Sky API developer portal.

---

### Learn more about Sky API
- [Getting started as a Sky API developer](https://developer.blackbaud.com/skyapi/docs/getting-started)
- [Managing a Sky API application](https://developer.blackbaud.com/skyapi/docs/createapp)

### Learn more about OAuth code grant flow
- [Sky API authorizaton code flow](https://developer.blackbaud.com/skyapi/docs/authorization/auth-code-flow)
- [OAuth authorization code grant type](https://oauth.net/2/grant-types/authorization-code/)

### Learn more about Payments API
- [Payments endpoint reference](https://developer.sky.blackbaud.com/docs/services/payments/)
- [Checkout reference](https://developer.blackbaud.com/skyapi/beta/payments/checkout)

### Getting help
- [Sky API developer community portal](https://community.blackbaud.com/developer)
- [Issues with this application](https://github.com/blackbaud/payments-checkout-authcodegrant-sample/issues)