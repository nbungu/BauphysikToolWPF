namespace BauphysikToolWPF.Models.Domain
{
    public interface IDomainObject<T> where T : class // generic type constraint on T that restricts it to be a reference type
    {
        int InternalId { get; set; }
        long CreatedAt { get; set; }
        long UpdatedAt { get; set; }
        string CreatedAtString { get; }
        string UpdatedAtString { get; }
        bool IsValid { get; }
        T Copy();
        void UpdateTimestamp();
    }
}
