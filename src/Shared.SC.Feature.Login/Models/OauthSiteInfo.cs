using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sitecore.Sites;
using Sitecore.Web;

using StringDictionary = Sitecore.Collections.StringDictionary;

namespace Shared.SC.Feature.Login.Models
{
    [CLSCompliant(false)]
    public class OAuthSiteInfo : LoginSiteInfo
    {
        private const string AuthorityUriKey = "oauthAuthorityUri";

        private const string ClientIdKey = "oauthClientId";

        private const string ClientSecretKey = "oauthClientSecret";

        private const string ScopeKey = "oauthScope";

        private const string RedirectUriKey = "oauthRedirectUri";

        public OAuthSiteInfo(Site site)
            : base(site)
        {
        }

        public OAuthSiteInfo(SiteInfo site)
            : base(site)
        {
        }

        public OAuthSiteInfo(SiteContext site)
            : base(site)
        {
        }

        public OAuthSiteInfo(NameValueCollection properties)
            : base(properties)
        {
        }

        public OAuthSiteInfo(StringDictionary properties)
            : base(properties)
        {
        }

        [SuppressMessage("Managed Binary Analysis", "CA1056", Justification = "XML configuration")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3996", Justification = "XML configuration")]
        public string AuthorityUri
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(AuthorityUriKey))
                {
                    result = Properties[AuthorityUriKey];
                }

                return result;
            }
        }

        public string ClientId
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(ClientIdKey))
                {
                    result = Properties[ClientIdKey];
                }

                return result;
            }
        }

        public string ClientSecret
        {
            get
            {
                string result = "D34DB33F";
                if (Properties.ContainsKey(ClientSecretKey))
                {
                    result = Properties[ClientSecretKey];
                }

                return result;
            }
        }

        public string Scope
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(ScopeKey))
                {
                    result = Properties[ScopeKey];
                }

                return result;
            }
        }

        [SuppressMessage("Managed Binary Analysis", "CA1056", Justification = "XML configuration")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3996", Justification = "XML configuration")]
        public string RedirectUri
        {
            get
            {
                string result = "/";
                if (Properties.ContainsKey(RedirectUriKey))
                {
                    result = Properties[RedirectUriKey];
                }

                return result;
            }
        }

        public bool UsesOauth
            =>
                !(string.IsNullOrWhiteSpace(AuthorityUri)
                  || string.IsNullOrWhiteSpace(ClientId)
                  || string.IsNullOrWhiteSpace(Scope)
                  || string.IsNullOrWhiteSpace(RedirectUri));

        public static bool FastUsesOAuthCheck(SiteContext site)
        {
            return site?.Properties.AllKeys.Contains(ClientIdKey) ?? false;
        }
    }
}