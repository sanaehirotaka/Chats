using System.ComponentModel.DataAnnotations;

namespace Chats.Models.ApiProviders
{
    /// <summary>
    /// Gemini API設定の入力モデルを表すクラス。
    /// 管理画面からのGemini API設定の入力に使用されます。
    /// </summary>
    public class GeminiApiSettingsInputModel
    {
        /// <summary>
        /// Gemini APIキーを取得または設定します。
        /// このフィールドは必須であり、"Gemini API Key"として表示されます。
        /// </summary>
        [Required(ErrorMessage = "API Keyは必須です。")]
        [Display(Name = "Gemini API Key")]
        public string ApiKey { get; set; }

        /// <summary>
        /// 使用するGeminiモデルのテキスト識別子を取得または設定します。
        /// このフィールドは必須であり、"Model"として表示されます。
        /// </summary>
        [Required(ErrorMessage = "モデルは必須です。")]
        [Display(Name = "Model")]
        public string ModelText { get; set; }
    }
}
