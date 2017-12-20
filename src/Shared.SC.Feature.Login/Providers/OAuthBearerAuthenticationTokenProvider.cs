using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;

using Shared.SC.Feature.Login.Configuration;
using Shared.SC.Feature.Login.Data;
using Shared.SC.Feature.Login.Identity;
using Shared.SC.Feature.Login.Identity.Interfaces;

namespace Shared.SC.Feature.Login.Providers
{
    [CLSCompliant(false)]
    public class OAuthBearerAuthenticationTokenProvider : AuthenticationTokenProvider
    {
        private readonly IAuthenticationSessionStore _authSessionStore;

        private readonly ICookieManager _cookieManager;

        private readonly IIdentityHelper _identityHelper;

        public OAuthBearerAuthenticationTokenProvider()
        {
            OnReceive = OAuthAuthenticationTokenProviderOnReceive;
            _authSessionStore = new SqlAuthSessionStore(IdentityHelper.TicketDataFormat);
            _cookieManager = new SystemWebChunkingCookieManager();
            _identityHelper = new IdentityHelper(_authSessionStore, null);
        }

        internal OAuthBearerAuthenticationTokenProvider(IAuthenticationSessionStore authSessionStore, ICookieManager cookieManager)
        {
            OnReceive = OAuthAuthenticationTokenProviderOnReceive;
            _authSessionStore = authSessionStore;
            _cookieManager = cookieManager;
        }

        private static AuthenticationTicket BuildCookiesAuthenticationTicket(string sessionId)
        {
            return new AuthenticationTicket(
                new ClaimsIdentity(
                    new[] { new Claim(IdentityHelper.AuthenticationKeyClaimType, sessionId) },
                    CookieAuthenticationDefaults.AuthenticationType),
                null);
        }

        private static AuthenticationTicket BuildOAuthAuthenticationTicket(ClaimsIdentity identity)
        {
            AuthenticationTicket result = new AuthenticationTicket(
                identity,
                new AuthenticationProperties(
                    new Dictionary<string, string> { { "AuthenticationScheme", OAuthDefaults.AuthenticationType } }));

            int expiresSecondsOffset;
            string expiresIn = identity.Claims
                .FirstOrDefault(c => c.Type == OAuthBearerAuthenticationProvider.OAuthExpiresInClaimType)?.Value;
            if (!int.TryParse(expiresIn, out expiresSecondsOffset))
            {
                expiresSecondsOffset = 3600;
            }

            DateTime utcNow = DateTime.UtcNow;
            result.Properties.IssuedUtc = new DateTimeOffset(utcNow);
            result.Properties.ExpiresUtc = new DateTimeOffset(utcNow.AddSeconds(expiresSecondsOffset));

            return result;
        }

        private void OAuthAuthenticationTokenProviderOnReceive(AuthenticationTokenReceiveContext context)
        {
            AuthenticationTicket result = null;
            ClaimsIdentity identity = _identityHelper.GetCurrentClaimsPrincipal()?.Identity as ClaimsIdentity;
            if (identity?.IsAuthenticated ?? false)
            {
                result = BuildOAuthAuthenticationTicket(identity);
            }
            else
            {
                identity = context.OwinContext.Get<ClaimsIdentity>(OAuthAuthentication.OAuthOwinContextKey);
                if (identity != null)
                {
                    result = BuildOAuthAuthenticationTicket(identity);
                    string sessionId = _authSessionStore.StoreAsync(result).Result;
                    _cookieManager.AppendResponseCookie(
                        context.OwinContext,
                        IdentityHelper.AuthenticationCookieKey,
                        IdentityHelper.TicketDataFormat.Protect(BuildCookiesAuthenticationTicket(sessionId)),
                        new CookieOptions { HttpOnly = true, Secure = true });
                }
            }

            context.SetTicket(result);
        }
    }
}