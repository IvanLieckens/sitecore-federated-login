using System;

using Shared.SC.Feature.Login.Extensions;
using Shared.SC.Feature.Login.Models;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    public class GetValidRoles : IDoLoginProcessor
    {
        public void Process(DoLoginPipelineArgs args)
        {
            LoginSiteInfo currentSiteInfo = new LoginSiteInfo(Sitecore.Context.Site.SiteInfo);
            if (args?.ValidRoles.Count == 0)
            {
                args.ValidRoles.AddRange(currentSiteInfo.ValidRoles);
            }
        }
    }
}