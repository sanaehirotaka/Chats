namespace Chats.Models
{
    /// <summary>
    /// チャット会話内の単一のメッセージを表します。
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// メッセージのタイプ（例: "user", "assistant", "system"）を取得または設定します。
        /// </summary>
        public string Type { get; set; } = default!;
        /// <summary>
        /// メッセージの内容を取得または設定します。
        /// </summary>
        public string Content { get; set; } = default!;
    }
}
