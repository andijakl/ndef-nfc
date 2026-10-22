using System.Text;

namespace NdefLibrary.Tests;

public class NdefMessageTests
{
    // Single short URI record: MB | ME | SR | TNF=Well Known, type "U",
    // abbreviation 0x02 ("https://www.") + "nfcinteractor.com"
    private static readonly byte[] UriMessageBytes =
        new byte[] { 0xD1, 0x01, 0x12, (byte)'U', 0x02 }
            .Concat(Encoding.UTF8.GetBytes("nfcinteractor.com"))
            .ToArray();

    [Fact]
    public void ToByteArray_SingleUriRecord_MatchesNdefSpecification()
    {
        var message = new NdefMessage { new NdefUriRecord { Uri = "https://www.nfcinteractor.com" } };

        Assert.Equal(UriMessageBytes, message.ToByteArray());
    }

    [Fact]
    public void FromByteArray_SingleUriRecord_ParsesRecord()
    {
        var message = NdefMessage.FromByteArray(UriMessageBytes);

        var record = Assert.Single(message);
        Assert.Equal(NdefRecord.TypeNameFormatType.NfcRtd, record.TypeNameFormat);
        Assert.Equal(typeof(NdefUriRecord), record.CheckSpecializedType(false));
        Assert.Equal("https://www.nfcinteractor.com", new NdefUriRecord(record).Uri);
    }

    [Fact]
    public void ToByteArray_MultipleRecords_SetsMessageBeginAndEndFlags()
    {
        var message = new NdefMessage
        {
            new NdefUriRecord { Uri = "https://example.com/1" },
            new NdefTextRecord { Text = "middle", LanguageCode = "en" },
            new NdefUriRecord { Uri = "https://example.com/3" },
        };

        var parsed = NdefMessage.FromByteArray(message.ToByteArray());

        Assert.Equal(3, parsed.Count);
        Assert.Equal("https://example.com/1", new NdefUriRecord(parsed[0]).Uri);
        Assert.Equal("middle", new NdefTextRecord(parsed[1]).Text);
        Assert.Equal("https://example.com/3", new NdefUriRecord(parsed[2]).Uri);

        var bytes = message.ToByteArray();
        Assert.True((bytes[0] & 0x80) != 0, "First record must have the MB flag set");
        Assert.True((bytes[0] & 0x40) == 0, "First record must not have the ME flag set");
    }

    [Fact]
    public void RoundTrip_PayloadLongerThan255Bytes_UsesLongRecordFormat()
    {
        var longText = new string('x', 400);
        var message = new NdefMessage { new NdefTextRecord { Text = longText, LanguageCode = "en" } };

        var bytes = message.ToByteArray();
        Assert.True((bytes[0] & 0x10) == 0, "SR flag must not be set for payloads > 255 bytes");

        var parsed = NdefMessage.FromByteArray(bytes);
        Assert.Equal(longText, new NdefTextRecord(Assert.Single(parsed)).Text);
    }

    [Fact]
    public void FromByteArray_MissingMessageBegin_Throws()
    {
        var invalid = (byte[])UriMessageBytes.Clone();
        invalid[0] &= 0x7F; // Clear MB flag

        Assert.Throws<NdefException>(() => NdefMessage.FromByteArray(invalid));
    }
}
