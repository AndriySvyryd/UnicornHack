using System.ComponentModel.DataAnnotations;

namespace UnicornHack.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}