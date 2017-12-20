using System;
using System.Security.Claims;
using System.Web;

using Shared.SC.Feature.Login.Identity;
using Shared.SC.Feature.Login.Models;
using Shared.SC.Feature.Login.Pipelines.AuthenticationCheck;

using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Pipelines;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Security.Accounts;
using Sitecore.Web;

namespace Shared.SC.Feature.Login.Pipelines.HttpRequest
{
    /// <summary>
    /// Verifies authentication tickets:
    /// If .AspxAuth is available and no .AspNet.Cookies: deny permission, logout and redirect to login
    /// If .AspNet.Cookies is available and no .AspxAuth: login sitecore user
    /// If both are unavailable: anonymous user
    /// If both are available:
    ///     Check identities: if they are equal: OK
    ///     Else: logout both identities and redirect to a public page
    /// </summary>
    [CLSCompliant(false)]
    public class AuthenticationChecker : HttpRequestProcessor
    {
        private readonly IdentityHelper _identityHelper;

        public AuthenticationChecker() : this(new IdentityHelper())
        {
        }

        public AuthenticationChecker(IdentityHelper identityHelper)
        {
            _identityHelper = identityHelper;
        }

        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            
            // NOTE [ILs] Only check authentication for claimbased authentication sites
            if (LoginSiteInfo.FastIsClaimsBasedCheck(Context.Site))
            {
                ClaimsPrincipal federatedUser = _identityHelper.GetCurrentClaimsPrincipal() as ClaimsPrincipal;

                // algorithm:
                // 1 - if user is not logged in AND claimscookie is missing, return: anonymous visit -> handle in pipeline
                // 2 - if only claimscookie is available, delete this cookie -> handled by owin
                // 3 - if only ID in Database is available (not possible to check) -> handled by timer
                // 4 - if cookie, fedID and no sitecore ID is available -> redirect to login page, handled by sitecore
                // 5 - if only .ASPXAUTH cookiue is available (Context.IsLoggedIn) -> logout and redirect -> pipeline
                // 6 - if claimscookie, no fed ID and sitecore login is availalbe: logout and redirect -> pipeline
                // 7-  if no claimscookie, no fed ID and sitecore login available: logout and redirect -> pipeline. 
                // handled by  

                // 1 - anonymous
                if (!Context.IsLoggedIn && federatedUser == null)
                {
                    return;
                }

                if (Context.IsLoggedIn && federatedUser == null)
                {
                    // 5 & 7 - pipeline if user is logged in
                    LogoutAndRedirectToLogoutPage();
                }
                else if (Context.IsLoggedIn && federatedUser != null)
                {
                    // 8 all identities available
                    // check if identity matches.
                    // if not: redirect. Otherwise: return
                    User user = Context.User;

                    // compare identities
                    // if not equal, , there is a cookie mismatch: 
                    // remove tokens, 
                    // logout sitecore user and 
                    // redirect to loginpage.
                   LoginSiteInfo currentSiteInfo = new LoginSiteInfo(Context.Site);
                    IPrincipalClaimsInformation principalClaimsInformation =
                        (IPrincipalClaimsInformation)
                        // ReSharper disable once AssignNullToNotNullAttribute - Will never be null
                        Activator.CreateInstance(Type.GetType(currentSiteInfo.PrincipleClaimsInfoClass), federatedUser.Claims);
                    AuthenticationCheckPipelineArgs pipelineArgs = new AuthenticationCheckPipelineArgs
                    {
                        ClaimsUser = federatedUser,
                        SitecoreUser = user,
                        PrincipalClaimsInformation = principalClaimsInformation
                    };
                    CorePipeline.Run("authenticationCheck", pipelineArgs);
                    if (!pipelineArgs.IsCheckSuccess)
                    {
                        LogoutAndRedirectToLogoutPage();
                    }
                }
                else
                {
                    // several options:
                    // Callback from the federated Identity provider, or an unexpected situation

                    // Callback from the identity provider
                    // entry from /login, auth context
                    if (HttpContext.Current.Request.Url.PathAndQuery.StartsWith(
                        Context.Site.LoginPage,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    // For all other situations:
                    // Log to database for other situation
                    LogoutAndRedirectToLogoutPage();
                }
            }
        }

        private static void LogoutAndRedirectToLogoutPage()
        {
            string logoutPage = Context.Site.Properties["logoutPage"];
            if (string.IsNullOrWhiteSpace(logoutPage))
            {
                // NOTE [ILs] In case there's no defined logout page, redirect to root.
                Item homeItem = Context.Database.GetItem(Context.Site.StartItem);
                if (homeItem != null)
                {
                    logoutPage = LinkManager.GetItemUrl(homeItem);
                }
            }
            
            WebUtil.Redirect(logoutPage, false);
        }
    }
}