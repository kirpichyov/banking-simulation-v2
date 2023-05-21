### Summary

This is a 'joy' service to play with in perspective of the integration and handling of the webhooks.

---

### Start with docker-compose
1. Run CLI from root directory (where Dockerfile located).
2. Run `docker-compose up -d`.
3. Wait for containers to build and run. API will be ready when api container is running.

#### Note:
If you want to use own postgres container just replace the connection string in the `.env` file.

---

### Task for practising

Create your own API that will act like the integrator of the `banking-simulations` API.
Suggested features to implement in your API:

1. Implement Auth (e.x. JWT sign-in, register, refresh token).
2. Add CRUD for user cards/bank accounts.
3. Add endpoints for payments management:
   * Initiate payment (this endpoint internally calls the `POST /api/v1/payments` in `banking-simulations`). Your service should create the payment/transaction in the DB with appropriate status (e.x. Created) and assign the id or external id property to returned from banking.
   * View user payments (e.x. id, date of creation and update, amount, payment methods, bank fee, if credit applied, comment, etc). Add filtering, sorting, pagination.
4. Register your webhooks in banking service and implement endpoint in your API to handle it and update the payment/transaction in your service.
   * Ensure that your handling endpoint verifies the webhook secret (in the `x-webhook-secret` header).
   * To retrieve the additional information you could call the `GET /api/v1/payments?paymentId={id}` in the banking while handling webhook.
   * When `PaymentInitiated` webhook is received: update your entity status.
   * When `CreditRequestInitiated` webhook is received: update your entity status and set `RequestedCreditPercent` and `RequestedCreditPricePerMonth` properties which payment in banking has.
   * When `CreditRequestFailed` webhook is received: update your entity status and set `FailReason` which payment in banking has.
   * When `PaymentFailed` webhook is received: update your entity status and set `FailReason` which payment in banking has.
   * When `PaymentCompleted` webhook is received: update your entity status and set `BankFee` which payment in banking has.
5. Add payment statistics page (e.x. counts/percentage for total payments, failed payments, success payments, approved credits, average/max/min amount, etc). 
6. Cover your API with Unit and Integration tests.

---

### Contract explanation

`POST /api/v1/payments`

```csharp
{
  // should only contain cardNumber or bankAccountNumber at once
  "source": {
    "cardNumber": "1234-1234-1234-1234", // any string, max 64 chars
    "bankAccountNumber": "ABWEQWE12301320" // any string, max 64 chars
  },
  // should only contain cardNumber or bankAccountNumber at once
  "destination": {
    "cardNumber": "4321-4321-4321-4321", // any string, max 64 chars
    "bankAccountNumber": "XSXADE12301320" // any string, max 64 chars
  },
  "creditAllowance": {
    "isAllowed": true, // if set 'true' then credit line will be requested
    "maxPercent": 0, // decimal value from '0' (0%) to '1' (100%)
    "maxPricePerMonth": 0 // decimal value from '0' to decimal max value
  },
  "amount": 0, // decimal value greater than '0'
  "comment": "string" // optional
}
```

---

### Suggested libs to try

- RestEase (communicate with external API): https://github.com/canton7/RestEase
- FriendlyJwt (JWT Bearer user friendly auth tools): https://github.com/kirpichyov/FriendlyJwt
- UneOf (deterministic result approach over exceptions with middleware): https://github.com/mcintyre321/OneOf
- Bogus (testing, random generation): https://github.com/bchavez/Bogus
- Fluent assertions (testing, assert): https://github.com/fluentassertions/fluentassertions
- Test containers (automatically run containers for integration testing): https://dotnet.testcontainers.org/