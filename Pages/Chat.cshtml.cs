using Chats.Data;
using Chats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Chats.Pages;

public class ChatModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SelectedModel { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AiPersonalityId { get; set; }

    public OutputModel? AiPersonality { get; set; }

    [BindProperty]
    public IList<SelectListItem> AvaiableModels { get; set; } = [];

    private ApiProviderService _apiProviderService;
    private AppDbContext _context;

    public class OutputModel
    {
        public string Id { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string SystemPrompt { get; set; } = default!;
    }

    /// <summary>
    /// ChatModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="apiProviderService">APIプロバイダーサービス。</param>
    /// <param name="context">データベースコンテキスト。</param>
    public ChatModel(ApiProviderService apiProviderService, AppDbContext context)
    {
        _apiProviderService = apiProviderService;
        _context = context;
    }

    /// <summary>
    /// ページがリクエストされたときに呼び出されます。
    /// 利用可能なモデルをロードします。
    /// </summary>
    /// <returns>非同期操作を表すタスク。</returns>
    public async Task OnGetAsync()
    {
        var providers = await _apiProviderService.GetProvidersByLoginUser().ToListAsync();
        AvaiableModels = [.. providers.SelectMany(p => p.Models.Select(m => $"{p.ProviderName}/{m}")).Select(m => new SelectListItem(m, m))];
        SelectedModel ??= AvaiableModels.FirstOrDefault()?.Value;

        if (!string.IsNullOrEmpty(AiPersonalityId))
        {
            var aiPersonality = await _context.AiPersonalitySettings.FindAsync(AiPersonalityId);
            if (aiPersonality != null)
            {
                AiPersonality = new()
                {
                    Id = aiPersonality.Id,
                    Name = aiPersonality.Name,
                    Description = aiPersonality.Description,
                    SystemPrompt = aiPersonality.SystemPrompt 
                };
            }
        }
    }
}
