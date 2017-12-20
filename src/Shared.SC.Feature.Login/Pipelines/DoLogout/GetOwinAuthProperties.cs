using System;

using Microsoft.Owin.Security;

using Shared.SC.Feature.Login.Models;

using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Links;

namespace Shared.SC.Feature.Login.Pipelines.DoLogout
{
    [CLSCompliant(false)]
    public class GetOwinAuthProperties : IDoLogoutProcessor
    {
        public void Process(DoLogoutPipelineArgs args)
        {
            if (args != null && WsFederatedSiteInfo.FastIsFederatedCheck(Context.Site))
            {
                Item startItem = Context.Database.GetItem(Context.Site.StartPath);
                UrlOptions urlOptions = UrlOptions.DefaultOptions;
                urlOptions.AlwaysIncludeServerUrl = true;

                args.OwinAuthenticationProperties = new AuthenticationProperties();
                args.OwinAuthenticationProperties.RedirectUri = LinkManager.GetItemUrl(startItem, urlOptions);
                args.OwinAuthenticationProperties.AllowRefresh = false;
            }
            else if (args != null && OpenIdConnectSiteInfo.FastUsesOpenIdConnectCheck(Context.Site))
            {
                OpenIdConnectSiteInfo site = new OpenIdConnectSiteInfo(Context.Site);

                args.OwinAuthenticationProperties = new AuthenticationProperties();
                args.OwinAuthenticationProperties.RedirectUri = site.PostlogoutRedirectUri;
                args.OwinAuthenticationProperties.AllowRefresh = false;
            }
            else
            {
                // No OWIN imlementation found, continue process
            }
        }
    }
}