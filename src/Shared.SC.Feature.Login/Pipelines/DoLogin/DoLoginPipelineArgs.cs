using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

using Shared.SC.Feature.Login.Models;

using Sitecore.Pipelines;
using Sitecore.Security.Accounts;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    [SuppressMessage("SonarAnalyzer.CSharp", "S3925", Justification = "Outside our jurisdiction")]
    [Serializable]
    public class DoLoginPipelineArgs : PipelineArgs
    {
        public DoLoginPipelineArgs()
        {
            ValidRoles = new List<string>();
        }

        protected DoLoginPipelineArgs(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Principal = (IPrincipal)info.GetValue(nameof(Principal), typeof(IPrincipal));
            User = (User)info.GetValue(nameof(User), typeof(User));
            PostLoginAction = (ActionResult)info.GetValue(nameof(PostLoginAction), typeof(ActionResult));
            ReturnUrlQueryString = (Uri)info.GetValue(nameof(ReturnUrlQueryString), typeof(Uri));
            PrincipalClaimsInformation = (IPrincipalClaimsInformation)info.GetValue(nameof(PrincipalClaimsInformation), typeof(IPrincipalClaimsInformation));
            ValidRoles = (List<string>)info.GetValue(nameof(ValidRoles), typeof(List<string>));
            HttpContext = (HttpContextBase)info.GetValue(nameof(HttpContext), typeof(HttpContextBase));
        }

        public IPrincipal Principal { get; set; }

        public User User { get; set; }

        public ActionResult PostLoginAction { get; set; }

        public Uri ReturnUrlQueryString { get; set; }

        public IPrincipalClaimsInformation PrincipalClaimsInformation { get; set; }

        public IList<string> ValidRoles { get; }

        public HttpContextBase HttpContext { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Principal), Principal);
            info.AddValue(nameof(User), User);
            info.AddValue(nameof(PostLoginAction), PostLoginAction);
            info.AddValue(nameof(ReturnUrlQueryString), ReturnUrlQueryString);
            info.AddValue(nameof(PrincipalClaimsInformation), PrincipalClaimsInformation);
            info.AddValue(nameof(ValidRoles), ValidRoles);
            info.AddValue(nameof(HttpContext), HttpContext);

            base.GetObjectData(info, context);
        }
    }
}