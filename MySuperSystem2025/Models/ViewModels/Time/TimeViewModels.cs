using System.ComponentModel.DataAnnotations;

namespace MySuperSystem2025.Models.ViewModels.Time
{
    public class CreateTimeEntryViewModel
    {
        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes (Optional)")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
    }

    public class EditTimeEntryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes (Optional)")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
    }

    public class TimeEntryListItemViewModel
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        public string FormattedDuration
        {
            get
            {
                var hours = DurationMinutes / 60;
                var minutes = DurationMinutes % 60;
                if (hours > 0)
                    return $"{hours}h {minutes}m";
                return $"{minutes}m";
            }
        }
    }

    public class TimeEntryListViewModel
    {
        public List<TimeEntryListItemViewModel> TimeEntries { get; set; } = new();
        public List<TimeCategoryViewModel> Categories { get; set; } = new();
        public string? FilterPeriod { get; set; }
        public int? FilterCategoryId { get; set; }
        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }
        public int TotalMinutes { get; set; }
        public int TotalCount { get; set; }

        public string FormattedTotalDuration
        {
            get
            {
                var hours = TotalMinutes / 60;
                var minutes = TotalMinutes % 60;
                if (hours > 0)
                    return $"{hours}h {minutes}m";
                return $"{minutes}m";
            }
        }
    }

    public class CategorySummaryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalMinutes { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }

        public string FormattedDuration
        {
            get
            {
                var hours = TotalMinutes / 60;
                var minutes = TotalMinutes % 60;
                if (hours > 0)
                    return $"{hours}h {minutes}m";
                return $"{minutes}m";
            }
        }
    }

    public class TimeDashboardViewModel
    {
        public int TodayMinutes { get; set; }
        public int WeeklyMinutes { get; set; }
        public int MonthlyMinutes { get; set; }
        public int TodayCount { get; set; }
        public int WeeklyCount { get; set; }
        public int MonthlyCount { get; set; }
        public List<TimeEntryListItemViewModel> RecentEntries { get; set; } = new();
        public List<CategorySummaryViewModel> CategoryBreakdown { get; set; } = new();
        public string BreakdownPeriod { get; set; } = "monthly";
        public string BreakdownPeriodName { get; set; } = "This Month";

        public string FormattedTodayDuration
        {
            get
            {
                var hours = TodayMinutes / 60;
                var minutes = TodayMinutes % 60;
                if (hours > 0) return $"{hours}h {minutes}m";
                return $"{minutes}m";
            }
        }

        public string FormattedWeeklyDuration
        {
            get
            {
                var hours = WeeklyMinutes / 60;
                var minutes = WeeklyMinutes % 60;
                if (hours > 0) return $"{hours}h {minutes}m";
                return $"{minutes}m";
            }
        }

        public string FormattedMonthlyDuration
        {
            get
            {
                var hours = MonthlyMinutes / 60;
                var minutes = MonthlyMinutes % 60;
                if (hours > 0) return $"{hours}h {minutes}m";
                return $"{minutes}m";
            }
        }
    }

    public class TimeCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public int EntryCount { get; set; }
    }

    public class CreateTimeCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description (Optional)")]
        public string? Description { get; set; }
    }

    public class EditTimeCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description (Optional)")]
        public string? Description { get; set; }

        public bool IsDefault { get; set; }
        public int TotalEntries { get; set; }
    }
}
