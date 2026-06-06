[CmdletBinding()]
param(
    [string]$ApiProject = "src/Host/ECommerce.API/ECommerce.API.csproj",
    [string]$ApiLaunchProfile = "https",
    [string]$FrontendDirectory = "frontend",
    [string]$FrontendCommand = "run dev -- --port 3000",
    [string]$ApiLocalUrl = "https://localhost:7177",
    [string]$FrontendLocalUrl = "https://localhost:3000",
    [string]$DefaultStoreSlug = "cengiz-technics",
    [string]$FrontendTunnelSettingsFile = "scripts/dev-tunnel.settings.ps1",
    [ValidateSet("auto", "quick", "token")]
    [string]$FrontendTunnelMode = "auto",
    [string]$FrontendTunnelToken = $env:FRONTEND_CLOUDFLARED_TUNNEL_TOKEN,
    [string]$FrontendPublicUrl = $env:FRONTEND_PUBLIC_URL,
    [int]$CloudflaredQuickRetryCount = 3,
    [int]$CloudflaredQuickRetryDelaySeconds = 5,
    [switch]$KeepExistingAppProcesses,
    [switch]$KeepExistingTunnelProcesses
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step([string]$Message) {
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Resolve-RepoPath([string]$RelativePath) {
    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\$RelativePath"))
}

function New-RunLogPath {
    param(
        [string]$Directory,
        [string]$BaseName,
        [string]$Extension
    )

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    return Join-Path $Directory "$BaseName.$timestamp.$Extension.log"
}

function Stop-ProcessesByPredicate {
    param(
        [scriptblock]$Predicate,
        [string]$Label
    )

    $processes = Get-CimInstance Win32_Process | Where-Object $Predicate
    if ($processes) {
        $processIds = $processes | Select-Object -ExpandProperty ProcessId
        Write-Host "Stopping existing $Label process(es): $($processIds -join ', ')" -ForegroundColor Yellow
        $processIds | ForEach-Object {
            Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue
        }
    }
}

$repoRoot = Resolve-RepoPath "."
$resolvedApiProject = Resolve-RepoPath $ApiProject
$resolvedFrontendDirectory = Resolve-RepoPath $FrontendDirectory
$startTunnelsScript = Resolve-RepoPath "scripts/start-tunnels.ps1"
$logsDirectory = Join-Path $repoRoot "Logs"

New-Item -ItemType Directory -Path $logsDirectory -Force | Out-Null

$apiOutLog = New-RunLogPath -Directory $logsDirectory -BaseName "api-dev-stack" -Extension "out"
$apiErrLog = New-RunLogPath -Directory $logsDirectory -BaseName "api-dev-stack" -Extension "err"
$frontendOutLog = New-RunLogPath -Directory $logsDirectory -BaseName "frontend-dev-stack" -Extension "out"
$frontendErrLog = New-RunLogPath -Directory $logsDirectory -BaseName "frontend-dev-stack" -Extension "err"

if (-not (Test-Path $startTunnelsScript)) {
    throw "start-tunnels.ps1 could not be found at $startTunnelsScript"
}

Write-Step "Preparing application processes"
if (-not $KeepExistingAppProcesses) {
    Stop-ProcessesByPredicate -Label "API" -Predicate {
        $_.Name -eq "dotnet.exe" -and $_.CommandLine -like "*ECommerce.API.csproj*"
    }

    Stop-ProcessesByPredicate -Label "frontend" -Predicate {
        $commandLine = [string]$_.CommandLine
        $isFrontendCommand = $commandLine.IndexOf($resolvedFrontendDirectory, [System.StringComparison]::OrdinalIgnoreCase) -ge 0

        $_.Name -eq "node.exe" -and $isFrontendCommand -and (
            $commandLine -like "*next*start*" -or
            $commandLine -like "*next*dev*" -or
            $commandLine -like "*npm-cli.js*start*" -or
            $commandLine -like "*npm-cli.js*run dev*"
        )
    }
}

Write-Step "Starting API"
Start-Process `
    -FilePath "dotnet" `
    -ArgumentList @("run", "--launch-profile", $ApiLaunchProfile, "--project", "`"$resolvedApiProject`"") `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput $apiOutLog `
    -RedirectStandardError $apiErrLog `
    -WindowStyle Hidden

Write-Step "Starting frontend"
Start-Process `
    -FilePath "C:\Program Files\nodejs\npm.cmd" `
    -ArgumentList $FrontendCommand `
    -WorkingDirectory $resolvedFrontendDirectory `
    -RedirectStandardOutput $frontendOutLog `
    -RedirectStandardError $frontendErrLog `
    -WindowStyle Hidden

Start-Sleep -Seconds 8

Write-Step "Starting tunnels and syncing config"
$tunnelArguments = @{
    ApiLocalUrl = $ApiLocalUrl
    FrontendLocalUrl = $FrontendLocalUrl
    ApiProject = $ApiProject
    FrontendEnvFile = "frontend/.env.local"
    DefaultStoreSlug = $DefaultStoreSlug
    FrontendTunnelSettingsFile = $FrontendTunnelSettingsFile
    FrontendTunnelMode = $FrontendTunnelMode
    FrontendTunnelToken = $FrontendTunnelToken
    FrontendPublicUrl = $FrontendPublicUrl
    CloudflaredQuickRetryCount = $CloudflaredQuickRetryCount
    CloudflaredQuickRetryDelaySeconds = $CloudflaredQuickRetryDelaySeconds
}

if ($KeepExistingTunnelProcesses) {
    $tunnelArguments["KeepExistingProcesses"] = $true
}

& $startTunnelsScript @tunnelArguments

Write-Step "Dev stack is up"
Write-Host "API logs:" -ForegroundColor Green
Write-Host "  stdout -> $apiOutLog"
Write-Host "  stderr -> $apiErrLog"
Write-Host "Frontend logs:" -ForegroundColor Green
Write-Host "  stdout -> $frontendOutLog"
Write-Host "  stderr -> $frontendErrLog"
Write-Host ""
Write-Host "If this is the first run after config changes, wait a few seconds and open the public frontend URL shown above." -ForegroundColor Yellow
