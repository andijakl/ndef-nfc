/****************************************************************************
**
** Copyright (C) 2014 Andreas Jakl / Mopius.
** All rights reserved.
**
** Code example for the NDEF Library for Proximity APIs (NFC).
**
** Created by Andreas Jakl (2014).
** More information: http://ndef.mopius.com/
**
** GNU General Public License Usage
** Alternatively, this file may be used under the terms of the GNU General
** Public License version 3.0 as published by the Free Software Foundation
** and appearing in the file LICENSE.GPL included in the packaging of this
** file. Please review the following information to ensure the GNU General
** Public License version 3.0 requirements will be met:
** http://www.gnu.org/copyleft/gpl.html.
**
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NdefLibrary.Ndef;

namespace NfcTagGenerator
{
    public class NfcTagGenerator
    {
        private const string FileDirectory = "NFCtags";
        private const string FileNameHex = "NfcTag-Hex-{0}.ndef";
        private const string FileNameBin = "NfcTag-Bin-{0}.ndef";

        private static readonly Dictionary<string, NdefRecord> NfcRecords = new Dictionary<string, NdefRecord>
        {
            {"URL-http", new NdefUriRecord { Uri = "http://ndef.mopius.com"}},
            {"URL", new NdefUriRecord { Uri = "nfcinteractor:compose"}},
            {"URL-SpecialChars", new NdefUriRecord { Uri = "custom:Testmessage -_(){}\":@äöüÄÖÜ"}},
            {"Mailto", new NdefMailtoRecord { Subject = "Feedback for the NDEF Library", Body = "I think the NDEF library is ...", Address = "andreas.jakl@mopius.com"}},
            {"SMS", new NdefSmsRecord { SmsNumber = "+1234", SmsBody = "Check out the NDEF library at http://ndef.mopius.com/"}},
            {"SMS-SpecialChars", new NdefSmsRecord { SmsNumber = "+1 (2) 3456 - 789", SmsBody = "Testmessage -_(){}\":@äöüÄÖÜ"}},
            {"Geo", new NdefGeoRecord { Latitude = 48.168604, Longitude = 16.33375, GeoType = NdefGeoRecord.NfcGeoType.GeoUri}},
            {"Android", new NdefAndroidAppRecord { PackageName = "com.twitter.android"}},
            {"Social", new NdefSocialRecord { SocialType = NdefSocialRecord.NfcSocialType.Twitter, SocialUserName = "mopius"}},
            {"TextUtf8", new NdefTextRecord { Text = "Mopius", LanguageCode = "en", TextEncoding = NdefTextRecord.TextEncodingType.Utf8}},
            {"TextUtf16", new NdefTextRecord { Text = "Mopius", LanguageCode = "en", TextEncoding = NdefTextRecord.TextEncodingType.Utf16}},
            {"Tel", new NdefTelRecord { TelNumber = "+1234" }},
            {"Ext", new NdefRecord { TypeNameFormat = NdefRecord.TypeNameFormatType.ExternalRtd, Type = Encoding.UTF8.GetBytes("mopius.com:nfc"), Payload = Encoding.UTF8.GetBytes("Testing")}},
            {"Empty", new NdefRecord { TypeNameFormat = NdefRecord.TypeNameFormatType.Empty }}
        };

        static void Main(string[] args)
        {
            // Dynamically construct some more NDEF records
            var spRecord = new NdefSpRecord
            {
                Uri = "http://ndef.mopius.com",
                NfcAction = NdefSpActRecord.NfcActionType.DoAction
            };
            spRecord.AddTitle(new NdefTextRecord { LanguageCode = "en", Text = "NFC Library" });
            spRecord.AddTitle(new NdefTextRecord { LanguageCode = "de", Text = "NFC Bibliothek" });
            NfcRecords.Add("SmartPoster", spRecord);

            // Ensure the path exists
            var tagsDirectory = Path.Combine(Environment.CurrentDirectory, FileDirectory);
            Directory.CreateDirectory(tagsDirectory);

            // Write tag contents to files
            foreach (var curNdefRecord in NfcRecords)
            {
                WriteTagFile(tagsDirectory, curNdefRecord.Key, curNdefRecord.Value);
            }

            // Multi-record file
            var record1 = new NdefUriRecord {Uri = "http://www.twitter.com"};
            var record2 = new NdefAndroidAppRecord {PackageName = "com.twitter.android"};
            var twoRecordsMsg = new NdefMessage {record1, record2};
            WriteTagFile(tagsDirectory, "TwoRecords", twoRecordsMsg);

            var record3 = new NdefRecord
            {
                TypeNameFormat = NdefRecord.TypeNameFormatType.ExternalRtd,
                Type = Encoding.UTF8.GetBytes("custom.com:myapp")
            };
            var threeRecordsMsg = new NdefMessage { record1, record3, record2 };
            WriteTagFile(tagsDirectory, "ThreeRecords", threeRecordsMsg);

            // Success message on output
            Console.WriteLine("Generated {0} tag files in {1}.", NfcRecords.Count, tagsDirectory);
            Debug.WriteLine("Generated {0} tag files in {1}.", NfcRecords.Count, tagsDirectory);
        }

        private static void WriteTagFile(string pathName, string tagName, NdefRecord ndefRecord)
        {
            WriteTagFile(pathName, tagName, new NdefMessage { ndefRecord });
        }

        private static void WriteTagFile(string pathName, string tagName, NdefMessage ndefMessage)
        {
            // NDEF message
            var ndefMessageBytes = ndefMessage.ToByteArray();

            // Write NDEF message to binary file
            var binFileName = String.Format(FileNameBin, tagName);
            using (var fs = File.Create(Path.Combine(pathName, binFileName)))
            {
                foreach (var curByte in ndefMessageBytes)
                {
                    fs.WriteByte(curByte);
                }
            }

            // Write NDEF message to hex file
            var hexFileName = String.Format(FileNameHex, tagName);
            using (var fs = File.Create(Path.Combine(pathName, hexFileName)))
            {
                using (var logFileWriter = new StreamWriter(fs))
                {
                    logFileWriter.Write(ConvertToHexByteString(ndefMessageBytes));
                }
            }
        }

        private static string ConvertToHexByteString(byte[] ndefMessageBytes)
        {
            return BitConverter.ToString(ndefMessageBytes).Replace("-", " ");
        }
    }
}
