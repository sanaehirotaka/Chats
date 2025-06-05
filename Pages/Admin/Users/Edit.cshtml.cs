using Chats.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Chats.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public EditModel(UserManager<User> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        public string Id { get; set; } = default!;

        [Required]
        [EmailAddress]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = default!;

        [Display(Name = "表示名")]
        public string? DisplayName { get; set; }

        [Required]
        [Display(Name = "役割")]
        public UserRole Role { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Role = user.Role
        };
        return Page();
    }

    [TempData]
    public string? GeneratedPassword { get; set; }

    public async Task<IActionResult> OnPostResetPasswordAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Generate a new password
        var newPassword = GenerateRandomPassword(); // Implement this helper method
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            GeneratedPassword = newPassword;
            return new JsonResult(new { success = true, newPassword = newPassword });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
    }

    private string GenerateRandomPassword()
    {
        // This is a simple example. In a real application, use a strong password generation library.
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByIdAsync(Input.Id);
        if (user == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            // ユーザーが認証されていない場合は400 Bad Requestを返します。
            return BadRequest("ユーザーが認証されていません。");
        }

        user.Email = Input.Email;
        user.UserName = Input.Email;
        user.DisplayName = Input.DisplayName;

        if (user.Id == currentUser.Id)
        {
            if (user.Role != Input.Role)
            {
                ModelState.AddModelError(string.Empty, "自分の役割を変更することはできません。");
                return Page();
            }
        }
        else
        {
            user.Role = Input.Role;
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return RedirectToPage("./Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
