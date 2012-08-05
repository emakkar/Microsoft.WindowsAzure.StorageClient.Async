@ECHO OFF

if "%1"=="" (
    ECHO USAGE: %0 version
    EXIT /b 1
)

setlocal
SET OUTDIR=%~dp0Microsoft.WindowsAzure.StorageClient.Async\bin\Release
@echo on
NuGet.exe pack "%~dp0Microsoft.WindowsAzure.StorageClient.Async\Microsoft.WindowsAzure.StorageClient.Async.csproj" -Properties Configuration=Release -Build -OutputDirectory "%OUTDIR%" -Version %1 -Symbols
@echo Package built: "%OUTDIR%Microsoft.WindowsAzure.StorageClient.Async.%1.nupkg"
