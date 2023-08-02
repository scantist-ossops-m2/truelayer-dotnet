using System;
using System.Threading;
using System.Threading.Tasks;
using TrueLayer.Auth;
using OneOf;

namespace TrueLayer.Mandates
{
    using TrueLayer.Mandates.Model;
    using TrueLayer.Models;
    using AuthorizationResponseUnion = OneOf<Models.AuthorisationFlowResponse.AuthorizationFlowAuthorizing, Models.AuthorisationFlowResponse.AuthorizationFlowAuthorizationFailed>;
    using MandateDetailUnion = OneOf<Model.MandateDetail.AuthorizationRequiredMandateDetail, Model.MandateDetail.AuthorizingMandateDetail, Model.MandateDetail.AuthorizedMandateDetail, Model.MandateDetail.FailedMandateDetail, Model.MandateDetail.RevokedMandateDetail>;

    internal class MandatesApi : IMandatesApi
    {
        private const string ProdUrl = "https://api.truelayer.com/v3/mandates";
        private const string SandboxUrl = "https://api.truelayer-sandbox.com/v3/mandates";

        private readonly IApiClient _apiClient;
        private readonly TrueLayerOptions _options;
        private readonly Uri _baseUri;
        private readonly IAuthApi _auth;

        public MandatesApi(IApiClient apiClient, IAuthApi auth, TrueLayerOptions options)
        {
            _apiClient = apiClient.NotNull(nameof(apiClient));
            _options = options.NotNull(nameof(options));
            _auth = auth.NotNull(nameof(auth));

            options.Payments.NotNull(nameof(options.Payments))!.Validate();

            _baseUri = options.Payments.Uri is not null
                ? new Uri(options.Payments.Uri, "/v3/mandates")
                : new Uri((options.UseSandbox ?? true) ? SandboxUrl : ProdUrl);
        }

        /// <inheritdoc />
        public async Task<ApiResponse<CreateMandateResponse>> CreateMandate(CreateMandateRequest mandateRequest, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            mandateRequest.NotNull(nameof(mandateRequest));
            idempotencyKey.NotNullOrWhiteSpace(nameof(idempotencyKey));
            var type = mandateRequest.Mandate.Match(t0 => t0.Type, t1 => t1.Type);
            ApiResponse<GetAuthTokenResponse> authResponse = await _auth.GetAuthToken(new GetAuthTokenRequest($"recurring_payments:{type}"), cancellationToken);

            if (!authResponse.IsSuccessful)
            {
                return new(authResponse.StatusCode, authResponse.TraceId);
            }

            return await _apiClient.PostAsync<CreateMandateResponse>(
                _baseUri,
                mandateRequest,
                idempotencyKey,
                authResponse.Data!.AccessToken,
                _options.Payments!.SigningKey,
                cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<ApiResponse<MandateDetailUnion>> GetMandate(string mandateId, MandateType mandateType, CancellationToken cancellationToken = default)
        {
            mandateId.NotNullOrWhiteSpace(nameof(mandateId));

            ApiResponse<GetAuthTokenResponse> authResponse = await _auth.GetAuthToken(new GetAuthTokenRequest($"recurring_payments:{mandateType}"), cancellationToken);

            if (!authResponse.IsSuccessful)
            {
                return new(authResponse.StatusCode, authResponse.TraceId);
            }

            return await _apiClient.GetAsync<MandateDetailUnion>(
                new Uri(_baseUri, $"/v3/mandates/{mandateId}"),
                authResponse.Data!.AccessToken,
                cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<ApiResponse<ResourceCollection<MandateDetailUnion>>> ListMandates(ListMandatesQuery query, MandateType mandateType, CancellationToken cancellationToken = default)
        {
            ApiResponse<GetAuthTokenResponse> authResponse = await _auth.GetAuthToken(new GetAuthTokenRequest($"recurring_payments:{mandateType}"), cancellationToken);

            if (!authResponse.IsSuccessful)
            {
                return new(authResponse.StatusCode, authResponse.TraceId);
            }

            var queryParameters = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryParameters["client_id"] = _options.ClientId;
            queryParameters["user_id"] = query.UserId;
            queryParameters["cursor"] = query.Cursor;
            queryParameters["limit"] = query.Limit.ToString();
            var baseUri = new Uri(_baseUri, "/v3/mandates?" + queryParameters);

            return await _apiClient.GetAsync<ResourceCollection<MandateDetailUnion>>(
                baseUri,
                authResponse.Data!.AccessToken,
                cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<ApiResponse<AuthorizationResponseUnion>> StartAuthorizationFlow(string mandateId, StartAuthorizationFlowRequest request, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            mandateId.NotNull(nameof(mandateId));
            request.NotNull(nameof(request));
            idempotencyKey.NotNullOrWhiteSpace(nameof(idempotencyKey));
            ApiResponse<GetAuthTokenResponse> authResponse = await _auth.GetAuthToken(new GetAuthTokenRequest($"recurring_payments:sweeping"), cancellationToken);

            if (!authResponse.IsSuccessful)
            {
                return new(authResponse.StatusCode, authResponse.TraceId);
            }

            return await _apiClient.PostAsync<AuthorizationResponseUnion>(
                new Uri(_baseUri, $"/v3/mandates/{mandateId}/authorization-flow"),
                request,
                idempotencyKey,
                authResponse.Data!.AccessToken,
                _options.Payments!.SigningKey,
                cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<ApiResponse<AuthorizationResponseUnion>> SubmitProviderSelection(string mandateId, SubmitProviderSelectionRequest request, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            mandateId.NotNull(nameof(mandateId));
            request.NotNull(nameof(request));
            idempotencyKey.NotNullOrWhiteSpace(nameof(idempotencyKey));
            ApiResponse<GetAuthTokenResponse> authResponse = await _auth.GetAuthToken(new GetAuthTokenRequest($"recurring_payments:sweeping"), cancellationToken);

            if (!authResponse.IsSuccessful)
            {
                return new(authResponse.StatusCode, authResponse.TraceId);
            }

            return await _apiClient.PostAsync<AuthorizationResponseUnion>(
                new Uri(_baseUri, $"/v3/mandates/{mandateId}/authorization-flow/actions/provider-selection"),
                request,
                idempotencyKey,
                authResponse.Data!.AccessToken,
                _options.Payments!.SigningKey,
                cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<ApiResponse<GetConfirmationOfFundsResponse>> GetConfirmationOfFunds(string mandateId, int amountInMinor, string currency, CancellationToken cancellationToken = default)
        {
            mandateId.NotNullOrWhiteSpace(nameof(mandateId));

            ApiResponse<GetAuthTokenResponse> authResponse = await _auth.GetAuthToken(new GetAuthTokenRequest($"recurring_payments:sweeping"), cancellationToken);

            if (!authResponse.IsSuccessful)
            {
                return new(authResponse.StatusCode, authResponse.TraceId);
            }

            return await _apiClient.GetAsync<GetConfirmationOfFundsResponse>(
                new Uri(_baseUri, $"/v3/mandates/{mandateId}/funds?amount_in_minor={amountInMinor}&currency={currency}"),
                authResponse.Data!.AccessToken,
                cancellationToken
            );
        }
    }
}
