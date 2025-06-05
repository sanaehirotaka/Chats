using Chats.Data;
using Chats.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// アプリケーションのエントリポイントとサービス設定を定義するクラス。
/// </summary>
public class Program
{
    /// <summary>
    /// アプリケーションのメインメソッド。
    /// </summary>
    /// <param name="args">コマンドライン引数。</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // データベースコンテキストをサービスに追加します。SQLiteを使用し、DefaultConnectionから接続文字列を取得します。
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // AppSettingsセクションから設定を読み込み、AppSettingsオブジェクトとして構成します。
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

        builder.Services
            .AddScoped<ApiProviderService>()
            .AddScoped<ChatCompletionService>();

        // MVCサービスとセッション状態の一時データプロバイダーを追加します。
        builder.Services
            .AddMvc()
            .AddSessionStateTempDataProvider();
        // セッションサービスを追加します。
        builder.Services
            .AddSession();

        // ASP.NET Core Identityサービスを設定します。
        builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // アカウント確認を必須としない設定。
                options.SignIn.RequireConfirmedAccount = false;
                // ユーザー名に使用できる文字の制限を解除します。
                options.User.AllowedUserNameCharacters = "";
            })
            // データベースストアとしてAppDbContextを使用します。
            .AddEntityFrameworkStores<AppDbContext>()
            // カスタムクレームプリンシパルファクトリを追加します。
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
            // デフォルトのトークンプロバイダーを追加します。
            .AddDefaultTokenProviders();

        // Identityのパスワード設定を構成します。
        builder.Services.Configure<IdentityOptions>(options =>
        {
            // パスワードの最小長を設定ファイルから取得します。
            options.Password.RequiredLength = builder.Configuration.GetValue<int>("Identity:Password:RequiredLength");
            // パスワードに数字を必須とするか設定ファイルから取得します。
            options.Password.RequireDigit = builder.Configuration.GetValue<bool>("Identity:Password:RequireDigit");
            // パスワードに小文字を必須とするか設定ファイルから取得します。
            options.Password.RequireLowercase = builder.Configuration.GetValue<bool>("Identity:Password:RequireLowercase");
            // パスワードに大文字を必須とするか設定ファイルから取得します。
            options.Password.RequireUppercase = builder.Configuration.GetValue<bool>("Identity:Password:RequireUppercase");
            // パスワードに英数字以外の文字を必須とするか設定ファイルから取得します。
            options.Password.RequireNonAlphanumeric = builder.Configuration.GetValue<bool>("Identity:Password:RequireNonAlphanumeric");
        });

        // Google認証が環境変数で設定されている場合、Google認証を追加します。
        if (Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_ID") != null && Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_SECRET") != null)
        {
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    // GoogleクライアントIDを環境変数から取得します。
                    options.ClientId = Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_ID");
                    // Googleクライアントシークレットを環境変数から取得します。
                    options.ClientSecret = Environment.GetEnvironmentVariable("AUTH_GOOGLE_CLIENT_SECRET");
                });
        }

        // すべてのRazor Pagesにグローバルな承認フィルターを追加します。
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");
        });

        // アプリケーションクッキーの設定を構成します。
        builder.Services.ConfigureApplicationCookie(options =>
        {
            // ログインページのパスを設定します。
            options.LoginPath = "/Account/Login";
            // アクセス拒否ページのパスを設定します。
            options.AccessDeniedPath = "/Account/AccessDenied";
            // クッキーの有効期限を1時間に設定します。
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            // スライディング有効期限を有効にします。
            options.SlidingExpiration = true;
        });

        var app = builder.Build();

        // HTTPリクエストパイプラインを設定します。
        if (!app.Environment.IsDevelopment())
        {
            // 非開発環境でカスタムエラーハンドラページを使用します。
            app.UseExceptionHandler("/Error");
            // HSTS（HTTP Strict Transport Security）を有効にします。
            app.UseHsts();
        }

        // HTTPSリダイレクトを有効にします。
        app.UseHttpsRedirection();
        // 静的ファイル（CSS、JavaScript、画像など）の提供を有効にします。
        app.UseStaticFiles();
        // ルーティングを有効にします。
        app.UseRouting();
        // セッションミドルウェアを有効にします。
        app.UseSession();
        // 認証ミドルウェアを有効にします。
        app.UseAuthentication();
        // 承認ミドルウェアを有効にします。
        app.UseAuthorization();
        // Razor Pagesのエンドポイントをマップします。
        app.MapRazorPages();
        // デフォルトのコントローラールートをマップします。
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}");

        // アプリケーション起動時に保留中のデータベースマイグレーションを適用します。
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                // DbContextの保留中のマイグレーションを適用します。
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                // データベースマイグレーション中に発生したエラーをログに記録します。
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }

        // アプリケーションを実行し、PORT環境変数で指定されたポートをリッスンします。指定がない場合はデフォルトを使用します。
        app.Run(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")) ? null : $"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
