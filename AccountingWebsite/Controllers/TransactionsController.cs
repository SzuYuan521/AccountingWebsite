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

            // 獲取User自己的交易記錄
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            // 如果有選日期, 則篩選出該如果有選日期的交易紀錄
            if (date.HasValue)
            {
                transactions = transactions.Where(t => t.Date.Date == date.Value.Date).ToList();
            }

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

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage); // 你可以用這段來調試具體的錯誤訊息
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,TransactionTitle,TransactionDescription,Amount,Date,TransactionType,UserId")] Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
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
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
          return (_context.Transactions?.Any(e => e.TransactionId == id)).GetValueOrDefault();
        }
    }
}
