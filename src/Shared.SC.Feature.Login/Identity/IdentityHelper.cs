using System;
using System.Linq;
using System.Security.Claims;
using System.Web;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;

using Shared.SC.Feature.Login.Data;
using Shared.SC.Feature.Login.Identity.Interfaces;

namespace Shared.SC.Feature.Login.Identity
{
    /// <summary>
    /// Helper class which helps to retrieve the claimsprincipal from the .AspNet.Cookies cookie
    /// </summary>
    public class IdentityHelper : IIdentityHelper
    {
        public static readonly string AuthenticationCookieKey = ".AspNet.Cookies";

        public static readonly string AuthenticationKeyClaimType = "Microsoft.Owin.Security.Cookies-SessionId";

        public static readonly TicketDataFormat TicketDataFormat = new TicketDataFormat(new MachineKeyProtector());

        private readonly IAuthenticationSessionStore _store;

        private readonly HttpRequestBase _requestBase;

        public IdentityHelper() : this(new SqlAuthSessionStore(TicketDataFormat), null)
        {
        }

        public IdentityHelper(IAuthenticationSessionStore store, HttpRequestBase requestBase)
        {
            _store = store;
            _requestBase = requestBase;
        }

        public ClaimsPrincipal GetCurrentClaimsPrincipal()
        {
            AuthenticationTicket ticket = GetAuthenticationTicketForCurrentUser();
            ClaimsPrincipal p = null;
            if (ticket?.Identity != null)
            {
                p = new ClaimsPrincipal(ticket.Identity);
            }

            return p;
        }

        public bool AddClaim(Claim claim)
        {
            bool result = false;
            AuthenticationTicket ticket = GetAuthenticationTicketForCurrentUser();
            if (ticket?.Identity != null)
            {
                ticket.Identity.AddClaim(claim);
                SaveAuthenticationTicket(ticket);
                result = true;
            }

            return result;
        }

        public bool UpdateClaim(Claim claim)
        {
            bool result = false;
            AuthenticationTicket ticket = GetAuthenticationTicketForCurrentUser();
            Claim existingClaim = ticket?.Identity?.Claims.FirstOrDefault(c => c.Type == claim.Type);
            if (existingClaim != null)
            {
                ticket.Identity.RemoveClaim(existingClaim);
                ticket.Identity.AddClaim(claim);
                SaveAuthenticationTicket(ticket);
                result = true;
            }

            return result;
        }

        public bool RemoveClaim(Claim claim)
        {
            bool result = false;
            AuthenticationTicket ticket = GetAuthenticationTicketForCurrentUser();
            Claim existingClaim = ticket?.Identity?.Claims.FirstOrDefault(c => c.Type == claim.Type);
            if (existingClaim != null)
            {
                ticket.Identity.RemoveClaim(existingClaim);
                SaveAuthenticationTicket(ticket);
                result = true;
            }

            return result;
        }

        internal string GetAuthTokenFromCookie()
        {
            string authKey = string.Empty;
            AuthenticationTicket ticket = GetAuthenticationKeyTicket();
            if (ticket != null)
            {
                authKey = ticket.Identity.Claims.FirstOrDefault(claim => claim.Type.Equals(AuthenticationKeyClaimType))?.Value;
            }

            return authKey;
        }

        private AuthenticationTicket GetAuthenticationTicketByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(key);
            }

            return _store.RetrieveAsync(key).Result;
        }

        private AuthenticationTicket GetAuthenticationTicketForCurrentUser()
        {
            AuthenticationTicket ticket = null;
            string authKey = GetAuthTokenFromCookie();
            if (!string.IsNullOrEmpty(authKey))
            {
                ticket = GetAuthenticationTicketByKey(authKey);
            }

            return ticket;
        }

        private AuthenticationTicket GetAuthenticationKeyTicket()
        {
            AuthenticationTicket ticket = null;
            HttpCookieCollection cookies = _requestBase?.Cookies ?? HttpContext.Current.Request.Cookies;

            if (cookies[AuthenticationCookieKey] != null)
            {
                HttpCookie cookie = cookies[AuthenticationCookieKey];
                ticket = TicketDataFormat.Unprotect(cookie.Value);
            }

            return ticket;
        }

        private void SaveAuthenticationTicket(AuthenticationTicket ticket)
        {
            string key = GetAuthTokenFromCookie();
            _store.RenewAsync(key, ticket).Wait();
        }
    }
}