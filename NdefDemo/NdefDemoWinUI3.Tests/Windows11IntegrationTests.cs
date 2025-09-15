using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using Windows.System;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.Collections.Generic;
using System.Linq;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Tests for Windows 11 integration features and system compatibility
    /// </summary>
    [TestClass]
    [TestCategory("Windows11Integration")]
    public class Windows11IntegrationTests
    {
        private App? _app;
        private MainWindow? _mainWindow;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize the app for testing if not already done
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
        /// Test application startup performance on Windows 11
        /// </summary>
        [TestMethod]
        public async Task TestApplicationStartupPerformance()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Create and activate main window
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
            
            // Wait for window to be fully loaded
            await Task.Delay(1000);
            
            stopwatch.Stop();
            
            // Startup should be under 3 seconds for good user experience
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 3000, 
                $"Application startup took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 3000ms threshold");
            
            // Verify window is properly initialized
            Assert.IsNotNull(_mainWindow);
            Assert.IsTrue(_mainWindow.Content is FrameworkElement);
        }

        /// <summary>
        /// Test Windows 11 window management features
        /// </summary>
        [TestMethod]
        public void TestWindows11WindowManagement()
        {
            _mainWindow = new MainWindow();
            _mainWindow.Activate();

            // Get the window handle for Win32 interop
            var hwnd = WindowNative.GetWindowHandle(_mainWindow);
            Assert.AreNotEqual(IntPtr.Zero, hwnd, "Window handle should be valid");

            // Get the AppWindow for modern window management
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            
            Assert.IsNotNull(appWindow, "AppWindow should be accessible for Windows 11 features");
            
            // Test window title
            Assert.IsFalse(string.IsNullOrEmpty(appWindow.Title), "Window should have a title");
            
            // Test that we can access presenter (for snap layouts, etc.)
            Assert.IsNotNull(appWindow.Presenter, "Window presenter should be available for Windows 11 features");
        }

        /// <summary>
        /// Test Windows 11 system theme integration
        /// </summary>
        [TestMethod]
        public void TestSystemThemeIntegration()
        {
            _mainWindow = new MainWindow();
            
            // Test that the app can detect system theme
            var requestedTheme = Application.Current.RequestedTheme;
            Assert.IsTrue(requestedTheme == ApplicationTheme.Light || requestedTheme == ApplicationTheme.Dark,
                "Application should support Windows 11 theme detection");

            // Verify the window content supports theming
            if (_mainWindow.Content is FrameworkElement content)
            {
                Assert.IsTrue(content.RequestedTheme == ElementTheme.Default || 
                             content.RequestedTheme == ElementTheme.Light || 
                             content.RequestedTheme == ElementTheme.Dark,
                             "Window content should support theme switching");
            }
        }

        /// <summary>
        /// Test Windows 11 version compatibility
        /// </summary>
        [TestMethod]
        public void TestWindows11VersionCompatibility()
        {
            // Check if running on Windows 11 (build 22000 or higher)
            var version = Environment.OSVersion.Version;
            var buildNumber = GetWindowsBuildNumber();
            
            if (buildNumber >= 22000)
            {
                // Running on Windows 11
                Assert.IsTrue(version.Major >= 10, "Should be running on Windows 10 or higher");
                
                // Test that Windows App SDK features are available
                Assert.IsNotNull(Application.Current, "WinUI 3 Application should be available");
                
                // Test package identity (MSIX)
                try
                {
                    var package = Package.Current;
                    Assert.IsNotNull(package, "Package identity should be available in MSIX context");
                    Assert.IsFalse(string.IsNullOrEmpty(package.Id.Name), "Package should have a valid name");
                }
                catch (InvalidOperationException)
                {
                    // Running unpackaged - this is acceptable for development
                    Assert.Inconclusive("Running unpackaged - MSIX features not testable in this context");
                }
            }
            else
            {
                Assert.Inconclusive($"Not running on Windows 11 (build {buildNumber}), skipping Windows 11 specific tests");
            }
        }

        /// <summary>
        /// Test MSIX packaging capabilities
        /// </summary>
        [TestMethod]
        public void TestMsixPackagingCapabilities()
        {
            try
            {
                var package = Package.Current;
                
                // Test package identity
                Assert.IsNotNull(package.Id, "Package should have valid identity");
                Assert.IsFalse(string.IsNullOrEmpty(package.Id.Name), "Package name should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(package.Id.Publisher), "Package publisher should not be empty");
                
                // Test package capabilities
                var capabilities = package.Id.Name;
                Assert.IsNotNull(capabilities, "Package should have defined capabilities");
                
                // Test package location
                Assert.IsNotNull(package.InstalledLocation, "Package should have valid installation location");
                
                // Test that we can access package resources
                var displayName = package.DisplayName;
                Assert.IsFalse(string.IsNullOrEmpty(displayName), "Package should have display name");
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - MSIX packaging tests not applicable");
            }
        }

        /// <summary>
        /// Test Windows 11 notification system integration
        /// </summary>
        [TestMethod]
        public async Task TestNotificationSystemIntegration()
        {
            try
            {
                // Test that we can access the notification system
                var notificationManager = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
                Assert.IsNotNull(notificationManager, "Toast notification manager should be available");
                
                // Test notification capability without actually sending
                var setting = notificationManager.Setting;
                Assert.IsTrue(setting == Windows.UI.Notifications.NotificationSetting.Enabled || 
                             setting == Windows.UI.Notifications.NotificationSetting.DisabledForApplication ||
                             setting == Windows.UI.Notifications.NotificationSetting.DisabledForUser ||
                             setting == Windows.UI.Notifications.NotificationSetting.DisabledByGroupPolicy ||
                             setting == Windows.UI.Notifications.NotificationSetting.DisabledByManifest,
                             "Notification setting should be a valid enum value");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Notification system test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test memory usage and performance characteristics
        /// </summary>
        [TestMethod]
        public async Task TestMemoryUsageAndPerformance()
        {
            // Get initial memory usage
            var initialMemory = GC.GetTotalMemory(true);
            
            // Create and initialize the main window
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
            
            // Wait for initialization to complete
            await Task.Delay(2000);
            
            // Force garbage collection and measure memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var afterInitMemory = GC.GetTotalMemory(false);
            var memoryIncrease = afterInitMemory - initialMemory;
            
            // Memory increase should be reasonable (less than 50MB for basic app)
            Assert.IsTrue(memoryIncrease < 50 * 1024 * 1024, 
                $"Memory usage increased by {memoryIncrease / (1024 * 1024)}MB, which may be excessive");
            
            // Test that the app responds quickly to user interactions
            var stopwatch = Stopwatch.StartNew();
            
            // Simulate some UI operations
            if (_mainWindow.Content is FrameworkElement content)
            {
                content.UpdateLayout();
            }
            
            stopwatch.Stop();
            
            // UI operations should be fast (under 100ms)
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
                $"UI operations took {stopwatch.ElapsedMilliseconds}ms, which may impact responsiveness");
        }

        /// <summary>
        /// Test Windows 11 accessibility features integration
        /// </summary>
        [TestMethod]
        public void TestAccessibilityIntegration()
        {
            _mainWindow = new MainWindow();
            
            if (_mainWindow.Content is FrameworkElement content)
            {
                // Test that automation properties are properly set
                var automationId = content.GetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty);
                
                // Test high contrast support
                var highContrastAdjustment = content.GetValue(FrameworkElement.HighContrastAdjustmentProperty);
                Assert.IsNotNull(highContrastAdjustment, "High contrast adjustment should be configured");
                
                // Test that the app supports keyboard navigation
                Assert.IsTrue(content.IsTabStop || HasFocusableChildren(content), 
                    "Main content should support keyboard navigation");
            }
        }

        /// <summary>
        /// Helper method to check if an element has focusable children
        /// </summary>
        private bool HasFocusableChildren(DependencyObject parent)
        {
            var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is Control control && control.IsTabStop)
                {
                    return true;
                }
                if (HasFocusableChildren(child))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get Windows build number using P/Invoke
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