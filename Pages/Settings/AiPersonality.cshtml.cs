using Chats.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Chats.Pages.Settings
{
    public class AiPersonalityModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public AiPersonalityModel(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<InputModel> AiPersonalities { get; set; }

        public class InputModel
        {
            public string? Id { get; set; }

            [Required(ErrorMessage = "名前は必須です。")]
            [Display(Name = "名前")]
            public string Name { get; set; } = default!;

            [Required(ErrorMessage = "自己紹介は必須です。")]
            [Display(Name = "自己紹介")]
            public string Description { get; set; } = default!;

            [Required(ErrorMessage = "システムプロンプトは必須です。")]
            [Display(Name = "システムプロンプト")]
            public string SystemPrompt { get; set; } = default!;
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var records = await _context.AiPersonalitySettings
                                            .Where(a => a.UserId == user.Id)
                                            .OrderBy(a => a.Name)
                                            .ToListAsync();

            AiPersonalities = records.Select(r => new InputModel()
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                SystemPrompt = r.SystemPrompt,
            }).ToList();

            Input = AiPersonalities.FirstOrDefault(a => a.Id == id) ?? new();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var records = await _context.AiPersonalitySettings
                                            .Where(a => a.UserId == user.Id)
                                            .OrderBy(a => a.Name)
                                            .ToListAsync();

            AiPersonalities = records.Select(r => new InputModel()
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                SystemPrompt = r.SystemPrompt,
            }).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            AiPersonalitySettings personalityToSave;

            if (Input.Id == null)
            {
                personalityToSave = new AiPersonalitySettings
                {
                    UserId = user.Id,
                    Name = Input.Name,
                    Description = Input.Description,
                    SystemPrompt = Input.SystemPrompt,
                };
                _context.AiPersonalitySettings.Add(personalityToSave);
            }
            else
            {
                personalityToSave = await _context.AiPersonalitySettings
                                                .Where(a => a.Id == Input.Id && a.UserId == user.Id)
                                                .FirstAsync();
                if (personalityToSave == null)
                {
                    return NotFound();
                }
                personalityToSave.Name = Input.Name;
                personalityToSave.Description = Input.Description;
                personalityToSave.SystemPrompt = Input.SystemPrompt;
                _context.Attach(personalityToSave).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AiPersonalityExists(personalityToSave.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./AiPersonality", new { id = personalityToSave.Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var personalityToDelete = await _context.AiPersonalitySettings
                                                    .Where(a => a.Id == id && a.UserId == user.Id)
                                                    .FirstOrDefaultAsync();

            if (personalityToDelete == null)
            {
                return NotFound();
            }

            _context.AiPersonalitySettings.Remove(personalityToDelete);
            await _context.SaveChangesAsync();

            return RedirectToPage("./AiPersonality");
        }

        private bool AiPersonalityExists(string id)
        {
            return _context.AiPersonalitySettings.Any(e => e.Id == id);
        }
    }
}
