using Chats.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chats.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    /// <summary>
    /// LogoutModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="signInManager">サインインマネージャー。</param>
    /// <param name="logger">ロガーインスタンス。</param>
    public LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// ログアウトページのGETリクエストを処理します。
    /// ユーザーをサインアウトさせ、ホームページにリダイレクトします。
    /// </summary>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnGet()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Index");
    }
}
