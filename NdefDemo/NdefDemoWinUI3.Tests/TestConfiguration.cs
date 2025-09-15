using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Test configuration and setup for NFC functionality tests.
    /// Provides common test utilities and configuration.
    /// </summary>
    [TestClass]
    public class TestConfiguration
    {
        public static TestContext? TestContext { get; private set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            TestContext = context;
            
            // Log test environment information
            context.WriteLine($"Test Assembly: {Assembly.GetExecutingAssembly().GetName().Name}");
            context.WriteLine($"Test Framework: MSTest");
            context.WriteLine($"Target Framework: .NET 8.0");
            context.WriteLine($"Test Environment: {Environment.OSVersion}");
            context.WriteLine($"NFC Hardware Available: {CheckNfcHardware()}");
            
            // Initialize any global test resources
            InitializeTestResources();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Cleanup any global test resources
            CleanupTestResources();
        }

        private static void InitializeTestResources()
        {
            // Create test data directory if needed
            var testDataPath = Path.Combine(Path.GetTempPath(), "NdefDemoTests");
            if (!Directory.Exists(testDataPath))
            {
                Directory.CreateDirectory(testDataPath);
            }
            
            TestContext?.WriteLine($"Test data directory: {testDataPath}");
        }

        private static void CleanupTestResources()
        {
            // Cleanup test data directory
            var testDataPath = Path.Combine(Path.GetTempPath(), "NdefDemoTests");
            if (Directory.Exists(testDataPath))
            {
                try
                {
                    Directory.Delete(testDataPath, true);
                }
                catch (Exception ex)
                {
                    TestContext?.WriteLine($"Warning: Could not cleanup test directory: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Creates test NDEF data for use in unit tests
        /// </summary>
        public static class TestData
        {
            public static readonly string TestUri = "http://www.nfcinteractor.com/";
            public static readonly string TestEmail = "test@example.com";
            public static readonly string TestPhoneNumber = "+1234567890";
            public static readonly string TestName = "Test User";
            public static readonly double TestLatitude = 48.208415;
            public static readonly double TestLongitude = 16.371282;
            public static readonly string TestLocationName = "Vienna, Austria";
            
            public static byte[] CreateValidNdefMessage()
            {
                var record = new NdefLibrary.Ndef.NdefUriRecord { Uri = TestUri };
                var message = new NdefLibrary.Ndef.NdefMessage { record };
                return message.ToByteArray();
            }
            
            public static byte[] CreateInvalidNdefData()
            {
                return new byte[] { 0x00, 0x01, 0x02, 0x03 };
            }
            
            public static byte[] CreateLargeNdefMessage(int sizeInBytes)
            {
                var largeText = new string('A', sizeInBytes - 100); // Account for NDEF overhead
                var record = new NdefLibrary.Ndef.NdefTextRecord 
                { 
                    Text = largeText, 
                    LanguageCode = "en" 
                };
                var message = new NdefLibrary.Ndef.NdefMessage { record };
                return message.ToByteArray();
            }
        }

        /// <summary>
        /// Test utilities for common operations
        /// </summary>
        private static bool CheckNfcHardware()
        {
            // In test environment, simulate no NFC hardware
            return false;
        }

        public static class TestUtilities
        {
            public static bool IsNfcHardwareAvailable()
            {
                // In test environment, simulate no NFC hardware
                return false;
            }
            
            public static void SkipIfNoNfcHardware()
            {
                if (!IsNfcHardwareAvailable())
                {
                    Assert.Inconclusive("NFC hardware not available - skipping hardware-dependent test");
                }
            }
            
            public static void LogTestInfo(string message)
            {
                TestContext?.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            }
            
            public static void LogTestError(string message, Exception? exception = null)
            {
                TestContext?.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {message}");
                if (exception != null)
                {
                    TestContext?.WriteLine($"Exception: {exception}");
                }
            }
        }

        /// <summary>
        /// Performance measurement utilities for tests
        /// </summary>
        public static class PerformanceUtilities
        {
            public static TimeSpan MeasureExecutionTime(Action action)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                action();
                stopwatch.Stop();
                return stopwatch.Elapsed;
            }
            
            public static async System.Threading.Tasks.Task<TimeSpan> MeasureExecutionTimeAsync(Func<System.Threading.Tasks.Task> asyncAction)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await asyncAction();
                stopwatch.Stop();
                return stopwatch.Elapsed;
            }
            
            public static void AssertExecutionTime(Action action, TimeSpan maxExpectedTime, string operationName)
            {
                var actualTime = MeasureExecutionTime(action);
                TestContext?.WriteLine($"{operationName} execution time: {actualTime.TotalMilliseconds:F2}ms");
                
                if (actualTime > maxExpectedTime)
                {
                    Assert.Fail($"{operationName} took {actualTime.TotalMilliseconds:F2}ms, expected less than {maxExpectedTime.TotalMilliseconds:F2}ms");
                }
            }
        }
    }
}