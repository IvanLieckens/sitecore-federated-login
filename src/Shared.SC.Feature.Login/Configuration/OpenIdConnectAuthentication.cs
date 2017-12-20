using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;

using Owin;

using Shared.SC.Feature.Login.Extensions;
using Shared.SC.Feature.Login.Models;

using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Web;

namespace Shared.SC.Feature.Login.Configuration
{
    [CLSCompliant(false)]
    public static class OpenIdConnectAuthentication
    {
        public static void ConfigureOpenIdConnectWebsites(IAppBuilder app, IEnumerable<SiteInfo> siteInfoList)
        {
            IEnumerable<OpenIdConnectSiteInfo> sites =
                siteInfoList.Select(s => new OpenIdConnectSiteInfo(s)).Where(s => s.UsesOpenIdConnect);

            foreach (OpenIdConnectSiteInfo site in sites)
            {
                app.MapWhen(
                    ctx => ctx.MapDomain(site.HostName) && ctx.MapFolder(site.PhysicalFolder),
                    conf =>
                        {
                            CookieAuthentication.ConfigureCookieAuthentication(conf);
                            conf.UseOpenIdConnectAuthentication(CreateOptionsFromSiteInfo(site));
                        });
            }
        }

        private static OpenIdConnectAuthenticationOptions CreateOptionsFromSiteInfo(OpenIdConnectSiteInfo site)
        {
            return new OpenIdConnectAuthenticationOptions
                       {
                           // Generate the metadata address using the tenant and policy information
                           MetadataAddress = site.Authority,

                           // These are standard OpenID Connect parameters
                           ClientId = site.ClientId,
                           RedirectUri = site.RedirectUri,
                           PostLogoutRedirectUri = site.PostlogoutRedirectUri,

                           // Specify the callbacks for each type of notifications
                           Notifications = new OpenIdConnectAuthenticationNotifications
                                               {
                                                   RedirectToIdentityProvider =
                                                       context => HandleOpenIdConnectRedirectToIdentityProvider(context, site),
                                                   AuthenticationFailed =
                                                       context => HandleOpenIdConnectAuthenticationFailed(context, site)
                                               },

                           // Specify the scope by appending all of the scopes requested into one string (seperated by a blank space)
                           Scope = site.Scope,

                           // Specify the claims to validate
                           TokenValidationParameters = new TokenValidationParameters
                                                           {
                                                               NameClaimType = site.NameClaimType,
                                                               SaveSigninToken = true
                                                           }
                       };
        }

        /*
         *  On each call to Azure AD B2C, check if a policy (e.g. the profile edit or password reset policy) has been specified in the OWIN context.
         *  If so, use that policy when making the call. Also, don't request a code (since it won't be needed).
         */
        private static Task HandleOpenIdConnectRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification, OpenIdConnectSiteInfo site)
        {
            string policy = notification.OwinContext.Get<string>("Policy");

            if (!string.IsNullOrEmpty(policy) && !policy.Equals(site.SignInPolicyId))
            {
                notification.ProtocolMessage.Scope = OpenIdConnectScopes.OpenId;
                notification.ProtocolMessage.ResponseType = OpenIdConnectResponseTypes.IdToken;
                notification.ProtocolMessage.IssuerAddress = notification.ProtocolMessage.IssuerAddress.Replace(site.SignInPolicyId, policy);
            }

            return Task.FromResult(0);
        }

        private static Task HandleOpenIdConnectAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context, OpenIdConnectSiteInfo site)
        {
            context.HandleResponse();
            Log.Fatal(context.Exception.Message, context.Exception, typeof(OpenIdConnectAuthentication));
            UrlString errorUrl = new UrlString(site.ErrorUri);
            errorUrl.Add("message", context.Exception.Message);
            context.Response.Redirect(errorUrl.ToString());
            return Task.FromResult(0);
        }
    }
}