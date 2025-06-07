using Chats.Data;
using Chats.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace Chats.Pages.Account;

[AllowAnonymous]
/// <summary>
/// ユーザー登録ページのモデルを表します。
/// 新規ユーザーの作成と検証を処理します。
/// </summary>
public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly AppDbContext _dbContext;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// <see cref="RegisterModel"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="userManager">ユーザーを管理するためのマネージャー。</param>
    /// <param name="signInManager">ユーザーのサインインを管理するためのマネージャー。</param>
    /// <param name="logger">ロギング機能を提供します。</param>
    /// <param name="dbContext">アプリケーションのデータベースコンテキスト。</param>
    /// <param name="appSettings">アプリケーション設定。</param>
    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        AppDbContext dbContext,
        IOptions<AppSettings> appSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _dbContext = dbContext;
        _appSettings = appSettings.Value;
    }

    /// <summary>
    /// 登録詳細の入力モデルを取得または設定します。
    /// このプロパティはHTTPリクエストからバインドされます。
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// 登録フォームの入力フィールドを表します。
    /// </summary>
    public class InputModel : UserAccountInputModel // Inherit from UserAccountInputModel
    {
        /// <summary>
        /// ユーザーの表示名を取得または設定します。
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "表示名")]
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// 確認用パスワードを取得または設定します。
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "パスワードの確認")]
        [Compare("Password", ErrorMessage = "パスワードと確認用パスワードが一致しません。")]
        public string ConfirmPassword { get; set; } = default!;
    }

    /// <summary>
    /// 登録ページのGETリクエストを処理します。
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// 登録ページのPOSTリクエストを処理します。
    /// 入力を検証し、既存のユーザーをチェックし、パスワードをハッシュ化し、新しいユーザーをデータベースに保存します。
    /// </summary>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    /// 入力を検証し、既存のユーザーをチェックし、パスワードをハッシュ化し、新しいユーザーをデータベースに保存します。
    /// </summary>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (_appSettings.SingleUserMode)
        {
            // SingleUserModeが有効な場合のみユーザー数をチェック
            if (await _dbContext.GetUserCountAsync() > 0)
            {
                ModelState.AddModelError(string.Empty, "新規ユーザー登録を停止しています");
                return Page();
            }
        }

        if (!ModelState.IsValid)
        {
            // モデルの状態が無効な場合、検証エラーとともにフォームを再表示します。
            return Page();
        }

        var user = new User { UserName = Input.Email, Email = Input.Email, DisplayName = Input.DisplayName, CreatedAt = DateTime.UtcNow };

        // 最初のユーザーが登録される場合、そのユーザーを管理者として設定します。
        if (!_userManager.Users.Any())
        {
            user.Role = UserRole.User | UserRole.Admin;
        }

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            // ユーザー作成後、自動的にサインインさせます。
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect("~/");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        // ユーザー作成に失敗した場合、エラーとともにフォームを再表示します。
        return Page();
    }
}
