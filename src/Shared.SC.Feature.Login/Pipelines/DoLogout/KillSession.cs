using System;
using System.Web;

namespace Shared.SC.Feature.Login.Pipelines.DoLogout
{
    [CLSCompliant(false)]
    public class KillSession : IDoLogoutProcessor
    {
        public void Process(DoLogoutPipelineArgs args)
        {
            HttpContext.Current.Session.Abandon();
        }
    }
}