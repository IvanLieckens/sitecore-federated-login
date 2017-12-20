using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Sitecore.Mvc.Extensions;
using Sitecore.Sites;
using Sitecore.Web;

using StringDictionary = Sitecore.Collections.StringDictionary;

namespace Shared.SC.Feature.Login.Models
{
    [CLSCompliant(false)]
    public class LoginSiteInfo
    {
        private const string HostNameKey = "hostName";

        private const string PhysicalFolderKey = "physicalFolder";

        private const string PrincipleClaimsInfoClassKey = "principleClaimsInfoClass";

        private const string ValidRolesKey = "validRoles";

        public LoginSiteInfo(Site site) : this(site?.Properties)
        {
        }

        public LoginSiteInfo(SiteInfo site) : this(site?.Properties)
        {
        }

        public LoginSiteInfo(SiteContext site) : this(site?.Properties)
        {
        }

        public LoginSiteInfo(NameValueCollection properties)
            : this(new StringDictionary(properties.ToKeyValues().Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value))))
        {
        }

        public LoginSiteInfo(StringDictionary properties)
        {
            Properties = properties;
        }

        public StringDictionary Properties { get; set; }

        public string HostName
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(HostNameKey))
                {
                    result = Properties[HostNameKey];
                }

                return result;
            }
        }

        public string PhysicalFolder
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(PhysicalFolderKey))
                {
                    result = Properties[PhysicalFolderKey];
                }

                return result;
            }
        }

        public string PrincipleClaimsInfoClass
        {
            get
            {
                string result = "Shared.SC.Feature.Login.Models.PrincipalClaimsInformationBase, Shared.SC.Feature.Login";
                if (Properties.ContainsKey(PrincipleClaimsInfoClassKey))
                {
                    result = Properties[PrincipleClaimsInfoClassKey];
                }

                return result;
            }
        }

        public IReadOnlyCollection<string> ValidRoles
        {
            get
            {
                List<string> result = new List<string>();
                if (Properties.ContainsKey(ValidRolesKey))
                {
                    result = Properties[ValidRolesKey]?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                return result;
            }
        }

        public static bool FastIsClaimsBasedCheck(SiteContext site)
        {
            return WsFederatedSiteInfo.FastIsFederatedCheck(site)
                   || OpenIdConnectSiteInfo.FastUsesOpenIdConnectCheck(site)
                   || OAuthSiteInfo.FastUsesOAuthCheck(site);
        }
    }
}