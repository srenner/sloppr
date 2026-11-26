namespace sloppr.DTOs
{
    public class MealIdeaDTO
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; } = "system";
        public string UpdatedBy { get; set; } = "system";

        public required string Name { get; set; }
        public bool? IsUserApproved { get; set; }
        public ICollection<KeyIngredientDTO> KeyIngredients { get; set; } = [];
    }
}
