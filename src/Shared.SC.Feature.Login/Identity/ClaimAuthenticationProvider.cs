using System;
using System.Collections.Specialized;

using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;

namespace Shared.SC.Feature.Login.Identity
{
    [CLSCompliant(false)]
    public class ClaimAuthenticationProvider : FormsAuthenticationProvider
    {
        private ClaimAuthenticationHelper _helper;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            _helper = new ClaimAuthenticationHelper(this);
        }

        public override User GetActiveUser()
        {
            return _helper.GetActiveUser();
        }
    }
}