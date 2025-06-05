using Chats.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Chats.Pages.Settings;

public class ProfileModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<ProfileModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [StringLength(50)]
        [Required(ErrorMessage = "表示名は必須です。")]
        [Display(Name = "表示名")]
        public string DisplayName { get; set; } = default!;
    }

    [Display(Name = "メールアドレス")]
    public string? Email { get; set; }

    private async Task LoadAsync(User user)
    {
        Input.DisplayName ??= user.DisplayName!;
        Email ??= await _userManager.GetEmailAsync(user);
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

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest("ユーザーが認証されていません。");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        user.DisplayName = Input.DisplayName;
        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest("ユーザーが認証されていません。");
        }

        StatusMessage = "プロフィールが更新されました。";
        await LoadAsync(user);
        return Page();
    }
}
