using Chats.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    /// <summary>
    /// DeleteModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="userManager">ユーザーを管理するためのマネージャー。</param>
    /// <param name="context">アプリケーションのデータベースコンテキスト。</param>
    public DeleteModel(UserManager<User> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public User DisplayUser { get; set; } = default!;

    /// <summary>
    /// ユーザー削除ページのGETリクエストを処理します。
    /// 指定されたIDのユーザー情報を取得し、表示します。
    /// </summary>
    /// <param name="id">削除するユーザーのID。</param>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        DisplayUser = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

        if (DisplayUser == null)
        {
            return NotFound();
        }
        return Page();
    }

    /// <summary>
    /// ユーザー削除ページのPOSTリクエストを処理します。
    /// 指定されたIDのユーザーを削除します。
    /// </summary>
    /// <param name="id">削除するユーザーのID。</param>
    /// <returns>操作の結果を表す <see cref="IActionResult"/>。</returns>
    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userToDelete = await _userManager.FindByIdAsync(id);
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // ユーザーが認証されていない場合は400 Bad Requestを返します。
            return BadRequest("ユーザーが認証されていません。");
        }
        if (userToDelete != null)
        {
            if (userToDelete.Id == user.Id)
            {
                ModelState.AddModelError(string.Empty, "自分のアカウントを削除することはできません。");
                DisplayUser = userToDelete;
                return Page();
            }

            var result = await _userManager.DeleteAsync(userToDelete);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                DisplayUser = userToDelete;
                return Page();
            }
        }

        return RedirectToPage("./Index");
    }
}
