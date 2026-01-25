using iTextSharp.text;
using iTextSharp.text.pdf;
using MySuperSystem2025.Models.ViewModels.Expense;
using MySuperSystem2025.Models.ViewModels.Task;
using MySuperSystem2025.Models.ViewModels.Password;
using MySuperSystem2025.Models.ViewModels.Food;

namespace MySuperSystem2025.Services
{
    /// <summary>
    /// Service for generating PDF reports from dashboard data
    /// </summary>
    public class PdfService
    {
        private readonly ILogger<PdfService> _logger;

        // Color definitions
        private static readonly BaseColor ColorDarkGray = new BaseColor(51, 51, 51);
        private static readonly BaseColor ColorGray = new BaseColor(128, 128, 128);
        private static readonly BaseColor ColorBlack = new BaseColor(0, 0, 0);
        private static readonly BaseColor ColorBlue = new BaseColor(0, 0, 255);
        private static readonly BaseColor ColorRed = new BaseColor(255, 0, 0);
        private static readonly BaseColor ColorWhite = new BaseColor(255, 255, 255);
        private static readonly BaseColor ColorLightGray = new BaseColor(211, 211, 211);
        private static readonly BaseColor ColorHeaderBg = new BaseColor(41, 128, 185);
        private static readonly BaseColor ColorCellBg = new BaseColor(245, 245, 245);
        private static readonly BaseColor ColorGreen = new BaseColor(22, 163, 74);

        // Peso currency symbol - using "PHP" as fallback for PDF compatibility
        private const string PesoSymbol = "PHP ";

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a font that supports Unicode characters including peso sign
        /// </summary>
        private Font GetUnicodeFont(int size, int style = Font.NORMAL, BaseColor color = null)
        {
            // Register Arial Unicode MS or fallback to standard with Identity-H encoding
            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            return new Font(baseFont, size, style, color ?? ColorBlack);
        }

        /// <summary>
        /// Generates PDF for Expense Dashboard
        /// </summary>
        public byte[] GenerateExpenseDashboardPdf(ExpenseDashboardViewModel model, string userName)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, ColorDarkGray);
            var title = new Paragraph("Expense Dashboard Report\n\n", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // User and Date Info
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, ColorGray);
            document.Add(new Paragraph($"Generated for: {userName}", infoFont));
            document.Add(new Paragraph($"Date: {DateTime.Now:MMMM dd, yyyy hh:mm tt}\n\n", infoFont));

            // Summary Section
            var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, ColorBlack);
            document.Add(new Paragraph("Expense Summary", sectionFont));
            document.Add(new Paragraph(" "));

            var summaryTable = new PdfPTable(2) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 1, 1 });

            AddSummaryRow(summaryTable, "Today", $"{PesoSymbol}{model.TodayTotal:N2}", $"{model.TodayCount} expenses");
            AddSummaryRow(summaryTable, "This Week", $"{PesoSymbol}{model.WeeklyTotal:N2}", $"{model.WeeklyCount} expenses");
            AddSummaryRow(summaryTable, "This Month", $"{PesoSymbol}{model.MonthlyTotal:N2}", $"{model.MonthlyCount} expenses");
            AddSummaryRow(summaryTable, "This Year", $"{PesoSymbol}{model.YearlyTotal:N2}", $"{model.YearlyCount} expenses");

            document.Add(summaryTable);
            document.Add(new Paragraph(" "));

            // Budget Overview (if any budget set)
            if (model.TotalBudget > 0)
            {
                document.Add(new Paragraph("Budget Overview", sectionFont));
                document.Add(new Paragraph(" "));

                var budgetTable = new PdfPTable(3) { WidthPercentage = 100 };
                budgetTable.SetWidths(new float[] { 1, 1, 1 });

                AddHeaderCell(budgetTable, "Total Budget");
                AddHeaderCell(budgetTable, "Total Spent");
                AddHeaderCell(budgetTable, "Remaining Balance");

                AddDataCell(budgetTable, $"{PesoSymbol}{model.TotalBudget:N2}");
                AddDataCell(budgetTable, $"{PesoSymbol}{model.TotalExpenses:N2}");
                AddDataCell(budgetTable, $"{PesoSymbol}{model.TotalRemainingBalance:N2}");

                document.Add(budgetTable);
                document.Add(new Paragraph(" "));
            }

            // Category Breakdown
            if (model.CategoryBreakdown.Any())
            {
                document.Add(new Paragraph($"{model.BreakdownPeriodName} by Category", sectionFont));
                document.Add(new Paragraph(" "));

                var categoryTable = new PdfPTable(3) { WidthPercentage = 100 };
                categoryTable.SetWidths(new float[] { 2, 1, 1 });

                AddHeaderCell(categoryTable, "Category");
                AddHeaderCell(categoryTable, "Amount");
                AddHeaderCell(categoryTable, "Percentage");

                foreach (var category in model.CategoryBreakdown)
                {
                    AddDataCell(categoryTable, category.CategoryName);
                    AddDataCell(categoryTable, $"{PesoSymbol}{category.Total:N2}");
                    AddDataCell(categoryTable, $"{category.Percentage}%");
                }

                document.Add(categoryTable);
                document.Add(new Paragraph(" "));
            }

            // Recent Expenses
            if (model.RecentExpenses.Any())
            {
                document.Add(new Paragraph("Recent Expenses", sectionFont));
                document.Add(new Paragraph(" "));

                var expenseTable = new PdfPTable(4) { WidthPercentage = 100 };
                expenseTable.SetWidths(new float[] { 1.5f, 1.5f, 3, 1.5f });

                AddHeaderCell(expenseTable, "Date");
                AddHeaderCell(expenseTable, "Category");
                AddHeaderCell(expenseTable, "Reason");
                AddHeaderCell(expenseTable, "Amount");

                foreach (var expense in model.RecentExpenses.Take(15))
                {
                    AddDataCell(expenseTable, expense.Date.ToString("MMM dd, yyyy"));
                    AddDataCell(expenseTable, expense.CategoryName);
                    AddDataCell(expenseTable, expense.Reason);
                    AddDataCell(expenseTable, $"{PesoSymbol}{expense.Amount:N2}");
                }

                document.Add(expenseTable);
            }

            document.Close();
            writer.Close();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Generates PDF for Task Dashboard
        /// </summary>
        public byte[] GenerateTaskDashboardPdf(TaskDashboardViewModel model, string userName)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, ColorDarkGray);
            var title = new Paragraph("Task Dashboard Report\n\n", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // User and Date Info
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, ColorGray);
            document.Add(new Paragraph($"Generated for: {userName}", infoFont));
            document.Add(new Paragraph($"Date: {DateTime.Now:MMMM dd, yyyy hh:mm tt}\n\n", infoFont));

            // Summary Section
            var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, ColorBlack);
            document.Add(new Paragraph("Task Summary", sectionFont));
            document.Add(new Paragraph(" "));

            var summaryTable = new PdfPTable(4) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 1, 1, 1, 1 });

            AddHeaderCell(summaryTable, "To Do");
            AddHeaderCell(summaryTable, "Ongoing");
            AddHeaderCell(summaryTable, "Completed");
            AddHeaderCell(summaryTable, "Overdue");

            AddDataCell(summaryTable, model.PendingCount.ToString());
            AddDataCell(summaryTable, model.OngoingCount.ToString());
            AddDataCell(summaryTable, model.CompletedCount.ToString());
            AddDataCell(summaryTable, model.OverdueCount.ToString());

            document.Add(summaryTable);
            document.Add(new Paragraph(" "));

            // To Do Tasks
            if (model.ToDoTasks.Any())
            {
                AddTaskSection(document, "To Do Tasks", model.ToDoTasks, sectionFont);
            }

            // Ongoing Tasks
            if (model.OngoingTasks.Any())
            {
                AddTaskSection(document, "Ongoing Tasks", model.OngoingTasks, sectionFont);
            }

            // Overdue Tasks
            if (model.OverdueTasks.Any())
            {
                AddTaskSection(document, "Overdue Tasks", model.OverdueTasks, sectionFont);
            }

            document.Close();
            writer.Close();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Generates PDF for Password Dashboard
        /// </summary>
        public byte[] GeneratePasswordDashboardPdf(PasswordDashboardViewModel model, string userName)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, ColorDarkGray);
            var title = new Paragraph("Password Vault Report\n\n", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // User and Date Info
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, ColorGray);
            document.Add(new Paragraph($"Generated for: {userName}", infoFont));
            document.Add(new Paragraph($"Date: {DateTime.Now:MMMM dd, yyyy hh:mm tt}\n\n", infoFont));

            // Warning
            var warningFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, ColorRed);
            var warning = new Paragraph("? CONFIDENTIAL - Handle with care. Passwords are NOT included in this report.\n\n", warningFont);
            warning.Alignment = Element.ALIGN_CENTER;
            document.Add(warning);

            // Summary Section
            var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, ColorBlack);
            document.Add(new Paragraph("Password Summary", sectionFont));
            document.Add(new Paragraph(" "));

            var summaryTable = new PdfPTable(2) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 1, 1 });

            AddSummaryRow(summaryTable, "Total Passwords", model.TotalPasswords.ToString(), "");

            document.Add(summaryTable);
            document.Add(new Paragraph(" "));

            // Recent Passwords (without actual passwords)
            if (model.Passwords.Any())
            {
                document.Add(new Paragraph("Password Vault Entries", sectionFont));
                document.Add(new Paragraph(" "));

                var passwordTable = new PdfPTable(3) { WidthPercentage = 100 };
                passwordTable.SetWidths(new float[] { 2, 2, 1.5f });

                AddHeaderCell(passwordTable, "Website/App");
                AddHeaderCell(passwordTable, "Username");
                AddHeaderCell(passwordTable, "Category");

                foreach (var password in model.Passwords.Take(15))
                {
                    AddDataCell(passwordTable, password.WebsiteOrAppName);
                    AddDataCell(passwordTable, password.Username);
                    AddDataCell(passwordTable, password.CategoryName);
                }

                document.Add(passwordTable);
            }

            document.Close();
            writer.Close();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Generates PDF for Food Tracker Dashboard
        /// </summary>
        public byte[] GenerateFoodDashboardPdf(FoodDashboardViewModel dashboard, FoodEntryListViewModel entries, string userName)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, ColorDarkGray);
            var title = new Paragraph("Food Tracker Report\n\n", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // User and Date Info
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, ColorGray);
            document.Add(new Paragraph($"Generated for: {userName}", infoFont));
            document.Add(new Paragraph($"Date: {DateTime.Now:MMMM dd, yyyy hh:mm tt}", infoFont));
            document.Add(new Paragraph($"Period: {dashboard.BreakdownPeriodName}\n\n", infoFont));

            // Summary Section
            var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, ColorBlack);
            document.Add(new Paragraph("Nutrition Summary", sectionFont));
            document.Add(new Paragraph(" "));

            var summaryTable = new PdfPTable(2) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 1, 1 });

            AddSummaryRow(summaryTable, "Today's Calories", $"{dashboard.TodayCalories} kcal", $"{dashboard.TodayMealCount} meals");
            AddSummaryRow(summaryTable, "Weekly Calories", $"{dashboard.WeeklyCalories} kcal", $"{dashboard.WeeklyMealCount} meals");
            AddSummaryRow(summaryTable, "Monthly Calories", $"{dashboard.MonthlyCalories} kcal", $"{dashboard.MonthlyMealCount} meals");
            AddSummaryRow(summaryTable, "Daily Average", $"{dashboard.AverageDailyCalories} kcal", dashboard.BreakdownPeriodName);

            document.Add(summaryTable);
            document.Add(new Paragraph(" "));

            // Macros Summary
            document.Add(new Paragraph("Today's Macronutrients", sectionFont));
            document.Add(new Paragraph(" "));

            var macroTable = new PdfPTable(4) { WidthPercentage = 100 };
            macroTable.SetWidths(new float[] { 1, 1, 1, 1 });

            AddHeaderCell(macroTable, "Calories");
            AddHeaderCell(macroTable, "Protein");
            AddHeaderCell(macroTable, "Carbs");
            AddHeaderCell(macroTable, "Fats");

            AddDataCell(macroTable, $"{dashboard.TodayCalories} kcal");
            AddDataCell(macroTable, $"{dashboard.TodayProtein:N1}g");
            AddDataCell(macroTable, $"{dashboard.TodayCarbs:N1}g");
            AddDataCell(macroTable, $"{dashboard.TodayFats:N1}g");

            document.Add(macroTable);
            document.Add(new Paragraph(" "));

            // Meal Type Breakdown
            if (dashboard.MealTypeBreakdown.Any())
            {
                document.Add(new Paragraph($"Calories by Meal Type - {dashboard.BreakdownPeriodName}", sectionFont));
                document.Add(new Paragraph(" "));

                var mealTable = new PdfPTable(6) { WidthPercentage = 100 };
                mealTable.SetWidths(new float[] { 1.5f, 1, 1, 1, 1, 1 });

                AddHeaderCell(mealTable, "Meal Type");
                AddHeaderCell(mealTable, "Count");
                AddHeaderCell(mealTable, "Calories");
                AddHeaderCell(mealTable, "Protein");
                AddHeaderCell(mealTable, "Carbs");
                AddHeaderCell(mealTable, "Fats");

                foreach (var meal in dashboard.MealTypeBreakdown)
                {
                    AddDataCell(mealTable, meal.MealType);
                    AddDataCell(mealTable, meal.Count.ToString());
                    AddDataCell(mealTable, $"{meal.TotalCalories} kcal");
                    AddDataCell(mealTable, $"{meal.TotalProtein:N1}g");
                    AddDataCell(mealTable, $"{meal.TotalCarbs:N1}g");
                    AddDataCell(mealTable, $"{meal.TotalFats:N1}g");
                }

                document.Add(mealTable);
                document.Add(new Paragraph(" "));
            }

            // Food Entries
            if (entries.FoodEntries.Any())
            {
                document.Add(new Paragraph("Food Entries", sectionFont));
                document.Add(new Paragraph(" "));

                var entriesTable = new PdfPTable(7) { WidthPercentage = 100 };
                entriesTable.SetWidths(new float[] { 1.2f, 2f, 1f, 1f, 0.8f, 0.8f, 0.8f });

                AddHeaderCell(entriesTable, "Date");
                AddHeaderCell(entriesTable, "Food");
                AddHeaderCell(entriesTable, "Meal");
                AddHeaderCell(entriesTable, "Calories");
                AddHeaderCell(entriesTable, "P");
                AddHeaderCell(entriesTable, "C");
                AddHeaderCell(entriesTable, "F");

                foreach (var entry in entries.FoodEntries.Take(30))
                {
                    AddDataCell(entriesTable, entry.Date.ToString("MMM dd"));
                    AddDataCell(entriesTable, entry.Name);
                    AddDataCell(entriesTable, entry.MealType);
                    AddDataCell(entriesTable, $"{entry.Calories}");
                    AddDataCell(entriesTable, $"{entry.Protein:N0}g");
                    AddDataCell(entriesTable, $"{entry.Carbs:N0}g");
                    AddDataCell(entriesTable, $"{entry.Fats:N0}g");
                }

                document.Add(entriesTable);
                document.Add(new Paragraph(" "));

                // Totals row
                var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, ColorBlack);
                var totals = new Paragraph($"\nTotals: {entries.TotalCount} entries | {entries.TotalCalories} kcal | Protein: {entries.TotalProtein:N1}g | Carbs: {entries.TotalCarbs:N1}g | Fats: {entries.TotalFats:N1}g", totalFont);
                document.Add(totals);
            }

            // Footer note
            document.Add(new Paragraph(" "));
            var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, ColorGray);
            var footer = new Paragraph("This report is generated for health tracking purposes. Consult a healthcare professional for personalized dietary advice.", footerFont);
            footer.Alignment = Element.ALIGN_CENTER;
            document.Add(footer);

            document.Close();
            writer.Close();

            return memoryStream.ToArray();
        }

        // Helper methods
        private void AddSummaryRow(PdfPTable table, string label, string value, string subValue)
        {
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            var valueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, ColorBlue);
            var subFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, ColorGray);

            var cell1 = new PdfPCell(new Phrase(label, headerFont))
            {
                Border = Rectangle.BOX,
                Padding = 10,
                BackgroundColor = ColorCellBg
            };
            table.AddCell(cell1);

            var valuePhrase = new Phrase();
            valuePhrase.Add(new Chunk(value, valueFont));
            if (!string.IsNullOrEmpty(subValue))
            {
                valuePhrase.Add(new Chunk($"\n{subValue}", subFont));
            }

            var cell2 = new PdfPCell(valuePhrase)
            {
                Border = Rectangle.BOX,
                Padding = 10,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            table.AddCell(cell2);
        }

        private void AddHeaderCell(PdfPTable table, string text)
        {
            var font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, ColorWhite);
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = ColorHeaderBg,
                Padding = 8,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            table.AddCell(cell);
        }

        private void AddDataCell(PdfPTable table, string text)
        {
            var font = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 6,
                Border = Rectangle.BOX,
                BorderColor = ColorLightGray
            };
            table.AddCell(cell);
        }

        private void AddTaskSection(Document document, string title, List<TaskListItemViewModel> tasks, Font sectionFont)
        {
            document.Add(new Paragraph(title, sectionFont));
            document.Add(new Paragraph(" "));

            var taskTable = new PdfPTable(4) { WidthPercentage = 100 };
            taskTable.SetWidths(new float[] { 2, 3, 1.5f, 1 });

            AddHeaderCell(taskTable, "Title");
            AddHeaderCell(taskTable, "Description");
            AddHeaderCell(taskTable, "Deadline");
            AddHeaderCell(taskTable, "Status");

            foreach (var task in tasks.Take(15))
            {
                AddDataCell(taskTable, task.Title);
                AddDataCell(taskTable, task.Description ?? "-");
                AddDataCell(taskTable, task.Deadline?.ToString("MMM dd, yyyy") ?? "No deadline");
                AddDataCell(taskTable, task.StatusDisplay);
            }

            document.Add(taskTable);
            document.Add(new Paragraph(" "));
        }
    }
}
