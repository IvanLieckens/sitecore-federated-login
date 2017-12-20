using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Claims;
using System.Linq;

using Sitecore.Sites;
using Sitecore.Web;

using StringDictionary = Sitecore.Collections.StringDictionary;

namespace Shared.SC.Feature.Login.Models
{
    [CLSCompliant(false)]
    public class WsFederatedSiteInfo : LoginSiteInfo
    {
        private const string MetadataAddressKey = "wsFedMetadataAddress";

        private const string ReplyUrlKey = "wsFedReplyUrl";

        private const string NameClaimTypeKey = "wsFedNameClaimType";

        public WsFederatedSiteInfo(Site site) : base(site)
        {
        }

        public WsFederatedSiteInfo(SiteInfo site) : base(site)
        {
        }

        public WsFederatedSiteInfo(SiteContext site) : base(site)
        {
        }

        public WsFederatedSiteInfo(NameValueCollection properties)
            : base(properties)
        {
        }

        public WsFederatedSiteInfo(StringDictionary properties)
            : base(properties)
        {
        }

        public string MetadataAddress
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(MetadataAddressKey))
                {
                    result = Properties[MetadataAddressKey];
                }

                return result;
            }
        }

        public string Realm => HostName;

        [SuppressMessage("Managed Binary Analysis", "CA1056", Justification = "XML configuration")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3996", Justification = "XML configuration")]
        public string ReplyUrl
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(ReplyUrlKey))
                {
                    result = Properties[ReplyUrlKey];
                }

                return result;
            }
        }

        public bool IsFederated => !(string.IsNullOrWhiteSpace(MetadataAddress) || 
                                   string.IsNullOrWhiteSpace(Realm) ||
                                   string.IsNullOrWhiteSpace(ReplyUrl));

        public string NameClaimType
        {
            get
            {
                string result = ClaimTypes.Name;
                if (Properties.ContainsKey(NameClaimTypeKey))
                {
                    result = Properties[NameClaimTypeKey];
                }

                return result;
            }
        }

        public static bool FastIsFederatedCheck(SiteContext site)
        {
            return site?.Properties.AllKeys.Contains(MetadataAddressKey) ?? false;
        }
    }
}