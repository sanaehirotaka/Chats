using System.ComponentModel.DataAnnotations;

namespace Chats.Models.Account;

public class UserAccountInputModel
{
    /// <summary>
    /// ユーザーのメールアドレスを取得または設定します。
    /// このフィールドは必須であり、有効なメール形式である必要があります。
    /// </summary>
    [Required]
    [EmailAddress]
    [Display(Name = "メールアドレス")]
    public string Email { get; set; } = default!;

    /// <summary>
    /// ユーザーのパスワードを取得または設定します。
    /// このフィールドは必須であり、入力タイプはパスワードです。
    /// </summary>
    [Required]
    [StringLength(255, ErrorMessage = "{0} は {2} 文字以上 {1} 文字以下である必要があります。", MinimumLength = 10)]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; set; } = default!;
}
