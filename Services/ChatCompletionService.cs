using Chats.Data;
using Chats.Models.ApiProviders;
using Microsoft.AspNetCore.Identity;
using Microsoft.SemanticKernel;
using System.Security.Claims;
using System.Text.Json;

namespace Chats.Services;

public class ChatCompletionService
{
    private IHttpContextAccessor _httpContextAccessor;

    private ApiProviderService _apiProviderService;

    private UserManager<User> _userManager;

    public ChatCompletionService(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager, ApiProviderService apiProviderService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _apiProviderService = apiProviderService;
    }

    private ClaimsPrincipal? GetCurrentUserClaimsPrincipal()
    {
        return _httpContextAccessor.HttpContext?.User;
    }

    public async Task<Kernel?> GetKernel()
    {
        var user = GetCurrentUserClaimsPrincipal();
        if (user == null)
        {
            return null;
        }
        var userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            return null;
        }
        var provider = await _apiProviderService.GetAsync("Gemini", userId);

        if (provider == null)
        {
            return null;
        }

        var builder = Kernel.CreateBuilder();

        builder.AddGoogleAIGeminiChatCompletion(
            modelId: provider.Model.Split(",")[0],
            apiKey: JsonSerializer.Deserialize<GeminiCredential>(provider.Credential)!.ApiKey
        );

        return builder.Build();
    }
}
