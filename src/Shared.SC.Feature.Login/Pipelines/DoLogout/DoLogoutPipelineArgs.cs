using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;
using System.Web.Mvc;

using Microsoft.Owin.Security;

using Sitecore.Pipelines;

namespace Shared.SC.Feature.Login.Pipelines.DoLogout
{
    [CLSCompliant(false)]
    [Serializable]
    public class DoLogoutPipelineArgs : PipelineArgs
    {
        public DoLogoutPipelineArgs()
        {
        }

        protected DoLogoutPipelineArgs(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            OwinAuthenticationProperties = (AuthenticationProperties)info.GetValue(
                nameof(OwinAuthenticationProperties),
                typeof(AuthenticationProperties));
            PostLogoutAction = (ActionResult)info.GetValue(nameof(PostLogoutAction), typeof(ActionResult));
            Request = (HttpRequestBase)info.GetValue(nameof(Request), typeof(HttpRequestBase));
        }

        public AuthenticationProperties OwinAuthenticationProperties { get; set; }

        public ActionResult PostLogoutAction { get; set; }

        public HttpRequestBase Request { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(OwinAuthenticationProperties), OwinAuthenticationProperties);
            info.AddValue(nameof(PostLogoutAction), PostLogoutAction);
            info.AddValue(nameof(Request), Request);

            base.GetObjectData(info, context);
        }
    }
}