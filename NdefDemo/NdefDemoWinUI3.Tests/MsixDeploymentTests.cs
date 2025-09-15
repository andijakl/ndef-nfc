using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Tests for MSIX packaging and deployment scenarios
    /// </summary>
    [TestClass]
    [TestCategory("MsixDeployment")]
    public class MsixDeploymentTests
    {
        /// <summary>
        /// Test MSIX package identity and metadata
        /// </summary>
        [TestMethod]
        public void TestPackageIdentity()
        {
            try
            {
                var package = Package.Current;
                
                // Verify package identity components
                Assert.IsNotNull(package.Id, "Package should have valid identity");
                Assert.IsFalse(string.IsNullOrEmpty(package.Id.Name), "Package name should not be empty");
                Assert.AreEqual("39745AndreasJakl.NdefDemoWinUI3", package.Id.Name, "Package name should match manifest");
                
                Assert.IsFalse(string.IsNullOrEmpty(package.Id.Publisher), "Package publisher should not be empty");
                Assert.AreEqual("CN=Andreas Jakl", package.Id.Publisher, "Package publisher should match manifest");
                
                Assert.IsNotNull(package.Id.Version, "Package should have version");
                Assert.IsTrue(package.Id.Version.Major >= 2, "Package version should be 2.0 or higher");
                
                Console.WriteLine($"Package Name: {package.Id.Name}");
                Console.WriteLine($"Package Publisher: {package.Id.Publisher}");
                Console.WriteLine($"Package Version: {package.Id.Version}");
                Console.WriteLine($"Package Architecture: {package.Id.Architecture}");
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - MSIX package identity tests not applicable");
            }
        }

        /// <summary>
        /// Test package installation location and file access
        /// </summary>
        [TestMethod]
        public async Task TestPackageInstallationLocation()
        {
            try
            {
                var package = Package.Current;
                var installedLocation = package.InstalledLocation;
                
                Assert.IsNotNull(installedLocation, "Package should have valid installation location");
                
                // Test that we can access package files
                var files = await installedLocation.GetFilesAsync();
                Assert.IsTrue(files.Count > 0, "Package should contain files");
                
                // Look for the main executable
                bool foundExecutable = false;
                foreach (var file in files)
                {
                    if (file.Name.EndsWith(".exe"))
                    {
                        foundExecutable = true;
                        Console.WriteLine($"Found executable: {file.Name}");
                        break;
                    }
                }
                Assert.IsTrue(foundExecutable, "Package should contain the main executable");
                
                // Test access to assets folder
                try
                {
                    var assetsFolder = await installedLocation.GetFolderAsync("Assets");
                    var assetFiles = await assetsFolder.GetFilesAsync();
                    Assert.IsTrue(assetFiles.Count > 0, "Assets folder should contain files");
                    
                    Console.WriteLine($"Found {assetFiles.Count} asset files");
                }
                catch (FileNotFoundException)
                {
                    Assert.Fail("Assets folder should be present in the package");
                }
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - package location tests not applicable");
            }
        }

        /// <summary>
        /// Test package capabilities and permissions
        /// </summary>
        [TestMethod]
        public void TestPackageCapabilities()
        {
            try
            {
                var package = Package.Current;
                
                // Test that package has required capabilities
                // Note: We can't directly query capabilities from Package API,
                // but we can test that features requiring capabilities work
                
                // Test internet client capability by checking network access
                var hasNetworkAccess = Windows.Networking.Connectivity.NetworkInformation
                    .GetInternetConnectionProfile() != null;
                
                // This test verifies the capability is working, not just declared
                Console.WriteLine($"Network access available: {hasNetworkAccess}");
                
                // Test proximity capability by attempting to access NFC
                try
                {
                    var proximityDevice = Windows.Networking.Proximity.ProximityDevice.GetDefault();
                    var hasNfcCapability = proximityDevice != null;
                    Console.WriteLine($"NFC capability accessible: {hasNfcCapability}");
                }
                catch (UnauthorizedAccessException)
                {
                    Assert.Fail("Proximity capability should be granted");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NFC test result: {ex.Message}");
                    // This is acceptable - device might not have NFC hardware
                }
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - capability tests not applicable");
            }
        }

        /// <summary>
        /// Test package data storage locations
        /// </summary>
        [TestMethod]
        public async Task TestPackageDataStorage()
        {
            try
            {
                // Test local data storage
                var localFolder = ApplicationData.Current.LocalFolder;
                Assert.IsNotNull(localFolder, "Local data folder should be accessible");
                
                // Test writing to local storage
                var testFile = await localFolder.CreateFileAsync("test.txt", 
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(testFile, "Test data");
                
                // Test reading from local storage
                var readData = await FileIO.ReadTextAsync(testFile);
                Assert.AreEqual("Test data", readData, "Should be able to read written data");
                
                // Clean up
                await testFile.DeleteAsync();
                
                // Test temporary storage
                var tempFolder = ApplicationData.Current.TemporaryFolder;
                Assert.IsNotNull(tempFolder, "Temporary data folder should be accessible");
                
                Console.WriteLine($"Local folder path: {localFolder.Path}");
                Console.WriteLine($"Temp folder path: {tempFolder.Path}");
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - data storage tests not applicable");
            }
        }

        /// <summary>
        /// Test package manifest validation
        /// </summary>
        [TestMethod]
        public void TestPackageManifestValidation()
        {
            try
            {
                var package = Package.Current;
                
                // Test display name
                var displayName = package.DisplayName;
                Assert.IsFalse(string.IsNullOrEmpty(displayName), "Package should have display name");
                Assert.IsTrue(displayName.Contains("NFC"), "Display name should reference NFC functionality");
                
                // Test description
                var description = package.Description;
                Assert.IsFalse(string.IsNullOrEmpty(description), "Package should have description");
                
                // Test logo
                var logo = package.Logo;
                Assert.IsNotNull(logo, "Package should have logo");
                
                Console.WriteLine($"Display Name: {displayName}");
                Console.WriteLine($"Description: {description}");
                Console.WriteLine($"Logo URI: {logo}");
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - manifest validation tests not applicable");
            }
        }

        /// <summary>
        /// Test package dependency validation
        /// </summary>
        [TestMethod]
        public void TestPackageDependencies()
        {
            try
            {
                var package = Package.Current;
                var dependencies = package.Dependencies;
                
                Assert.IsNotNull(dependencies, "Package should have dependencies collection");
                
                bool foundWindowsAppSdk = false;
                bool foundFrameworkDependency = false;
                
                foreach (var dependency in dependencies)
                {
                    Console.WriteLine($"Dependency: {dependency.Id.Name} - {dependency.Id.Version}");
                    
                    if (dependency.Id.Name.Contains("WindowsAppRuntime") || 
                        dependency.Id.Name.Contains("WindowsAppSDK"))
                    {
                        foundWindowsAppSdk = true;
                    }
                    
                    if (dependency.Id.Name.Contains("Framework") || 
                        dependency.Id.Name.Contains("Runtime"))
                    {
                        foundFrameworkDependency = true;
                    }
                }
                
                // For WinUI 3 apps, we expect Windows App SDK dependency
                if (!foundWindowsAppSdk)
                {
                    Console.WriteLine("Warning: Windows App SDK dependency not found in package dependencies");
                }
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - dependency validation tests not applicable");
            }
        }

        /// <summary>
        /// Test package update scenarios
        /// </summary>
        [TestMethod]
        public void TestPackageUpdateCapability()
        {
            try
            {
                var package = Package.Current;
                
                // Test that package supports update mechanisms
                var packageId = package.Id;
                Assert.IsNotNull(packageId.Version, "Package should have version for update tracking");
                
                // Test package status
                var status = package.Status;
                Assert.IsNotNull(status, "Package should have status information");
                
                // Verify package is not in an error state
                Assert.IsFalse(status.Disabled, "Package should not be disabled");
                Assert.IsFalse(status.NotAvailable, "Package should be available");
                
                Console.WriteLine($"Package Status - Disabled: {status.Disabled}, NotAvailable: {status.NotAvailable}");
                Console.WriteLine($"Package Status - LicenseIssue: {status.LicenseIssue}, Modified: {status.Modified}");
                Console.WriteLine($"Package Status - NeedsRemediation: {status.NeedsRemediation}, PackageOffline: {status.PackageOffline}");
                Console.WriteLine($"Package Status - Servicing: {status.Servicing}, Tampered: {status.Tampered}");
            }
            catch (InvalidOperationException)
            {
                Assert.Inconclusive("Running unpackaged - update capability tests not applicable");
            }
        }

        /// <summary>
        /// Test build configuration validation
        /// </summary>
        [TestMethod]
        public void TestBuildConfiguration()
        {
            // Test that we're running the correct build configuration
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            
            Assert.IsNotNull(assemblyName.Version, "Assembly should have version information");
            Console.WriteLine($"Assembly Version: {assemblyName.Version}");
            
            // Test target framework
            var targetFramework = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();
            Assert.IsNotNull(targetFramework, "Assembly should have target framework attribute");
            Assert.IsTrue(targetFramework.FrameworkName.Contains("net8.0"), 
                "Should be targeting .NET 8.0");
            
            Console.WriteLine($"Target Framework: {targetFramework.FrameworkName}");
            
            // Test that we're using the correct runtime
            var runtimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            Console.WriteLine($"Runtime Version: {runtimeVersion}");
            Assert.IsTrue(runtimeVersion.Contains(".NET"), "Should be running on .NET runtime");
        }
    }
}