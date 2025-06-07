using Chats.Data;
using Chats.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly UserManager<User> _userManager;

    /// <summary>
    /// ApiProviderServiceクラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="context">アプリケーションのデータベースコンテキスト。</param>
    public ApiProviderService(AppDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
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
    /// 指定されたユーザーIDのすべてのAPIプロバイダー設定を非同期に取得します。
    /// </summary>
    /// <param name="userId">ユーザーのID。</param>
    /// <returns>指定されたユーザーIDのApiProviderオブジェクトのコレクション。</returns>
    public async Task<IEnumerable<ApiProvider>> GetAllAsync(string userId)
    {
        return await _context.ApiProviders.Where(p => p.UserId == userId).ToListAsync();
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

    /// <summary>
    /// ログインユーザーのAPIプロバイダー設定を非同期ストリームとして取得します。
    /// </summary>
    /// <returns>ApiProviderModelオブジェクトの非同期列挙。</returns>
    public async IAsyncEnumerable<ApiProviderModel> GetProvidersByLoginUser()
    {
        var user = GetCurrentUserClaimsPrincipal();
        if (user == null)
        {
            yield break;
        }
        var userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            yield break;
        }
        var providers = await GetAllAsync(userId);

        foreach (var provider in providers)
        {
            yield return new ApiProviderModel(provider);
        }
    }
    /// <summary>
    /// 現在ログインしているユーザーのClaimsPrincipalを取得します。
    /// </summary>
    /// <returns>現在のユーザーのClaimsPrincipal、またはユーザーがログインしていない場合はnull。</returns>
    private ClaimsPrincipal? GetCurrentUserClaimsPrincipal()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}
