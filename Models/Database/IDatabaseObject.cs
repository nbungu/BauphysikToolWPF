namespace BauphysikToolWPF.Models.Database
{
    interface IDatabaseObject<T> where T : class
    {
        int Id { get; set; }
        long CreatedAt { get; set; }
        long UpdatedAt { get; set; }
        void UpdateTimestamp();
        T Copy();
    }
}
