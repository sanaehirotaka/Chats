using Azure.Core;
using Chats.Models;
using Chats.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace Chats.Controllers;

/// <summary>
/// AIを使用してキャラクタープロンプトを生成するためのAPIエンドポイントを提供します。
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CharaGeneratorController : ControllerBase
{
    private readonly string Prompt1 = @"
チャットボットが特定のキャラクターになりきるためのベースになる設定を作成して欲しいです。

${request}

---

### 1. 基本情報

*   **名前**：
*   **年齢**： (具体的な年齢)
*   **性別**： (男、女、不明、中性、その他)
*   **一人称/二人称**： (例：私/あなた、俺/お前、僕/君、わたくし/貴方、など)

### 2. パーソナリティ（内面）

*   **性格**：

### 3. 外見的特徴

*   **容姿**：

### 4. コミュニケーションスタイル

*   **口調/語尾**： (例：丁寧語、タメ口、古風な言葉遣い、専門用語が多い、スラングを使う、特定の語尾（〜だね、〜ですわ、〜じゃ、〜っス）など)

### 5. 背景設定

*   **職業/役割/身分**： (例：学生、会社員、冒険者、魔術師、店主、専業主婦、教師、兵士、など)

### 6. チャットボットとしての振る舞い設定（運用面）

*   **ユーザーとの関係性**： (例：友人、恋人、先生、先輩、後輩、相談相手、主従関係、ファンとアイドル、など。キャラクターがユーザーに対してどのような立場で振る舞うか)

### 7. その他

*   **その他**： (あれば)
";

    private readonly string Prompt2 = @"
このキャラクター設定をベースに以下の要素を明確にしたシステムプロンプトを作成して。

1.  **AIの役割（ペルソナ）設定**: AIが誰になりきるのか、その名前、職業、年齢、性格、背景など。
2.  **口調・話し方**: 一人称、二人称、語尾、特徴的なフレーズ、声のトーン（文章で表現）。
3.  **ユーザーとの関係性**: ユーザーをどう認識し、どう接するか（友人、顧客、生徒、見知らぬ人など）。
4.  **対話の目的・目標**: このロールプレイを通じて何を達成したいのか。
5.  **制約・禁止事項**: ロールプレイ中に守るべきルールや、行ってはいけないこと。
6.  **現在の状況・シナリオ**: ロールプレイが始まる時点の背景設定。
7.  **初回応答の指示**: ロールプレイの導入として、最初の発言で何をすべきか。
";
    private readonly ChatCompletionService _chatCompletionService;

    /// <summary>
    /// <see cref="CharaGeneratorController"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="chatCompletionService">チャット補完サービス。</param>
    public CharaGeneratorController(ChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;
    }

    /// <summary>
    /// ユーザーからのチャットリクエストを処理し、応答を返します。
    /// </summary>
    /// <param name="request">チャットリクエスト。</param>
    /// <returns>チャットメッセージのリストを含むアクション結果。</returns>
    [HttpPost]
    public async Task<ActionResult<PromptModel?>> Random(PromptRequest request)
    {
        var model = await _chatCompletionService.GetAvaiableModel();
        var kernel = await _chatCompletionService.GetKernelAsync(model.Provider, model.Model);
        if (kernel == null)
        {
            return Problem("Failed to get Kernel.");
        }
        var chatHistory = new ChatHistory();

        string prompt = "次の項目をランダムに埋めてください。";
        if (!string.IsNullOrEmpty(request.Request))
        {
            prompt = $"次の要素を取り入れてください: `{request.Request}`";
        }

        chatHistory.AddUserMessage(Prompt1.Replace("${request}", prompt));
        chatHistory.AddRange(await _chatCompletionService.GetChatMessageContentsAsync(kernel, chatHistory, _chatCompletionService.GetPromptExecutionSettings(model.Provider!)));
        chatHistory.AddUserMessage(Prompt2);
        var result = await _chatCompletionService.GetChatMessageContentsAsync(kernel, chatHistory, _chatCompletionService.GetPromptExecutionSettings(model.Provider!, typeof(PromptModel)));

        var generateJson = result.LastOrDefault(m => m.Role == AuthorRole.Assistant)?.Content;
        return generateJson != null ? JsonSerializer.Deserialize<PromptModel>(generateJson) ?? null : null;
    }

    /// <summary>
    /// キャラクタープロンプト生成のリクエストを表します。
    /// </summary>
    public class PromptRequest
    {
        /// <summary>
        /// キャラクター生成に対するユーザーのリクエストを取得または設定します。
        /// </summary>
        public string? Request { get; set; }
    }

    /// <summary>
    /// 生成されたキャラクタープロンプトモデルを表します。
    /// </summary>
    public class PromptModel
    {
        /// <summary>
        /// キャラクター名を取得または設定します。
        /// </summary>
        [System.ComponentModel.Description("Character Name (plain text)")]
        public string CharactorName { get; set; } = default!;

        [System.ComponentModel.Description("System Prompt (markdowm format)")]
        public string SystemPrompt { get; set; } = default!;
    }
}
