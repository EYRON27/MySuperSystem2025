using System.ComponentModel.DataAnnotations;
using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Models.ViewModels.Food
{
    /// <summary>
    /// View model for creating a new food entry
    /// </summary>
    public class CreateFoodEntryViewModel
    {
        [Required(ErrorMessage = "Food name is required")]
        [StringLength(200, ErrorMessage = "Food name cannot exceed 200 characters")]
        [Display(Name = "Food Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Meal type is required")]
        [Display(Name = "Meal Type")]
        public string MealType { get; set; } = MealTypes.Breakfast;

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(100)]
        [Display(Name = "Serving Size (Optional)")]
        public string? ServingSize { get; set; }

        [Range(0, 10000, ErrorMessage = "Calories must be between 0 and 10,000")]
        [Display(Name = "Calories (kcal)")]
        public int Calories { get; set; }

        [Range(0, 1000, ErrorMessage = "Protein must be between 0 and 1,000g")]
        [Display(Name = "Protein (g)")]
        public decimal Protein { get; set; }

        [Range(0, 1000, ErrorMessage = "Carbs must be between 0 and 1,000g")]
        [Display(Name = "Carbohydrates (g)")]
        public decimal Carbs { get; set; }

        [Range(0, 1000, ErrorMessage = "Fats must be between 0 and 1,000g")]
        [Display(Name = "Fats (g)")]
        public decimal Fats { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes (Optional)")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// View model for editing an existing food entry
    /// </summary>
    public class EditFoodEntryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Food name is required")]
        [StringLength(200, ErrorMessage = "Food name cannot exceed 200 characters")]
        [Display(Name = "Food Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Meal type is required")]
        [Display(Name = "Meal Type")]
        public string MealType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [StringLength(100)]
        [Display(Name = "Serving Size (Optional)")]
        public string? ServingSize { get; set; }

        [Range(0, 10000, ErrorMessage = "Calories must be between 0 and 10,000")]
        [Display(Name = "Calories (kcal)")]
        public int Calories { get; set; }

        [Range(0, 1000, ErrorMessage = "Protein must be between 0 and 1,000g")]
        [Display(Name = "Protein (g)")]
        public decimal Protein { get; set; }

        [Range(0, 1000, ErrorMessage = "Carbs must be between 0 and 1,000g")]
        [Display(Name = "Carbohydrates (g)")]
        public decimal Carbs { get; set; }

        [Range(0, 1000, ErrorMessage = "Fats must be between 0 and 1,000g")]
        [Display(Name = "Fats (g)")]
        public decimal Fats { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes (Optional)")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// View model for displaying a food entry in lists
    /// </summary>
    public class FoodEntryListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MealType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? ServingSize { get; set; }
        public int Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fats { get; set; }
        public string? Notes { get; set; }

        public string MealTypeIcon => MealType switch
        {
            MealTypes.Breakfast => "bi-sunrise",
            MealTypes.Lunch => "bi-sun",
            MealTypes.Dinner => "bi-moon-stars",
            MealTypes.Snack => "bi-cup-hot",
            _ => "bi-egg-fried"
        };

        public string MealTypeBadgeClass => MealType switch
        {
            MealTypes.Breakfast => "bg-warning text-dark",
            MealTypes.Lunch => "bg-success",
            MealTypes.Dinner => "bg-primary",
            MealTypes.Snack => "bg-info",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// View model for food entry list with filters
    /// </summary>
    public class FoodEntryListViewModel
    {
        public List<FoodEntryListItemViewModel> FoodEntries { get; set; } = new();
        public string? FilterPeriod { get; set; }
        public string? FilterMealType { get; set; }
        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }
        public int TotalCount { get; set; }
        public int TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFats { get; set; }
    }

    /// <summary>
    /// Summary of nutrition by meal type
    /// </summary>
    public class MealTypeSummaryViewModel
    {
        public string MealType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFats { get; set; }
        public decimal Percentage { get; set; }

        public string MealTypeIcon => MealType switch
        {
            MealTypes.Breakfast => "bi-sunrise",
            MealTypes.Lunch => "bi-sun",
            MealTypes.Dinner => "bi-moon-stars",
            MealTypes.Snack => "bi-cup-hot",
            _ => "bi-egg-fried"
        };

        public string MealTypeBadgeClass => MealType switch
        {
            MealTypes.Breakfast => "bg-warning text-dark",
            MealTypes.Lunch => "bg-success",
            MealTypes.Dinner => "bg-primary",
            MealTypes.Snack => "bg-info",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Main dashboard view model for Food Tracker
    /// </summary>
    public class FoodDashboardViewModel
    {
        // Today's summary
        public int TodayCalories { get; set; }
        public decimal TodayProtein { get; set; }
        public decimal TodayCarbs { get; set; }
        public decimal TodayFats { get; set; }
        public int TodayMealCount { get; set; }

        // Weekly summary
        public int WeeklyCalories { get; set; }
        public decimal WeeklyProtein { get; set; }
        public decimal WeeklyCarbs { get; set; }
        public decimal WeeklyFats { get; set; }
        public int WeeklyMealCount { get; set; }

        // Monthly summary
        public int MonthlyCalories { get; set; }
        public decimal MonthlyProtein { get; set; }
        public decimal MonthlyCarbs { get; set; }
        public decimal MonthlyFats { get; set; }
        public int MonthlyMealCount { get; set; }

        // Breakdown by meal type
        public List<MealTypeSummaryViewModel> MealTypeBreakdown { get; set; } = new();
        public string BreakdownPeriod { get; set; } = "daily";
        public string BreakdownPeriodName { get; set; } = "Today";

        // Recent entries
        public List<FoodEntryListItemViewModel> RecentEntries { get; set; } = new();

        // Daily averages (for the selected period)
        public int AverageDailyCalories { get; set; }
        public decimal AverageDailyProtein { get; set; }
        public decimal AverageDailyCarbs { get; set; }
        public decimal AverageDailyFats { get; set; }

        // Formatted strings
        public string FormattedTodayMacros => $"P: {TodayProtein:N0}g | C: {TodayCarbs:N0}g | F: {TodayFats:N0}g";
    }

    /// <summary>
    /// View model for month filter options
    /// </summary>
    public class FoodMonthOption
    {
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
