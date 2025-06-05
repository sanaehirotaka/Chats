using System.ComponentModel.DataAnnotations;

namespace Chats.Data
{
    /// <summary>
    /// APIプロバイダーの設定情報を表すデータモデル。
    /// データベースのApiProvidersテーブルに対応します。
    /// </summary>
    public class ApiProvider
    {
        /// <summary>
        /// APIプロバイダーの名前を取得または設定します。
        /// これは主キーとして機能します。
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// このAPIプロバイダー設定が関連付けられているユーザーのIDを取得または設定します。
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// このAPIプロバイダー設定が関連付けられているユーザーを取得または設定します。
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// APIプロバイダーの認証情報を取得または設定します。
        /// このフィールドは必須です。
        /// </summary>
        [Required]
        public string Credential { get; set; }

        /// <summary>
        /// APIプロバイダーが使用するモデルの識別子を取得または設定します。
        /// このフィールドは必須です。
        /// </summary>
        [Required]
        public string Model { get; set; }
    }
}
