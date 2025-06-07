using Chats.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Chats.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly UserManager<User> _userManager;

    /// <summary>
    /// CreateModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="userManager">ユーザーを管理するためのマネージャー。</param>
    public CreateModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(255, ErrorMessage = "{0}は{2}文字以上{1}文字以下である必要があります。", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "パスワードの確認")]
        [Compare("Password", ErrorMessage = "パスワードと確認パスワードが一致しません。")]
        public string ConfirmPassword { get; set; } = default!;

        [Display(Name = "表示名")]
        public string? DisplayName { get; set; }

        [Required]
        [Display(Name = "役割")]
        public UserRole Role { get; set; }
    }

    /// <summary>
    /// ユーザー作成ページのGETリクエストを処理します。
    /// </summary>
    /// <returns>ページのアクション結果。</returns>
    public IActionResult OnGet()
    {
        return Page();
    }

    /// <summary>
    /// ユーザー作成ページのPOSTリクエストを処理します。
    /// 新しいユーザーを作成し、データベースに保存します。
    /// </summary>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new User
        {
            UserName = Input.Email,
            Email = Input.Email,
            DisplayName = Input.DisplayName,
            Role = Input.Role
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            return RedirectToPage("./Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
