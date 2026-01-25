using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Food entry entity representing a food item consumed.
    /// Tracks nutritional information for health monitoring.
    /// </summary>
    public class FoodEntry : BaseEntity
    {
        [Required(ErrorMessage = "Food name is required")]
        [StringLength(200, ErrorMessage = "Food name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Meal type is required")]
        [StringLength(50)]
        public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [StringLength(100)]
        public string? ServingSize { get; set; }

        [Range(0, 10000, ErrorMessage = "Calories must be between 0 and 10,000")]
        public int Calories { get; set; }

        [Range(0, 1000, ErrorMessage = "Protein must be between 0 and 1,000g")]
        public decimal Protein { get; set; }

        [Range(0, 1000, ErrorMessage = "Carbs must be between 0 and 1,000g")]
        public decimal Carbs { get; set; }

        [Range(0, 1000, ErrorMessage = "Fats must be between 0 and 1,000g")]
        public decimal Fats { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        // Foreign keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }

    /// <summary>
    /// Meal type constants for consistency
    /// </summary>
    public static class MealTypes
    {
        public const string Breakfast = "Breakfast";
        public const string Lunch = "Lunch";
        public const string Dinner = "Dinner";
        public const string Snack = "Snack";

        public static readonly string[] All = { Breakfast, Lunch, Dinner, Snack };
    }
}
