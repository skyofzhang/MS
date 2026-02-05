@echo off
chcp 65001 >nul
title MS Project - AI Pipeline

echo MS Project - AI Game Pipeline
echo.
cd /d E:\AI_Project\MS
git pull origin main
start "Executor" powershell -ExecutionPolicy Bypass -File "auto_executor.ps1"
start "" "%LOCALAPPDATA%\Programs\cursor\Cursor.exe" "E:\AI_Project\MS"
echo Pipeline started!
pause
