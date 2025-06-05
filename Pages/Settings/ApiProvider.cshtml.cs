using Chats.Data;
using Chats.Models.ApiProviders;
using Chats.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Chats.Pages.Settings;

/// <summary>
/// APIプロバイダー設定ページを処理するRazor Pageモデル。
/// Gemini APIの設定の表示と保存を行います。
/// </summary>
public class ApiProviderModel : PageModel
{
    private readonly ApiProviderService _apiProviderService;
    private readonly UserManager<User> _userManager;

    /// <summary>
    /// Gemini API設定の入力モデルを取得または設定します。
    /// このプロパティはHTTPリクエストからバインドされます。
    /// </summary>
    [BindProperty]
    public GeminiApiSettingsInputModel GeminiSettings { get; set; }

    /// <summary>
    /// ApiProviderModelクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="apiProviderService">APIプロバイダーサービス。</param>
    /// <param name="userManager">ユーザーマネージャー。</param>
    public ApiProviderModel(ApiProviderService apiProviderService, UserManager<User> userManager)
    {
        _apiProviderService = apiProviderService;
        _userManager = userManager;
    }

    /// <summary>
    /// ページがGETリクエストでアクセスされたときに呼び出されます。
    /// 既存のGemini API設定を読み込み、フォームに表示します。
    /// </summary>
    /// <returns>非同期操作を表すタスク。</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // ユーザーが認証されていない場合は400 Bad Requestを返します。
            return BadRequest("ユーザーが認証されていません。");
        }

        // "Gemini"プロバイダーの設定を取得します。
        var apiProviderSettings = await _apiProviderService.GetAsync("Gemini", user.Id);

        // GeminiSettingsを初期化します。
        GeminiSettings = new GeminiApiSettingsInputModel();
        if (apiProviderSettings != null)
        {
            // 認証情報が存在する場合、APIキーをデシリアライズして設定します。
            if (!string.IsNullOrEmpty(apiProviderSettings.Credential))
            {
                var geminiCredential = JsonSerializer.Deserialize<GeminiCredential>(apiProviderSettings.Credential);
                GeminiSettings.ApiKey = geminiCredential?.ApiKey ?? "";
            }
            // モデル情報が存在する場合、改行で区切られた文字列として設定します。
            if (!string.IsNullOrEmpty(apiProviderSettings.Model))
            {
                GeminiSettings.ModelText = apiProviderSettings.Model.Replace(",", Environment.NewLine);
            }
        }
        return Page();
    }

    /// <summary>
    /// ページがPOSTリクエストで送信されたときに呼び出されます。
    /// Gemini API設定を検証し、保存または削除します。
    /// </summary>
    /// <returns>非同期操作を表すIActionResult。</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // ユーザーが認証されていない場合は400 Bad Requestを返します。
            return BadRequest("ユーザーが認証されていません。");
        }

        // モデルの状態が無効な場合、ページを再表示します。
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // APIキーとモデルテキストが両方とも入力されている場合、設定を保存します。
        if (!string.IsNullOrEmpty(GeminiSettings.ApiKey) && !string.IsNullOrEmpty(GeminiSettings.ModelText))
        {
            // 新しいApiProviderオブジェクトを作成します。
            var apiProvider = new ApiProvider
            {
                ProviderName = "Gemini",
                UserId = user.Id, // ユーザーIDを設定
                // GeminiCredentialをJSONとしてシリアライズします。
                Credential = JsonSerializer.Serialize(new GeminiCredential { ApiKey = GeminiSettings.ApiKey }),
                // モデルテキストを改行で分割し、トリムしてカンマ区切りで結合します。
                Model = string.Join(",",
                    GeminiSettings.ModelText.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim())
                                   .Where(s => !string.IsNullOrWhiteSpace(s)))
            };
            // 設定を保存します。
            await _apiProviderService.SaveAsync(apiProvider);
        }
        else
        {
            // APIキーまたはモデルテキストが空の場合、既存のGemini設定を削除します。
            await _apiProviderService.DeleteAsync("Gemini", user.Id); // ユーザーIDを指定して削除
        }

        // 成功メッセージをTempDataに設定します。
        TempData["Message"] = "APIプロバイダー設定が正常に保存されました。";
        // 現在のページにリダイレクトします。
        return RedirectToPage();
    }
}
