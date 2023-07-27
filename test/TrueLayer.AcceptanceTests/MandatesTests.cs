using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using OneOf;
using Shouldly;
using TrueLayer.Mandates.Model;
using TrueLayer.Payments.Model;
using Xunit;

namespace TrueLayer.AcceptanceTests
{
    using TrueLayer.Models;
    using ProviderUnion = OneOf<Payments.Model.Provider.UserSelected, Mandates.Model.Provider.Preselected>;
    using AccountIdentifierUnion = OneOf<
        AccountIdentifier.SortCodeAccountNumber,
        AccountIdentifier.Iban,
        AccountIdentifier.Bban,
        AccountIdentifier.Nrb>;

    public class MandatesTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public MandatesTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }

        private static CreateMandateRequest CreateTestMandateRequest(
            ProviderUnion providerSelection,
            AccountIdentifierUnion accountIdentifier,
            string currency = Currencies.GBP)
            => new(
                OneOf<Mandate.VRPCommercialMandate, Mandate.VRPSweepingMandate>.FromT1(new Mandate.VRPSweepingMandate(
                    "sweeping",
                    providerSelection,
                    new Mandates.Model.Beneficiary.ExternalAccount(
                        "external_account",
                        "My Bank Account",
                        accountIdentifier))),
                currency,
                new Constraints(
                    MaximumIndividualAmount: 100,
                    new PeriodicLimits(Week: new Limit(1000, PeriodAlignment.Calendar)),
                    ValidFrom: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    ValidTo: DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                new PaymentUserRequest(
                    id: "f9b48c9d-176b-46dd-b2da-fe1a2b77350c",
                    name: "Remi Terr",
                    email: "remi.terr@example.com",
                    phone: "+44777777777"));

        private static IEnumerable<object[]> CreateTestMandateRequests()
        {
            var sortCodeAccountNumber = new AccountIdentifier.SortCodeAccountNumber("111111", "10001000");
            yield return new object[]
            {
                CreateTestMandateRequest(new Payments.Model.Provider.UserSelected
                    {
                        Filter = new ProviderFilter {Countries = new[] {"GB"}, ReleaseChannel = "private_beta"},
                    },
                    sortCodeAccountNumber),
            };
        }

        [Theory]
        [MemberData(nameof(CreateTestMandateRequests))]
        public async Task Can_create_mandate(CreateMandateRequest mandateRequest)
        {
            // Act
            var response = await _fixture.Client.Mandates.CreateMandate(
                mandateRequest, idempotencyKey: Guid.NewGuid().ToString());

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
        }


        [Theory]
        [MemberData(nameof(CreateTestMandateRequests))]
        public async Task Can_get_mandate(CreateMandateRequest mandateRequest)
        {
            // Arrange
            var createResponse = await _fixture.Client.Mandates.CreateMandate(
                mandateRequest, idempotencyKey: Guid.NewGuid().ToString());
            var mandateId = createResponse.Data!.Id;
            // Act
            var response = await _fixture.Client.Mandates.GetMandate(mandateId, MandateType.sweeping);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        [Theory]
        [MemberData(nameof(CreateTestMandateRequests))]
        public async Task Can_list_mandate(CreateMandateRequest mandateRequest)
        {
            // Arrange
            var createResponse = await _fixture.Client.Mandates.CreateMandate(
                mandateRequest, idempotencyKey: Guid.NewGuid().ToString());
            // Act
            var response = await _fixture.Client.Mandates.ListMandates(new ListMandatesQuery(createResponse.Data!.User.Id, null, 10), MandateType.sweeping);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        [Theory]
        [MemberData(nameof(CreateTestMandateRequests),nameof(CreateTestAuthorizationRequests))]
        public async Task Can_start_authorization(CreateMandateRequest mandateRequest, CreateAuthorizationRequest authorizationRequest)
        {
            // Arrange
            var createResponse = await _fixture.Client.Mandates.CreateMandate(
                mandateRequest, idempotencyKey: Guid.NewGuid().ToString());
            var mandateId = createResponse.Data!.Id;
            // Act
            var response = await _fixture.Client.Mandates.StartAuthorizationFlow(
                mandateId, authorizationRequest, idempotencyKey: Guid.NewGuid().ToString());

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        private static AuthorizationFlow CreateTestAuthorizationRequests()
        {
            return new AuthorizationFlow(new Actions())
        }
    }
}
