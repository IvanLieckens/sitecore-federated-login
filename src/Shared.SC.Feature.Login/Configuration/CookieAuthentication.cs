using System;

using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

using Owin;

using Shared.SC.Feature.Login.Data;
using Shared.SC.Feature.Login.Identity;

using Sitecore.Diagnostics;

namespace Shared.SC.Feature.Login.Configuration
{
    public static class CookieAuthentication
    {
        public static void ConfigureCookieAuthentication(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(
                new CookieAuthenticationOptions
                    {
                        SlidingExpiration = true,
                        ExpireTimeSpan = new TimeSpan(0, 15, 0),
                        SessionStore =
                            new SqlAuthSessionStore(IdentityHelper.TicketDataFormat),
                        TicketDataFormat = IdentityHelper.TicketDataFormat,
                        Provider =
                            new CookieAuthenticationProvider
                                {
                                    OnException = HandleCookieException
                                },
                        CookieManager = new SystemWebChunkingCookieManager()
                    });
        }

        private static void HandleCookieException(CookieExceptionContext ex)
        {
            Log.Fatal(ex.Exception.Message, ex.Exception, typeof(CookieAuthentication));
        }
    }
}