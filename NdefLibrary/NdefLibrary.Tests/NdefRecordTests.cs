using System.Globalization;
using System.Text;

namespace NdefLibrary.Tests;

public class NdefRecordTests
{
    /// <summary>
    /// Serialize the record into a message and parse it back, as it would happen
    /// when writing to and reading from an NFC tag.
    /// </summary>
    private static NdefRecord RoundTrip(NdefRecord record)
    {
        var bytes = new NdefMessage { record }.ToByteArray();
        return Assert.Single(NdefMessage.FromByteArray(bytes));
    }

    [Theory]
    [InlineData("http://www.nfcinteractor.com", 0x01, "nfcinteractor.com")]
    [InlineData("https://www.nfcinteractor.com", 0x02, "nfcinteractor.com")]
    [InlineData("http://example.com", 0x03, "example.com")]
    [InlineData("https://example.com", 0x04, "example.com")]
    [InlineData("tel:+431234", 0x05, "+431234")]
    [InlineData("mailto:a@b.c", 0x06, "a@b.c")]
    [InlineData("custom-scheme:abc", 0x00, "custom-scheme:abc")]
    public void UriRecord_UsesUriIdentifierCodeAbbreviation(string uri, byte expectedCode, string expectedRest)
    {
        var record = new NdefUriRecord { Uri = uri };

        Assert.Equal(expectedCode, record.Payload[0]);
        Assert.Equal(expectedRest, Encoding.UTF8.GetString(record.Payload, 1, record.Payload.Length - 1));
        Assert.Equal(uri, new NdefUriRecord(RoundTrip(record)).Uri);
    }

    [Fact]
    public void UriRecord_NonAsciiCharacters_AreUtf8Encoded()
    {
        const string uri = "https://例え.jp/äöü";
        var parsed = new NdefUriRecord(RoundTrip(new NdefUriRecord { Uri = uri }));

        Assert.Equal(uri, parsed.Uri);
    }

    [Fact]
    public void TextRecord_Utf8_EncodesStatusByteAndLanguage()
    {
        var record = new NdefTextRecord { Text = "Hello", LanguageCode = "en" };

        var expected = new byte[] { 0x02, (byte)'e', (byte)'n' }.Concat(Encoding.UTF8.GetBytes("Hello"));
        Assert.Equal(expected, record.Payload);
        Assert.Equal(NdefTextRecord.TextEncodingType.Utf8, record.TextEncoding);
    }

    [Fact]
    public void TextRecord_Utf16_SetsEncodingFlagAndRoundTrips()
    {
        var record = new NdefTextRecord
        {
            TextEncoding = NdefTextRecord.TextEncodingType.Utf16,
            LanguageCode = "de-AT",
            Text = "Grüß Gott",
        };

        Assert.Equal(0x80 | 5, record.Payload[0]);

        var parsed = new NdefTextRecord(RoundTrip(record));
        Assert.Equal(NdefTextRecord.TextEncodingType.Utf16, parsed.TextEncoding);
        Assert.Equal("de-AT", parsed.LanguageCode);
        Assert.Equal("Grüß Gott", parsed.Text);
    }

    [Fact]
    public void SmartPoster_WithTitleAndAction_RoundTrips()
    {
        var record = new NdefSpRecord
        {
            Uri = "https://andijakl.github.io/ndef-nfc/",
            NfcAction = NdefSpActRecord.NfcActionType.SaveForLater,
        };
        record.AddTitle(new NdefTextRecord { Text = "NDEF Library", LanguageCode = "en" });
        record.AddTitle(new NdefTextRecord { Text = "NDEF Bibliothek", LanguageCode = "de" });

        var parsed = RoundTrip(record);
        Assert.Equal(typeof(NdefSpRecord), parsed.CheckSpecializedType(false));

        var sp = new NdefSpRecord(parsed);
        Assert.Equal("https://andijakl.github.io/ndef-nfc/", sp.Uri);
        Assert.True(sp.ActionInUse());
        Assert.Equal(NdefSpActRecord.NfcActionType.SaveForLater, sp.NfcAction);
        Assert.Equal(2, sp.TitleCount());
        Assert.Equal("NDEF Library", sp.Title(0).Text);
        Assert.Equal("de", sp.Title(1).LanguageCode);
    }

    [Fact]
    public void GeoRecord_UsesInvariantDecimalSeparator_RegardlessOfCurrentCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            // German uses "," as decimal separator, which must not end up in the URI
            CultureInfo.CurrentCulture = new CultureInfo("de-AT");
            var record = new NdefGeoRecord { Latitude = 48.2082, Longitude = 16.3738 };

            Assert.Equal("geo:48.2082,16.3738", record.Uri);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void GeoRecord_MsDriveTo_AddsEscapedPlaceTitle()
    {
        var record = new NdefGeoRecord
        {
            GeoType = NdefGeoRecord.NfcGeoType.MsDriveTo,
            Latitude = 48.2,
            Longitude = 16.3,
            PlaceTitle = "St. Pölten",
        };

        Assert.Equal(
            "ms-drive-to:?destination.latitude=48.2&destination.longitude=16.3&destination.name=St.%20P%C3%B6lten",
            record.Uri);
    }

    [Fact]
    public void TelRecord_IsDetectedAsSpecializedType()
    {
        var parsed = RoundTrip(new NdefTelRecord { TelNumber = "+431234567" });

        Assert.Equal(typeof(NdefTelRecord), parsed.CheckSpecializedType(true));
        Assert.Equal(typeof(NdefUriRecord), parsed.CheckSpecializedType(false));
        Assert.Equal("+431234567", new NdefTelRecord(parsed).TelNumber);
    }

    [Fact]
    public void SmsRecord_RoundTripsNumberAndBody()
    {
        var record = new NdefSmsRecord { SmsNumber = "+431234567", SmsBody = "Hello NFC & co" };

        var parsed = RoundTrip(record);
        Assert.Equal(typeof(NdefSmsRecord), parsed.CheckSpecializedType(true));

        var sms = new NdefSmsRecord(parsed);
        Assert.Equal("+431234567", sms.SmsNumber);
        Assert.Equal("Hello NFC & co", sms.SmsBody);
    }

    [Fact]
    public void MailtoRecord_RoundTripsAddressSubjectAndBody()
    {
        var record = new NdefMailtoRecord { Address = "nfc@example.com", Subject = "Hello World", Body = "Body text" };

        Assert.Equal("mailto:nfc@example.com?subject=Hello%20World&body=Body%20text", record.Uri);

        var parsed = RoundTrip(record);
        Assert.Equal(typeof(NdefMailtoRecord), parsed.CheckSpecializedType(true));

        var mail = new NdefMailtoRecord(parsed);
        Assert.Equal("nfc@example.com", mail.Address);
        Assert.Equal("Hello World", mail.Subject);
        Assert.Equal("Body text", mail.Body);
    }

    [Fact]
    public void LaunchAppRecord_RoundTripsPlatformIdsAndArguments()
    {
        var record = new NdefLaunchAppRecord { Arguments = "user=default" };
        record.AddPlatformAppId("Windows", "Example.Proximity_8wekyb3d8bbwe!App");
        record.AddPlatformAppId("WindowsPhone", "12345678-1234-1234-1234-123456789012");

        var parsed = RoundTrip(record);
        Assert.Equal(typeof(NdefLaunchAppRecord), parsed.CheckSpecializedType(false));

        var launch = new NdefLaunchAppRecord(parsed);
        Assert.Equal("user=default", launch.Arguments);
        Assert.Equal("Example.Proximity_8wekyb3d8bbwe!App", launch.PlatformIds["Windows"]);
        // Windows Phone IDs are automatically wrapped in braces
        Assert.Equal("{12345678-1234-1234-1234-123456789012}", launch.PlatformIds["WindowsPhone"]);
    }

    [Fact]
    public void AndroidAppRecord_UsesExternalTypeAndRoundTrips()
    {
        var record = new NdefAndroidAppRecord { PackageName = "com.example.nfc" };

        Assert.Equal(NdefRecord.TypeNameFormatType.ExternalRtd, record.TypeNameFormat);
        Assert.Equal("android.com:pkg", Encoding.ASCII.GetString(record.Type));

        var parsed = RoundTrip(record);
        Assert.Equal(typeof(NdefAndroidAppRecord), parsed.CheckSpecializedType(false));
        Assert.Equal("com.example.nfc", new NdefAndroidAppRecord(parsed).PackageName);
    }
}
