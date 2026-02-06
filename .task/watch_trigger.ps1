# Watch Trigger Script for Cursor Auto-Execution
# This script monitors .task/workflow_state.json for changes
# When cursor_execute phase is detected, it triggers task execution

$watchPath = "E:\AI_Project\MS\.task"
$stateFile = "E:\AI_Project\MS\.task\workflow_state.json"
$logFile = "E:\AI_Project\MS\.task\watch_log.txt"

function Write-Log {
    param($Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $Message" | Out-File -Append -FilePath $logFile
    Write-Host "$timestamp - $Message"
}

Write-Log "Starting task watcher..."

# Create FileSystemWatcher
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $watchPath
$watcher.Filter = "workflow_state.json"
$watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite
$watcher.EnableRaisingEvents = $true

# Track last processed state to avoid duplicate triggers
$lastProcessedTask = ""

# Check state function
function Check-And-Execute {
    try {
        if (Test-Path $stateFile) {
            $state = Get-Content $stateFile -Raw | ConvertFrom-Json

            if ($state.current_phase -eq "cursor_execute") {
                $currentTask = $state.current_task

                if ($currentTask -ne $lastProcessedTask) {
                    Write-Log "Detected cursor_execute phase for task: $currentTask"

                    # Read current_task.json
                    $taskFile = "E:\AI_Project\MS\.task\current_task.json"
                    if (Test-Path $taskFile) {
                        $task = Get-Content $taskFile -Raw | ConvertFrom-Json
                        Write-Log "Task: $($task.title)"
                        Write-Log "Mode: $($task.mode)"

                        # Signal Cursor to execute
                        # Create a trigger file that Cursor's .cursorrules will detect
                        "EXECUTE:$currentTask:$(Get-Date -Format 'o')" | Out-File "E:\AI_Project\MS\.task\.execute_now"

                        Write-Log "Trigger file created: .execute_now"
                        $script:lastProcessedTask = $currentTask
                    }
                }
            }
        }
    }
    catch {
        Write-Log "Error: $_"
    }
}

# Initial check
Check-And-Execute

# Register event handler
$action = {
    Start-Sleep -Milliseconds 500  # Wait for file write to complete
    Check-And-Execute
}

Register-ObjectEvent -InputObject $watcher -EventName Changed -Action $action

Write-Log "Watcher registered. Monitoring for changes..."

# Keep script running
while ($true) {
    Start-Sleep -Seconds 5

    # Also do periodic check (in case file event was missed)
    Check-And-Execute
}
