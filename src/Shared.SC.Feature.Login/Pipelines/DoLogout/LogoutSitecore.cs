using System;

using Sitecore.Security.Authentication;

namespace Shared.SC.Feature.Login.Pipelines.DoLogout
{
    [CLSCompliant(false)]
    public class LogoutSitecore : IDoLogoutProcessor
    {
        public void Process(DoLogoutPipelineArgs args)
        {
            AuthenticationManager.Logout();
        }
    }
}