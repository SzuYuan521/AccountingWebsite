using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AccountingWebsite.Data;
using AccountingWebsite.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using AccountingWebsite.Services;
using System.Diagnostics;

namespace AccountingWebsite.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, JwtService jwtService, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("未登入");
            }

            return Content("已登入"); // 之後改成return 記帳畫面
        }

        // GET: Users/Details/5
        /// <summary>
        /// 顯示特定用戶的詳細信息
        /// </summary>
        /// <param name="id">id ? User View : NotFound</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        /// <summary>
        /// 顯示創建新用戶的表單
        /// </summary>
        /// <returns>創建View</returns>
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        /// <summary>
        /// 處理創建新User的請求
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns>success: User View, failed: Create View</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Email")] User user, string password)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(user);
        }

        // GET: Users/Edit/5
        /// <summary>
        /// 顯示編輯 User 資料的表單
        /// </summary>
        /// <param name="id"></param>
        /// <returns>檢查User Id 返回編輯 View</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        /// <summary>
        /// 處理 User 資料的編輯請求
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns>檢查id是否一樣並更新用戶信息, Success: User列表, Failed: 編輯View</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CreatedAt,Balance,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        /// <summary>
        /// 刪除 User 的 View
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        /// <summary>
        /// 處理刪除User的請求
        /// </summary>
        /// <param name="id"></param>
        /// <returns>刪除然後重導向User列表</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 檢查User是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult Login()
        {
            return View();
        }

        // 登入
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByNameAsync(model.UserName);
                if(user == null) 
                {
                    return Unauthorized("UserName錯誤");
                }

                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                
                if (result.Succeeded)
                {
                    var jwt = _jwtService.GenerateJWTToken(user);
                    Debug.WriteLine(jwt);
                    var expires = Convert.ToDouble(_configuration["Jwt:JwtTokenExpireMinutes"]);

                    Response.Cookies.Append("jwt", jwt, new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddMinutes(expires),
                        SameSite = SameSiteMode.Lax // 防止大部分 CSRF
                    });

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "登入失敗，請檢查帳號和密碼");
            }
            return View(model);
        }

        // 登出
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // 清除 JWT Cookie
            Response.Cookies.Delete("jwt");

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
