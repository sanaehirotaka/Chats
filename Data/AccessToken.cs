using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chats.Data
{
    /// <summary>
    /// ユーザー認証とAPIアクセス用のアクセストークンを表します。
    /// </summary>
    public class AccessToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = default!;

        [Required]
        public string UserId { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = default!;
    }
}
