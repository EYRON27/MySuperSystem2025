# PowerShell script to run Time Tracking migration
$server = "(localdb)\mssqllocaldb"
$database = "MySuperSystem2025"
$scriptPath = "AddTimeTracking.sql"

Write-Host "Running Time Tracking Migration..." -ForegroundColor Cyan
Write-Host "Server: $server" -ForegroundColor Gray
Write-Host "Database: $database" -ForegroundColor Gray
Write-Host ""

try {
    # Execute SQL script
    sqlcmd -S $server -d $database -i $scriptPath
    
    Write-Host ""
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Run the application: dotnet run" -ForegroundColor Gray
    Write-Host "2. Navigate to Time Tracker from the dashboard" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "Error running migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Please ensure:" -ForegroundColor Yellow
    Write-Host "- SQL Server LocalDB is running" -ForegroundColor Gray
    Write-Host "- The MySuperSystem2025 database exists" -ForegroundColor Gray
    Write-Host "- You have permissions to create tables" -ForegroundColor Gray
    Write-Host ""
}
