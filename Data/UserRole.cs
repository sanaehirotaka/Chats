namespace Chats.Data;

/// <summary>
/// ユーザーの役割を定義します。
/// </summary>
[Flags]
public enum UserRole
{
    /// <summary>
    /// 役割なし。
    /// </summary>
    None = 0,
    /// <summary>
    /// 一般ユーザー。
    /// </summary>
    User = 1,
    /// <summary>
    /// 管理者。
    /// </summary>
    Admin = 2
}
