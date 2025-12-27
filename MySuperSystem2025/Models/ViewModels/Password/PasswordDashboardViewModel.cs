namespace MySuperSystem2025.Models.ViewModels.Password
{
    /// <summary>
    /// Password manager dashboard view model
    /// </summary>
    public class PasswordDashboardViewModel
    {
        public int TotalPasswords { get; set; }
        public List<PasswordListItemViewModel> Passwords { get; set; } = new();
        public List<PasswordCategoryViewModel> Categories { get; set; } = new();
        public int? FilterCategoryId { get; set; }
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Password list item view model (password is masked)
    /// </summary>
    public class PasswordListItemViewModel
    {
        public int Id { get; set; }
        public string WebsiteOrAppName { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string MaskedPassword { get; set; } = "••••••••";
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
