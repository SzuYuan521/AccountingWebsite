using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AccountingWebsite.Data;
using AccountingWebsite.Models;
using System.Security.Claims;
using System.Globalization;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Identity;

namespace AccountingWebsite.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(DateTime? date)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!int.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Users");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user == null)
            {
                return Unauthorized("無法找到用戶");
            }

            // 將用戶的餘額存到 ViewBag
            ViewBag.Balance = user.Balance;

            // 如果沒選日期就設為今天
            date ??= DateTime.Today;

            // 獲取User自己的於該日期的交易記錄
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date == date.Value.Date)
                .ToListAsync();

            ViewBag.SelectedDate = date.Value;

            return View(transactions);
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new
            {
                Value = t,
                Text = t == TransactionType.Income ? "收入" : "支出"
            }), "Value", "Text");

            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,TransactionTitle,TransactionDescription,Amount,Date,TransactionType")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("用戶未登入");
                }

                transaction.UserId = userId;

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("用戶不存在");
                }

                if (transaction.TransactionType == TransactionType.Income)
                {
                    user.Balance += transaction.Amount; // 如果是收入, 增加餘額
                }
                else if (transaction.TransactionType == TransactionType.Expense)
                {
                    user.Balance -= transaction.Amount; // 如果是支出, 減少餘額
                }

                DateTime date = transaction.Date;

                _context.Add(transaction);
                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new {date});
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new
            {
                Value = t,
                Text = t == TransactionType.Income ? "收入" : "支出"
            }), "Value", "Text");

            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,TransactionTitle,TransactionDescription,Amount,Date,TransactionType")] Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalTransaction = await _context.Transactions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TransactionId == id);

                    if(originalTransaction == null)
                    {
                        return NotFound();
                    }

                    decimal originalAmount = originalTransaction.Amount;
                    TransactionType originalTransactionType = originalTransaction.TransactionType;

                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!int.TryParse(userIdClaim, out var userId))
                    {
                        return Unauthorized("用戶未登入");
                    }

                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        return NotFound("用戶不存在");
                    }

                    transaction.UserId = userId;

                    // 如果金額或是類型有變動, 先把舊交易改變的餘額算回去, 再重算新餘額
                    if (originalAmount != transaction.Amount || originalTransactionType != transaction.TransactionType)
                    {
                        // 算回原本餘額
                        if (originalTransactionType == TransactionType.Income)
                        {
                            user.Balance -= originalAmount; // 如果是收入, 把增加的餘額扣回去
                        }
                        else if (originalTransactionType == TransactionType.Expense)
                        {
                            user.Balance += originalAmount; // 如果是支出, 把減少的餘額加回去
                        }
                    }

                    if (transaction.TransactionType == TransactionType.Income)
                    {
                        user.Balance += transaction.Amount; // 如果是收入, 增加餘額
                    }
                    else if (transaction.TransactionType == TransactionType.Expense)
                    {
                        user.Balance -= transaction.Amount; // 如果是支出, 減少餘額
                    }

                    DateTime date = transaction.Date;

                    _context.Update(transaction);
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index), new { date });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", transaction.UserId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Transactions'  is null.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("用戶未登入");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("用戶不存在");
            }

            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction != null)
            {
                // 餘額更新
                if (transaction.TransactionType == TransactionType.Income)
                {
                    user.Balance -= transaction.Amount; // 如果是收入, 把增加的餘額扣回去
                }
                else if (transaction.TransactionType == TransactionType.Expense)
                {
                    user.Balance += transaction.Amount; // 如果是支出, 把減少的餘額加回去
                }

                _context.Transactions.Remove(transaction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
          return (_context.Transactions?.Any(e => e.TransactionId == id)).GetValueOrDefault();
        }

        public IActionResult UpdateBalance()
        {
            return View();
        }


        /// <summary>
        /// 調整餘額
        /// </summary>
        /// <param name="Balance">新餘額</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBalance(BalanceViewModel model)
        {
            if(ModelState.IsValid) {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("用戶未登入");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("用戶不存在");
                }

                // 變動金額計算
                decimal amount = model.Balance - user.Balance; // 新餘額 - 現餘額 = 變動金額

                var transaction = new Transaction
                {
                    TransactionTitle = "餘額調整",
                    TransactionDescription = $"調整後餘額 {(model.Balance >= 0 ? "+" : "-")}${model.Balance}", // 顯示調整後餘額
                    Amount = Math.Abs(amount), // 金額顯示會是正數
                    Date = DateTime.Now,
                    TransactionType = amount >= 0 ? TransactionType.Income : TransactionType.Expense, // 變動金額正負值決定收入還是支出
                    UserId = userId,
                };

                user.Balance = model.Balance;

                _context.Add(transaction);
                _context.Update(user);
                await _context.SaveChangesAsync();
           

                return RedirectToAction("Index", "Transactions");
            }

            return View(model);
        }

        public async Task<IActionResult> MonthlyReport(int? year, int? month)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("用戶未登入");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("用戶不存在");
                }

                // 如果沒有設定年/月, 就設置為當前年/月
                year ??= DateTime.Now.Year;
                month ??= DateTime.Now.Month;

                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date.Year == year && t.Date.Month == month)
                    .ToListAsync();

                // 計算收入總和
                var totalIncome = transactions
                    .Where(t => t.TransactionType == TransactionType.Income)
                    .Sum(t => t.Amount);

                // 計算支出總和
                var totalExpense = transactions
                    .Where(t => t.TransactionType == TransactionType.Expense)
                    .Sum(t => t.Amount);

                var viewModel = new MonthlyReportViewModel
                {
                    Year = year.Value,
                    Month = month.Value,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    Total = totalIncome - totalExpense,
                    Transactions = transactions
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "生成月報告時發生錯誤，請稍後再試");
            }
        }
    }
}
