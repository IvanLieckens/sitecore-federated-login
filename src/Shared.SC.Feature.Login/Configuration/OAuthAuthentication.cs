using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Owin.Security.OAuth;

using Owin;

using Shared.SC.Feature.Login.Extensions;
using Shared.SC.Feature.Login.Models;
using Shared.SC.Feature.Login.Providers;

using Sitecore.Web;

using OAuthBearerAuthenticationProvider = Shared.SC.Feature.Login.Providers.OAuthBearerAuthenticationProvider;

namespace Shared.SC.Feature.Login.Configuration
{
    [CLSCompliant(false)]
    public static class OAuthAuthentication
    {
        public const string OAuthOwinContextKey = "Shared.SC.Feature.Login.Configuration.OAuthAuthentication";

        public static void ConfigureOauthWebsites(IAppBuilder app, IEnumerable<SiteInfo> siteInfoList)
        {
            IEnumerable<OAuthSiteInfo> sites =
                siteInfoList.Select(s => new OAuthSiteInfo(s)).Where(s => s.UsesOauth);

            foreach (OAuthSiteInfo site in sites)
            {
                app.MapWhen(
                    ctx => ctx.MapDomain(site.HostName) && ctx.MapFolder(site.PhysicalFolder),
                    conf =>
                        {
                            CookieAuthentication.ConfigureCookieAuthentication(conf);
                            conf.UseOAuthBearerAuthentication(CreateOptionsFromSiteInfo(site));
                        });
            }
        }

        private static OAuthBearerAuthenticationOptions CreateOptionsFromSiteInfo(OAuthSiteInfo site)
        {
            OAuthBearerAuthenticationOptions result = new OAuthBearerAuthenticationOptions();
            result.AccessTokenProvider = new OAuthBearerAuthenticationTokenProvider();
            result.Provider = new OAuthBearerAuthenticationProvider(site);

            return result;
        }
    }
}