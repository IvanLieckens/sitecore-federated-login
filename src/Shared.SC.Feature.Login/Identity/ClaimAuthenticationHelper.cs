using System;
using System.Web;
using System.Web.Security;

using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;
using Sitecore.Security.Principal;

namespace Shared.SC.Feature.Login.Identity
{
    [CLSCompliant(false)]
    public class ClaimAuthenticationHelper : FormsAuthenticationHelper
    {
        public ClaimAuthenticationHelper(AuthenticationProvider provider)
            : base(provider)
        {
        }

        protected override User GetCurrentUser()
        {
            User user;

            // NOTE [ILs] Special exception for login to allow OWIN to work
            if (
                HttpContext.Current != null
                && !HttpContext.Current.Request.RawUrl.StartsWith("/login")
                && !(HttpContext.Current.User?.Identity is SitecoreIdentity))
            {
                HttpCookie httpCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (!string.IsNullOrEmpty(httpCookie?.Value))
                {
                    FormsAuthenticationTicket authenticationTicket = null;
                    try
                    {
                        authenticationTicket = FormsAuthentication.Decrypt(httpCookie.Value);
                    }
                    catch
                    {
                        HandleAuthenticationError();
                    }

                    user = !string.IsNullOrEmpty(authenticationTicket?.Name)
                               ? GetUser(authenticationTicket.Name, true)
                               : base.GetCurrentUser();
                }
                else
                {
                    user = base.GetCurrentUser();
                }
            }
            else
            {
                user = base.GetCurrentUser();
            }

            return user;
        }
    }
}