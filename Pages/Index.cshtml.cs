using Chats.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _dbContext;

    public List<AiPersonalitySettings> AiPersonalities { get; set; } = new();

    /// <summary>
    /// IndexModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="logger">ロガーインスタンス。</param>
    /// <param name="dbContext">データベースコンテキストインスタンス。</param>
    public IndexModel(ILogger<IndexModel> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// HTTP GETリクエストがページに送信されたときに呼び出されます。
    /// </summary>
    public async Task OnGet()
    {
        AiPersonalities = await _dbContext.AiPersonalitySettings.ToListAsync();
    }
}
