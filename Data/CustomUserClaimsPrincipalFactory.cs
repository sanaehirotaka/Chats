using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Chats.Data;

/// <summary>
/// <see cref="User"/> から <see cref="ClaimsPrincipal"/> を作成するためのカスタムファクトリを提供し、
/// ユーザーロールなどのカスタムクレームを追加します。
/// </summary>
public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole>
{
    /// <summary>
    /// <see cref="CustomUserClaimsPrincipalFactory"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="userManager">使用する <see cref="UserManager{TUser}"/>。</param>
    /// <param name="roleManager">使用する <see cref="RoleManager{TRole}"/>。</param>
    /// <param name="optionsAccessor"> <see cref="IdentityOptions"/> のアクセサー。</param>
    public CustomUserClaimsPrincipalFactory(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    /// <summary>
    /// 指定されたユーザーのクレームアイデンティティを非同期的に生成します。
    /// </summary>
    /// <param name="user">クレームを生成するユーザー。</param>
    /// <returns>生成された <see cref="ClaimsIdentity"/>。</returns>
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
