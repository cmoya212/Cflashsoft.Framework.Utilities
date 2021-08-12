using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Cflashsoft.Framework.Types;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Cflashsoft.Framework.AspNetCore.Identity
{
    /// <summary>
    /// Support functions for Cflashsoft Framework Identity authentication. 
    /// </summary>
    public static class CfFxAppAuth
    {
        /// <summary>
        /// Set an Cflashsoft Framework Identity auth cookie.
        /// </summary>
        public static void SetCookie(IConfiguration configuration, HttpResponse response, string value, bool isPermanent)
        {
            var authConfig = configuration.GetSection("CfFxAuth");

            CookieOptions options = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Secure = authConfig.GetValue<bool>("CookieIsSecure"),
            };

            string domain = authConfig["CookieDomain"];

            if (!string.IsNullOrEmpty(domain))
                options.Domain = domain;

            if (isPermanent)
                options.Expires = DateTime.UtcNow.AddMinutes(authConfig.GetValue<int>("PermCookieMinutes"));

            response.Cookies.Append(authConfig["CookieName"], value, options);
        }

        /// <summary>
        /// Delete the Cflashsoft Framework Identity auth cookie.
        /// </summary>
        public static void DeleteCookie(IConfiguration configuration, HttpResponse response)
        {
            var authConfig = configuration.GetSection("CfFxAuth");

            response.Cookies.Delete(authConfig["CookieName"]);
        }
    }

    /// <summary>
    /// Constants for Cflashsoft Framework Identity authentication. 
    /// </summary>
    public static class CfFxAppAuthDefaults
    {
        /// <summary>
        /// The default scheme for Cflashsoft Framework Identity authentication.
        /// </summary>
        public static readonly string AuthenticationScheme = "CfFxAuth";
    }

    /// <summary>
    /// Delegate for retrieveing User information from the store and creating an authentication ticket.
    /// </summary>
    public delegate Task<AuthenticateResult> CreateAuthenticationTicketAsyncDelegate(IServiceProvider serviceProvider, IAppSystemUser systemAppUser, string authKey, string authenticationScheme);

    /// <summary>
    /// Represents Authentication Scheme options for Cflashsoft Framework Identity authentication. 
    /// </summary>
    public class CfFxAppAuthSchemeOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Delegate for retrieveing User information from the store and creating an authentication ticket.
        /// </summary>
        public CreateAuthenticationTicketAsyncDelegate CreateAuthenticationTicketAsync { get; set; }
    }

    /// <summary>
    /// Represents the default handler to process header and cookie auth for Cflashsoft Framework Identity authentication. 
    /// </summary>
    public class CfFxAppAuthHandler : AuthenticationHandler<CfFxAppAuthSchemeOptions>
    {
        private IServiceProvider _serviceProvider { get; }
        private IConfigurationSection _authConfig { get; }
        private IAppSystemUser _systemUser { get; }

        /// <summary>
        /// Initializes a new instance of the CfFxAppAuthHandler class.
        /// </summary>
        public CfFxAppAuthHandler(IServiceProvider serviceProvider, IConfiguration configuration, IOptionsMonitor<CfFxAppAuthSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAppSystemUser systemUser = null)
            : base(options, logger, encoder, clock)
        {
            _serviceProvider = serviceProvider;
            _authConfig = configuration.GetSection("CfFxAuth");
            _systemUser = systemUser;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            var returnUrl = properties.RedirectUri;
            //if (string.IsNullOrEmpty(returnUrl))
            //{
            //    returnUrl = OriginalPathBase + OriginalPath + Request.QueryString;
            //}
            //var accessDeniedUri = Options.AccessDeniedPath + QueryString.Create(Options.ReturnUrlParameter, returnUrl);
            //var redirectContext = new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options, properties, BuildRedirectUri(accessDeniedUri));
            //await Events.RedirectToAccessDenied(redirectContext);

            this.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            //var redirectUri = properties.RedirectUri;
            //if (string.IsNullOrEmpty(redirectUri))
            //{
            //    redirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
            //}

            //var loginUri = Options.LoginPath + QueryString.Create(Options.ReturnUrlParameter, redirectUri);
            //var redirectContext = new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options, properties, BuildRedirectUri(loginUri));
            //await Events.RedirectToLogin(redirectContext);

            string loginUrl = _authConfig["LoginUrl"];

            if (!string.IsNullOrEmpty(loginUrl))
            {
                string redirectUri = properties.RedirectUri;

                if (string.IsNullOrEmpty(redirectUri))
                    redirectUri = $"{OriginalPathBase}{OriginalPath}{Request.QueryString}";
                    
                this.Response.Redirect($"{OriginalPathBase}{loginUrl}{QueryString.Create("ReturnUrl", redirectUri)}");
            }
            else
            {
                this.Response.StatusCode = 401;
            }

            return Task.CompletedTask;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticateResult result = null;

            //TODO: see about caching these config lookups

            string authKey = null;

            if (_authConfig.GetValue<bool>("EnableCookieAuth"))
                authKey = this.Request.Cookies[_authConfig["CookieName"]];

            if (string.IsNullOrWhiteSpace(authKey) && _authConfig.GetValue<bool>("EnableAltAuth"))
            {
                string[] apiKeyNames = _authConfig["AltAuthKeyNames"].Split(',');

                if (apiKeyNames != null && apiKeyNames.Length > 0)
                {
                    foreach (string keyName in apiKeyNames)
                    {
                        string param = this.Request.Query[keyName];

                        if (!string.IsNullOrEmpty(param))
                        {
                            authKey = param;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(authKey))
                    {
                        foreach (string keyName in apiKeyNames)
                        {
                            string header = this.Request.Headers[keyName];

                            if (!string.IsNullOrEmpty(header))
                            {
                                authKey = header;
                                break;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(authKey) && _authConfig.GetValue<bool>("EnableHeaderAuth"))
            {
                string authHeader = this.Request.Headers["Authorization"];

                if (!string.IsNullOrEmpty(authHeader))
                {
                    AuthenticationHeaderValue authHeaderValue = null;

                    try { authHeaderValue = AuthenticationHeaderValue.Parse(authHeader); }
                    catch { }

                    if (authHeaderValue != null && authHeaderValue.Scheme != null && authHeaderValue.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(authHeaderValue.Parameter))
                        authKey = authHeaderValue.Parameter;
                }
            }

            if (!string.IsNullOrWhiteSpace(authKey))
            {
                result = await this.Options.CreateAuthenticationTicketAsync(_serviceProvider, _systemUser, authKey, this.Scheme.Name);
            }

            return result ?? AuthenticateResult.NoResult();
        }
    }

    /// <summary>
    /// Cflashsoft Framework Identity authentication extensions. 
    /// </summary>
    public static class CfFxAppAuthExtensions
    {
        /// <summary>
        /// Add Cflashsoft Framework Identity authentication to your application.  
        /// </summary>
        public static AuthenticationBuilder AddCfFxAppAuth(this AuthenticationBuilder builder, Action<CfFxAppAuthSchemeOptions> configureOptions)
        {
            return builder.AddScheme<CfFxAppAuthSchemeOptions, CfFxAppAuthHandler>(CfFxAppAuthDefaults.AuthenticationScheme, configureOptions);
        }
    }
}
