using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ProjectWebApp.Security
{
    public class CustomClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity != null && principal.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)principal.Identity;

                // Check if FullName claim already exists
                if (!identity.HasClaim(c => c.Type == "FullName"))
                {
                    // In your DB or Identity, replace this with actual user lookup:
                    string firstName = identity.FindFirst(ClaimTypes.GivenName)?.Value ?? "First";
                    string lastName = identity.FindFirst(ClaimTypes.Surname)?.Value ?? "Last";

                    identity.AddClaim(new Claim("FullName", $"{firstName} {lastName}"));
                }
            }
            return Task.FromResult(principal);
        }
    }
}
