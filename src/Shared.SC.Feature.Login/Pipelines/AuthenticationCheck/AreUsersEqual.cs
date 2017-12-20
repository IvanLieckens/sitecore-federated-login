using System;

using Sitecore;

namespace Shared.SC.Feature.Login.Pipelines.AuthenticationCheck
{
    [CLSCompliant(false)]
    public class AreUsersEqual : IAuthenticationCheckProcessor
    {
        public void Process(AuthenticationCheckPipelineArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            string accountName = args.PrincipalClaimsInformation.AccountName;
            args.IsCheckSuccess = args.SitecoreUser.Name.Equals($"{Context.Domain.Name}\\{accountName}");
        }
    }
}