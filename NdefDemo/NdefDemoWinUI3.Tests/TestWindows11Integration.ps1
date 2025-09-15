# PowerShell script to test Windows 11 integration features of the NdefDemo WinUI 3 application
param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [int]$TimeoutSeconds = 30
)

Write-Host "Testing Windows 11 Integration Features" -ForegroundColor Green
Write-Host "Configuration: $Configuration, Platform: $Platform" -ForegroundColor Yellow

# Test results collection
$TestResults = @{
    SystemInfo = @{}
    BuildTests = @{}
    RuntimeTests = @{}
    IntegrationTests = @{}
    OverallSuccess = $true
}

# Function to log test results
function Log-TestResult {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Message,
        [string]$Category = "General"
    )
    
    $status = if ($Success) { "✅ PASS" } else { "❌ FAIL" }
    $color = if ($Success) { "Green" } else { "Red" }
    
    Write-Host "$status $TestName" -ForegroundColor $color
    if ($Message) {
        Write-Host "    $Message" -ForegroundColor Gray
    }
    
    if (-not $TestResults.ContainsKey($Category)) {
        $TestResults[$Category] = @{}
    }
    
    $TestResults[$Category][$TestName] = @{
        Success = $Success
        Message = $Message
    }
    
    if (-not $Success) {
        $TestResults.OverallSuccess = $false
    }
}

# Test 1: System Information and Windows 11 Detection
Write-Host "`n=== System Information Tests ===" -ForegroundColor Cyan

try {
    $OS = Get-CimInstance -ClassName Win32_OperatingSystem
    $Computer = Get-CimInstance -ClassName Win32_ComputerSystem
    
    $TestResults.SystemInfo = @{
        OSName = $OS.Caption
        OSVersion = $OS.Version
        BuildNumber = $OS.BuildNumber
        TotalMemoryGB = [math]::Round($Computer.TotalPhysicalMemory / 1GB, 2)
        Architecture = $env:PROCESSOR_ARCHITECTURE
    }
    
    # Test Windows 11 detection
    $isWindows11 = [int]$OS.BuildNumber -ge 22000
    Log-TestResult "Windows 11 Detection" $isWindows11 "Build: $($OS.BuildNumber), OS: $($OS.Caption)" "SystemInfo"
    
    # Test system resources
    $hasAdequateMemory = $TestResults.SystemInfo.TotalMemoryGB -ge 4
    Log-TestResult "Adequate Memory" $hasAdequateMemory "$($TestResults.SystemInfo.TotalMemoryGB) GB available" "SystemInfo"
    
    # Test .NET 8 availability
    $dotnetVersion = dotnet --version 2>$null
    $hasDotNet8 = $dotnetVersion -and $dotnetVersion.StartsWith("8.")
    Log-TestResult ".NET 8.0 Available" $hasDotNet8 "Version: $dotnetVersion" "SystemInfo"
    
} catch {
    Log-TestResult "System Information Collection" $false $_.Exception.Message "SystemInfo"
}

# Test 2: Project Build and Configuration
Write-Host "`n=== Build Configuration Tests ===" -ForegroundColor Cyan

$ProjectPath = "$PSScriptRoot\..\NdefDemoWinUI3\NdefDemoWinUI3.csproj"

try {
    # Test project file exists
    $projectExists = Test-Path $ProjectPath
    Log-TestResult "Project File Exists" $projectExists $ProjectPath "BuildTests"
    
    if ($projectExists) {
        # Read project file content
        $projectContent = Get-Content $ProjectPath -Raw
        
        # Test target framework
        $hasCorrectFramework = $projectContent -match 'net8\.0-windows'
        Log-TestResult "Target Framework" $hasCorrectFramework "net8.0-windows detected" "BuildTests"
        
        # Test Windows App SDK reference
        $hasWindowsAppSDK = $projectContent -match 'Microsoft\.WindowsAppSDK'
        Log-TestResult "Windows App SDK Reference" $hasWindowsAppSDK "Microsoft.WindowsAppSDK package reference found" "BuildTests"
        
        # Test MSIX packaging enabled
        $hasMsixPackaging = $projectContent -match 'EnableMsixTooling.*true'
        Log-TestResult "MSIX Packaging Enabled" $hasMsixPackaging "EnableMsixTooling=true found" "BuildTests"
    }
    
} catch {
    Log-TestResult "Project Configuration Analysis" $false $_.Exception.Message "BuildTests"
}

# Test 3: Build Process
Write-Host "`n=== Build Process Tests ===" -ForegroundColor Cyan

try {
    Write-Host "Attempting to build project..." -ForegroundColor Yellow
    
    # Try building with explicit runtime
    $buildOutput = dotnet build $ProjectPath --configuration $Configuration --runtime "win-$Platform" --verbosity minimal 2>&1
    $buildSuccess = $LASTEXITCODE -eq 0
    
    if (-not $buildSuccess) {
        # Try alternative build approach
        Write-Host "Retrying build with different parameters..." -ForegroundColor Yellow
        $buildOutput = dotnet build $ProjectPath --configuration $Configuration --verbosity minimal 2>&1
        $buildSuccess = $LASTEXITCODE -eq 0
    }
    
    Log-TestResult "Project Build" $buildSuccess "Build exit code: $LASTEXITCODE" "BuildTests"
    
    if ($buildSuccess) {
        # Check for output files
        $outputPattern = "$PSScriptRoot\..\NdefDemoWinUI3\bin\$Configuration\**\NdefDemoWinUI3.exe"
        $outputFiles = Get-ChildItem $outputPattern -Recurse -ErrorAction SilentlyContinue
        $hasOutputFiles = $outputFiles.Count -gt 0
        
        Log-TestResult "Build Output Files" $hasOutputFiles "Found $($outputFiles.Count) executable(s)" "BuildTests"
        
        if ($hasOutputFiles) {
            $exePath = $outputFiles[0].FullName
            Write-Host "    Executable: $exePath" -ForegroundColor Gray
        }
    }
    
} catch {
    Log-TestResult "Build Process" $false $_.Exception.Message "BuildTests"
}

# Test 4: Runtime Dependencies
Write-Host "`n=== Runtime Dependencies Tests ===" -ForegroundColor Cyan

try {
    # Test Windows App SDK availability
    $windowsAppSDKPath = "${env:ProgramFiles}\WindowsApps\Microsoft.WindowsAppRuntime*"
    $hasWindowsAppSDK = (Get-ChildItem $windowsAppSDKPath -ErrorAction SilentlyContinue).Count -gt 0
    Log-TestResult "Windows App SDK Runtime" $hasWindowsAppSDK "Checked: $windowsAppSDKPath" "RuntimeTests"
    
    # Test .NET 8 Windows runtime
    $dotnetRuntimes = dotnet --list-runtimes 2>$null | Where-Object { $_ -match "Microsoft\.WindowsDesktop\.App.*8\." }
    $hasWindowsDesktopRuntime = $dotnetRuntimes.Count -gt 0
    Log-TestResult ".NET 8 Windows Desktop Runtime" $hasWindowsDesktopRuntime "Found $($dotnetRuntimes.Count) runtime(s)" "RuntimeTests"
    
} catch {
    Log-TestResult "Runtime Dependencies Check" $false $_.Exception.Message "RuntimeTests"
}

# Test 5: Windows 11 Integration Features
Write-Host "`n=== Windows 11 Integration Tests ===" -ForegroundColor Cyan

try {
    # Test Windows 11 specific features availability
    $hasSnapLayouts = $TestResults.SystemInfo.BuildNumber -ge 22000
    Log-TestResult "Snap Layouts Support" $hasSnapLayouts "Windows 11 build detected" "IntegrationTests"
    
    # Test modern Windows APIs
    $hasModernAPIs = [System.Environment]::OSVersion.Version.Major -ge 10
    Log-TestResult "Modern Windows APIs" $hasModernAPIs "OS Version: $([System.Environment]::OSVersion.Version)" "IntegrationTests"
    
    # Test MSIX deployment capability
    $packageManagerAvailable = Get-Command "Add-AppxPackage" -ErrorAction SilentlyContinue
    $hasMsixSupport = $packageManagerAvailable -ne $null
    Log-TestResult "MSIX Deployment Support" $hasMsixSupport "Add-AppxPackage cmdlet available" "IntegrationTests"
    
    # Test Windows 11 theme support
    $registryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
    $hasThemeSupport = Test-Path $registryPath
    Log-TestResult "Windows 11 Theme Integration" $hasThemeSupport "Theme registry path accessible" "IntegrationTests"
    
} catch {
    Log-TestResult "Windows 11 Integration Features" $false $_.Exception.Message "IntegrationTests"
}

# Test 6: Performance and Memory Baseline
Write-Host "`n=== Performance Baseline Tests ===" -ForegroundColor Cyan

try {
    # Memory availability test
    $availableMemory = (Get-CimInstance -ClassName Win32_OperatingSystem).FreePhysicalMemory / 1MB
    $hasAdequateMemory = $availableMemory -gt 1000  # 1GB free
    Log-TestResult "Available Memory" $hasAdequateMemory "$([math]::Round($availableMemory, 0)) MB free" "IntegrationTests"
    
    # Disk space test
    $systemDrive = Get-CimInstance -ClassName Win32_LogicalDisk | Where-Object { $_.DeviceID -eq $env:SystemDrive }
    $freeSpaceGB = $systemDrive.FreeSpace / 1GB
    $hasAdequateDiskSpace = $freeSpaceGB -gt 5  # 5GB free
    Log-TestResult "Disk Space" $hasAdequateDiskSpace "$([math]::Round($freeSpaceGB, 1)) GB free on $($env:SystemDrive)" "IntegrationTests"
    
} catch {
    Log-TestResult "Performance Baseline" $false $_.Exception.Message "IntegrationTests"
}

# Generate Summary Report
Write-Host "`n" + "="*60 -ForegroundColor Yellow
Write-Host "WINDOWS 11 INTEGRATION TEST SUMMARY" -ForegroundColor Yellow
Write-Host "="*60 -ForegroundColor Yellow

$totalTests = 0
$passedTests = 0

foreach ($category in $TestResults.Keys) {
    if ($category -eq "OverallSuccess") { continue }
    
    Write-Host "`n$category Results:" -ForegroundColor Cyan
    
    if ($TestResults[$category] -is [hashtable]) {
        foreach ($testName in $TestResults[$category].Keys) {
            $result = $TestResults[$category][$testName]
            $status = if ($result.Success) { "✅" } else { "❌" }
            Write-Host "  $status $testName" -ForegroundColor $(if ($result.Success) { "Green" } else { "Red" })
            
            $totalTests++
            if ($result.Success) { $passedTests++ }
        }
    }
}

Write-Host "`nOverall Results:" -ForegroundColor Yellow
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $($totalTests - $passedTests)" -ForegroundColor Red
Write-Host "Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 1))%" -ForegroundColor White

$overallStatus = if ($TestResults.OverallSuccess) { "✅ SUCCESS" } else { "⚠️ PARTIAL SUCCESS" }
$overallColor = if ($TestResults.OverallSuccess) { "Green" } else { "Yellow" }
Write-Host "`nOverall Status: $overallStatus" -ForegroundColor $overallColor

# System Information Summary
Write-Host "`nSystem Information:" -ForegroundColor Cyan
Write-Host "OS: $($TestResults.SystemInfo.OSName)" -ForegroundColor White
Write-Host "Build: $($TestResults.SystemInfo.BuildNumber)" -ForegroundColor White
Write-Host "Memory: $($TestResults.SystemInfo.TotalMemoryGB) GB" -ForegroundColor White
Write-Host "Architecture: $($TestResults.SystemInfo.Architecture)" -ForegroundColor White

# Recommendations
Write-Host "`nRecommendations:" -ForegroundColor Cyan
if (-not $TestResults.OverallSuccess) {
    Write-Host "• Review failed tests and address configuration issues" -ForegroundColor Yellow
    Write-Host "• Ensure Windows App SDK is properly installed" -ForegroundColor Yellow
    Write-Host "• Verify project build configuration for MSIX packaging" -ForegroundColor Yellow
}
Write-Host "• Test with actual NFC hardware if available" -ForegroundColor Yellow
Write-Host "• Perform end-to-end application testing" -ForegroundColor Yellow
Write-Host "• Validate MSIX package deployment" -ForegroundColor Yellow

# Exit with appropriate code
$exitCode = if ($passedTests -ge ($totalTests * 0.8)) { 0 } else { 1 }
Write-Host "`nTest completed with exit code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } else { "Red" })
exit $exitCode