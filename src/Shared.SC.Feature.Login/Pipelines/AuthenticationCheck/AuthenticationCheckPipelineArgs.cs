using System;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Permissions;

using Shared.SC.Feature.Login.Models;

using Sitecore.Pipelines;
using Sitecore.Security.Accounts;

namespace Shared.SC.Feature.Login.Pipelines.AuthenticationCheck
{
    [CLSCompliant(false)]
    [Serializable]
    public class AuthenticationCheckPipelineArgs : PipelineArgs
    {
        public AuthenticationCheckPipelineArgs()
        {
            IsCheckSuccess = true;
        }

        protected AuthenticationCheckPipelineArgs(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ClaimsUser = (ClaimsPrincipal)info.GetValue(nameof(ClaimsUser), typeof(ClaimsPrincipal));
            SitecoreUser = (User)info.GetValue(nameof(SitecoreUser), typeof(User));
            PrincipalClaimsInformation = (IPrincipalClaimsInformation)info.GetValue(
                nameof(PrincipalClaimsInformation),
                typeof(IPrincipalClaimsInformation));
            IsCheckSuccess = info.GetBoolean(nameof(IsCheckSuccess));
        }

        public ClaimsPrincipal ClaimsUser { get; set; }

        public User SitecoreUser { get; set; }

        public IPrincipalClaimsInformation PrincipalClaimsInformation { get; set; }

        public bool IsCheckSuccess { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ClaimsUser), ClaimsUser);
            info.AddValue(nameof(SitecoreUser), SitecoreUser);
            info.AddValue(nameof(PrincipalClaimsInformation), PrincipalClaimsInformation);
            info.AddValue(nameof(IsCheckSuccess), IsCheckSuccess);

            base.GetObjectData(info, context);
        }
    }
}