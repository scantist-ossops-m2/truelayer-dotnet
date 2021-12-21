using TrueLayer.Auth;
using TrueLayer.Payments;
using TrueLayer.MerchantAccounts;
using TrueLayer.Users;

namespace TrueLayer
{
    /// <summary>
    /// Provides access to TrueLayer APIs
    /// </summary>
    public interface ITrueLayerClient
    {
        /// <summary>
        /// Gets the Authorization API resource
        /// </summary>
        IAuthApi Auth { get; }

        /// <summary>
        /// Gets the Payments API resource
        /// </summary>
        IPaymentsApi Payments { get; }

        /// <summary>
        /// Gets the Merchant Accounts API resource
        /// </summary>
        IMerchantAccountsApi MerchantAccounts { get; }

        /// <summary>
        /// Gets the Users API resource
        /// </summary>
        IUsersApi Users { get; }
    }
}