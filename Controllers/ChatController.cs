using Chats.Models;
using Chats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace Chats.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private ChatCompletionService _chatCompletionService;

    /// <summary>
    /// ChatControllerの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="chatCompletionService">チャット補完サービス。</param>
    public ChatController(ChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;
    }

    /// <summary>
    /// ユーザーからのチャットリクエストを処理し、応答を返します。
    /// </summary>
    /// <param name="request">チャットリクエスト。</param>
    /// <returns>チャットメッセージのリストを含むアクション結果。</returns>
    [HttpPost]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> Talk([FromBody] ChatRequest request)
    {
        var model = await _chatCompletionService.GetAvaiableModel(request.Provider, request.Model);
        var kernel = await _chatCompletionService.GetKernelAsync(model.Provider, model.Model);
        if (kernel == null)
        {
            return Problem("Failed to get Kernel.");
        }
        var chatHistory = ToChatHistory(request);
        if (!chatHistory.Any(m => m.Role == AuthorRole.User))
        {
            chatHistory.AddUserMessage("初対面のユーザーがあなたの目の前にいます。チャットを開始するためのあいさつ文を１つ返してください。");
        }
        var promptExecutionSettings = _chatCompletionService.GetPromptExecutionSettings(model.Provider!);
        var result = await _chatCompletionService.GetChatMessageContentsAsync(kernel, chatHistory, promptExecutionSettings);

        request.Messages.AddRange(result.Where(m => m.Role == AuthorRole.Assistant)
            .Select(m => new ChatMessage() { Type = "assistant", Content = m.Content ?? "" }));

        return Ok(request.Messages);
    }

    /// <summary>
    /// ユーザーが回答するための候補アクションを生成します。
    /// </summary>
    /// <param name="request">チャットリクエスト。</param>
    /// <returns>候補アクションのリストを含むアクション結果。</returns>
    [HttpPost]
    public async Task<ActionResult<List<string>>> ActionCandicate([FromBody] ChatRequest request)
    {
        var kernel = await _chatCompletionService.GetKernelAsync(request.Provider, request.Model);
        if (kernel == null)
        {
            return Problem("Failed to get Kernel.");
        }
        var chatHistory = ToChatHistory(request);
        var promptExecutionSettings = _chatCompletionService.GetPromptExecutionSettings(request.Provider, typeof(List<string>));

        if (chatHistory.Any(m => m.Role == AuthorRole.User))
        {
            chatHistory.AddUserMessage("今までのやり取りから、ユーザーが回答するための適切で簡潔な候補を3つ提示してください。 (アシスタントへ返信するためのショートカットボタンで使います)");
        }
        else
        {
            chatHistory.AddUserMessage("会話を開始するための話題を3つ提示してください。(例: ほうれん草を使ったレシピを教えて, 地球から月まで歩くと何年かかる？,プログラミングの効率的な学習方法を教えて) (アシスタントへ返信するためのショートカットボタンで使います)");
        }
        var result = await _chatCompletionService.GetChatMessageContentsAsync(kernel, chatHistory, promptExecutionSettings);

        var candicateStr = result.LastOrDefault(m => m.Role == AuthorRole.Assistant)?.Content;
        return candicateStr != null ? JsonSerializer.Deserialize<List<string>>(candicateStr) ?? [] : [];
    }

    /// <summary>
    /// ChatRequestオブジェクトをChatHistoryオブジェクトに変換します。
    /// </summary>
    /// <param name="request">変換するチャットリクエスト。</param>
    /// <returns>変換されたチャット履歴。</returns>
    private ChatHistory ToChatHistory(ChatRequest request)
    {
        var chatHistory = new ChatHistory();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            chatHistory.AddSystemMessage(request.SystemPrompt);
        }
        foreach (var message in request.Messages)
        {
            switch (message.Type)
            {
                case "system":
                    chatHistory.AddSystemMessage(message.Content);
                    break;
                case "user":
                    chatHistory.AddUserMessage(message.Content);
                    break;
                case "assistant":
                    chatHistory.AddAssistantMessage(message.Content);
                    break;
            }
        }
        return chatHistory;
    }
}
