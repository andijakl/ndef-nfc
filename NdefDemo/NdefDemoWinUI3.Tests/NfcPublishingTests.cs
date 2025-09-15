using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using NdefLibrary.Ndef;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Unit tests for NFC publishing scenarios (tag writing, device-to-device).
    /// Tests Requirements: 3.1, 3.2, 3.3
    /// </summary>
    [TestClass]
    public class NfcPublishingTests
    {
        private TestNfcPublishingService? _publishingService;

        [TestInitialize]
        public void Setup()
        {
            _publishingService = new TestNfcPublishingService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _publishingService?.Dispose();
        }

        [TestMethod]
        public void PublishUriRecordToTag_ValidRecord_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            var uriRecord = new NdefUriRecord { Uri = "http://www.nfcinteractor.com/" };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishRecordToTag(uriRecord);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
            mockDevice.Verify(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()), Times.Once);
        }

        [TestMethod]
        public void PublishUriRecordToDevice_ValidRecord_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            var uriRecord = new NdefUriRecord { Uri = "http://www.nfcinteractor.com/" };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishRecordToDevice(uriRecord);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
            mockDevice.Verify(d => d.PublishBinaryMessage("NDEF", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()), Times.Once);
        }

        [TestMethod]
        public void PublishBusinessCardToTag_ValidRecord_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            
            var contact = new NdefContact
            {
                FirstName = "Andreas",
                LastName = "Jakl"
            };
            contact.Emails.Add(new NdefContactEmail { Address = "andreas.jakl@live.com", Kind = NdefContactEmailKind.Work });
            var vcardRecord = new NdefVcardRecord { ContactData = contact };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishRecordToTag(vcardRecord);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
        }

        [TestMethod]
        public void PublishMailtoRecordToTag_ValidRecord_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            
            var mailtoRecord = new NdefMailtoRecord
            {
                Address = "andreas.jakl@live.com",
                Subject = "Feedback for the NDEF Library",
                Body = "I think the NDEF library is ..."
            };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishRecordToTag(mailtoRecord);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
        }

        [TestMethod]
        public void PublishLaunchAppRecordToTag_ValidRecord_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            
            var launchAppRecord = new NdefLaunchAppRecord { Arguments = "Hello World" };
            launchAppRecord.AddPlatformAppId("Windows", "{test-family-name!test-app-id}");
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishRecordToTag(launchAppRecord);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
        }

        [TestMethod]
        public void PublishGeoRecordToTag_ValidRecord_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            
            var geoRecord = new NdefGeoRecord
            {
                GeoType = NdefGeoRecord.NfcGeoType.MsDriveTo,
                Latitude = 48.208415,
                Longitude = 16.371282,
                PlaceTitle = "Vienna, Austria"
            };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishRecordToTag(geoRecord);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
        }

        [TestMethod]
        public void PublishWindowsSettingsRecordToTag_ValidRecord_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            
            var settingsRecord = new NdefWindowsSettingsRecord { SettingsApp = NdefWindowsSettingsRecord.NfcSettingsApp.DevicesNfc };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishRecordToTag(settingsRecord);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
        }

        [TestMethod]
        public void PublishMultipleRecordsToTag_ValidMessage_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 54321L;
            
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            var textRecord = new NdefTextRecord { Text = "Hello NFC", LanguageCode = "en" };
            var message = new NdefMessage { uriRecord, textRecord };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishMessageToTag(message);
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
        }

        [TestMethod]
        public void PublishRecord_WithoutInitialization_ShouldThrowException()
        {
            // Arrange
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            
            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _publishingService!.PublishRecordToTag(uriRecord));
        }

        [TestMethod]
        public void StopPublishing_WithActivePublication_ShouldStopSuccessfully()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var messageId = 54321L;
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(messageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            _publishingService.PublishRecordToTag(uriRecord);
            
            // Act
            _publishingService.StopPublishing();
            
            // Assert
            Assert.IsFalse(_publishingService.IsPublishing);
            mockDevice.Verify(d => d.StopPublishingMessage(messageId), Times.Once);
        }

        [TestMethod]
        public void StopPublishing_WithoutActivePublication_ShouldNotThrow()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act & Assert
            // Should not throw exception
            _publishingService.StopPublishing();
            Assert.IsFalse(_publishingService.IsPublishing);
        }

        [TestMethod]
        public void MessageTransmitted_WhenPublishing_ShouldTriggerEvent()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var messageId = 54321L;
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            
            var eventTriggered = false;
            long transmittedMessageId = 0;
            _publishingService!.MessageTransmitted += (id) => 
            {
                eventTriggered = true;
                transmittedMessageId = id;
            };
            
            // Capture the transmitted handler
            MessageTransmittedHandler? capturedHandler = null;
            mockDevice.Setup(d => d.PublishBinaryMessage("NDEF:WriteTag", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Callback<string, byte[], MessageTransmittedHandler>((type, buffer, handler) => capturedHandler = handler)
                     .Returns(messageId);
            
            _publishingService.Initialize(mockDevice.Object);
            _publishingService.PublishRecordToTag(uriRecord);
            
            // Act
            capturedHandler?.Invoke(messageId);
            
            // Assert
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(messageId, transmittedMessageId);
        }

        [TestMethod]
        public void PublishTagLockCommand_ValidDevice_ShouldPublishLockCommand()
        {
            // Arrange
            var mockDevice = new Mock<IProximityDeviceWrapper>();
            var expectedMessageId = 99999L;
            
            mockDevice.Setup(d => d.PublishBinaryMessage("SetTagReadOnly", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()))
                     .Returns(expectedMessageId);
            
            _publishingService!.Initialize(mockDevice.Object);
            
            // Act
            var messageId = _publishingService.PublishTagLockCommand();
            
            // Assert
            Assert.AreEqual(expectedMessageId, messageId);
            Assert.IsTrue(_publishingService.IsPublishing);
            mockDevice.Verify(d => d.PublishBinaryMessage("SetTagReadOnly", It.IsAny<byte[]>(), It.IsAny<MessageTransmittedHandler>()), Times.Once);
        }

        [TestMethod]
        public void ValidateNdefMessageSize_SmallMessage_ShouldReturnTrue()
        {
            // Arrange
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            var message = new NdefMessage { uriRecord };
            
            // Act
            var isValidSize = _publishingService!.ValidateMessageSize(message, 8192); // 8KB limit
            
            // Assert
            Assert.IsTrue(isValidSize);
        }

        [TestMethod]
        public void ValidateNdefMessageSize_LargeMessage_ShouldReturnFalse()
        {
            // Arrange
            var largeText = new string('A', 10000); // 10KB text
            var textRecord = new NdefTextRecord { Text = largeText, LanguageCode = "en" };
            var message = new NdefMessage { textRecord };
            
            // Act
            var isValidSize = _publishingService!.ValidateMessageSize(message, 8192); // 8KB limit
            
            // Assert
            Assert.IsFalse(isValidSize);
        }
    }

    // Test helper class for NFC publishing functionality
    public class TestNfcPublishingService : IDisposable
    {
        private IProximityDeviceWrapper? _device;
        private long _publishingMessageId;
        private bool _disposed;

        public bool IsInitialized => _device != null;
        public bool IsPublishing => _publishingMessageId != 0;

        public event Action<long>? MessageTransmitted;

        public void Initialize(IProximityDeviceWrapper? device)
        {
            _device = device;
        }

        public long PublishRecordToTag(NdefRecord record)
        {
            if (_device == null)
                throw new InvalidOperationException("Device not initialized");

            var message = new NdefMessage { record };
            return PublishMessageToTag(message);
        }

        public long PublishRecordToDevice(NdefRecord record)
        {
            if (_device == null)
                throw new InvalidOperationException("Device not initialized");

            var message = new NdefMessage { record };
            return PublishMessageToDevice(message);
        }

        public long PublishMessageToTag(NdefMessage message)
        {
            if (_device == null)
                throw new InvalidOperationException("Device not initialized");

            StopPublishing(); // Stop any existing publication
            
            var messageData = message.ToByteArray();
            _publishingMessageId = _device.PublishBinaryMessage("NDEF:WriteTag", messageData, OnMessageTransmitted);
            return _publishingMessageId;
        }

        public long PublishMessageToDevice(NdefMessage message)
        {
            if (_device == null)
                throw new InvalidOperationException("Device not initialized");

            StopPublishing(); // Stop any existing publication
            
            var messageData = message.ToByteArray();
            _publishingMessageId = _device.PublishBinaryMessage("NDEF", messageData, OnMessageTransmitted);
            return _publishingMessageId;
        }

        public long PublishTagLockCommand()
        {
            if (_device == null)
                throw new InvalidOperationException("Device not initialized");

            StopPublishing(); // Stop any existing publication
            
            var emptyData = new byte[0];
            _publishingMessageId = _device.PublishBinaryMessage("SetTagReadOnly", emptyData, OnMessageTransmitted);
            return _publishingMessageId;
        }

        public void StopPublishing()
        {
            if (_publishingMessageId != 0 && _device != null)
            {
                _device.StopPublishingMessage(_publishingMessageId);
                _publishingMessageId = 0;
            }
        }

        public bool ValidateMessageSize(NdefMessage message, int maxSizeBytes)
        {
            var messageData = message.ToByteArray();
            return messageData.Length <= maxSizeBytes;
        }

        private void OnMessageTransmitted(long messageId)
        {
            MessageTransmitted?.Invoke(messageId);
            _publishingMessageId = 0; // Reset after transmission
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopPublishing();
                _device = null;
                _disposed = true;
            }
        }
    }
}