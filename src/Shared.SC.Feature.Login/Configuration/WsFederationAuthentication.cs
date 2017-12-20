using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;

using Microsoft.Owin.Security.WsFederation;

using Owin;

using Shared.SC.Feature.Login.Extensions;
using Shared.SC.Feature.Login.Models;

using Sitecore.Mvc.Extensions;
using Sitecore.Web;

namespace Shared.SC.Feature.Login.Configuration
{
    [CLSCompliant(false)]
    public static class WsFederationAuthentication
    {
        public static void ConfigureWsFederatedWebsites(IAppBuilder app, IEnumerable<SiteInfo> siteInfoList)
        {
            IEnumerable<WsFederatedSiteInfo> sites =
                siteInfoList.Select(s => new WsFederatedSiteInfo(s)).Where(s => s.IsFederated);

            foreach (WsFederatedSiteInfo site in sites)
            {
                app.MapWhen(
                    ctx => ctx.MapDomain(site.Realm),
                    conf =>
                        {
                            CookieAuthentication.ConfigureCookieAuthentication(conf);

                            conf.UseWsFederationAuthentication(
                                new WsFederationAuthenticationOptions
                                    {
                                        UseTokenLifetime = true,
                                        MetadataAddress = site.MetadataAddress,
                                        Wtrealm = "https://" + site.Realm,
                                        Wreply = site.ReplyUrl,
                                        TokenValidationParameters = new TokenValidationParameters
                                                                        {
                                                                            NameClaimType = site.NameClaimType,
                                                                            ValidAudiences = new List<string> { "https://" + site.Realm.WithPostfix('/') }
                                                                        }
                                    });
                        });
            }
        }
    }
}