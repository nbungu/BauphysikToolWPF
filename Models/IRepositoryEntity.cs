namespace BauphysikToolWPF.Models
{
    public interface IRepositoryEntity<T>
    {
        int Id { get; set; }
        long CreatedAt { get; set; }
        long UpdatedAt { get; set; }
        void UpdateTimestamp();
        T Copy();
        IRepositoryEntity<T> Convert();
    }
}
