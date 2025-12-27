using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MySuperSystem2025.Models.ViewModels.Expense
{
    /// <summary>
    /// View model for creating a new expense
    /// </summary>
    public class CreateExpenseViewModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Reason/Description is required")]
        [StringLength(255, ErrorMessage = "Reason cannot exceed 255 characters")]
        [Display(Name = "Reason / Description")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public SelectList? Categories { get; set; }
    }

    /// <summary>
    /// View model for editing an existing expense
    /// </summary>
    public class EditExpenseViewModel : CreateExpenseViewModel
    {
        public int Id { get; set; }
    }
}
