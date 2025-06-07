using Chats.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats.Pages.Account;

[AllowAnonymous]
public class SSOModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly AppDbContext _context;

    /// <summary>
    /// SSOModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="signInManager">サインインマネージャー。</param>
    /// <param name="context">アプリケーションのデータベースコンテキスト。</param>
    public SSOModel(SignInManager<User> signInManager, AppDbContext context)
    {
        _signInManager = signInManager;
        _context = context;
    }

    /// <summary>
    /// SSOページのGETリクエストを処理します。
    /// 提供されたトークンを検証し、有効な場合はユーザーをサインインさせます。
    /// </summary>
    /// <param name="token">アクセスに使用するトークン。</param>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnGetAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("./Login");
        }

        var accessToken = await _context.AccessTokens
            .Include(at => at.User)
            .FirstOrDefaultAsync(at => at.Token == token && at.ExpiresAt > DateTime.UtcNow);

        if (accessToken == null)
        {
            // トークンが無効または期限切れ
            return RedirectToPage("./Login");
        }

        // ユーザーをサインインさせる
        await _signInManager.SignInAsync(accessToken.User, isPersistent: false);

        return LocalRedirect("~/");
    }
}
