using System;
using System.Security.Claims;
using System.Web;

using Shared.SC.Feature.Login.Configuration;
using Shared.SC.Feature.Login.Identity;
using Shared.SC.Feature.Login.Models;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    public class GetPrincipal : IDoLoginProcessor
    {
        private readonly IdentityHelper _identityHelper;

        public GetPrincipal() : this(new IdentityHelper())
        {
        }

        public GetPrincipal(IdentityHelper identityHelper)
        {
            _identityHelper = identityHelper;
        }

        public void Process(DoLoginPipelineArgs args)
        {
            LoginSiteInfo currentSiteInfo = new LoginSiteInfo(Sitecore.Context.Site.SiteInfo);
            if (args != null && args.Principal == null)
            {
                args.Principal = _identityHelper.GetCurrentClaimsPrincipal();
            }

            // NOTE [ILs] OAuth Principal is hidden during login so try fetching it
            if (args != null && args.Principal == null)
            {
                ClaimsIdentity identity = args.HttpContext.GetOwinContext().Get<ClaimsIdentity>(OAuthAuthentication.OAuthOwinContextKey);
                if (identity != null)
                {
                    args.Principal = new ClaimsPrincipal(identity);
                }
            }

            if (args?.Principal != null && args.PrincipalClaimsInformation == null)
            {
                ClaimsPrincipal principal = args.Principal as ClaimsPrincipal;
                
                Type principalClaimsInformationType = Type.GetType(currentSiteInfo.PrincipleClaimsInfoClass);
                if (principalClaimsInformationType != null)
                {
                    args.PrincipalClaimsInformation =
                        (IPrincipalClaimsInformation)
                        Activator.CreateInstance(principalClaimsInformationType, principal?.Claims);
                }
            }
        }
    }
}