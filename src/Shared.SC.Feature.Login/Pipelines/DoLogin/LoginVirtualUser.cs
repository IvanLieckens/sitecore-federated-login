using System;
using System.Linq;
using System.Security.Claims;

using Sitecore;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    public class LoginVirtualUser : IDoLoginProcessor
    {
        public void Process(DoLoginPipelineArgs args)
        {
            if (args?.User == null && 
                args?.Principal != null && 
                args.Principal.Identity.IsAuthenticated)
            {
                ClaimsPrincipal principal = args.Principal as ClaimsPrincipal;
                if (principal != null &&
                    (!args.ValidRoles.Any() || args.PrincipalClaimsInformation.Roles.Intersect(args.ValidRoles).Any()))
                {
                    string accountName = args.PrincipalClaimsInformation.AccountName;
                    string userName = $"{Context.Domain.Name}\\{accountName}";
                    User virtualUser = AuthenticationManager.BuildVirtualUser(userName, true);
                    AuthenticationManager.Login(virtualUser);
                    args.User = virtualUser;
                }
            }
        }
    }
}