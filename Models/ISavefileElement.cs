﻿namespace BauphysikToolWPF.Models
{
    // TODO:

    interface ISavefileElement<T>
    {
        int Id { get; }
        int InternalId { get; set; }
        long CreatedAt { get; set; }
        long UpdatedAt { get; set; }
        T Copy();
        //T Convert(T entity);
        void UpdateTimestamp();
    }
}