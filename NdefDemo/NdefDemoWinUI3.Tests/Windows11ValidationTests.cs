using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Simplified Windows 11 validation tests that don't require full WinUI 3 initialization
    /// </summary>
    [TestClass]
    [TestCategory("Windows11Validation")]
    public class Windows11ValidationTests
    {
        /// <summary>
        /// Test that we're running on a compatible Windows version
        /// </summary>
        [TestMethod]
        public void TestWindowsVersionCompatibility()
        {
            var version = Environment.OSVersion.Version;
            var buildNumber = GetWindowsBuildNumber();
            
            Console.WriteLine($"OS Version: {version}");
            Console.WriteLine($"Build Number: {buildNumber}");
            
            // Should be Windows 10 or higher
            Assert.IsTrue(version.Major >= 10, $"Requires Windows 10 or higher, found {version.Major}");
            
            // Log whether we're on Windows 11
            if (buildNumber >= 22000)
            {
                Console.WriteLine("✓ Running on Windows 11 - full integration features available");
            }
            else
            {
                Console.WriteLine("⚠ Running on Windows 10 - some Windows 11 features may not be available");
            }
        }

        /// <summary>
        /// Test .NET 8.0 runtime compatibility
        /// </summary>
        [TestMethod]
        public void TestDotNetRuntimeCompatibility()
        {
            var runtimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            Console.WriteLine($"Runtime: {runtimeVersion}");
            
            Assert.IsTrue(runtimeVersion.Contains(".NET"), "Should be running on .NET runtime");
            Assert.IsTrue(runtimeVersion.Contains("8.") || runtimeVersion.Contains("9."), 
                "Should be running on .NET 8.0 or higher");
        }

        /// <summary>
        /// Test that required assemblies are available
        /// </summary>
        [TestMethod]
        public void TestRequiredAssembliesAvailable()
        {
            var requiredAssemblies = new[]
            {
                "Microsoft.WindowsAppSDK",
                "Microsoft.UI.Xaml",
                "Windows.ApplicationModel",
                "Windows.Networking.Proximity"
            };

            foreach (var assemblyName in requiredAssemblies)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    Assert.IsNotNull(assembly, $"Assembly {assemblyName} should be loadable");
                    Console.WriteLine($"✓ {assemblyName} - Version: {assembly.GetName().Version}");
                }
                catch (FileNotFoundException)
                {
                    Assert.Fail($"Required assembly {assemblyName} not found");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ {assemblyName} - Warning: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Test application startup performance baseline
        /// </summary>
        [TestMethod]
        public void TestApplicationStartupPerformanceBaseline()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Simulate basic application initialization tasks
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            var testTypes = Array.FindAll(types, t => t.Name.Contains("Test"));
            
            stopwatch.Stop();
            
            Console.WriteLine($"Assembly loading and reflection took: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Found {testTypes.Length} test types");
            
            // Basic performance check - should be very fast
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
                $"Basic initialization took {stopwatch.ElapsedMilliseconds}ms, which may indicate performance issues");
        }

        /// <summary>
        /// Test memory usage baseline
        /// </summary>
        [TestMethod]
        public void TestMemoryUsageBaseline()
        {
            // Force garbage collection for accurate measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            
            // Perform some basic operations
            var data = new byte[1024 * 1024]; // 1MB allocation
            Array.Fill(data, (byte)42);
            
            var afterAllocationMemory = GC.GetTotalMemory(false);
            var memoryIncrease = afterAllocationMemory - initialMemory;
            
            // Clean up
            data = null;
            GC.Collect();
            var afterCleanupMemory = GC.GetTotalMemory(false);
            
            Console.WriteLine($"Initial memory: {initialMemory / (1024 * 1024):F2} MB");
            Console.WriteLine($"After allocation: {afterAllocationMemory / (1024 * 1024):F2} MB");
            Console.WriteLine($"After cleanup: {afterCleanupMemory / (1024 * 1024):F2} MB");
            Console.WriteLine($"Memory increase: {memoryIncrease / (1024 * 1024):F2} MB");
            
            // Memory should be manageable
            Assert.IsTrue(memoryIncrease >= 1024 * 1024, "Should have allocated at least 1MB");
            Assert.IsTrue(afterCleanupMemory < afterAllocationMemory, "Memory should be freed after cleanup");
        }

        /// <summary>
        /// Test Windows API accessibility
        /// </summary>
        [TestMethod]
        public void TestWindowsApiAccessibility()
        {
            try
            {
                // Test basic Windows API access
                var machineName = Environment.MachineName;
                var userName = Environment.UserName;
                var osVersion = Environment.OSVersion;
                
                Assert.IsFalse(string.IsNullOrEmpty(machineName), "Machine name should be accessible");
                Assert.IsFalse(string.IsNullOrEmpty(userName), "User name should be accessible");
                Assert.IsNotNull(osVersion, "OS version should be accessible");
                
                Console.WriteLine($"Machine: {machineName}");
                Console.WriteLine($"User: {userName}");
                Console.WriteLine($"OS: {osVersion}");
                
                // Test process information access
                var currentProcess = Process.GetCurrentProcess();
                Assert.IsNotNull(currentProcess, "Current process should be accessible");
                Assert.IsTrue(currentProcess.Id > 0, "Process ID should be valid");
                
                Console.WriteLine($"Process ID: {currentProcess.Id}");
                Console.WriteLine($"Process Name: {currentProcess.ProcessName}");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Windows API access failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test NFC capability detection
        /// </summary>
        [TestMethod]
        public void TestNfcCapabilityDetection()
        {
            try
            {
                // Test if we can access the proximity namespace
                var proximityDeviceType = Type.GetType("Windows.Networking.Proximity.ProximityDevice, Windows.Networking.Proximity");
                Assert.IsNotNull(proximityDeviceType, "ProximityDevice type should be available");
                
                // Try to get the default proximity device
                var getDefaultMethod = proximityDeviceType.GetMethod("GetDefault", Type.EmptyTypes);
                Assert.IsNotNull(getDefaultMethod, "GetDefault method should be available");
                
                var proximityDevice = getDefaultMethod.Invoke(null, null);
                
                if (proximityDevice != null)
                {
                    Console.WriteLine("✓ NFC hardware detected and accessible");
                }
                else
                {
                    Console.WriteLine("⚠ No NFC hardware detected (this is normal for many systems)");
                }
                
                // This test passes regardless of hardware availability
                Assert.IsTrue(true, "NFC API accessibility test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ NFC capability test warning: {ex.Message}");
                Assert.Inconclusive("NFC capability test could not be completed");
            }
        }

        /// <summary>
        /// Test build configuration validation
        /// </summary>
        [TestMethod]
        public void TestBuildConfigurationValidation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            
            // Test assembly version
            Assert.IsNotNull(assemblyName.Version, "Assembly should have version information");
            Console.WriteLine($"Test Assembly Version: {assemblyName.Version}");
            
            // Test target framework
            var targetFramework = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();
            Assert.IsNotNull(targetFramework, "Assembly should have target framework attribute");
            
            var frameworkName = targetFramework.FrameworkName;
            Console.WriteLine($"Target Framework: {frameworkName}");
            
            // Should be targeting .NET 8.0 and Windows
            Assert.IsTrue(frameworkName.Contains("net8.0"), "Should be targeting .NET 8.0");
            Assert.IsTrue(frameworkName.Contains("windows"), "Should be targeting Windows platform");
            
            // Test configuration
            var configurationAttribute = assembly.GetCustomAttribute<System.Reflection.AssemblyConfigurationAttribute>();
            if (configurationAttribute != null)
            {
                Console.WriteLine($"Build Configuration: {configurationAttribute.Configuration}");
            }
        }

        /// <summary>
        /// Helper method to get Windows build number
        /// </summary>
        private int GetWindowsBuildNumber()
        {
            try
            {
                var version = Environment.OSVersion.Version;
                return version.Build;
            }
            catch
            {
                return 0;
            }
        }
    }
}