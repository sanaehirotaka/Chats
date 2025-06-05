namespace Chats.Models.ApiProviders
{
    /// <summary>
    /// Gemini APIの認証情報を表すクラス。
    /// </summary>
    public class GeminiCredential
    {
        /// <summary>
        /// Gemini APIキーを取得または設定します。
        /// </summary>
        public string ApiKey { get; set; } = default!;
    }
}
