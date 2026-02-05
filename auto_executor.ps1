# MS Project Auto Executor V1.0
# 自动监听 .task/current_task.json 变化并触发 Cursor 执行

$ProjectPath = "E:\AI_Project\MS"
$TaskFile = "$ProjectPath\.task\current_task.json"
$CheckInterval = 10

Write-Host "MS Project Auto Executor V1.0" -ForegroundColor Cyan
Write-Host "监听路径: $TaskFile" -ForegroundColor Yellow

$lastTaskId = ""
$lastStatus = ""

function Get-TaskInfo {
    if (Test-Path $TaskFile) {
        try {
            $content = Get-Content $TaskFile -Raw | ConvertFrom-Json
            return @{TaskId = $content.task_id; Title = $content.title; Status = $content.status}
        } catch { return $null }
    }
    return $null
}

Write-Host "开始监听... (Ctrl+C 停止)" -ForegroundColor Green

while ($true) {
    Start-Sleep -Seconds $CheckInterval
    Push-Location $ProjectPath
    git pull origin main 2>&1 | Out-Null
    Pop-Location
    
    $task = Get-TaskInfo
    if ($task -and ($task.TaskId -ne $lastTaskId -or ($task.Status -eq "pending" -and $lastStatus -ne "pending"))) {
        Write-Host "[新任务] $($task.TaskId): $($task.Title)" -ForegroundColor Green
        $lastTaskId = $task.TaskId
        $lastStatus = $task.Status
    }
}
