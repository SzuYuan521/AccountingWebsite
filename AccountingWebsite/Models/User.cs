using Microsoft.AspNetCore.Identity;

namespace AccountingWebsite.Models
{
    public class User: IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal Balance { get; set; } = 0; // 餘額
    }
}
