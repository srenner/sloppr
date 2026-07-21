using System;

namespace sloppr.Models
{
    /// <summary>
    /// Used for database entities
    /// </summary>
    public abstract class BaseModel
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = "system";
        public string UpdatedBy { get; set; } = "system";
    }
}