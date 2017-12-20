using System;
using System.Linq;
using System.Web;

namespace Shared.SC.Feature.Login.Pipelines.DoLogout
{
    [CLSCompliant(false)]
    public class LogoutOwin : IDoLogoutProcessor
    {
        public void Process(DoLogoutPipelineArgs args)
        {
            if (args != null)
            {
                string[] authTypes =
                        args.Request.GetOwinContext()
                            .Authentication.GetAuthenticationTypes()
                            .Select(at => at.AuthenticationType)
                            .ToArray();

                if (args.OwinAuthenticationProperties != null)
                {
                    args.Request.GetOwinContext()
                        .Authentication.SignOut(
                            args.OwinAuthenticationProperties,
                            authTypes);
                }
                else
                {
                    args.Request.GetOwinContext()
                        .Authentication.SignOut(authTypes);
                }
            }
        }
    }
}