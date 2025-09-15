using Microsoft.VisualStudio.TestTools.UnitTesting;
using NdefLibrary.Ndef;
using System;
using System.Linq;
using System.Text;

namespace NdefDemoWinUI3.Tests
{
    /// <summary>
    /// Unit tests for NDEF record creation and parsing functionality.
    /// Tests Requirements: 3.1, 3.2, 3.3
    /// </summary>
    [TestClass]
    public class NdefRecordTests
    {
        [TestMethod]
        public void CreateUriRecord_ValidUri_ShouldCreateCorrectRecord()
        {
            // Arrange
            var expectedUri = "http://www.nfcinteractor.com/";
            
            // Act
            var record = new NdefUriRecord { Uri = expectedUri };
            
            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(expectedUri, record.Uri);
            Assert.AreEqual(NdefRecord.TypeNameFormatType.NfcRtd, record.TypeNameFormat);
        }

        [TestMethod]
        public void CreateMailtoRecord_ValidEmailData_ShouldCreateCorrectRecord()
        {
            // Arrange
            var expectedAddress = "test@example.com";
            var expectedSubject = "Test Subject";
            var expectedBody = "Test Body";
            
            // Act
            var record = new NdefMailtoRecord
            {
                Address = expectedAddress,
                Subject = expectedSubject,
                Body = expectedBody
            };
            
            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(expectedAddress, record.Address);
            Assert.AreEqual(expectedSubject, record.Subject);
            Assert.AreEqual(expectedBody, record.Body);
        }

        [TestMethod]
        public void CreateVcardRecord_ValidContactData_ShouldCreateCorrectRecord()
        {
            // Arrange
            var contact = new NdefContact
            {
                FirstName = "John",
                LastName = "Doe"
            };
            contact.Emails.Add(new NdefContactEmail { Address = "john.doe@example.com", Kind = NdefContactEmailKind.Work });
            contact.Phones.Add(new NdefContactPhone { Number = "+1234567890", Kind = NdefContactPhoneKind.Home });
            
            // Act
            var record = new NdefVcardRecord { ContactData = contact };
            
            // Assert
            Assert.IsNotNull(record);
            Assert.IsNotNull(record.ContactData);
            Assert.AreEqual("John", record.ContactData.FirstName);
            Assert.AreEqual("Doe", record.ContactData.LastName);
            Assert.AreEqual(1, record.ContactData.Emails.Count);
            Assert.AreEqual("john.doe@example.com", record.ContactData.Emails.First().Address);
            Assert.AreEqual(1, record.ContactData.Phones.Count);
            Assert.AreEqual("+1234567890", record.ContactData.Phones.First().Number);
        }

        [TestMethod]
        public void CreateLaunchAppRecord_ValidAppData_ShouldCreateCorrectRecord()
        {
            // Arrange
            var expectedArguments = "test arguments";
            var expectedPlatform = "Windows";
            var expectedAppId = "test.app.id";
            
            // Act
            var record = new NdefLaunchAppRecord { Arguments = expectedArguments };
            record.AddPlatformAppId(expectedPlatform, expectedAppId);
            
            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(expectedArguments, record.Arguments);
            Assert.IsNotNull(record.PlatformIds);
            Assert.IsTrue(record.PlatformIds.Any(p => p.Key == expectedPlatform && p.Value == expectedAppId));
        }

        [TestMethod]
        public void CreateGeoRecord_ValidLocationData_ShouldCreateCorrectRecord()
        {
            // Arrange
            var expectedLatitude = 48.208415;
            var expectedLongitude = 16.371282;
            var expectedTitle = "Vienna, Austria";
            
            // Act
            var record = new NdefGeoRecord
            {
                GeoType = NdefGeoRecord.NfcGeoType.MsDriveTo,
                Latitude = expectedLatitude,
                Longitude = expectedLongitude,
                PlaceTitle = expectedTitle
            };
            
            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(expectedLatitude, record.Latitude);
            Assert.AreEqual(expectedLongitude, record.Longitude);
            Assert.AreEqual(expectedTitle, record.PlaceTitle);
            Assert.AreEqual(NdefGeoRecord.NfcGeoType.MsDriveTo, record.GeoType);
        }

        [TestMethod]
        public void CreateWindowsSettingsRecord_ValidSettingsApp_ShouldCreateCorrectRecord()
        {
            // Arrange
            var expectedSettingsApp = NdefWindowsSettingsRecord.NfcSettingsApp.DevicesNfc;
            
            // Act
            var record = new NdefWindowsSettingsRecord { SettingsApp = expectedSettingsApp };
            
            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(expectedSettingsApp, record.SettingsApp);
        }

        [TestMethod]
        public void CreateNdefMessage_MultipleRecords_ShouldCreateValidMessage()
        {
            // Arrange
            var uriRecord = new NdefUriRecord { Uri = "http://example.com" };
            var mailtoRecord = new NdefMailtoRecord { Address = "test@example.com" };
            
            // Act
            var message = new NdefMessage { uriRecord, mailtoRecord };
            
            // Assert
            Assert.IsNotNull(message);
            Assert.AreEqual(2, message.Count);
            Assert.IsInstanceOfType(message[0], typeof(NdefUriRecord));
            Assert.IsInstanceOfType(message[1], typeof(NdefMailtoRecord));
        }

        [TestMethod]
        public void SerializeAndDeserializeNdefMessage_ValidMessage_ShouldPreserveData()
        {
            // Arrange
            var originalUri = "http://www.nfcinteractor.com/";
            var originalRecord = new NdefUriRecord { Uri = originalUri };
            var originalMessage = new NdefMessage { originalRecord };
            
            // Act
            var serializedData = originalMessage.ToByteArray();
            var deserializedMessage = NdefMessage.FromByteArray(serializedData);
            
            // Assert
            Assert.IsNotNull(deserializedMessage);
            Assert.AreEqual(1, deserializedMessage.Count);
            
            var deserializedRecord = deserializedMessage[0];
            Assert.AreEqual(typeof(NdefUriRecord), deserializedRecord.CheckSpecializedType(true));
            
            var uriRecord = new NdefUriRecord(deserializedRecord);
            Assert.AreEqual(originalUri, uriRecord.Uri);
        }

        [TestMethod]
        public void ParseInvalidNdefData_MalformedData_ShouldThrowNdefException()
        {
            // Arrange
            var invalidData = new byte[] { 0x00, 0x01, 0x02 }; // Invalid NDEF data
            
            // Act & Assert
            Assert.ThrowsException<NdefException>(() => NdefMessage.FromByteArray(invalidData));
        }

        [TestMethod]
        public void CreateSmartPosterRecord_ValidData_ShouldCreateCorrectRecord()
        {
            // Arrange
            var expectedUri = "http://example.com";
            var expectedTitle = "Example Title";
            
            // Act
            var spRecord = new NdefSpRecord();
            spRecord.Uri = expectedUri;
            spRecord.AddTitle(new NdefTextRecord { Text = expectedTitle, LanguageCode = "en" });
            spRecord.NfcAction = NdefSpActRecord.NfcActionType.DoAction;
            
            // Assert
            Assert.IsNotNull(spRecord);
            Assert.AreEqual(expectedUri, spRecord.Uri);
            Assert.AreEqual(1, spRecord.TitleCount());
            Assert.AreEqual(expectedTitle, spRecord.Titles[0].Text);
            Assert.IsTrue(spRecord.ActionInUse());
            Assert.AreEqual(NdefSpActRecord.NfcActionType.DoAction, spRecord.NfcAction);
        }

        [TestMethod]
        public void CreateTextRecord_ValidTextData_ShouldCreateCorrectRecord()
        {
            // Arrange
            var expectedText = "Hello, NFC World!";
            var expectedLanguage = "en";
            
            // Act
            var record = new NdefTextRecord
            {
                Text = expectedText,
                LanguageCode = expectedLanguage
            };
            
            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(expectedText, record.Text);
            Assert.AreEqual(expectedLanguage, record.LanguageCode);
        }

        [TestMethod]
        public void CreateEmptyNdefMessage_ShouldCreateValidEmptyMessage()
        {
            // Act
            var message = new NdefMessage();
            
            // Assert
            Assert.IsNotNull(message);
            Assert.AreEqual(0, message.Count);
        }

        [TestMethod]
        public void AddRecordToNdefMessage_ValidRecord_ShouldIncreaseCount()
        {
            // Arrange
            var message = new NdefMessage();
            var record = new NdefUriRecord { Uri = "http://example.com" };
            
            // Act
            message.Add(record);
            
            // Assert
            Assert.AreEqual(1, message.Count);
            Assert.AreSame(record, message[0]);
        }
    }
}