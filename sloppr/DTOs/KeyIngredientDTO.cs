namespace sloppr.DTOs
{
    public class KeyIngredientDTO
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public required string Name { get; set; }
        public int NumQueried { get; set; }
        public int NumUsed { get; set; }
        public DateTime? LastUsed { get; set; }
    }
}