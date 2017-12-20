using System;

using Sitecore.Caching;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    public class FillVirtualUserProfile : IDoLoginProcessor
    {
        public void Process(DoLoginPipelineArgs args)
        {
            if (args?.User != null)
            {
                if (string.IsNullOrWhiteSpace(args.User.Profile.Email))
                {
                    args.User.Profile.Email = args.PrincipalClaimsInformation.Email;
                }

                if (string.IsNullOrWhiteSpace(args.User.Profile.Name))
                {
                    args.User.Profile.Name = args.PrincipalClaimsInformation.DisplayName;
                }

                if (string.IsNullOrEmpty(args.User.Profile.FullName))
                {
                    args.User.Profile.FullName = args.PrincipalClaimsInformation.DisplayName;
                }

                args.User.Profile.Save();

                // force a refresh of the user profile cache for the user
                CacheManager.GetUserProfileCache()
                            .RemoveUser(args.User.Name);
            }
        }
    }
}