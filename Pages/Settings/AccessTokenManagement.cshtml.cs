using Chats.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Chats.Pages.Settings;

public class AccessTokenManagementModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AccessTokenManagementModel> _logger;
    private readonly AppDbContext _context;

    public AccessTokenManagementModel(UserManager<User> userManager, ILogger<AccessTokenManagementModel> logger, AppDbContext context)
    {
        _userManager = userManager;
        _logger = logger;
        _context = context;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public IList<AccessToken> AccessTokens { get; set; } = [];

    private async Task LoadAsync(User user)
    {
        AccessTokens = await _context.AccessTokens
            .Where(at => at.UserId == user.Id)
            .OrderByDescending(at => at.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest("ユーザーが認証されていません。");
        }
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostGenerateAccessTokenAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest("ユーザーが認証されていません。");
        }

        var randomNumber = new byte[48];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        var newToken = Convert.ToBase64String(randomNumber)
                            .Replace('+', '-')
                            .Replace('/', '_')
                            .Replace("=", "");

        var accessToken = new AccessToken
        {
            Token = newToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };

        _context.AccessTokens.Add(accessToken);
        await _context.SaveChangesAsync();

        StatusMessage = "新しいアクセストークンが発行されました。";
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostRevokeAccessTokenAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest("ユーザーが認証されていません。");
        }

        var accessToken = await _context.AccessTokens.FirstOrDefaultAsync(at => at.Id == id && at.UserId == user.Id);

        if (accessToken == null)
        {
            StatusMessage = "エラー: 指定されたアクセストークンが見つからないか、権限がありません。";
            await LoadAsync(user);
            return Page();
        }

        _context.AccessTokens.Remove(accessToken);
        await _context.SaveChangesAsync();

        StatusMessage = "アクセストークンが破棄されました。";
        await LoadAsync(user);
        return Page();
    }
}
