using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace Shared.SC.Feature.Login.Models
{
    public class PrincipalClaimsInformationBase : IPrincipalClaimsInformation
    {
        private string _accountName;

        private string _displayName;

        private string _givenName;

        private Guid _objectIdentifierId;

        private string _surname;

        private Guid _tenantId;

        private string _email;

        public PrincipalClaimsInformationBase(IEnumerable<Claim> claims)
        {
            Claims = claims;
        }

        internal PrincipalClaimsInformationBase()
        {
        }

        public virtual string AccountNameKey => "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public virtual string AccountName
        {
            get
            {
                if (string.IsNullOrEmpty(_accountName))
                {
                    _accountName = GetStringFromClaimValue(AccountNameKey);
                }

                return _accountName;
            }
        }

        public virtual string DisplayNameKey => "http://schemas.microsoft.com/identity/claims/displayname";

        public virtual string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    _displayName = GetStringFromClaimValue(DisplayNameKey);
                }

                return _displayName;
            }
        }

        public virtual string GivenNameKey => ClaimTypes.GivenName;

        public virtual string GivenName
        {
            get
            {
                if (string.IsNullOrEmpty(_givenName))
                {
                    _givenName = GetStringFromClaimValue(GivenNameKey);
                }

                return _givenName;
            }
        }

        public virtual string ObjectIdentifierIdKey => "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public virtual Guid ObjectIdentifierId
        {
            get
            {
                if (_objectIdentifierId == Guid.Empty)
                {
                    _objectIdentifierId = GetGuidFromClaimValue(ObjectIdentifierIdKey);
                }

                return _objectIdentifierId;
            }
        }

        public virtual string SurnameKey => ClaimTypes.Surname;

        public virtual string Surname
        {
            get
            {
                if (string.IsNullOrEmpty(_surname))
                {
                    _surname = GetStringFromClaimValue(SurnameKey);
                }

                return _surname;
            }
        }

        public virtual string TenantIdKey => "http://schemas.microsoft.com/identity/claims/tenantid";

        public virtual Guid TenantId
        {
            get
            {
                if (_tenantId == Guid.Empty)
                {
                    _tenantId = GetGuidFromClaimValue(TenantIdKey);
                }

                return _tenantId;
            }
        }

        public virtual string RoleClaimTypeKey => "http://schemas.xmlsoap.org/claims/Group";

        [SuppressMessage("SonarAnalyzer.CSharp", "S2365", Justification = "Backing this field would lead to unexpected results")]
        public virtual IEnumerable<string> Roles
        {
            get
            {
                return Claims?.Where(c => c.Type == RoleClaimTypeKey).Select(c => c.Value).ToList()
                       ?? new List<string>();
            }
        }

        public virtual string EmailKey => ClaimTypes.Email;

        public virtual string Email
        {
            get
            {
                if (string.IsNullOrEmpty(_email))
                {
                    _email = GetStringFromClaimValue(EmailKey);
                }

                return _email;
            }
        }

        private IEnumerable<Claim> Claims { get; set; }

        public void UpdateClaims(IEnumerable<Claim> claims)
        {
            Claims = claims;
        }

        protected Guid GetGuidFromClaimValue(string claimIdKey)
        {
            Guid claimValue = Guid.Empty;
            Claim claim = Claims?.FirstOrDefault(c => c.Type == claimIdKey);
            if (claim != null)
            {
                claimValue = Guid.Parse(claim.Value);
            }

            return claimValue;
        }

        protected string GetStringFromClaimValue(string claimIdKey)
        {
            string claimValue = string.Empty;
            Claim claim = Claims?.FirstOrDefault(c => c.Type == claimIdKey);
            if (claim != null)
            {
                claimValue = claim.Value;
            }

            return claimValue;
        }
    }
}