using Chats.Data;
using Microsoft.EntityFrameworkCore;

namespace Chats.Services;

/// <summary>
/// APIプロバイダーの設定を管理するためのサービス。
/// データベースとのやり取りを担当します。
/// </summary>
public class ApiProviderService
{
    /// <summary>
    /// アプリケーションのデータベースコンテキスト。
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// ApiProviderServiceクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="context">アプリケーションのデータベースコンテキスト。</param>
    public ApiProviderService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 指定されたプロバイダー名のAPIプロバイダー設定を非同期に取得します。
    /// </summary>
    /// <param name="providerName">取得するAPIプロバイダーの名前。</param>
    /// <param name="userId">ユーザーのID。</param>
    /// <returns>指定されたプロバイダー名とユーザーIDのApiProviderオブジェクト、または見つからない場合はnull。</returns>
    public async Task<ApiProvider?> GetAsync(string providerName, string userId)
    {
        return await _context.ApiProviders.FirstOrDefaultAsync(p => p.ProviderName == providerName && p.UserId == userId);
    }

    /// <summary>
    /// 指定されたプロバイダー名とユーザーIDのAPIプロバイダー設定を非同期に削除します。
    /// </summary>
    /// <param name="providerName">削除するAPIプロバイダーの名前。</param>
    /// <param name="userId">ユーザーのID。</param>
    /// <returns>非同期操作を表すタスク。</returns>
    public async Task DeleteAsync(string providerName, string userId)
    {
        var entity = await _context.ApiProviders.FirstOrDefaultAsync(p => p.ProviderName == providerName && p.UserId == userId);
        if (entity != null)
        {
            _context.ApiProviders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// APIプロバイダー設定を非同期に保存します。
    /// 既存の設定がある場合は更新し、ない場合は新規追加します。
    /// </summary>
    /// <param name="settings">保存するApiProviderオブジェクト。</param>
    /// <returns>非同期操作を表すタスク。</returns>
    public async Task SaveAsync(ApiProvider settings)
    {
        var existingSettings = await _context.ApiProviders.FirstOrDefaultAsync(p => p.ProviderName == settings.ProviderName && p.UserId == settings.UserId);

        if (existingSettings == null)
        {
            _context.ApiProviders.Add(settings);
        }
        else
        {
            existingSettings.Credential = settings.Credential;
            existingSettings.Model = settings.Model;
            _context.ApiProviders.Update(existingSettings);
        }

        await _context.SaveChangesAsync();
    }
}
