using System.Security.Claims;

namespace Shared.SC.Feature.Login.Identity.Interfaces
{
    public interface IIdentityHelper
    {
        bool AddClaim(Claim claim);

        bool UpdateClaim(Claim claim);

        bool RemoveClaim(Claim claim);

        ClaimsPrincipal GetCurrentClaimsPrincipal();
    }
}