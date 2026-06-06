[CmdletBinding()]
param(
    [string]$ApiLocalUrl = "https://localhost:7177",
    [string]$FrontendLocalUrl = "https://localhost:3000",
    [string]$ApiProject = "src/Host/ECommerce.API/ECommerce.API.csproj",
    [string]$FrontendEnvFile = "frontend/.env.local",
    [string]$DefaultStoreSlug = "cengiz-technics",
    [string]$NgrokPath = "ngrok",
    [string]$CloudflaredPath = "cloudflared",
    [string]$SshPath = "ssh",
    [string]$FrontendTunnelSettingsFile = "scripts/dev-tunnel.settings.ps1",
    [ValidateSet("auto", "quick", "token", "localhostrun")]
    [string]$FrontendTunnelMode = "auto",
    [string]$FrontendTunnelToken = $env:FRONTEND_CLOUDFLARED_TUNNEL_TOKEN,
    [string]$FrontendPublicUrl = $env:FRONTEND_PUBLIC_URL,
    [int]$CloudflaredQuickRetryCount = 3,
    [int]$CloudflaredQuickRetryDelaySeconds = 5,
    [switch]$KeepExistingProcesses
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

function Read-FrontendTunnelSettings {
    param(
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        return @{}
    }

    $settings = & $FilePath
    if ($null -eq $settings) {
        return @{}
    }

    if ($settings -isnot [System.Collections.IDictionary]) {
        throw "Frontend tunnel settings file must return a hashtable/dictionary: $FilePath"
    }

    return $settings
}

function Remove-FileIfExists {
    param(
        [string]$Path
    )

    if (Test-Path $Path) {
        Remove-Item -Path $Path -Force
    }
}

function Stop-ExistingTunnelProcesses {
    param(
        [string]$ProcessName
    )

    $processes = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($processes) {
        Write-Host "Stopping existing $ProcessName process(es): $($processes.Id -join ', ')" -ForegroundColor Yellow
        $processes | Stop-Process -Force
    }
}

function Stop-ProcessesByCommandLinePattern {
    param(
        [string]$ProcessName,
        [string]$CommandLinePattern,
        [string]$Label
    )

    $processes = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -eq $ProcessName -and $_.CommandLine -like $CommandLinePattern }

    if ($processes) {
        $processIds = $processes | Select-Object -ExpandProperty ProcessId
        Write-Host "Stopping existing $Label process(es): $($processIds -join ', ')" -ForegroundColor Yellow
        $processIds | ForEach-Object {
            Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue
        }
    }
}

function Wait-Until {
    param(
        [scriptblock]$Condition,
        [string]$ErrorMessage,
        [int]$TimeoutSeconds = 45,
        [int]$PollIntervalMilliseconds = 1000
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        $result = & $Condition
        if ($result) {
            return $result
        }

        Start-Sleep -Milliseconds $PollIntervalMilliseconds
    }

    throw $ErrorMessage
}

function Get-NgrokPublicUrl {
    try {
        $response = Invoke-RestMethod -Uri "http://127.0.0.1:4040/api/tunnels" -Method Get
        $httpsTunnel = $response.tunnels | Where-Object { $_.public_url -like "https://*" } | Select-Object -First 1
        return $httpsTunnel.public_url
    }
    catch {
        return $null
    }
}

function Get-CloudflaredPublicUrl {
    param(
        [string[]]$LogFiles
    )

    foreach ($logFile in $LogFiles) {
        if (-not (Test-Path $logFile)) {
            continue
        }

        $content = Get-Content -Path $logFile -Raw -ErrorAction SilentlyContinue
        if (-not $content) {
            continue
        }

        $matches = [regex]::Matches($content, 'https://[a-z0-9-]+\.trycloudflare\.com')
        foreach ($match in $matches) {
            $candidateUrl = $match.Value
            $candidateHost = ([Uri]$candidateUrl).Host.ToLowerInvariant()
            if ($candidateHost -eq "api.trycloudflare.com") {
                continue
            }

            return $candidateUrl
        }
    }

    return $null
}

function Test-CloudflaredTunnelRegistered {
    param(
        [string[]]$LogFiles
    )

    foreach ($logFile in $LogFiles) {
        if (-not (Test-Path $logFile)) {
            continue
        }

        $content = Get-Content -Path $logFile -Raw -ErrorAction SilentlyContinue
        if (-not $content) {
            continue
        }

        if ($content -match "Registered tunnel connection") {
            return $true
        }
    }

    return $false
}

function Get-LocalhostRunPublicUrl {
    param(
        [string[]]$LogFiles
    )

    foreach ($logFile in $LogFiles) {
        if (-not (Test-Path $logFile)) {
            continue
        }

        $content = Get-Content -Path $logFile -Raw -ErrorAction SilentlyContinue
        if (-not $content) {
            continue
        }

        $match = [regex]::Match($content, 'tunneled with tls termination,\s*(https://[^\s]+)')
        if ($match.Success) {
            return $match.Groups[1].Value
        }
    }

    return $null
}

function Get-CloudflaredConfigConflicts {
    $configDirectory = Join-Path $env:USERPROFILE ".cloudflared"
    return @(
        Join-Path $configDirectory "config.yml"
        Join-Path $configDirectory "config.yaml"
    ) | Where-Object { Test-Path $_ }
}

function Get-CloudflaredLogTail {
    param(
        [string[]]$LogFiles,
        [int]$TailLineCount = 20
    )

    $entries = foreach ($logFile in $LogFiles) {
        if (-not (Test-Path $logFile)) {
            continue
        }

        Get-Content -Path $logFile -Tail $TailLineCount -ErrorAction SilentlyContinue |
            ForEach-Object { "[${logFile}] $_" }
    }

    if (-not $entries) {
        return "No cloudflared log output was captured."
    }

    return ($entries -join [Environment]::NewLine)
}

function Resolve-FrontendTunnelModeValue {
    param(
        [string]$Mode,
        [string]$Token,
        [string]$PublicUrl
    )

    if ($Mode -eq "auto") {
        return "auto"
    }

    return $Mode
}

function Test-PublicUrlReachable {
    param(
        [string]$Url
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -Method Head -MaximumRedirection 0 -TimeoutSec 10 -UseBasicParsing
        return $response.StatusCode -ge 200 -and $response.StatusCode -lt 400
    }
    catch {
        $webResponse = $_.Exception.Response
        if ($null -eq $webResponse) {
            return $false
        }

        $statusCode = [int]$webResponse.StatusCode
        return $statusCode -ge 200 -and $statusCode -lt 400
    }
}

function Start-CloudflaredQuickTunnel {
    param(
        [string]$CloudflaredExecutable,
        [string]$FrontendUrl,
        [string]$WorkingDirectory,
        [string]$StdOutLog,
        [string]$StdErrLog,
        [int]$RetryCount,
        [int]$RetryDelaySeconds
    )

    $configConflicts = @(Get-CloudflaredConfigConflicts)
    if ($configConflicts.Count -gt 0) {
        $conflictList = $configConflicts -join ", "
        throw "Cloudflare quick tunnels are not supported while a .cloudflared config file exists. Rename or remove: $conflictList. Alternatively, use a named tunnel with -FrontendTunnelMode token and provide -FrontendTunnelToken plus -FrontendPublicUrl, or create $resolvedFrontendTunnelSettingsFile from scripts/dev-tunnel.settings.example.ps1."
    }

    for ($attempt = 1; $attempt -le $RetryCount; $attempt++) {
        Remove-FileIfExists -Path $StdOutLog
        Remove-FileIfExists -Path $StdErrLog

        $frontendUri = [Uri]$FrontendUrl
        $cloudflaredArgs = @("tunnel", "--url", $FrontendUrl, "--no-autoupdate")
        if ($frontendUri.Scheme -eq "https") {
            $cloudflaredArgs += "--no-tls-verify"
        }

        $process = Start-Process `
            -FilePath $CloudflaredExecutable `
            -ArgumentList $cloudflaredArgs `
            -WorkingDirectory $WorkingDirectory `
            -RedirectStandardOutput $StdOutLog `
            -RedirectStandardError $StdErrLog `
            -WindowStyle Hidden `
            -PassThru

        try {
            $publicUrl = Wait-Until `
                -Condition { Get-CloudflaredPublicUrl -LogFiles @($StdOutLog, $StdErrLog) } `
                -ErrorMessage "Cloudflare quick tunnel URL could not be resolved." `
                -TimeoutSeconds 30

            Wait-Until `
                -Condition {
                    if ($process.HasExited) {
                        return $null
                    }

                    if (Test-CloudflaredTunnelRegistered -LogFiles @($StdOutLog, $StdErrLog)) {
                        return $publicUrl
                    }

                    return $null
                } `
                -ErrorMessage "Cloudflare quick tunnel URL was announced, but the tunnel connection was not registered." `
                -TimeoutSeconds 45 | Out-Null

            try {
                Wait-Until `
                    -Condition {
                        if ($process.HasExited) {
                            return $null
                        }

                        if (Test-PublicUrlReachable -Url $publicUrl) {
                            return $publicUrl
                        }

                        return $null
                    } `
                    -ErrorMessage "Cloudflare quick tunnel URL was announced and connected, but it did not pass the local public reachability check yet." `
                    -TimeoutSeconds 20 | Out-Null
            }
            catch {
                Write-Host "$($_.Exception.Message) Continuing with announced URL: $publicUrl" -ForegroundColor Yellow
            }

            return $publicUrl
        }
        catch {
            if (-not $process.HasExited) {
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }

            if ($attempt -lt $RetryCount) {
                Write-Host "Cloudflare quick tunnel attempt $attempt failed. Retrying in $RetryDelaySeconds second(s)..." -ForegroundColor Yellow
                Start-Sleep -Seconds $RetryDelaySeconds
                continue
            }

            $logTail = Get-CloudflaredLogTail -LogFiles @($StdOutLog, $StdErrLog)
            throw "Cloudflare quick tunnel failed after $RetryCount attempt(s). Recent log output:`n$logTail`nFor a stable setup, create a named Cloudflare Tunnel and rerun with -FrontendTunnelMode token -FrontendTunnelToken <token> -FrontendPublicUrl <public-url>, or create $resolvedFrontendTunnelSettingsFile from scripts/dev-tunnel.settings.example.ps1."
        }
    }

    throw "Cloudflare quick tunnel failed unexpectedly."
}

function Start-CloudflaredTokenTunnel {
    param(
        [string]$CloudflaredExecutable,
        [string]$TunnelToken,
        [string]$PublicUrl,
        [string]$WorkingDirectory,
        [string]$StdOutLog,
        [string]$StdErrLog
    )

    if ([string]::IsNullOrWhiteSpace($TunnelToken)) {
        throw "Frontend tunnel mode 'token' requires -FrontendTunnelToken or FRONTEND_CLOUDFLARED_TUNNEL_TOKEN."
    }

    if ([string]::IsNullOrWhiteSpace($PublicUrl)) {
        throw "Frontend tunnel mode 'token' requires -FrontendPublicUrl or FRONTEND_PUBLIC_URL."
    }

    Remove-FileIfExists -Path $StdOutLog
    Remove-FileIfExists -Path $StdErrLog

    $process = Start-Process `
        -FilePath $CloudflaredExecutable `
        -ArgumentList @("tunnel", "--no-autoupdate", "run", "--token", $TunnelToken) `
        -WorkingDirectory $WorkingDirectory `
        -RedirectStandardOutput $StdOutLog `
        -RedirectStandardError $StdErrLog `
        -WindowStyle Hidden `
        -PassThru

    try {
        $verifiedUrl = Wait-Until `
            -Condition {
                if ($process.HasExited) {
                    return $null
                }

                if (Test-PublicUrlReachable -Url $PublicUrl) {
                    return $PublicUrl
                }

                return $null
            } `
            -ErrorMessage "Cloudflare named tunnel process started, but the configured frontend public URL did not become reachable." `
            -TimeoutSeconds 45

        return $verifiedUrl
    }
    catch {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        }

        $logTail = Get-CloudflaredLogTail -LogFiles @($StdOutLog, $StdErrLog)
        throw "Cloudflare named tunnel failed. Recent log output:`n$logTail"
    }
}

function Start-LocalhostRunTunnel {
    param(
        [string]$SshExecutable,
        [string]$FrontendUrl,
        [string]$WorkingDirectory,
        [string]$StdOutLog,
        [string]$StdErrLog
    )

    Remove-FileIfExists -Path $StdOutLog
    Remove-FileIfExists -Path $StdErrLog

    $frontendUri = [Uri]$FrontendUrl
    if ($frontendUri.Scheme -ne "http") {
        throw "localhost.run fallback requires an HTTP frontend origin. Current FrontendLocalUrl is '$FrontendUrl'. Use Cloudflare quick/token for the HTTPS Next.js dev server, or run the frontend without --experimental-https and pass -FrontendLocalUrl http://localhost:3000."
    }

    $forwardTarget = "80:{0}:{1}" -f $frontendUri.Host, $frontendUri.Port

    $localhostRunArgs = @(
        "-o", "StrictHostKeyChecking=no",
        "-o", "ExitOnForwardFailure=yes",
        "-o", "ServerAliveInterval=30",
        "-R", $forwardTarget,
        "nokey@localhost.run"
    )

    $process = Start-Process `
        -FilePath $SshExecutable `
        -ArgumentList $localhostRunArgs `
        -WorkingDirectory $WorkingDirectory `
        -RedirectStandardOutput $StdOutLog `
        -RedirectStandardError $StdErrLog `
        -WindowStyle Hidden `
        -PassThru

    try {
        $publicUrl = Wait-Until `
            -Condition { Get-LocalhostRunPublicUrl -LogFiles @($StdOutLog, $StdErrLog) } `
            -ErrorMessage "localhost.run public URL could not be resolved from the SSH tunnel output." `
            -TimeoutSeconds 30

        $verifiedUrl = Wait-Until `
            -Condition {
                if ($process.HasExited) {
                    return $null
                }

                if (Test-PublicUrlReachable -Url $publicUrl) {
                    return $publicUrl
                }

                return $null
            } `
            -ErrorMessage "localhost.run public URL was announced, but it did not become publicly reachable." `
            -TimeoutSeconds 30

        return $verifiedUrl
    }
    catch {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        }

        $logTail = Get-CloudflaredLogTail -LogFiles @($StdOutLog, $StdErrLog)
        throw "localhost.run tunnel failed. Recent log output:`n$logTail"
    }
}

function Set-Or-AppendEnvValue {
    param(
        [string]$FilePath,
        [string]$Key,
        [string]$Value
    )

    $lines = @()
    if (Test-Path $FilePath) {
        $lines = [System.Collections.Generic.List[string]](Get-Content -Path $FilePath)
    }
    else {
        $directory = Split-Path -Path $FilePath -Parent
        if ($directory) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
    }

    $updated = $false
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match "^${Key}=") {
            $lines[$index] = "${Key}=${Value}"
            $updated = $true
            break
        }
    }

    if (-not $updated) {
        $lines.Add("${Key}=${Value}") | Out-Null
    }

    [System.IO.File]::WriteAllLines($FilePath, $lines)
}

$repoRoot = Resolve-RepoPath "."
$resolvedApiProject = Resolve-RepoPath $ApiProject
$resolvedFrontendEnvFile = Resolve-RepoPath $FrontendEnvFile
$resolvedFrontendTunnelSettingsFile = Resolve-RepoPath $FrontendTunnelSettingsFile
$logsDirectory = Join-Path $repoRoot "Logs"
$ngrokOutLog = Join-Path $logsDirectory "ngrok-tunnel.out.log"
$ngrokErrLog = Join-Path $logsDirectory "ngrok-tunnel.err.log"
$cloudflaredOutLog = Join-Path $logsDirectory "cloudflared-tunnel.out.log"
$cloudflaredErrLog = Join-Path $logsDirectory "cloudflared-tunnel.err.log"
$localhostRunOutLog = Join-Path $logsDirectory "localhostrun-tunnel.out.log"
$localhostRunErrLog = Join-Path $logsDirectory "localhostrun-tunnel.err.log"

New-Item -ItemType Directory -Path $logsDirectory -Force | Out-Null

$frontendTunnelSettings = Read-FrontendTunnelSettings -FilePath $resolvedFrontendTunnelSettingsFile

if ($FrontendTunnelMode -eq "auto" -and $frontendTunnelSettings.Contains("FrontendTunnelMode")) {
    $FrontendTunnelMode = [string]$frontendTunnelSettings["FrontendTunnelMode"]
}

if ([string]::IsNullOrWhiteSpace($FrontendTunnelToken) -and $frontendTunnelSettings.Contains("FrontendTunnelToken")) {
    $FrontendTunnelToken = [string]$frontendTunnelSettings["FrontendTunnelToken"]
}

if ([string]::IsNullOrWhiteSpace($FrontendPublicUrl) -and $frontendTunnelSettings.Contains("FrontendPublicUrl")) {
    $FrontendPublicUrl = [string]$frontendTunnelSettings["FrontendPublicUrl"]
}

Write-Step "Preparing tunnel processes"
if (-not $KeepExistingProcesses) {
    Stop-ExistingTunnelProcesses -ProcessName "ngrok"
    Stop-ExistingTunnelProcesses -ProcessName "cloudflared"
    Stop-ProcessesByCommandLinePattern -ProcessName "ssh.exe" -CommandLinePattern "*localhost.run*" -Label "localhost.run"
}

Write-Step "Starting ngrok for API"
Remove-FileIfExists -Path $ngrokOutLog
Remove-FileIfExists -Path $ngrokErrLog

Start-Process `
    -FilePath $NgrokPath `
    -ArgumentList @("http", $ApiLocalUrl, "--host-header=rewrite", "--log=stdout") `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput $ngrokOutLog `
    -RedirectStandardError $ngrokErrLog `
    -WindowStyle Hidden

$ngrokPublicUrl = Wait-Until `
    -Condition { Get-NgrokPublicUrl } `
    -ErrorMessage "ngrok public URL could not be resolved from http://127.0.0.1:4040/api/tunnels."

$resolvedFrontendTunnelMode = Resolve-FrontendTunnelModeValue `
    -Mode $FrontendTunnelMode `
    -Token $FrontendTunnelToken `
    -PublicUrl $FrontendPublicUrl

$actualFrontendTunnelMode = $resolvedFrontendTunnelMode
Write-Step "Starting frontend tunnel ($resolvedFrontendTunnelMode)"

if ($resolvedFrontendTunnelMode -eq "auto") {
    $frontendTunnelErrors = [System.Collections.Generic.List[string]]::new()
    $frontendPublicUrl = $null

    if (-not [string]::IsNullOrWhiteSpace($FrontendTunnelToken) -and -not [string]::IsNullOrWhiteSpace($FrontendPublicUrl)) {
        try {
            $frontendPublicUrl = Start-CloudflaredTokenTunnel `
                -CloudflaredExecutable $CloudflaredPath `
                -TunnelToken $FrontendTunnelToken `
                -PublicUrl $FrontendPublicUrl `
                -WorkingDirectory $repoRoot `
                -StdOutLog $cloudflaredOutLog `
                -StdErrLog $cloudflaredErrLog
            $actualFrontendTunnelMode = "token"
        }
        catch {
            $frontendTunnelErrors.Add($_.Exception.Message) | Out-Null
            Write-Host "Named Cloudflare Tunnel failed. Falling back to quick tunnel..." -ForegroundColor Yellow
        }
    }

    if (-not $frontendPublicUrl) {
        try {
            $frontendPublicUrl = Start-CloudflaredQuickTunnel `
                -CloudflaredExecutable $CloudflaredPath `
                -FrontendUrl $FrontendLocalUrl `
                -WorkingDirectory $repoRoot `
                -StdOutLog $cloudflaredOutLog `
                -StdErrLog $cloudflaredErrLog `
                -RetryCount $CloudflaredQuickRetryCount `
                -RetryDelaySeconds $CloudflaredQuickRetryDelaySeconds
            $actualFrontendTunnelMode = "quick"
        }
        catch {
            $frontendTunnelErrors.Add($_.Exception.Message) | Out-Null
            Write-Host "Cloudflare quick tunnel failed. Falling back to localhost.run..." -ForegroundColor Yellow
        }
    }

    if (-not $frontendPublicUrl) {
        try {
            $frontendPublicUrl = Start-LocalhostRunTunnel `
                -SshExecutable $SshPath `
                -FrontendUrl $FrontendLocalUrl `
                -WorkingDirectory $repoRoot `
                -StdOutLog $localhostRunOutLog `
                -StdErrLog $localhostRunErrLog
            $actualFrontendTunnelMode = "localhostrun"
        }
        catch {
            $frontendTunnelErrors.Add($_.Exception.Message) | Out-Null
        }
    }

    if (-not $frontendPublicUrl) {
        throw ($frontendTunnelErrors -join "`n`n")
    }
}
else {
    $frontendPublicUrl = switch ($resolvedFrontendTunnelMode) {
        "token" {
            $actualFrontendTunnelMode = "token"
            Start-CloudflaredTokenTunnel `
                -CloudflaredExecutable $CloudflaredPath `
                -TunnelToken $FrontendTunnelToken `
                -PublicUrl $FrontendPublicUrl `
                -WorkingDirectory $repoRoot `
                -StdOutLog $cloudflaredOutLog `
                -StdErrLog $cloudflaredErrLog
        }
        "quick" {
            $actualFrontendTunnelMode = "quick"
            Start-CloudflaredQuickTunnel `
                -CloudflaredExecutable $CloudflaredPath `
                -FrontendUrl $FrontendLocalUrl `
                -WorkingDirectory $repoRoot `
                -StdOutLog $cloudflaredOutLog `
                -StdErrLog $cloudflaredErrLog `
                -RetryCount $CloudflaredQuickRetryCount `
                -RetryDelaySeconds $CloudflaredQuickRetryDelaySeconds
        }
        "localhostrun" {
            $actualFrontendTunnelMode = "localhostrun"
            Start-LocalhostRunTunnel `
                -SshExecutable $SshPath `
                -FrontendUrl $FrontendLocalUrl `
                -WorkingDirectory $repoRoot `
                -StdOutLog $localhostRunOutLog `
                -StdErrLog $localhostRunErrLog
        }
        default {
            throw "Unsupported frontend tunnel mode: $resolvedFrontendTunnelMode"
        }
    }
}

Write-Step "Updating frontend environment file"
Set-Or-AppendEnvValue -FilePath $resolvedFrontendEnvFile -Key "NEXT_PUBLIC_API_BASE_URL" -Value $ApiLocalUrl
Set-Or-AppendEnvValue -FilePath $resolvedFrontendEnvFile -Key "NEXT_PUBLIC_DEFAULT_STORE_SLUG" -Value $DefaultStoreSlug
Set-Or-AppendEnvValue -FilePath $resolvedFrontendEnvFile -Key "NODE_TLS_REJECT_UNAUTHORIZED" -Value "0"

Write-Step "Updating API user-secrets"
dotnet user-secrets set "Frontend:BaseUrl" $frontendPublicUrl --project $resolvedApiProject | Out-Null
dotnet user-secrets set "Modules:Payment:Iyzico:CallbackUrl" "$ngrokPublicUrl/api/payments/callbacks/iyzico/checkout-form" --project $resolvedApiProject | Out-Null

Write-Step "Tunnel setup completed"
Write-Host "Frontend public URL : $frontendPublicUrl" -ForegroundColor Green
Write-Host "API public URL      : $ngrokPublicUrl" -ForegroundColor Green
Write-Host "Frontend API base   : $ApiLocalUrl" -ForegroundColor Green
Write-Host "Iyzico callback URL : $ngrokPublicUrl/api/payments/callbacks/iyzico/checkout-form" -ForegroundColor Green
Write-Host "Frontend tunnel mode: $actualFrontendTunnelMode" -ForegroundColor Green
Write-Host ""
Write-Host "If API or frontend are already running, restart them so they pick up the new configuration." -ForegroundColor Yellow
Write-Host "Frontend env file updated: $resolvedFrontendEnvFile"
Write-Host "Tunnel logs:"
Write-Host "  ngrok stdout -> $ngrokOutLog"
Write-Host "  ngrok stderr -> $ngrokErrLog"
Write-Host "  cloudflared stdout -> $cloudflaredOutLog"
Write-Host "  cloudflared stderr -> $cloudflaredErrLog"
Write-Host "  localhost.run stdout -> $localhostRunOutLog"
Write-Host "  localhost.run stderr -> $localhostRunErrLog"
