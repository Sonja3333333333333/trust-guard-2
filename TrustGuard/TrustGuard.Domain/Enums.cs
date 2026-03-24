namespace TrustGuard.Domain.Entities
{
    public enum ContentType
    {
        Text,       
        Url,        
        Document,  
        Image  
    }

    public enum Verdict
    {
        Real,
        Fake,
        Uncertain
    }
}