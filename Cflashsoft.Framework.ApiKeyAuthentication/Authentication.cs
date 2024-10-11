using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cflashsoft.Framework.ApiKeyAuthentication
{
    /// <summary>
    /// Options to configure API key authentication.
    /// </summary>
    public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Optional, used by the built-in API key authentication implementation to validate an API key against (default: null). See AddApiKeyAuthentication extension methods on IServiceCollection.
        /// </summary>
        public IEnumerable<AppApiKeyModel> AppApiKeys { get; set; }
        /// <summary>
        /// If true, looks for the API key in the Authorization header with a matching scheme name (default: true).
        /// </summary>
        public bool UseAuthorizationHeader { get; set; } = true;
        /// <summary>
        /// If supplied, looks for the API key in a header with the supplied name (default: "X-API-KEY").
        /// </summary>
        public string HeaderKeyName { get; set; } = "X-API-KEY";
        /// <summary>
        /// If supplied, looks for the API key in a query string variable with the supplied name (default: "api_key").
        /// </summary>
        public string QueryStringKeyName { get; set; } = "api_key";
        /// <summary>
        /// If supplied, looks for the API key in a request cookie with the supplied name (default: null).
        /// </summary>
        public string CookieKeyName { get; set; }
    }

    /// <summary>
    /// An opinionated abstraction for implementing <see cref="IAuthenticationHandler"/>.
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
    {
        private IApiKeyAuthenticationService _authenticationService = null;

        /// <summary>
        /// Initializes a new instance of the ApiKeyAuthenticationHandler class.
        /// </summary>
        /// <param name="options">The monitor for the options instance.</param>
        /// <param name="logger">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="encoder">The <see cref="System.Text.Encodings.Web.UrlEncoder"/>.</param>
        /// <param name="authenticationService">The <see cref="IApiKeyAuthenticationService"/>.</param>
        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IApiKeyAuthenticationService authenticationService)
            : base(options, logger, encoder, clock)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Authenticate the API key provided.
        /// </summary>
        /// <returns>An AuthenticationResult Success/Fail ticket.</returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string apiKey = null;
            ClientApiKeySource apiKeySource = ClientApiKeySource.None;

            //find apikey in authorization header
            if (this.Options.UseAuthorizationHeader && AuthenticationHeaderValue.TryParse(this.Request.Headers["Authorization"], out AuthenticationHeaderValue authenticationHeaderValue) && authenticationHeaderValue.Scheme.Equals(this.Scheme.Name, StringComparison.OrdinalIgnoreCase))
            {
                apiKey = authenticationHeaderValue.Parameter;
                apiKeySource = ClientApiKeySource.AuthorizationHeader;
            }

            //if api key is still blank, find apikey in a custom cookie
            if (string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(this.Options.CookieKeyName))
            {
                apiKey = this.Request.Cookies[this.Options.CookieKeyName];
                apiKeySource = ClientApiKeySource.Cookie;
            }

            //if api key is still blank, find apikey in a custom header
            if (string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(this.Options.HeaderKeyName))
            {
                apiKey = this.Request.Headers[this.Options.HeaderKeyName];
                apiKeySource = ClientApiKeySource.Header;
            }

            //if api key is still blank, find apikey in the querystring
            if (string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(this.Options.QueryStringKeyName))
            {
                apiKey = this.Request.Query[this.Options.QueryStringKeyName];
                apiKeySource = ClientApiKeySource.QueryString;
            }

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var principal = await _authenticationService.AuthenticateAsync(apiKey, apiKeySource, this.Scheme.Name, this.Options);

                if (principal != null)
                    return AuthenticateResult.Success(new AuthenticationTicket(principal, this.Scheme.Name));
                else
                    return AuthenticateResult.Fail(new UnauthorizedAccessException($"Invalid API key: {apiKey}"));
            }

            return AuthenticateResult.NoResult();
        }
    }

    /// <summary>
    /// Represents an API key model.
    /// </summary>
    public class AppApiKeyModel
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Roles
        /// </summary>
        public IEnumerable<string> Roles { get; set; }
    }

    /// <summary>
    /// Describes where an API key was found in the request.
    /// </summary>
    public enum ClientApiKeySource
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// AuthorizationHeader
        /// </summary>
        AuthorizationHeader = 1,
        /// <summary>
        /// Header
        /// </summary>
        Header = 2,
        /// <summary>
        /// QueryString
        /// </summary>
        QueryString = 3,
        /// <summary>
        /// Cookie
        /// </summary>
        Cookie = 4,
    }

    /// <summary>
    /// Represents a service that can authentication an API key.
    /// </summary>
    public interface IApiKeyAuthenticationService
    {
        /// <summary>
        /// Handler to authenticate an API key.
        /// </summary>
        /// <param name="apiKey">The API key provided.</param>
        /// <param name="apiKeySource">The source of the API key.</param>
        /// <param name="scheme">The authentication scheme used.</param>
        /// <param name="options">Configured options.</param>
        /// <returns>A ClaimsPrincipal object that represents the authenticated user or null if the authentication failed.</returns>
        Task<ClaimsPrincipal> AuthenticateAsync(string apiKey, ClientApiKeySource apiKeySource, string scheme, ApiKeyAuthenticationSchemeOptions options);
    }

    /// <summary>
    /// Represents a service that can retrieve the applications API keys from a store.
    /// </summary>
    public interface IAppApiKeysProvider
    {
        /// <summary>
        /// Retrieve a single app API key info model.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="options">Configured options.</param>
        /// <returns>An AppApiKeyModel.</returns>
        Task<AppApiKeyModel> GetAppApiKeyInfoAsync(string apiKey, ApiKeyAuthenticationSchemeOptions options);
    }

    internal class DefaultApiKeyAuthenticationService : IApiKeyAuthenticationService
    {
        private IAppApiKeysProvider _appApiKeysProvider = null;

        public DefaultApiKeyAuthenticationService(IAppApiKeysProvider appApiKeysProvider = null)
        {
            _appApiKeysProvider = appApiKeysProvider;
        }

        public async Task<ClaimsPrincipal> AuthenticateAsync(string apiKey, ClientApiKeySource apiKeySource, string scheme, ApiKeyAuthenticationSchemeOptions options)
        {
            ClaimsPrincipal result = null;

            var appApiKeyInfo = await _appApiKeysProvider.GetAppApiKeyInfoAsync(apiKey, options);

            if (appApiKeyInfo != null)
            {
                var claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.Name, appApiKeyInfo.Name));
                claims.Add(new Claim(ClaimTypes.AuthenticationMethod, apiKeySource.ToString()));

                if (appApiKeyInfo.Roles != null)
                    foreach (var role in appApiKeyInfo.Roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));

                result = new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
            }

            return result;
        }
    }

    internal class DefaultAppApiKeysProvider : IAppApiKeysProvider
    {
        private ApiKeyAuthenticationSchemeOptions _configOptions = null;

        public DefaultAppApiKeysProvider(IConfiguration configuration)
        {
            var apiKeyConfigSection = configuration.GetSection("Authentication").GetSection("ApiKeyAuthentication");

            if (apiKeyConfigSection.Exists())
            {
                _configOptions = new ApiKeyAuthenticationSchemeOptions();

                _configOptions.UseAuthorizationHeader = apiKeyConfigSection.GetValue<bool>("UseAuthorizationHeader");
                _configOptions.HeaderKeyName = apiKeyConfigSection["HeaderKeyName"];
                _configOptions.QueryStringKeyName = apiKeyConfigSection["QueryStringKeyName"];
                _configOptions.CookieKeyName = apiKeyConfigSection["CookieKeyName"];

                var appApiKeys = new List<AppApiKeyModel>();

                foreach (var apiKeyInfoConfig in apiKeyConfigSection.GetSection("ApiKeys").GetChildren())
                    appApiKeys.Add(new AppApiKeyModel
                    {
                        Name = apiKeyInfoConfig["Name"],
                        Key = apiKeyInfoConfig["Key"],
                        //Roles = apiKeyInfoConfig.GetSection("Roles").Get<string[]>()
                    });

                if (appApiKeys.Count > 0)
                    _configOptions.AppApiKeys = appApiKeys;
            }
        }

        public Task<AppApiKeyModel> GetAppApiKeyInfoAsync(string apiKey, ApiKeyAuthenticationSchemeOptions options)
        {
            IEnumerable<AppApiKeyModel> appApiKeys = null;

            if (_configOptions != null && _configOptions.AppApiKeys != null)
                appApiKeys = _configOptions.AppApiKeys;
            else if (options != null && options.AppApiKeys != null)
                appApiKeys = options.AppApiKeys;

            if (appApiKeys != null)
                return Task.FromResult(appApiKeys.FirstOrDefault(k => k.Key == apiKey));
            else
                return Task.FromResult((AppApiKeyModel)null);
        }
    }

    /// <summary>
    /// Extension methods for API Key authentication.
    /// </summary>
    public static class ApiKeyAuthenticationExtensions
    {
        /// <summary>
        /// Add API key authentication that will use IConfiguration/ApiKeyAuthentication/ApiKey for the source of the API key to compare against.
        /// </summary>
        public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
        {
            return AddApiKeyAuthentication(services, options => { });
        }

        /// <summary>
        /// Add API key authentication with configured options such as the API key to compare against.
        /// </summary>
        public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services, Action<ApiKeyAuthenticationSchemeOptions> configureOptions)
        {
            return AddApiKeyAuthentication<DefaultAppApiKeysProvider>(services, configureOptions);
        }

        /// <summary>
        /// Add API key authentication that will use a custom service to retrieve API keys such as from a database.
        /// </summary>
        public static IServiceCollection AddApiKeyAuthentication<TAppApiKeysProvider>(this IServiceCollection services) where TAppApiKeysProvider : class, IAppApiKeysProvider
        {
            return AddApiKeyAuthentication<TAppApiKeysProvider>(services, options => { });
        }

        /// <summary>
        /// Add API key authentication that will use a custom service to retrieve API keys such as from a database.
        /// </summary>
        public static IServiceCollection AddApiKeyAuthentication<TAppApiKeysProvider>(this IServiceCollection services, Action<ApiKeyAuthenticationSchemeOptions> configureOptions) where TAppApiKeysProvider : class, IAppApiKeysProvider
        {
            services = AddCustomApiKeyAuthentication<DefaultApiKeyAuthenticationService>(services, "ApiKey", configureOptions);

            services.AddSingleton(typeof(IAppApiKeysProvider), typeof(TAppApiKeysProvider));

            return services;
        }

        /// <summary>
        /// Add API key authentication that will use a custom IApiKeyAuthenticationService implementation.
        /// </summary>
        private static IServiceCollection AddCustomApiKeyAuthentication<TAuthenticationService>(this IServiceCollection services) where TAuthenticationService : class, IApiKeyAuthenticationService
        {
            return AddCustomApiKeyAuthentication<TAuthenticationService>(services, options => { });
        }

        /// <summary>
        /// Add API key authentication that will use a custom IApiKeyAuthenticationService implementation.
        /// </summary>
        private static IServiceCollection AddCustomApiKeyAuthentication<TAuthenticationService>(this IServiceCollection services, Action<ApiKeyAuthenticationSchemeOptions> configureOptions) where TAuthenticationService : class, IApiKeyAuthenticationService
        {
            return AddCustomApiKeyAuthentication<TAuthenticationService>(services, "ApiKey", configureOptions);
        }

        /// <summary>
        /// Add API key authentication that will use a custom IApiKeyAuthenticationService implementation.
        /// </summary>
        private static IServiceCollection AddCustomApiKeyAuthentication<TAuthenticationService>(this IServiceCollection services, string scheme, Action<ApiKeyAuthenticationSchemeOptions> configureOptions) where TAuthenticationService : class, IApiKeyAuthenticationService
        {
            services.AddAuthentication(scheme)
                .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(scheme, configureOptions);

            services.AddSingleton(typeof(IApiKeyAuthenticationService), typeof(TAuthenticationService));

            return services;
        }
    }
}
