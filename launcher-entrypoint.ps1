# --- PATH INITIALIZATION ---
# Forces the script to recognize the directory it is sitting in
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
if ([string]::IsNullOrEmpty($PSScriptRoot)) { $PSScriptRoot = Get-Location }
Set-Location $PSScriptRoot

# --- ENVIRONMENT VARIABLES ---
$RepoUrl       = "https://github.com/FeroxFoxxo/MQReawakened.git"
$DefaultUser   = if ($env:DEFAULT_USERNAME) { $env:DEFAULT_USERNAME } else { "admin" }
$DefaultPass   = if ($env:DEFAULT_PASSWORD) { $env:DEFAULT_PASSWORD } else { "admin123" }
$DefaultEmail  = if ($env:DEFAULT_EMAIL)    { $env:DEFAULT_EMAIL }    else { "admin@example.com" }
$DefaultGender = if ($env:DEFAULT_GENDER)   { $env:DEFAULT_GENDER }   else { "Male" }
$DefaultDOB    = if ($env:DEFAULT_DOB)      { $env:DEFAULT_DOB }      else { "01-01-2000" }
$ServerAddress = if ($env:SERVER_ADDRESS)   { $env:SERVER_ADDRESS }   else { "localhost" }
$GamePort      = if ($env:GAME_PORT)        { $env:GAME_PORT }        else { "9339" }

# --- CONFIGURATION & PATHS ---
$BaseDir = $PSScriptRoot
$GameFilesDir = Join-Path $BaseDir "MQData"
$ToolsDir = Join-Path $GameFilesDir "Tools"
$ServerDir = Join-Path $GameFilesDir "Server"

# Portable Tool Paths
$PortableGit = Join-Path $ToolsDir "Git\bin\git.exe"
$PortableDotNet = Join-Path $ToolsDir "dotnet"

# Ensure the MQData container exists
if (!(Test-Path $GameFilesDir)) { New-Item -ItemType Directory -Path $GameFilesDir -Force | Out-Null }

# Internal Path Mapping
$BuildOutputDir = Join-Path $GameFilesDir "app" 
$DataDir        = Join-Path $GameFilesDir "Game\Data"
$KeysDir        = Join-Path $GameFilesDir "Game\Data"
$ArchivesDir    = Join-Path $GameFilesDir "Build\Archives"
$InitProj       = Join-Path $ServerDir "Init\Init.csproj"
$DepsDir        = Join-Path $ServerDir "Server.Reawakened\Dependencies"
$SettingsDir    = Join-Path $DataDir "Settings"
$CachesDir      = Join-Path $DataDir "Caches"

$SettingsFileLocation = Join-Path $SettingsDir "settings.txt"
$CacheInfoLocation    = Join-Path $CachesDir "__info"

# Archive Source Folders
$ClientArchivesDir   = Join-Path $ArchivesDir "Client"
$ClientOverrideDir   = Join-Path $ArchivesDir "ClientOverride"
$CachesArchivesDir   = Join-Path $ArchivesDir "Caches"

# Asset Migration Paths
$SrcAssetsBase  = Join-Path $ServerDir "Server.Reawakened"
$DestAssetsBase = Join-Path $DataDir "Assets"

# --- PORTABLE ENVIRONMENT OVERRIDE ---
if (Test-Path $PortableGit) {
    Write-Host "[entrypoint] Using Portable Git from MQData/Tools" -ForegroundColor Cyan
    function git { & "$PortableGit" @args }
}

if (Test-Path $PortableDotNet) {
    Write-Host "[entrypoint] Using Portable .NET SDK from MQData/Tools" -ForegroundColor Cyan
    $env:DOTNET_ROOT = $PortableDotNet
    $env:PATH = "$PortableDotNet;$env:PATH"
    $env:DOTNET_MULTILEVEL_LOOKUP = 0 # Prevents looking at C:\Program Files
}

# --- HELPERS ---
function Check-Environment {
    $gitCheck = Get-Command git -ErrorAction SilentlyContinue
    $dotnetCheck = Get-Command dotnet -ErrorAction SilentlyContinue
    if (!$gitCheck) { Write-Host "ERROR: Git is not installed or found in Tools!" -ForegroundColor Red; return $false }
    if (!$dotnetCheck) { Write-Host "ERROR: .NET SDK is not installed or found in Tools!" -ForegroundColor Red; return $false }
    return $true
}

function Expand-ArchiveSmart {
    param([System.IO.FileInfo]$Archive, [string]$Destination)
    
    if (!(Test-Path $Destination)) { New-Item -ItemType Directory -Path $Destination -Force | Out-Null }
    Write-Host "  -> Extracting $($Archive.Name)..." -ForegroundColor Gray
    
    # Path to your portable NanaZip
    $NanaZipPath = Join-Path $PSScriptRoot "MQData\Tools\NanaZip\x64\NanaZip.Universal.Console.exe"

    if (Test-Path $NanaZipPath) {
        # x = Extract with full paths
        # -o = Output directory (no space after -o)
        # -y = Assume Yes to all queries (overwrite)
        & "$NanaZipPath" x "$($Archive.FullName)" "-o$Destination" -y | Out-Null
    } 
    else {
        Write-Host "  -> NanaZip not found, falling back to System Zip..." -ForegroundColor Yellow
        if ($Archive.Extension -eq ".7z") {
            Write-Error "Cannot extract .7z files without NanaZip or 7-Zip installed."
        } else {
            Expand-Archive -Path $Archive.FullName -DestinationPath $Destination -Force
        }
    }
}

function Flatten-Directory {
    param([string]$TargetDir)
    while ($true) {
        $items = Get-ChildItem -Path $TargetDir
        if ($items.Count -eq 1 -and $items[0].PSIsContainer) {
            $innerFolder = $items[0].FullName
            Get-ChildItem -Path $innerFolder | Move-Item -Destination $TargetDir -Force
            Remove-Item $innerFolder -Recurse -Force
        } else { break }
    }
}

function Sync-Directory {
    param([string]$Src, [string]$Dest, [string]$Label)
    if (Test-Path $Src) {
        Write-Host "[entrypoint] Syncing $Label..." -ForegroundColor Cyan
        if (Test-Path $Dest) { Remove-Item -Path $Dest -Recurse -Force }
        New-Item -ItemType Directory -Path $Dest -Force | Out-Null
        Copy-Item -Path "$Src\*" -Destination $Dest -Recurse -Force
    }
}

# --- EXECUTION ---
if (!(Check-Environment)) { 
    Write-Host "Please ensure your Tools folder is populated."
    Pause; return 
}

Write-Host "[entrypoint] Initializing MQReawakened" -ForegroundColor White

# --- STEP 1: REPO SYNC & UPDATE CHECK ---
$NeedsClean = $FORCE_REBUILD

# Ensure we use the bundled Git
$env:PATH = "$PSScriptRoot\MQData\Tools\Git\cmd;$env:PATH"

if (Test-Path (Join-Path $ServerDir ".git")) {
    Write-Host "[entrypoint] Checking for updates..." -ForegroundColor Yellow
    
    # Use --git-dir and --work-tree to prevent "not a git repo" errors
    $gitDir = Join-Path $ServerDir ".git"
    $workTree = $ServerDir
    
    # Capture output and errors
    $gitOutput = git --git-dir="$gitDir" --work-tree="$workTree" pull 2>&1
    Write-Host $gitOutput -ForegroundColor Gray

    # Check for changes
    if ($gitOutput -notmatch "Already up to date") {
        Write-Host "[entrypoint] Updates detected! Marking for clean rebuild..." -ForegroundColor Cyan
        $NeedsClean = $true
    }
} 
elseif (!(Test-Path $InitProj)) {
    Write-Host "[entrypoint] Cloning repository into MQData..." -ForegroundColor Yellow
    # Clone into the specific server directory
    git clone $RepoUrl $ServerDir
    $NeedsClean = $true
}

# --- STEP 2: CLEANING (If Updates Found or Forced) ---
if ($NeedsClean) {
    Write-Host "[entrypoint] Cleaning build and cache directories..." -ForegroundColor Red
    
    if (Test-Path $BuildOutputDir) { 
        Remove-Item -Path $BuildOutputDir -Recurse -Force -ErrorAction SilentlyContinue 
    }
    
    if (Test-Path $CachesDir) { 
        Remove-Item -Path $CachesDir -Recurse -Force -ErrorAction SilentlyContinue 
    }
    
    # Remove marker to force cache re-extraction
    if (Test-Path $CacheInfoLocation) { 
        Remove-Item -Path $CacheInfoLocation -Force -ErrorAction SilentlyContinue 
    }
	
	$DownloadsDir = Join-Path $DataDir "Downloads"
	if (Test-Path $DownloadsDir) { 
        Remove-Item -Path $DownloadsDir -Force -ErrorAction SilentlyContinue 
    }
}

# --- STEP 3: CLIENT & DEPENDENCY PREPARATION ---
$Client2014 = Get-ChildItem -Path $ClientArchivesDir -Include *.zip, *.7z -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$ClientOverride = Get-ChildItem -Path $ClientOverrideDir -Include *.zip, *.7z -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -eq $Client2014) {
    Write-Host "CRITICAL: No 2014 client found in $ClientArchivesDir" -ForegroundColor Red
    Pause; return
}

# Dependency Extraction
$ExistingDLLs = Get-ChildItem -Path $DepsDir -Filter *.dll -ErrorAction SilentlyContinue
if ($null -eq $ExistingDLLs -or $ExistingDLLs.Count -eq 0) {
    Write-Host "[entrypoint] Extracting 2014 DLLs for dependencies..." -ForegroundColor Magenta
    $TempDeps = Join-Path $env:TEMP "mq_deps_$(Get-Random)"
    Expand-ArchiveSmart -Archive $Client2014 -Destination $TempDeps
    Flatten-Directory -TargetDir $TempDeps
    $ManagedFolder = Get-ChildItem -Path $TempDeps -Filter "Managed" -Recurse -Directory | Select-Object -First 1
    if ($null -ne $ManagedFolder) {
        if (!(Test-Path $DepsDir)) { New-Item -ItemType Directory -Path $DepsDir -Force | Out-Null }
        Copy-Item -Path "$($ManagedFolder.FullName)\*.dll" -Destination $DepsDir -Force
    }
    Remove-Item $TempDeps -Recurse -Force
}

# Hosting Client Extraction
if (!(Test-Path $SettingsFileLocation)) {
    $TargetArchive = if ($null -ne $ClientOverride) { $ClientOverride } else { $Client2014 }
    Expand-ArchiveSmart -Archive $TargetArchive -Destination $SettingsDir
    Flatten-Directory -TargetDir $SettingsDir
}

# --- STEP 4: CACHE EXTRACTION ---
$CacheArchive = Get-ChildItem -Path $CachesArchivesDir -Include *.zip, *.7z -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -ne $CacheArchive -and !(Test-Path $CacheInfoLocation)) {
    Write-Host "[entrypoint] Caches missing or cleared. Extracting..." -ForegroundColor Magenta
    Expand-ArchiveSmart -Archive $CacheArchive -Destination $CachesDir
    Flatten-Directory -TargetDir $CachesDir
    New-Item -ItemType File -Path $CacheInfoLocation -Force | Out-Null
}

# --- STEP 5: BUILD ---
if (!(Test-Path (Join-Path $BuildOutputDir "Init.dll"))) {
    Write-Host "[entrypoint] Building Server binaries..." -ForegroundColor Green
    dotnet publish "$InitProj" -c Release -o "$BuildOutputDir"
}

# --- STEP 6: MIGRATIONS & LAUNCH ---
if (Test-Path (Join-Path $SrcAssetsBase "Assets\LocalAssets")) {
    Sync-Directory -Src (Join-Path $SrcAssetsBase "Assets\LocalAssets") -Dest (Join-Path $DestAssetsBase "LocalAssets") -Label "Assets"
}

# Launch
Write-Host "[entrypoint] Launching Server..." -ForegroundColor Green
Set-Location $BuildOutputDir

$env:DATA_PATH = $DataDir
$env:BASE_DIRECTORY = $DataDir
$env:KEYS_PATH = $KeysDir
$env:SETTINGS_FILE_LOCATION = $SettingsFileLocation
$env:CACHE_INFO_LOCATION = $CacheInfoLocation
$env:DOTNET_RUNNING_IN_CONTAINER = "true"
$env:DEFAULT_USERNAME = $DefaultUser
$env:DEFAULT_PASSWORD = $DefaultPass
$env:DEFAULT_EMAIL = $DefaultEmail
$env:DEFAULT_GENDER = $DefaultGender
$env:DEFAULT_DOB = $DefaultDOB
$env:SERVER_ADDRESS = $ServerAddress
$env:GAME_PORT = $GamePort

# --- NATIVE SIGNAL DEFINITION ---
$SigHandler = Add-Type -MemberDefinition @'
    [DllImport("kernel32.dll")]
    public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
'@ -Name "Kernel32" -Namespace "Win32" -PassThru

# --- UPDATED LAUNCH LOOP ---
Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host " SERVER IS RUNNING" -ForegroundColor Green
Write-Host " PRESS [ENTER] IN THIS WINDOW TO SHUTDOWN" -ForegroundColor Yellow
Write-Host "===============================================" -ForegroundColor Cyan

# Start the server
$ServerProcess = Start-Process -FilePath "dotnet" -ArgumentList "Init.dll" `
                 -WorkingDirectory $BuildOutputDir -PassThru -NoNewWindow

while (!$ServerProcess.HasExited) {
    if ([Console]::KeyAvailable) {
        $key = [Console]::ReadKey($true)
		if ($key.Key -eq 'Enter') {
            Write-Host "`n[shutdown] Stopping..." -ForegroundColor Cyan
            [Win32.Kernel32]::GenerateConsoleCtrlEvent(0, 0) | Out-Null
            
            if (!$ServerProcess.WaitForExit(5000)) {
                taskkill /PID $ServerProcess.Id /F /T | Out-Null
            }

            Write-Host "[success] Done. Closing..." -ForegroundColor Green
            Start-Sleep -Seconds 2
            [System.Diagnostics.Process]::GetCurrentProcess().Kill()
        }
    }
    Start-Sleep -Milliseconds 200
}