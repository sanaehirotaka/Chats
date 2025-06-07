using Chats.Models.ApiProviders;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Chats.Services;

/// <summary>
/// さまざまなAIモデルを使用したチャット補完サービスを提供します。
/// </summary>
public class ChatCompletionService
{
    private readonly ApiProviderService _apiProviderService;

    /// <summary>
    /// ChatCompletionServiceクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="apiProviderService">APIプロバイダーサービス。</param>
    public ChatCompletionService(ApiProviderService apiProviderService)
    {
        _apiProviderService = apiProviderService;
    }

    /// <summary>
    /// 指定された、またはデフォルトのプロバイダー名とモデル名に基づいて、利用可能なチャット補完モデルを取得します。
    /// </summary>
    /// <param name="providerName">オプション。APIプロバイダーの名前。</param>
    /// <param name="modelName">オプション。モデルの名前。</param>
    /// <returns>プロバイダー名とモデル名を含むタプル。利用可能なモデルがない場合は (null, null)。</returns>
    public async Task<(string? Provider, string? Model)> GetAvaiableModel(string? providerName = null, string? modelName = null)
    {
        var providers = await _apiProviderService.GetProvidersByLoginUser().ToListAsync();
        var provider = providers.SingleOrDefault(p => p.ProviderName == providerName) ?? providers.FirstOrDefault();

        if (provider == null)
        {
            return (null, null);
        }

        var model = provider.Models.FirstOrDefault(m => m == modelName) ?? provider.Models.FirstOrDefault();

        if (model == null)
        {
            return (null, null);
        }

        return (Provider: provider.ProviderName, Model: model);
    }

    /// <summary>
    /// 指定されたプロバイダー名とモデル名に基づいてSemantic Kernelインスタンスを非同期に取得します。
    /// </summary>
    /// <param name="providerName">APIプロバイダーの名前。</param>
    /// <param name="modelName">使用するモデルの名前。</param>
    /// <returns>設定されたKernelインスタンス、または取得に失敗した場合はnull。</returns>
    public async Task<Kernel?> GetKernelAsync(string? providerName = null, string? modelName = null)
    {
        var providers = await _apiProviderService.GetProvidersByLoginUser().ToListAsync();
        var provider = providers.SingleOrDefault(p => p.ProviderName == providerName) ?? providers.FirstOrDefault();

        if (provider == null)
        {
            return null;
        }

        var model = provider.Models.FirstOrDefault(m => m == modelName) ?? provider.Models.FirstOrDefault();

        if (model == null)
        {
            return null;
        }

        var builder = Kernel.CreateBuilder();
        switch (provider.ProviderName)
        {
            case "Gemini":
                builder.AddGoogleAIGeminiChatCompletion(
                    modelId: model,
                    apiKey: provider.GetCredential<GeminiCredential>()!.ApiKey
                );
                break;
            default:
                return null;
        }
        return builder.Build();
    }

    /// <summary>
    /// 指定されたプロバイダー名とスキーマに基づいてプロンプト実行設定を取得します。
    /// </summary>
    /// <param name="providerName">APIプロバイダーの名前。</param>
    /// <param name="schema">応答スキーマの型（オプション）。</param>
    /// <returns>プロンプト実行設定。</returns>
    public PromptExecutionSettings GetPromptExecutionSettings(string providerName, Type? schema = null)
    {
        switch (providerName)
        {
            case "Gemini":
                var gemini = new GeminiPromptExecutionSettings()
                {
                    SafetySettings = [
                        new (GeminiSafetyCategory.Harassment, GeminiSafetyThreshold.BlockNone),
                        new (GeminiSafetyCategory.DangerousContent, GeminiSafetyThreshold.BlockNone),
                        new (GeminiSafetyCategory.SexuallyExplicit, GeminiSafetyThreshold.BlockNone)
                    ]
                };
                if (schema != null)
                {
                    gemini.ResponseMimeType = "application/json";
                    gemini.ResponseSchema = schema;
                }
                return gemini;
            default:
                throw new ArgumentOutOfRangeException(nameof(providerName), $"Unsupported provider: {providerName}");
        }
    }

    /// <summary>
    /// チャットメッセージの内容を非同期的に取得します。
    /// </summary>
    /// <param name="kernel">Semantic Kernelインスタンス。</param>
    /// <param name="chatHistory">現在のチャット履歴。</param>
    /// <param name="promptExecutionSettings">プロンプト実行設定。</param>
    /// <returns>チャット履歴。</returns>
    public async Task<ChatHistory> GetChatMessageContentsAsync(Kernel kernel, ChatHistory chatHistory, PromptExecutionSettings promptExecutionSettings)
    {
        var inputChatHistory = new ChatHistory(chatHistory);
        var resultChatHistory = new ChatHistory();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        while (!resultChatHistory.Any(m => "stop" == m?.Metadata?["FinishReason"]?.ToString()?.ToLower()))
        {
            var result = await chatCompletionService.GetChatMessageContentsAsync(inputChatHistory, promptExecutionSettings, kernel);
            resultChatHistory.AddRange(result);
            inputChatHistory.AddRange(result);
        }
        return resultChatHistory;
    }

}
