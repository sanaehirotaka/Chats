using Chats.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;

    /// <summary>
    /// DetailsModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="context">アプリケーションのデータベースコンテキスト。</param>
    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public User User { get; set; } = default!;

    /// <summary>
    /// ユーザー詳細ページのGETリクエストを処理します。
    /// 指定されたIDのユーザー情報を取得し、表示します。
    /// </summary>
    /// <param name="id">表示するユーザーのID。</param>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        User = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

        if (User == null)
        {
            return NotFound();
        }
        return Page();
    }
}
