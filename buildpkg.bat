@ECHO OFF

if "%1"=="" (
    ECHO USAGE: %0 version
    EXIT /b 1
)

msbuild "%~dp0Microsoft.WindowsAzure.StorageClient.Async\Microsoft.WindowsAzure.StorageClient.Async.csproj" /v:minimal /p:Configuration=Release
IF ERRORLEVEL 1 GOTO END
msbuild "%~dp0Microsoft.WindowsAzure.StorageClient.Async\Microsoft.WindowsAzure.StorageClient.Async.csproj" /v:minimal /p:Configuration=ReleaseNET40
IF ERRORLEVEL 1 GOTO END

setlocal
SET OUTDIR=%~dp0Microsoft.WindowsAzure.StorageClient.Async\bin\Release
NuGet.exe pack "%~dp0Microsoft.WindowsAzure.StorageClient.Async.nuspec" -OutputDirectory "%OUTDIR%" -Version %1 -Symbols
IF ERRORLEVEL 1 GOTO END

@echo Package built: "%OUTDIR%\Microsoft.WindowsAzure.StorageClient.Async.%1.nupkg"
