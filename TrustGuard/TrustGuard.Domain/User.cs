using Microsoft.AspNetCore.Identity;

namespace TrustGuard.Domain.Entities
{
    // Успадковуємо всі базові фічі безпеки від IdentityUser
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Зв'язок 1:N - один користувач має багато перевірок
        public virtual ICollection<NewsCheck> NewsChecks { get; set; } = new List<NewsCheck>();
    }
}