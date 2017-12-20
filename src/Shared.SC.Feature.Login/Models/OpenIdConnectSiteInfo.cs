using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.IdentityModel.Protocols;

using Sitecore.Sites;
using Sitecore.Web;

using StringDictionary = Sitecore.Collections.StringDictionary;

namespace Shared.SC.Feature.Login.Models
{
    [CLSCompliant(false)]
    public class OpenIdConnectSiteInfo : LoginSiteInfo
    {
        private const string TenantKey = "oicTenant";

        private const string ClientIdKey = "oicClientId";

        private const string ClientSecretKey = "oicClientSecret";

        private const string AadInstanceKey = "oicAadInstance";

        private const string SignUpPolicyIdKey = "oicSignUpPolicyId";

        private const string SignInPolicyIdKey = "oicSignInPolicyId";

        private const string UserProfilePolicyIdKey = "oicUserProfilePolicyId";

        private const string PostLogoutRedirectUriKey = "oicPostLogoutRedirectUri";

        private const string RedirectUriKey = "oicRedirectUri";

        private const string ErrorUriKey = "oicErrorUri";

        private const string ScopeKey = "oicScope";

        private const string ResponseTypeKey = "oicResponseType";

        private const string NameClaimTypeKey = "oicNameClaimType";

        public OpenIdConnectSiteInfo(Site site) : base(site)
        {
        }

        public OpenIdConnectSiteInfo(SiteInfo site) : base(site)
        {
        }

        public OpenIdConnectSiteInfo(SiteContext site) : base(site)
        {
        }

        public OpenIdConnectSiteInfo(NameValueCollection properties)
            : base(properties)
        {
        }

        public OpenIdConnectSiteInfo(StringDictionary properties)
            : base(properties)
        {
        }
        
        public bool UsesOpenIdConnect
            =>
                !(string.IsNullOrWhiteSpace(Tenant)
                  || string.IsNullOrWhiteSpace(ClientId)
                  || string.IsNullOrWhiteSpace(AadInstance)
                  || string.IsNullOrWhiteSpace(RedirectUri));

        public string Tenant
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(TenantKey))
                {
                    result = Properties[TenantKey];
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

        public string AadInstance
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(AadInstanceKey))
                {
                    result = Properties[AadInstanceKey];
                }

                return result;
            }
        }

        public string Authority => string.Format(AadInstance, Tenant, SignInPolicyId);

        public string SignUpPolicyId
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(SignUpPolicyIdKey))
                {
                    result = Properties[SignUpPolicyIdKey];
                }

                return result;
            }
        }

        public string SignInPolicyId
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(SignInPolicyIdKey))
                {
                    result = Properties[SignInPolicyIdKey];
                }

                return result;
            }
        }

        public string UserProfilePolicyId
        {
            get
            {
                string result = string.Empty;
                if (Properties.ContainsKey(UserProfilePolicyIdKey))
                {
                    result = Properties[UserProfilePolicyIdKey];
                }

                return result;
            }
        }

        [SuppressMessage("Managed Binary Analysis", "CA1056", Justification = "XML configuration")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3996", Justification = "XML configuration")]
        public string PostlogoutRedirectUri
        {
            get
            {
                string result = "/";
                if (Properties.ContainsKey(PostLogoutRedirectUriKey))
                {
                    result = Properties[PostLogoutRedirectUriKey];
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

        [SuppressMessage("Managed Binary Analysis", "CA1056", Justification = "XML configuration")]
        [SuppressMessage("SonarAnalyzer.CSharp", "S3996", Justification = "XML configuration")]
        public string ErrorUri
        {
            get
            {
                string result = "/";
                if (Properties.ContainsKey(ErrorUriKey))
                {
                    result = Properties[ErrorUriKey];
                }

                return result;
            }
        }

        public string Scope
        {
            get
            {
                string result = OpenIdConnectScopes.OpenId;
                if (Properties.ContainsKey(ScopeKey))
                {
                    result = Properties[ScopeKey];
                }

                return result;
            }
        }

        public string ResponseType
        {
            get
            {
                string result = OpenIdConnectResponseTypes.IdToken;
                if (Properties.ContainsKey(ResponseTypeKey))
                {
                    result = Properties[ResponseTypeKey];
                }

                return result;
            }
        }

        public string NameClaimType
        {
            get
            {
                string result = "name";
                if (Properties.ContainsKey(NameClaimTypeKey))
                {
                    result = Properties[NameClaimTypeKey];
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

        public static bool FastUsesOpenIdConnectCheck(SiteContext site)
        {
            return site?.Properties.AllKeys.Contains(ClientIdKey) ?? false;
        }
    }
}