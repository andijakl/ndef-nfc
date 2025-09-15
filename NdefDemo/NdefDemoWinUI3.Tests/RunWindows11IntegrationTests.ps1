# PowerShell script to run Windows 11 integration tests and generate comprehensive report
param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [switch]$GenerateReport = $true,
    [switch]$OpenReport = $true
)

Write-Host "Starting Windows 11 Integration Test Suite" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow

# Set up paths
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$SolutionRoot = Split-Path -Parent $ProjectRoot
$TestProject = "$PSScriptRoot\NdefDemoWinUI3.Tests.csproj"
$OutputDir = "$PSScriptRoot\TestResults"
$ReportFile = "$OutputDir\Windows11IntegrationReport.html"

# Create output directory
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Function to run tests and capture results
function Run-TestCategory {
    param(
        [string]$Category,
        [string]$TestClass
    )
    
    Write-Host "`nRunning $Category tests..." -ForegroundColor Cyan
    
    $TestResults = @()
    $StartTime = Get-Date
    
    try {
        # Run the specific test class
        $TestOutput = dotnet test $TestProject --configuration $Configuration --logger "console;verbosity=detailed" --filter "TestCategory=$Category|FullyQualifiedName~$TestClass" --no-build 2>&1
        
        $EndTime = Get-Date
        $Duration = ($EndTime - $StartTime).TotalSeconds
        
        # Parse test results
        $PassedTests = ($TestOutput | Select-String "Passed!").Count
        $FailedTests = ($TestOutput | Select-String "Failed!").Count
        $SkippedTests = ($TestOutput | Select-String "Skipped").Count
        
        $TestResults = @{
            Category = $Category
            TestClass = $TestClass
            Passed = $PassedTests
            Failed = $FailedTests
            Skipped = $SkippedTests
            Duration = $Duration
            Output = $TestOutput -join "`n"
            Success = $FailedTests -eq 0
        }
        
        if ($TestResults.Success) {
            Write-Host "✓ $Category tests completed successfully" -ForegroundColor Green
        } else {
            Write-Host "✗ $Category tests had failures" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "✗ Error running $Category tests: $($_.Exception.Message)" -ForegroundColor Red
        $TestResults = @{
            Category = $Category
            TestClass = $TestClass
            Passed = 0
            Failed = 1
            Skipped = 0
            Duration = 0
            Output = $_.Exception.Message
            Success = $false
        }
    }
    
    return $TestResults
}

# Function to get system information
function Get-SystemInfo {
    $OS = Get-CimInstance -ClassName Win32_OperatingSystem
    $Computer = Get-CimInstance -ClassName Win32_ComputerSystem
    $Processor = Get-CimInstance -ClassName Win32_Processor | Select-Object -First 1
    
    return @{
        OSName = $OS.Caption
        OSVersion = $OS.Version
        OSBuild = $OS.BuildNumber
        TotalMemory = [math]::Round($Computer.TotalPhysicalMemory / 1GB, 2)
        ProcessorName = $Processor.Name
        ProcessorCores = $Processor.NumberOfCores
        ProcessorThreads = $Processor.NumberOfLogicalProcessors
        TestDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
}

# Build the test project first
Write-Host "`nBuilding test project..." -ForegroundColor Cyan
$BuildResult = dotnet build $TestProject --configuration $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build completed successfully" -ForegroundColor Green

# Get system information
$SystemInfo = Get-SystemInfo
Write-Host "`nSystem Information:" -ForegroundColor Yellow
Write-Host "OS: $($SystemInfo.OSName) (Build $($SystemInfo.OSBuild))" -ForegroundColor White
Write-Host "Memory: $($SystemInfo.TotalMemory) GB" -ForegroundColor White
Write-Host "Processor: $($SystemInfo.ProcessorName)" -ForegroundColor White
Write-Host "Cores/Threads: $($SystemInfo.ProcessorCores)/$($SystemInfo.ProcessorThreads)" -ForegroundColor White

# Run test categories
$AllResults = @()

# Windows 11 Integration Tests
$AllResults += Run-TestCategory -Category "Windows11Integration" -TestClass "Windows11IntegrationTests"

# Performance Benchmark Tests
$AllResults += Run-TestCategory -Category "PerformanceBenchmark" -TestClass "PerformanceBenchmarkTests"

# MSIX Deployment Tests
$AllResults += Run-TestCategory -Category "MsixDeployment" -TestClass "MsixDeploymentTests"

# Generate HTML report if requested
if ($GenerateReport) {
    Write-Host "`nGenerating test report..." -ForegroundColor Cyan
    
    $TotalPassed = ($AllResults | Measure-Object -Property Passed -Sum).Sum
    $TotalFailed = ($AllResults | Measure-Object -Property Failed -Sum).Sum
    $TotalSkipped = ($AllResults | Measure-Object -Property Skipped -Sum).Sum
    $TotalDuration = ($AllResults | Measure-Object -Property Duration -Sum).Sum
    $OverallSuccess = ($AllResults | Where-Object { -not $_.Success }).Count -eq 0
    
    $HtmlReport = @"
<!DOCTYPE html>
<html>
<head>
    <title>Windows 11 Integration Test Report</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 30px; }
        .success { color: #28a745; }
        .failure { color: #dc3545; }
        .warning { color: #ffc107; }
        .info { color: #17a2b8; }
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }
        .summary-card { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; }
        .summary-card h3 { margin: 0 0 10px 0; font-size: 2em; }
        .summary-card p { margin: 0; opacity: 0.9; }
        .system-info { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px; }
        .test-category { margin-bottom: 30px; border: 1px solid #dee2e6; border-radius: 5px; overflow: hidden; }
        .category-header { background-color: #e9ecef; padding: 15px; font-weight: bold; }
        .category-content { padding: 15px; }
        .test-output { background-color: #f8f9fa; border: 1px solid #e9ecef; border-radius: 3px; padding: 10px; font-family: 'Courier New', monospace; font-size: 12px; max-height: 300px; overflow-y: auto; white-space: pre-wrap; }
        .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: bold; }
        .status-success { background-color: #d4edda; color: #155724; }
        .status-failure { background-color: #f8d7da; color: #721c24; }
        .status-warning { background-color: #fff3cd; color: #856404; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Windows 11 Integration Test Report</h1>
            <p class="$(if($OverallSuccess) { 'success' } else { 'failure' })">
                Overall Status: $(if($OverallSuccess) { '✓ PASSED' } else { '✗ FAILED' })
            </p>
            <p>Generated on $($SystemInfo.TestDate)</p>
        </div>
        
        <div class="summary">
            <div class="summary-card">
                <h3>$TotalPassed</h3>
                <p>Tests Passed</p>
            </div>
            <div class="summary-card">
                <h3>$TotalFailed</h3>
                <p>Tests Failed</p>
            </div>
            <div class="summary-card">
                <h3>$TotalSkipped</h3>
                <p>Tests Skipped</p>
            </div>
            <div class="summary-card">
                <h3>$([math]::Round($TotalDuration, 1))s</h3>
                <p>Total Duration</p>
            </div>
        </div>
        
        <div class="system-info">
            <h3>System Information</h3>
            <p><strong>Operating System:</strong> $($SystemInfo.OSName) (Build $($SystemInfo.OSBuild))</p>
            <p><strong>Total Memory:</strong> $($SystemInfo.TotalMemory) GB</p>
            <p><strong>Processor:</strong> $($SystemInfo.ProcessorName)</p>
            <p><strong>Cores/Threads:</strong> $($SystemInfo.ProcessorCores)/$($SystemInfo.ProcessorThreads)</p>
            <p><strong>Test Configuration:</strong> $Configuration ($Platform)</p>
        </div>
        
        <h2>Test Results by Category</h2>
"@

    foreach ($Result in $AllResults) {
        $StatusClass = if ($Result.Success) { "status-success" } else { "status-failure" }
        $StatusText = if ($Result.Success) { "✓ PASSED" } else { "✗ FAILED" }
        
        $HtmlReport += @"
        <div class="test-category">
            <div class="category-header">
                <span>$($Result.Category)</span>
                <span class="status-badge $StatusClass" style="float: right;">$StatusText</span>
            </div>
            <div class="category-content">
                <p><strong>Test Class:</strong> $($Result.TestClass)</p>
                <p><strong>Results:</strong> $($Result.Passed) passed, $($Result.Failed) failed, $($Result.Skipped) skipped</p>
                <p><strong>Duration:</strong> $([math]::Round($Result.Duration, 2)) seconds</p>
                <details>
                    <summary>Test Output</summary>
                    <div class="test-output">$($Result.Output -replace "`n", "<br/>")</div>
                </details>
            </div>
        </div>
"@
    }

    $HtmlReport += @"
    </div>
</body>
</html>
"@

    $HtmlReport | Out-File -FilePath $ReportFile -Encoding UTF8
    Write-Host "Report generated: $ReportFile" -ForegroundColor Green
    
    if ($OpenReport) {
        Start-Process $ReportFile
    }
}

# Summary
Write-Host "`n" + "="*60 -ForegroundColor Yellow
Write-Host "WINDOWS 11 INTEGRATION TEST SUMMARY" -ForegroundColor Yellow
Write-Host "="*60 -ForegroundColor Yellow

$TotalPassed = ($AllResults | Measure-Object -Property Passed -Sum).Sum
$TotalFailed = ($AllResults | Measure-Object -Property Failed -Sum).Sum
$TotalSkipped = ($AllResults | Measure-Object -Property Skipped -Sum).Sum
$OverallSuccess = ($AllResults | Where-Object { -not $_.Success }).Count -eq 0

Write-Host "Total Tests Passed: $TotalPassed" -ForegroundColor Green
Write-Host "Total Tests Failed: $TotalFailed" -ForegroundColor $(if($TotalFailed -eq 0) { "Green" } else { "Red" })
Write-Host "Total Tests Skipped: $TotalSkipped" -ForegroundColor Yellow

foreach ($Result in $AllResults) {
    $Status = if ($Result.Success) { "✓" } else { "✗" }
    $Color = if ($Result.Success) { "Green" } else { "Red" }
    Write-Host "$Status $($Result.Category): $($Result.Passed)/$($Result.Passed + $Result.Failed) passed" -ForegroundColor $Color
}

Write-Host "`nOverall Result: $(if($OverallSuccess) { '✓ SUCCESS' } else { '✗ FAILURE' })" -ForegroundColor $(if($OverallSuccess) { "Green" } else { "Red" })

if ($GenerateReport) {
    Write-Host "`nDetailed report available at: $ReportFile" -ForegroundColor Cyan
}

# Exit with appropriate code
exit $(if($OverallSuccess) { 0 } else { 1 })