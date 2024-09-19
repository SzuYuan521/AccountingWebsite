using AccountingWebsite.Data;
using AccountingWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
