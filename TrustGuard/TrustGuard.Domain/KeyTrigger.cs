namespace TrustGuard.Domain.Entities
{
    public class KeyTrigger
    {
        public int Id { get; set; }

        public string Word { get; set; } = string.Empty;

        public int NewsCheckId { get; set; }
        public virtual NewsCheck? NewsCheck { get; set; }
    }
}