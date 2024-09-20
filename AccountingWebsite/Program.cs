using AccountingWebsite.Data;
using AccountingWebsite.Models;
using AccountingWebsite.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 配置ApplicationDbContext 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 配置身份驗證
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";  // 允許的用戶名字符

    // 密碼設置
    options.Password.RequireDigit = true; // 密碼必須包含數字
    options.Password.RequireLowercase = true; // 密碼必須包含小寫字母
    options.Password.RequireUppercase = false; // 密碼必須包含大寫字母
    options.Password.RequireNonAlphanumeric = false; // 密碼必須包含非字母數字字符
    options.Password.RequiredLength = 6; // 密碼最小長度
    options.Password.RequiredUniqueChars = 1; // 密碼必須包含唯一字符數量

    // 帳號鎖定設置
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 帳號鎖定時間
    options.Lockout.MaxFailedAccessAttempts = 5; // 最大失敗登錄嘗試次數
    options.Lockout.AllowedForNewUsers = true; // 新用戶是否適用鎖定策略

    // 驗證設置
    options.User.RequireUniqueEmail = true; // 用戶必須擁有唯一的電子郵件

    // 2FA設置
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider; // 2FA的令牌提供者
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));

// 確保 JwtService 被正確註冊
builder.Services.AddScoped<JwtService>();

// 配置JWT認證
builder.Services.AddAuthentication(options =>
{
    // 設置默認的認證模式為 JWT 認證
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // 配置 JWT Bearer 認證選項
    options.RequireHttpsMetadata = false; //
    options.SaveToken = true; // 生成的 Token 保存到 HttpContext 中

    // 配置 JWT Token 的驗證參數
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true, // 驗證 Token 有效期
        ValidateIssuerSigningKey = true, // 驗證 Token 簽名密鑰
        IssuerSigningKey = key, // 使用 Key 進行 Token 簽名驗證
        ClockSkew = TimeSpan.Zero // 設置 Token 的過期時間允許的時間偏移量為0
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["jwt"];
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
