using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using Windows.System;
using System.Collections.Generic;
using System.Linq;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Performance benchmark tests to validate WinUI 3 performance compared to UWP baseline
    /// </summary>
    [TestClass]
    [TestCategory("PerformanceBenchmark")]
    public class PerformanceBenchmarkTests
    {
        private App? _app;
        private MainWindow? _mainWindow;

        [TestInitialize]
        public void TestInitialize()
        {
            if (Application.Current == null)
            {
                _app = new App();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _mainWindow?.Close();
            _mainWindow = null;
        }

        /// <summary>
        /// Benchmark application cold start time
        /// </summary>
        [TestMethod]
        public async Task BenchmarkColdStartTime()
        {
            var measurements = new List<long>();
            
            // Perform multiple measurements for statistical accuracy
            for (int i = 0; i < 5; i++)
            {
                // Force garbage collection before each test
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                var stopwatch = Stopwatch.StartNew();
                
                // Simulate cold start
                _mainWindow = new MainWindow();
                _mainWindow.Activate();
                
                // Wait for window to be fully rendered
                await Task.Delay(500);
                
                stopwatch.Stop();
                measurements.Add(stopwatch.ElapsedMilliseconds);
                
                _mainWindow.Close();
                _mainWindow = null;
                
                // Wait between measurements
                await Task.Delay(1000);
            }
            
            var averageStartTime = measurements.Average();
            var maxStartTime = measurements.Max();
            
            // Log performance metrics
            Console.WriteLine($"Average cold start time: {averageStartTime:F2}ms");
            Console.WriteLine($"Maximum cold start time: {maxStartTime}ms");
            Console.WriteLine($"All measurements: {string.Join(", ", measurements)}ms");
            
            // Performance targets (should be better than typical UWP performance)
            Assert.IsTrue(averageStartTime < 2000, 
                $"Average cold start time ({averageStartTime:F2}ms) exceeds target of 2000ms");
            Assert.IsTrue(maxStartTime < 3000, 
                $"Maximum cold start time ({maxStartTime}ms) exceeds target of 3000ms");
        }

        /// <summary>
        /// Benchmark memory usage during typical operations
        /// </summary>
        [TestMethod]
        public async Task BenchmarkMemoryUsage()
        {
            // Baseline memory measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var baselineMemory = GC.GetTotalMemory(false);
            
            // Initialize application
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
            await Task.Delay(1000);
            
            GC.Collect();
            var afterInitMemory = GC.GetTotalMemory(false);
            var initMemoryUsage = afterInitMemory - baselineMemory;
            
            // Simulate typical user operations
            await SimulateUserOperations();
            
            GC.Collect();
            var afterOperationsMemory = GC.GetTotalMemory(false);
            var operationsMemoryUsage = afterOperationsMemory - baselineMemory;
            
            // Log memory metrics
            Console.WriteLine($"Baseline memory: {baselineMemory / (1024 * 1024):F2} MB");
            Console.WriteLine($"After init memory: {afterInitMemory / (1024 * 1024):F2} MB");
            Console.WriteLine($"After operations memory: {afterOperationsMemory / (1024 * 1024):F2} MB");
            Console.WriteLine($"Init memory increase: {initMemoryUsage / (1024 * 1024):F2} MB");
            Console.WriteLine($"Operations memory increase: {operationsMemoryUsage / (1024 * 1024):F2} MB");
            
            // Memory usage targets
            Assert.IsTrue(initMemoryUsage < 30 * 1024 * 1024, 
                $"Initialization memory usage ({initMemoryUsage / (1024 * 1024):F2} MB) exceeds 30 MB target");
            Assert.IsTrue(operationsMemoryUsage < 50 * 1024 * 1024, 
                $"Operations memory usage ({operationsMemoryUsage / (1024 * 1024):F2} MB) exceeds 50 MB target");
        }

        /// <summary>
        /// Benchmark UI responsiveness
        /// </summary>
        [TestMethod]
        public async Task BenchmarkUIResponsiveness()
        {
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
            await Task.Delay(1000);
            
            var responseTimes = new List<long>();
            
            // Test multiple UI operations
            for (int i = 0; i < 10; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Simulate UI update
                if (_mainWindow.Content is FrameworkElement content)
                {
                    content.UpdateLayout();
                    content.InvalidateArrange();
                    content.InvalidateMeasure();
                }
                
                stopwatch.Stop();
                responseTimes.Add(stopwatch.ElapsedMilliseconds);
                
                await Task.Delay(100);
            }
            
            var averageResponseTime = responseTimes.Average();
            var maxResponseTime = responseTimes.Max();
            
            Console.WriteLine($"Average UI response time: {averageResponseTime:F2}ms");
            Console.WriteLine($"Maximum UI response time: {maxResponseTime}ms");
            
            // UI responsiveness targets
            Assert.IsTrue(averageResponseTime < 16, 
                $"Average UI response time ({averageResponseTime:F2}ms) exceeds 16ms target (60 FPS)");
            Assert.IsTrue(maxResponseTime < 50, 
                $"Maximum UI response time ({maxResponseTime}ms) exceeds 50ms target");
        }

        /// <summary>
        /// Benchmark CPU usage during idle and active states
        /// </summary>
        [TestMethod]
        public async Task BenchmarkCPUUsage()
        {
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
            await Task.Delay(1000);
            
            var process = Process.GetCurrentProcess();
            
            // Measure idle CPU usage
            var idleCpuTimes = new List<TimeSpan>();
            for (int i = 0; i < 5; i++)
            {
                var startCpuTime = process.TotalProcessorTime;
                await Task.Delay(1000);
                var endCpuTime = process.TotalProcessorTime;
                idleCpuTimes.Add(endCpuTime - startCpuTime);
            }
            
            var averageIdleCpuMs = idleCpuTimes.Average(t => t.TotalMilliseconds);
            
            // Measure active CPU usage during operations
            var activeCpuTimes = new List<TimeSpan>();
            for (int i = 0; i < 5; i++)
            {
                var startCpuTime = process.TotalProcessorTime;
                await SimulateUserOperations();
                var endCpuTime = process.TotalProcessorTime;
                activeCpuTimes.Add(endCpuTime - startCpuTime);
            }
            
            var averageActiveCpuMs = activeCpuTimes.Average(t => t.TotalMilliseconds);
            
            Console.WriteLine($"Average idle CPU usage: {averageIdleCpuMs:F2}ms per second");
            Console.WriteLine($"Average active CPU usage: {averageActiveCpuMs:F2}ms per operation cycle");
            
            // CPU usage should be minimal when idle
            Assert.IsTrue(averageIdleCpuMs < 100, 
                $"Idle CPU usage ({averageIdleCpuMs:F2}ms/sec) is too high");
        }

        /// <summary>
        /// Test application resource cleanup
        /// </summary>
        [TestMethod]
        public async Task TestResourceCleanup()
        {
            var initialHandleCount = GetProcessHandleCount();
            var initialThreadCount = Process.GetCurrentProcess().Threads.Count;
            
            // Create and destroy windows multiple times
            for (int i = 0; i < 5; i++)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Activate();
                await Task.Delay(500);
                
                _mainWindow.Close();
                _mainWindow = null;
                
                // Force cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                await Task.Delay(500);
            }
            
            var finalHandleCount = GetProcessHandleCount();
            var finalThreadCount = Process.GetCurrentProcess().Threads.Count;
            
            Console.WriteLine($"Initial handles: {initialHandleCount}, Final handles: {finalHandleCount}");
            Console.WriteLine($"Initial threads: {initialThreadCount}, Final threads: {finalThreadCount}");
            
            // Resource counts should not grow significantly
            Assert.IsTrue(finalHandleCount - initialHandleCount < 50, 
                $"Handle count increased by {finalHandleCount - initialHandleCount}, indicating potential resource leak");
            Assert.IsTrue(finalThreadCount - initialThreadCount < 10, 
                $"Thread count increased by {finalThreadCount - initialThreadCount}, indicating potential thread leak");
        }

        /// <summary>
        /// Simulate typical user operations for performance testing
        /// </summary>
        private async Task SimulateUserOperations()
        {
            if (_mainWindow?.Content is FrameworkElement content)
            {
                // Simulate layout updates
                for (int i = 0; i < 10; i++)
                {
                    content.UpdateLayout();
                    await Task.Delay(10);
                }
                
                // Simulate property changes
                content.Opacity = 0.9;
                await Task.Delay(50);
                content.Opacity = 1.0;
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// Get the current process handle count
        /// </summary>
        private int GetProcessHandleCount()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                return process.HandleCount;
            }
            catch
            {
                return 0;
            }
        }
    }
}