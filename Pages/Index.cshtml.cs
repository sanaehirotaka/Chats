using Chats.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public List<AiPersonalitySettings> AiPersonalities { get; set; } = new();

    /// <summary>
    /// IndexModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="logger">ロガーインスタンス。</param>
    /// <param name="dbContext">データベースコンテキストインスタンス。</param>
    public IndexModel(ILogger<IndexModel> logger, AppDbContext dbContext, UserManager<User> userManager)
    {
        _logger = logger;
        _dbContext = dbContext;
        _userManager = userManager;
    }

    /// <summary>
    /// HTTP GETリクエストがページに送信されたときに呼び出されます。
    /// </summary>
    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        AiPersonalities = await _dbContext.AiPersonalitySettings
                                            .Where(a => a.UserId == user.Id)
                                            .ToListAsync();

        return Page();
    }
}
