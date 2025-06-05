using Chats.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Chats.Pages.Settings;

public class PasswordChangeModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<PasswordChangeModel> _logger;

    public PasswordChangeModel(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<PasswordChangeModel> logger)
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
        [DataType(DataType.Password)]
        [Display(Name = "現在のパスワード")]
        public string? OldPassword { get; set; }

        [StringLength(512, ErrorMessage = "{0}は{2}文字以上{1}文字以下である必要があります。", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Display(Name = "新しいパスワード")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "新しいパスワードの確認")]
        [Compare("NewPassword", ErrorMessage = "新しいパスワードと確認用パスワードが一致しません。")]
        public string? ConfirmPassword { get; set; }
    }

    private async Task LoadAsync(User user)
    {
        // No specific data to load for password change, but keep the method for consistency
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

    public async Task<IActionResult> OnPostChangePasswordAsync()
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

        var hasPassword = await _userManager.HasPasswordAsync(user);
        IdentityResult changePasswordResult;

        if (hasPassword)
        {
            if (string.IsNullOrEmpty(Input.OldPassword))
            {
                ModelState.AddModelError(string.Empty, "パスワードを変更するには現在のパスワードが必要です。");
                await LoadAsync(user);
                return Page();
            }
            changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        }
        else
        {
            changePasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
        }

        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadAsync(user);
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "パスワードが変更されました。";
        await LoadAsync(user);
        return Page();
    }
}
