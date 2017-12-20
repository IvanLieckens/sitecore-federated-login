using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Owin.Security.OAuth;

using Shared.SC.Feature.Login.Configuration;
using Shared.SC.Feature.Login.Extensions;
using Shared.SC.Feature.Login.Identity;
using Shared.SC.Feature.Login.Identity.Interfaces;
using Shared.SC.Feature.Login.Models;

using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Extensions;

namespace Shared.SC.Feature.Login.Providers
{
    [CLSCompliant(false)]
    public class OAuthBearerAuthenticationProvider : Microsoft.Owin.Security.OAuth.OAuthBearerAuthenticationProvider
    {
        public static readonly string OAuthAccessTokenClaimType = "Shared.SC.Feature.Login.Configuration.OAuthAuthentication.Access_Token";

        public static readonly string OAuthRefreshTokenClaimType = "Shared.SC.Feature.Login.Configuration.OAuthAuthentication.Refresh_Token";

        public static readonly string OAuthTokenTypeClaimType = "Shared.SC.Feature.Login.Configuration.OAuthAuthentication.Token_Type";

        public static readonly string OAuthExpiresInClaimType = "Shared.SC.Feature.Login.Configuration.OAuthAuthentication.Expires_In";

        private static HttpClient _httpClient = new HttpClient();

        private static bool _isHttpClientInitialized;

        private readonly object _httpClientLock = new object();

        private readonly IIdentityHelper _identityHelper;

        [SuppressMessage("SonarAnalyzer.CSharp", "S3010", Justification = "Initialization of HttpClient is tricky since it must not be disposed lightly")]
        public OAuthBearerAuthenticationProvider(OAuthSiteInfo site)
        {
            OnRequestToken = context => HandleOnRequestToken(context, site);
            OnApplyChallenge = context => HandleOnApplyChallenge(context, site);
            OnValidateIdentity = HandleOnValidateIdentity;
            _identityHelper = new IdentityHelper();

            if (!_isHttpClientInitialized)
            {
                lock (_httpClientLock)
                {
                    if (!_isHttpClientInitialized)
                    {
                        HttpClient.BaseAddress = new Uri(site.AuthorityUri.WithPostfix('/'));

                        HttpClient.DefaultRequestHeaders.Clear();
                        HttpClient.DefaultRequestHeaders.Accept.Clear();
                        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        Type thisType = GetType();
                        HttpClient.DefaultRequestHeaders.UserAgent.Clear();
                        HttpClient.DefaultRequestHeaders.UserAgent.Add(
                            new ProductInfoHeaderValue(
                                thisType.FullName,
                                thisType.Assembly.GetName().Version.ToString()));

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        _isHttpClientInitialized = true;
                    }
                }
            }
        }
        
        [SuppressMessage("SonarAnalyzer.CSharp", "S3010", Justification = "Used in unit testing")]
        internal OAuthBearerAuthenticationProvider(OAuthSiteInfo site, IIdentityHelper helper, HttpClient client)
        {
            OnRequestToken = context => HandleOnRequestToken(context, site);
            OnApplyChallenge = context => HandleOnApplyChallenge(context, site);
            OnValidateIdentity = HandleOnValidateIdentity;
            _identityHelper = helper;
            _httpClient = client;
        }

        [SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter", Justification = "Not possible due to special initialization")]
        private static HttpClient HttpClient => _httpClient;

        public RefreshTokenResult RefreshToken(OAuthSiteInfo site)
        {
            RefreshTokenResult result = null;
            ClaimsPrincipal claimsPrincipal = _identityHelper.GetCurrentClaimsPrincipal();
            string refreshToken = claimsPrincipal?.Claims?.FirstOrDefault(c => c.Type == OAuthRefreshTokenClaimType)
                ?.Value;
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                HttpResponseMessage response;
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "token"))
                {
                    request.Content = new FormUrlEncodedContent(new OAuthRefreshTokenRequest(site, refreshToken).ToDictionary());
                    response = HttpClient.SendAsync(request).Result;
                }

                if (response.IsSuccessStatusCode)
                {
                    OAuthToken token = response.Content.ReadAsAsync<OAuthToken>().Result;
                    _identityHelper.UpdateClaim(new Claim(OAuthAccessTokenClaimType, token.Access_Token));
                    _identityHelper.UpdateClaim(new Claim(OAuthExpiresInClaimType, token.Expires_In));
                    _identityHelper.UpdateClaim(new Claim(OAuthRefreshTokenClaimType, token.Refresh_Token));

                    result = new RefreshTokenResult { AccessToken = token.Access_Token, ExpiresIn = token.Expires_In };
                }
                else
                {
                    Log.Error($"OAuth Authority responded with an error code: {response.Content.ReadAsStringAsync().Result}", this);
                }
            }

            return result;
        }

        private static OAuthToken ExchangeAuthorizationCode(OAuthAuthorizationTokenRequest authorizationTokenRequest)
        {
            OAuthToken result = null;
            HttpResponseMessage response;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "token"))
            {
                request.Content = new FormUrlEncodedContent(authorizationTokenRequest.ToDictionary());
                response = HttpClient.SendAsync(request).Result;
            }

            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsAsync<OAuthToken>().Result;
            }
            else
            {
                Log.Error($"OAuth Authority responded with an error code: {response.Content.ReadAsStringAsync().Result}", typeof(OAuthBearerAuthenticationProvider));
            }

            return result;
        }

        private static Task HandleOnValidateIdentity(OAuthValidateIdentityContext context)
        {
            context.Validated();
            return Task.CompletedTask;
        }

        private static Task HandleOnApplyChallenge(
            OAuthChallengeContext context,
            OAuthSiteInfo site)
        {
            UriBuilder authorizeUri = new UriBuilder(new Uri(new Uri(site.AuthorityUri.WithPostfix('/')), "authorize"));
            NameValueCollection query =
                new NameValueCollection(5)
                    {
                        { "client_id", site.ClientId },
                        { "redirect_uri", site.RedirectUri },
                        { "response_type", "code" },
                        { "scope", site.Scope },
                        { "locale", Context.Language.CultureInfo.Name }
                    };
            authorizeUri.Query = query.ToQueryString();
            context.Response.Redirect(authorizeUri.ToString());
            return Task.CompletedTask;
        }

        private Task HandleOnRequestToken(
            OAuthRequestTokenContext context,
            OAuthSiteInfo site)
        {
            string authcode = context.Request.Query.Get("code");
            ClaimsIdentity identity = _identityHelper.GetCurrentClaimsPrincipal()?.Identity as ClaimsIdentity;
            if (identity?.IsAuthenticated ?? false)
            {
                context.Token = identity.Claims.FirstOrDefault(c => c.Type == OAuthAccessTokenClaimType)?.Value;
            }
            else if (context.Request.Path.ToString().Contains("/login") && !string.IsNullOrWhiteSpace(authcode))
            {
                OAuthToken token = ExchangeAuthorizationCode(new OAuthAuthorizationTokenRequest(site, authcode));
                context.Token = token?.Access_Token;
                context.OwinContext.Set(OAuthAuthentication.OAuthOwinContextKey, token?.ToClaimsIdentity());
            }
            else
            {
                // No method to retrieve token, either anonymous request or challenge will be thrown
            }

            return Task.CompletedTask;
        }

        public class RefreshTokenResult
        {
            public string AccessToken { get; set; }

            public string ExpiresIn { get; set; }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Matching OAuth protocol")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used during serialization")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "Must be public for deserilization")]
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Only instantiated during deserialization")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S1144", Justification = "Used during serialization")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3459", Justification = "Used during serialization")]
        private class OAuthToken
        {
            public string Access_Token { get; set; }

            public string Refresh_Token { get; set; }

            public string Token_Type { get; set; }

            public string Expires_In { get; set; }

            public string Id_Token { get; set; }

            public ClaimsIdentity ToClaimsIdentity()
            {
                return new ClaimsIdentity(
                    new[]
                        {
                            new Claim(OAuthAccessTokenClaimType, Access_Token),
                            new Claim(OAuthRefreshTokenClaimType, Refresh_Token),
                            new Claim(OAuthTokenTypeClaimType, Token_Type),
                            new Claim(OAuthExpiresInClaimType, Expires_In),
                            new Claim("Name", "TokenUser")
                        },
                    OAuthDefaults.AuthenticationType,
                    "Name",
                    string.Empty);
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Matching OAuth protocol")]
        [SuppressMessage("Managed Binary Analysis", "CA1056", Justification = "Must be passed as string, no user input")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3996", Justification = "Must be passed as string, no user input")]
        private class OAuthAuthorizationTokenRequest
        {
            public OAuthAuthorizationTokenRequest(OAuthSiteInfo site, string code)
            {
                Client_Id = site.ClientId;
                Client_Secret = site.ClientSecret;
                Redirect_Uri = site.RedirectUri;
                Grant_Type = "authorization_code";
                Code = code;
            }

            private string Client_Id { get; }

            private string Client_Secret { get; }

            private string Redirect_Uri { get; }

            private string Grant_Type { get; }

            private string Code { get; }

            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> result =
                    new Dictionary<string, string>(5)
                        {
                            { nameof(Client_Id).ToLowerInvariant(), Client_Id },
                            { nameof(Client_Secret).ToLowerInvariant(), Client_Secret },
                            { nameof(Redirect_Uri).ToLowerInvariant(), Redirect_Uri },
                            { nameof(Grant_Type).ToLowerInvariant(), Grant_Type },
                            { nameof(Code).ToLowerInvariant(), Code }
                        };

                return result;
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Matching OAuth protocol")]
        [SuppressMessage("Managed Binary Analysis", "CA1056", Justification = "Must be passed as string, no user input")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3996", Justification = "Must be passed as string, no user input")]
        private class OAuthRefreshTokenRequest
        {
            public OAuthRefreshTokenRequest(OAuthSiteInfo site, string refreshToken)
            {
                Client_Id = site.ClientId;
                Client_Secret = site.ClientSecret;
                Redirect_Uri = site.RedirectUri;
                Grant_Type = "refresh_token";
                Refresh_Token = refreshToken;
            }

            private string Client_Id { get; }

            private string Client_Secret { get; }

            private string Redirect_Uri { get; }

            private string Grant_Type { get; }

            private string Refresh_Token { get; }

            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> result =
                    new Dictionary<string, string>(5)
                        {
                            { nameof(Client_Id).ToLowerInvariant(), Client_Id },
                            { nameof(Client_Secret).ToLowerInvariant(), Client_Secret },
                            { nameof(Redirect_Uri).ToLowerInvariant(), Redirect_Uri },
                            { nameof(Grant_Type).ToLowerInvariant(), Grant_Type },
                            { nameof(Refresh_Token).ToLowerInvariant(), Refresh_Token }
                        };

                return result;
            }
        }
    }
}