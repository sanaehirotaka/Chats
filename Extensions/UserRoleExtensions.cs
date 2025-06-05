using Chats.Data;

namespace Chats.Extensions;

/// <summary>
/// <see cref="UserRole"/> 列挙型に対する拡張メソッドを提供します。
/// </summary>
public static class UserRoleExtensions
{
    /// <summary>
    /// ユーザーロールに管理者フラグが含まれているかを確認します。
    /// </summary>
    /// <param name="role">確認するユーザーロール。</param>
    /// <returns>ロールに管理者が含まれている場合は <c>true</c>、それ以外の場合は <c>false</c>。</returns>
    public static bool IsAdmin(this UserRole role)
    {
        return role.HasFlag(UserRole.Admin);
    }

    /// <summary>
    /// ユーザーロールにユーザーフラグが含まれているかを確認します。
    /// </summary>
    /// <param name="role">確認するユーザーロール。</param>
    /// <returns>ロールにユーザーが含まれている場合は <c>true</c>、それ以外の場合は <c>false</c>。</returns>
    public static bool IsUser(this UserRole role)
    {
        return role.HasFlag(UserRole.User);
    }

    /// <summary>
    /// ユーザーロールから対応するロール名のリストを取得します。
    /// </summary>
    /// <param name="role">確認するユーザーロール。</param>
    /// <returns>ロール名のリスト。</returns>
    public static IEnumerable<string> GetRoleNames(this UserRole role)
    {
        if (role.HasFlag(UserRole.Admin))
        {
            yield return UserRole.Admin.ToString();
        }
        if (role.HasFlag(UserRole.User))
        {
            yield return UserRole.User.ToString();
        }
    }
}
