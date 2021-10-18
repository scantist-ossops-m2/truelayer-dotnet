namespace TrueLayer.Payments.Model
{
    /// <summary>
    /// Filter options for providers
    /// </summary>
    public class ProviderFilter
    {
        /// <summary>
        /// Gets or sets the an array of ISO 3166-1 alpha-2 country codes used to filter providers e.g. GB.
        /// </summary>
        public string[]? Countries { get; set; }

        /// <summary>
        /// The lowest stability release stage of a provider that should be returned.
        /// See <see cref="ReleaseChannels"/>.
        /// </summary>
        /// <example>ReleaseChannels.PrivateBeta</example>
        public string? ReleaseChannel { get; set; }

        /// <summary>
        /// Gets or sets the customer segments catered to by a provider that should be returned.
        /// See <see cref="CustomerSegments"/>.
        /// </summary>
        /// <example>CustomerSegments.Business</example>
        public string? CustomerSegments { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of the specific providers that should be returned.
        /// </summary>
        public string[]? ProviderIds { get; set; }

        /// <summary>
        /// Gets or sets the filters used to exclude specific providers
        /// </summary>
        public ExcludesFilter? Excludes { get; set; }

        public class ExcludesFilter
        {
            /// <summary>
            /// Gets or sets the identifiers of the specific providers that should be excluded
            /// </summary>
            /// <value></value>
            public string[]? ProviderIds { get; set; }
        }
    }
}
