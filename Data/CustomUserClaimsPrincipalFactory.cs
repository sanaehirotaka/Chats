using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Chats.Data;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Add custom UserRole claims
        if (user.Role.HasFlag(UserRole.Admin))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, UserRole.Admin.ToString()));
        }
        if (user.Role.HasFlag(UserRole.User))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, UserRole.User.ToString()));
        }
        // Add other roles if necessary

        return identity;
    }
}
