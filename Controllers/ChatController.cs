using Chats.Models;
using Chats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Chats.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private ChatCompletionService _chatCompletionService;

    public ChatController(ChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;
    }

    [HttpPost]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> Post([FromBody] IList<ChatMessage> messages)
    {
        var kernel = await _chatCompletionService.GetKernel();
        if (kernel == null)
        {
            return Problem("Failed to get Kernel.");
        }

        var chatHistory = new ChatHistory();
        foreach (var message in messages)
        {
            if (message.Type == "user")
            {
                chatHistory.AddUserMessage(message.Content);
            }
            else
            {
                chatHistory.AddAssistantMessage(message.Content);
            }
        }

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        while (!chatHistory.Any(m => "stop" == m?.Metadata?["FinishReason"]?.ToString()?.ToLower()))
        {
            var result = await chatCompletionService.GetChatMessageContentsAsync(chatHistory, new GeminiPromptExecutionSettings(), kernel);

            foreach (var message in result)
            {
                if (message.Role == AuthorRole.Assistant)
                {
                    messages.Add(new()
                    {
                        Content = message.Content ?? "",
                        Type = "assistant"
                    });
                }
            }
            chatHistory.AddRange(result);
        }

        return Ok(messages);
    }
}
