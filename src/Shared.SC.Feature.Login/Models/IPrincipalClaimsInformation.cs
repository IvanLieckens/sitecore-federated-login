using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Shared.SC.Feature.Login.Models
{
    public interface IPrincipalClaimsInformation
    {
        string AccountNameKey { get; }

        string AccountName { get; }

        string DisplayNameKey { get; }

        string DisplayName { get; }

        string GivenNameKey { get; }

        string GivenName { get; }

        string ObjectIdentifierIdKey { get; }

        Guid ObjectIdentifierId { get; }

        string SurnameKey { get; }

        string Surname { get; }

        string TenantIdKey { get; }

        Guid TenantId { get; }

        string RoleClaimTypeKey { get; }

        IEnumerable<string> Roles { get; }

        string EmailKey { get; }

        string Email { get; }

        void UpdateClaims(IEnumerable<Claim> claims);
    }
}
