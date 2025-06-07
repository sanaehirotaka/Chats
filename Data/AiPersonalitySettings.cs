using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chats.Data
{
    /// <summary>
    /// ユーザーが定義するAIパーソナリティ設定を表します。
    /// </summary>
    public class AiPersonalitySettings
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; } = default!;

        [Display(Name = "説明")]
        public string Description { get; set; } = default!;

        [Required]
        [Display(Name = "システムプロンプト")]
        public string SystemPrompt { get; set; } = default!;

        // Foreign key for User
        public string UserId { get; set; } = default!;

        [ForeignKey("UserId")]
        public User User { get; set; } = default!;
    }
}
