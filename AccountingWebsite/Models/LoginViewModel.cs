using System.ComponentModel.DataAnnotations;

namespace AccountingWebsite.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "帳號")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "密碼")]
        public string Password { get; set; }
    }
}
