namespace Chats.Models;

/// <summary>
/// チャット補完のリクエストを表します。
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// チャットのシステムプロンプトを取得または設定します。
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// チャットメッセージのリストを取得または設定します。
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// 使用するAPIプロバイダーの名前を取得または設定します。
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 使用するモデルの名前を取得または設定します。
    /// </summary>
    public string? Model { get; set; }
}
