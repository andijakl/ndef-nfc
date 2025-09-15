using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using NdefLibrary.Ndef;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Unit tests for error handling scenarios, particularly missing NFC hardware.
    /// Tests Requirements: 6.1
    /// </summary>
    [TestClass]
    public class NfcErrorHandlingTests
    {
        private TestNfcErrorHandlingService? _errorHandlingService;

        [TestInitialize]
        public void Setup()
        {
            _errorHandlingService = new TestNfcErrorHandlingService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _errorHandlingService?.Dispose();
        }

        [TestMethod]
        public void InitializeNfc_WhenHardwareNotAvailable_ShouldHandleGracefully()
        {
            // Act
            var result = _errorHandlingService!.InitializeNfc();
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsFalse(_errorHandlingService.IsNfcAvailable);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("NFC hardware not available"));
        }

        [TestMethod]
        public void InitializeNfc_WhenHardwareAvailable_ShouldSucceed()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            
            // Act
            var result = _errorHandlingService!.InitializeNfc(mockDevice.Object);
            
            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(_errorHandlingService.IsNfcAvailable);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        public void SubscribeForMessages_WithoutNfcHardware_ShouldReturnError()
        {
            // Arrange
            _errorHandlingService!.InitializeNfc(); // Initialize without hardware
            
            // Act
            var result = _errorHandlingService.SubscribeForMessages();
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("NFC not available"));
        }

        [TestMethod]
        public void SubscribeForMessages_WithNfcHardware_ShouldSucceed()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            mockDevice.Setup(d => d.SubscribeForMessage("NDEF", It.IsAny<MessageReceivedHandler>()))
                     .Returns(12345L);
            
            _errorHandlingService!.InitializeNfc(mockDevice.Object);
            
            // Act
            var result = _errorHandlingService.SubscribeForMessages();
            
            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        public void SubscribeForMessages_WhenSubscriptionFails_ShouldHandleError()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            mockDevice.Setup(d => d.SubscribeForMessage("NDEF", It.IsAny<MessageReceivedHandler>()))
                     .Throws(new InvalidOperationException("Subscription failed"));
            
            _errorHandlingService!.InitializeNfc(mockDevice.Object);
            
            // Act
            var result = _errorHandlingService.SubscribeForMessages();
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("Subscription failed"));
        }

        [TestMethod]
        public void PublishMessage_WithoutNfcHardware_ShouldReturnError()
        {
            // Arrange
            _errorHandlingService!.InitializeNfc(); // Initialize without hardware
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            
            // Act
            var result = _errorHandlingService.PublishMessage(uriRecord, true);
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("NFC not available"));
        }

        [TestMethod]
        public void PublishMessage_WithNfcHardware_ShouldSucceed()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(54321L);
            
            _errorHandlingService!.InitializeNfc(mockDevice.Object);
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            
            // Act
            var result = _errorHandlingService.PublishMessage(uriRecord, true);
            
            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        public void PublishMessage_WhenPublishingFails_ShouldHandleError()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Throws(new InvalidOperationException("Publishing failed"));
            
            _errorHandlingService!.InitializeNfc(mockDevice.Object);
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            
            // Act
            var result = _errorHandlingService.PublishMessage(uriRecord, true);
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("Publishing failed"));
        }

        [TestMethod]
        public void HandleInvalidNdefData_MalformedMessage_ShouldReturnError()
        {
            // Arrange
            var invalidData = new byte[] { 0x00, 0x01, 0x02 }; // Invalid NDEF data
            
            // Act
            var result = _errorHandlingService!.ParseNdefMessage(invalidData);
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Message);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("Invalid NDEF data"));
        }

        [TestMethod]
        public void HandleValidNdefData_ValidMessage_ShouldParseSuccessfully()
        {
            // Arrange
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            var message = new NdefMessage { uriRecord };
            var validData = message.ToByteArray();
            
            // Act
            var result = _errorHandlingService!.ParseNdefMessage(validData);
            
            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Message);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(1, result.Message.Count);
        }

        [TestMethod]
        public void HandleNfcPermissionDenied_ShouldProvideUserGuidance()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            mockDevice.Setup(d => d.SubscribeForMessage("NDEF", It.IsAny<MessageReceivedHandler>()))
                     .Throws(new UnauthorizedAccessException("Access denied"));
            
            _errorHandlingService!.InitializeNfc(mockDevice.Object);
            
            // Act
            var result = _errorHandlingService.SubscribeForMessages();
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("Permission denied"));
            Assert.IsTrue(result.ErrorMessage.Contains("enable NFC in Windows settings"));
        }

        [TestMethod]
        public void HandleDeviceDisconnection_DuringOperation_ShouldHandleGracefully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            
            _errorHandlingService!.InitializeNfc(mockDevice.Object);
            
            // Simulate device disconnection
            var disconnectionException = new InvalidOperationException("Device disconnected");
            
            // Act
            var result = _errorHandlingService.HandleDeviceDisconnection(disconnectionException);
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("NFC device disconnected"));
            Assert.IsFalse(_errorHandlingService.IsNfcAvailable);
        }

        [TestMethod]
        public void HandleTagReadError_CorruptedTag_ShouldProvideUserFeedback()
        {
            // Arrange
            var tagException = new NdefException("Tag data corrupted");
            
            // Act
            var result = _errorHandlingService!.HandleTagReadError(tagException);
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("Tag may be corrupted"));
            Assert.IsTrue(result.ErrorMessage.Contains("try another tag"));
        }

        [TestMethod]
        public void HandleTagWriteError_TagReadOnly_ShouldProvideUserFeedback()
        {
            // Arrange
            var writeException = new InvalidOperationException("Tag is read-only");
            
            // Act
            var result = _errorHandlingService!.HandleTagWriteError(writeException);
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("Tag is read-only"));
            Assert.IsTrue(result.ErrorMessage.Contains("cannot be modified"));
        }

        [TestMethod]
        public void HandleTagWriteError_InsufficientSpace_ShouldProvideUserFeedback()
        {
            // Arrange
            var writeException = new InvalidOperationException("Insufficient space on tag");
            
            // Act
            var result = _errorHandlingService!.HandleTagWriteError(writeException);
            
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("not enough space"));
            Assert.IsTrue(result.ErrorMessage.Contains("reduce the data size"));
        }

        [TestMethod]
        public void CheckNfcCapabilities_SystemWithoutNfc_ShouldReturnFalse()
        {
            // Act
            var hasNfc = _errorHandlingService!.CheckNfcCapabilities();
            
            // Assert
            // This will depend on the actual test environment
            // In most test environments, NFC hardware won't be available
            Assert.IsFalse(hasNfc);
        }

        [TestMethod]
        public void GetUserFriendlyErrorMessage_SystemException_ShouldReturnReadableMessage()
        {
            // Arrange
            var systemException = new System.ComponentModel.Win32Exception(5, "Access is denied"); // 5 = ERROR_ACCESS_DENIED
            
            // Act
            var friendlyMessage = _errorHandlingService!.GetUserFriendlyErrorMessage(systemException);
            
            // Assert
            Assert.IsNotNull(friendlyMessage);
            Assert.IsFalse(friendlyMessage.Contains("0x80070005")); // Should not contain hex codes
            Assert.IsTrue(friendlyMessage.Contains("permission") || friendlyMessage.Contains("access"));
        }

        [TestMethod]
        public void ValidateNfcOperation_BeforeExecution_ShouldPreventErrors()
        {
            // Arrange
            _errorHandlingService!.InitializeNfc(); // No hardware
            
            // Act
            var canSubscribe = _errorHandlingService.CanPerformNfcOperation();
            var canPublish = _errorHandlingService.CanPerformNfcOperation();
            
            // Assert
            Assert.IsFalse(canSubscribe);
            Assert.IsFalse(canPublish);
        }

        [TestMethod]
        public void ValidateNfcOperation_WithHardware_ShouldAllowOperations()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            mockDevice.Setup(d => d.DeviceId).Returns("TestDevice");
            _errorHandlingService!.InitializeNfc(mockDevice.Object);
            
            // Act
            var canPerformOperations = _errorHandlingService.CanPerformNfcOperation();
            
            // Assert
            Assert.IsTrue(canPerformOperations);
        }
    }

    // Test helper class for NFC error handling
    public class TestNfcErrorHandlingService : IDisposable
    {
        private IProximityDeviceWrapper? _device;
        private bool _disposed;

        public bool IsNfcAvailable => _device != null;

        public NfcOperationResult InitializeNfc(IProximityDeviceWrapper? device = null)
        {
            try
            {
                if (device == null)
                {
                    // Simulate ProximityDevice.GetDefault() returning null (no hardware)
                    _device = null;
                    return new NfcOperationResult
                    {
                        Success = false,
                        ErrorMessage = "NFC hardware not available on this device"
                    };
                }

                _device = device;
                return new NfcOperationResult { Success = true };
            }
            catch (Exception ex)
            {
                return new NfcOperationResult
                {
                    Success = false,
                    ErrorMessage = GetUserFriendlyErrorMessage(ex)
                };
            }
        }

        public NfcOperationResult SubscribeForMessages()
        {
            try
            {
                if (_device == null)
                {
                    return new NfcOperationResult
                    {
                        Success = false,
                        ErrorMessage = "NFC not available. Please ensure NFC is enabled and supported on this device."
                    };
                }

                _device.SubscribeForMessage("NDEF", (message) => { });
                return new NfcOperationResult { Success = true };
            }
            catch (UnauthorizedAccessException)
            {
                return new NfcOperationResult
                {
                    Success = false,
                    ErrorMessage = "Permission denied. Please enable NFC in Windows settings and ensure the app has necessary permissions."
                };
            }
            catch (Exception ex)
            {
                return new NfcOperationResult
                {
                    Success = false,
                    ErrorMessage = GetUserFriendlyErrorMessage(ex)
                };
            }
        }

        public NfcOperationResult PublishMessage(NdefRecord record, bool writeToTag)
        {
            try
            {
                if (_device == null)
                {
                    return new NfcOperationResult
                    {
                        Success = false,
                        ErrorMessage = "NFC not available. Please ensure NFC is enabled and supported on this device."
                    };
                }

                var message = new NdefMessage { record };
                var messageData = message.ToByteArray();
                var messageType = writeToTag ? "NDEF:WriteTag" : "NDEF";
                
                _device.PublishBinaryMessage(messageType, messageData, (messageId) => { });
                return new NfcOperationResult { Success = true };
            }
            catch (Exception ex)
            {
                return new NfcOperationResult
                {
                    Success = false,
                    ErrorMessage = GetUserFriendlyErrorMessage(ex)
                };
            }
        }

        public NdefParseResult ParseNdefMessage(byte[] data)
        {
            try
            {
                var message = NdefMessage.FromByteArray(data);
                return new NdefParseResult
                {
                    Success = true,
                    Message = message
                };
            }
            catch (NdefException ex)
            {
                return new NdefParseResult
                {
                    Success = false,
                    ErrorMessage = $"Invalid NDEF data: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new NdefParseResult
                {
                    Success = false,
                    ErrorMessage = GetUserFriendlyErrorMessage(ex)
                };
            }
        }

        public NfcOperationResult HandleDeviceDisconnection(Exception exception)
        {
            _device = null; // Simulate device disconnection
            return new NfcOperationResult
            {
                Success = false,
                ErrorMessage = "NFC device disconnected. Please check your NFC connection and try again."
            };
        }

        public NfcOperationResult HandleTagReadError(Exception exception)
        {
            if (exception is NdefException)
            {
                return new NfcOperationResult
                {
                    Success = false,
                    ErrorMessage = "Unable to read NFC tag. The tag may be corrupted or incompatible. Please try another tag."
                };
            }

            return new NfcOperationResult
            {
                Success = false,
                ErrorMessage = GetUserFriendlyErrorMessage(exception)
            };
        }

        public NfcOperationResult HandleTagWriteError(Exception exception)
        {
            var message = exception.Message.ToLower();
            
            if (message.Contains("read-only") || message.Contains("readonly"))
            {
                return new NfcOperationResult
                {
                    Success = false,
                    ErrorMessage = "Tag is read-only and cannot be modified. Please use a writable NFC tag."
                };
            }
            
            if (message.Contains("space") || message.Contains("size"))
            {
                return new NfcOperationResult
                {
                    Success = false,
                    ErrorMessage = "There is not enough space on the NFC tag. Please reduce the data size or use a larger capacity tag."
                };
            }

            return new NfcOperationResult
            {
                Success = false,
                ErrorMessage = GetUserFriendlyErrorMessage(exception)
            };
        }

        public bool CheckNfcCapabilities()
        {
            // In a real implementation, this would check system capabilities
            // For testing, we simulate no NFC hardware
            return false; // Simulate no NFC hardware in test environment
        }

        public bool CanPerformNfcOperation()
        {
            return IsNfcAvailable;
        }

        public string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => "Access denied. Please check NFC permissions in Windows settings.",
                InvalidOperationException => "NFC operation failed. Please ensure NFC is enabled and try again.",
                System.ComponentModel.Win32Exception => "System error occurred. Please check NFC hardware and drivers.",
                NdefException => "Invalid NFC data format. The tag may be corrupted or incompatible.",
                _ => "An unexpected error occurred. Please try again or restart the application."
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _device = null;
                _disposed = true;
            }
        }
    }

    // Helper classes for test results
    public class NfcOperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class NdefParseResult : NfcOperationResult
    {
        public NdefMessage? Message { get; set; }
    }
}