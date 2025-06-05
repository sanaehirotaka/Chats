using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Chats.Data;

namespace Chats.Data;

/// <summary>
/// アプリケーションのデータベースコンテキストを表し、IdentityDbContextを継承します。
/// このクラスは、基になるデータベースと対話し、エンティティ（ASP.NET Core Identityに必要なものを含む）を管理するために使用されます。
/// </summary>
public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<AccessToken> AccessTokens { get; set; }
    public DbSet<ApiProvider> ApiProviders { get; set; }

    /// <summary>
    /// <see cref="AppDbContext"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="options"><see cref="DbContext"/> で使用されるオプション。</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApiProviderエンティティの複合主キーを設定
        modelBuilder.Entity<ApiProvider>()
            .HasKey(ap => new { ap.ProviderName, ap.UserId });

        // ApiProviderとUser間のリレーションシップを設定
        modelBuilder.Entity<ApiProvider>()
            .HasOne(ap => ap.User)
            .WithMany()
            .HasForeignKey(ap => ap.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ユーザーが削除されたら関連するAPI設定も削除
    }

    /// <summary>
    /// 登録されているユーザーの数を非同期に取得します。
    /// </summary>
    /// <returns>登録されているユーザーの数。</returns>
    public async Task<int> GetUserCountAsync()
    {
        return await Users.CountAsync();
    }
}
