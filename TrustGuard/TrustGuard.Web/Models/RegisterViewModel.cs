using System.ComponentModel.DataAnnotations;

namespace TrustGuard.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Пошта обов'язкова")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Прізвище та ім'я обов'язкові")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}