using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Chats.Data;

/// <summary>
/// アプリケーションのユーザーを表します。
/// このクラスは、データベースに保存されるユーザーデータのスキーマを定義します。
/// </summary>
public class User : IdentityUser
{

    [Display(Name = "メールアドレス")]
    public override string? Email { get; set; }

    /// <summary>
    /// ユーザーアカウントが作成されたときのタイムスタンプを取得または設定します。
    /// 作成時には現在のUTC時刻がデフォルトとなります。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ユーザーの表示名を取得または設定します。
    /// </summary>
    [Display(Name = "表示名")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// ユーザーの役割を取得または設定します。
    /// </summary>
    [Display(Name = "役割")]
    public UserRole Role { get; set; } = UserRole.User;
}
