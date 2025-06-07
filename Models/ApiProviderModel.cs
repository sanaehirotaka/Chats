using Chats.Data;
using System.Text.Json;

namespace Chats.Models;

/// <summary>
/// APIプロバイダー設定のモデルを表し、<see cref="ApiProvider"/> データエンティティをカプセル化します。
/// </summary>
public class ApiProviderModel
{
    private ApiProvider _apiProvider;
    /// <summary>
    /// 指定されたAPIプロバイダーを使用して、<see cref="ApiProviderModel"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="apiProvider"> <see cref="ApiProvider"/> データエンティティ。</param>
    public ApiProviderModel(ApiProvider apiProvider)
    {
        _apiProvider = apiProvider;
    }

    /// <summary>
    /// APIプロバイダーの名前
    /// </summary>
    public string ProviderName => _apiProvider.ProviderName;

    /// <summary>
    /// APIプロバイダーの認証情報を指定された型で取得します。
    /// </summary>
    /// <typeparam name="T">認証情報の型。</typeparam>
    /// <returns>逆シリアル化された認証情報。逆シリアル化に失敗した場合は <c>null</c> を返します。</returns>
    public T? GetCredential<T>()
    {
        if (string.IsNullOrEmpty(_apiProvider.Credential))
        {
            return default;
        }
        try
        {
            return JsonSerializer.Deserialize<T>(_apiProvider.Credential);
        }
        catch (JsonException)
        {
            // JSONの逆シリアル化に失敗した場合
            return default;
        }
        catch (NotSupportedException)
        {
            // 指定された型Tがサポートされていない場合
            return default;
        }
    }

    /// <summary>
    /// APIプロバイダーが使用するモデルの識別子。
    /// カンマ区切りの文字列として格納されており、このプロパティはそれを文字列のコレクションとして提供します。
    /// </summary>
    public IEnumerable<string> Models
    {
        get
        {
            if (string.IsNullOrEmpty(_apiProvider.Model))
            {
                return Enumerable.Empty<string>();
            }
            return _apiProvider.Model.Split(",", StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
