/****************************************************************************
**
** Copyright (C) 2012-2013 Andreas Jakl, Mopius - http://www.mopius.com/
** Original version copyright (C) 2012 Nokia Corporation and/or its subsidiary(-ies).
** All rights reserved.
**
** This file is based on the respective class of the Connectivity module
** of Qt Mobility (http://qt.gitorious.org/qt-mobility).
**
** Ported to C# by Andreas Jakl (2012)
** More information: http://ndef.codeplex.com/
**
** GNU Lesser General Public License Usage
** This file may be used under the terms of the GNU Lesser General Public
** License version 2.1 as published by the Free Software Foundation and
** appearing in the file LICENSE.LGPL included in the packaging of this
** file. Please review the following information to ensure the GNU Lesser
** General Public License version 2.1 requirements will be met:
** http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html.
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
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// The Text record as specified by the NFC Forum URI record type definition.
    /// </summary>
    /// <remarks>
    /// Stores an arbritary string. Multiple text records can be part of a message,
    /// each of those should have a different language so that the reading device
    /// can choose the most appropriate language. Text can be encoded either in
    /// Utf-8 or Utf-16.
    /// 
    /// The text record is usually used within a Smart Poster record or other meta
    /// records, where the text contained here describes the properties of the
    /// other record, in order to for example guide the user.
    /// </remarks>
    public class NdefTextRecord : NdefRecord
    {
        /// <summary>
        /// Type of the NDEF Text record (well-known, type 'T').
        /// </summary>
        public static readonly byte[] TextType = { (byte)'T' };

        /// <summary>
        /// Text encoding to use - either Utf8 or Utf16 (big endian)
        /// </summary>
        public enum TextEncodingType
        {
            Utf8,
            Utf16   // Specs: If the BOM is omitted, the byte order shall be big-endian
        }

        /// <summary>
        /// Create an empty Text record.
        /// </summary>
        public NdefTextRecord()
            : base(TypeNameFormatType.NfcRtd, TextType)
        {
        }

        /// <summary>
        /// Create a Text record based on the record passed through the argument.
        /// </summary>
        /// <remarks> 
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a Text record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this Text record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a Text record
        /// based on an incompatible record type.</exception>
        public NdefTextRecord(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
        }

        /// <summary>
        /// Create / encode a byte array and set it as payload, considering all properties specified
        /// in the record class.
        /// </summary>
        private void AssemblePayload(string languageCode, TextEncodingType textEncoding, string text)
        {
            //Debug.WriteLine("Text record - AssemblePayload: {0}, {1}, {2}", languageCode, textEncoding, text);
            // Convert the language code to a byte array
            var languageEncoding = Encoding.UTF8;
            var encodedLanguage = languageEncoding.GetBytes(languageCode);
            // Encode and convert the text to a byte array
            var encoding = (textEncoding == TextEncodingType.Utf8) ? Encoding.UTF8 : Encoding.BigEndianUnicode;
            var encodedText = encoding.GetBytes(text);
            // Calculate the length of the payload & create the array
            var payloadLength = 1 + encodedLanguage.Length + encodedText.Length;
            Payload = new byte[payloadLength];

            // Assemble the status byte
            Payload[0] = 0; // Make sure also the RFU bit is set to 0
            // Text encoding
            if (textEncoding == TextEncodingType.Utf8)
                Payload[0] &= 0x7F; // ~0x80
            else
                Payload[0] |= 0x80;

            // Language code length
            Payload[0] |= (byte)(0x3f & (byte)encodedLanguage.Length);

            // Language code
            Array.Copy(encodedLanguage, 0, Payload, 1, encodedLanguage.Length);

            // Text
            Array.Copy(encodedText, 0, Payload, 1 + encodedLanguage.Length, encodedText.Length);
        }

        /// <summary>
        /// Language code that corresponds to the text.
        /// All language codes MUST be done according to RFC 3066 [RFC3066]. The language code MAY NOT be omitted. 
        /// </summary>
        public string LanguageCode
        {
            get
            {
                if (Payload == null || Payload.Length == 0)
                    return "en"; // Default

                var status = Payload.ElementAt(0);
                var codeLength = (byte) (status & 0x3f);

                // US-Ascii encoding, starting at byte 1, length codeLength
                // No ASCII encoding seems to be available, so use UTF8 instead
                // There shouldn't be any special chars in the language codes anyway.
                var encoding = Encoding.UTF8;
                return encoding.GetString(Payload, 1, codeLength);
            }
            set
            {
                AssemblePayload(value, TextEncoding, Text);
            }
        }

        /// <summary>
        /// The text stored in the text record as a string.
        /// </summary>
        public string Text
        {
            get
            {
                if (Payload == null || Payload.Length == 0)
                    return ""; // Default

                var encoding = (TextEncoding == TextEncodingType.Utf8) ? Encoding.UTF8 : Encoding.BigEndianUnicode;
                // Language code length
                var codeLength = (byte)(Payload[0] & 0x3f);
                return encoding.GetString(Payload, 1 + codeLength, Payload.Length - 1 - codeLength);
            }
            set
            {
                AssemblePayload(LanguageCode, TextEncoding, value);
            }
        }

        /// <summary>
        /// The encoding of the text - can be either Utf8 or Utf16.
        /// </summary>
        public TextEncodingType TextEncoding
        {
            get
            {
                if (Payload == null || Payload.Length == 0)
                    return TextEncodingType.Utf8;

                return ((Payload[0] & 0x80) != 0) ? TextEncodingType.Utf16 : TextEncodingType.Utf8;
            }
            set
            {
                AssemblePayload(LanguageCode, value, Text);
            }
        }
        

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Text record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a Text record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Type.SequenceEqual(TextType));
        }
    }
}
