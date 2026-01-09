-- Check all tasks with their deadlines
SELECT 
    Id,
    Title,
    Deadline,
    Status,
    IsDeleted,
    CreatedAt,
    UpdatedAt
FROM Tasks
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;

-- Count tasks with and without deadlines
SELECT 
    CASE 
        WHEN Deadline IS NULL THEN 'No Deadline'
        ELSE 'Has Deadline'
    END AS DeadlineStatus,
    COUNT(*) AS TaskCount
FROM Tasks
WHERE IsDeleted = 0
GROUP BY 
    CASE 
        WHEN Deadline IS NULL THEN 'No Deadline'
        ELSE 'Has Deadline'
    END;
