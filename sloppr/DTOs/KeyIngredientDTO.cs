using System;

namespace sloppr.DTOs
{
    public class KeyIngredientDTO
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = "system";
        public string UpdatedBy { get; set; } = "system";

        public required string Name { get; set; }
        public int NumQueried { get; set; }
        public int NumUsed { get; set; }
        public DateTime? LastUsed { get; set; }
    }
}