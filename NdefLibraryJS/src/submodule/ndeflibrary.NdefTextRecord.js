/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** More information: https://andijakl.github.io/ndef-nfc/
**
** GNU Lesser General Public License Usage
** This file may be used under the terms of the GNU Lesser General Public
** License version 3 as published by the Free Software Foundation and
** appearing in the file LICENSE.LGPL included in the packaging of this
** file. Please review the following information to ensure the GNU Lesser
** General Public License version 3 requirements will be met:
** http://www.gnu.org/licenses/lgpl.html.
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

function initLibraryNdefTextRecord(context) {
    'use strict';

    var NdefLibrary = context.NdefLibrary;


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
    var ndefTextRecord = NdefLibrary.NdefTextRecord = function (opt_config) {

        /// <summary>
        /// Language code that corresponds to the text.
        /// All language codes MUST be done according to RFC 3066 [RFC3066]. The language code MAY NOT be omitted. 
        /// </summary>
        this.LanguageCode = null;

        /// <summary>
        /// The text stored in the text record as a string.
        /// </summary>
        this.text = null;

        ///Constructors
        if (arguments.length == 1) {
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
            NdefLibrary.NdefRecord.call(this, arguments[0]);

            if (!NdefLibrary.NdefTextRecord.isRecordType(this))
                throw "NdefException(NdefExceptionMessages.ExInvalidCopy)";
        }
        else {
            /// <summary>
            /// Create an empty Text record.
            /// </summary>
            NdefLibrary.NdefRecord.call(this, NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd, NdefLibrary.NdefTextRecord.TextType);
        }
    };

    //Derive from NdefRecord
    ndefTextRecord.prototype = new NdefLibrary.NdefRecord();
    ndefTextRecord.prototype.constructor = NdefLibrary.NdefTextRecord;

    /// <summary>
    /// Type of the NDEF Text record (well-known, type 'T').
    /// </summary>
    NdefLibrary.NdefTextRecord.TextType = "T".getBytes(); // U

    /// <summary>
    /// Text encoding to use - either Utf8 or Utf16 (big endian)
    /// </summary>
    NdefLibrary.NdefTextRecord.TextEncodingType = {
        Utf8: 0,
        Utf16: 1
    };

    /// <summary>
    /// Checks if the record sent via the parameter is indeed a Text record.
    /// Only checks the type and type name format, doesn't analyse if the
    /// payload is valid.
    /// </summary>
    /// <param name="record">Record to check.</param>
    /// <returns>True if the record has the correct type and type name format
    /// to be a Text record, false if it's a different record.</returns>
    NdefLibrary.NdefTextRecord.isRecordType = function (record) {
        if (record.getType() == null || record.getType().length == 0) {
            return false;
        }
        return (record.getTypeNameFormat() == NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd && arraysEqual(record.getType(), NdefLibrary.NdefTextRecord.TextType));
    };


    ndefTextRecord.prototype.setLanguageCode = function (value) {
        this.assemblePayload(value, this.getTextEncoding(), this.getText());
    };

    ndefTextRecord.prototype.getLanguageCode = function () {
        if (this.getPayload() == null || this.getPayload().length == 0)
            return "en"; // Default

        var status = this.getPayload()[0];
        var codeLength = (status & 0x3f);

        // US-Ascii encoding, starting at byte 1, length codeLength
        // No ASCII encoding seems to be available, so use UTF8 instead
        // There shouldn't be any special chars in the language codes anyway.
        // var encoding = Encoding.UTF8;
        // return encoding.GetString(Payload, 1, codeLength);

        var encoding = new Array(codeLength);
        arrayCopy(this.getPayload(), 1, encoding, 0, codeLength);

        return decodeURI(fromArray(encoding));
    };


    ndefTextRecord.prototype.setText = function (value) {
        this.assemblePayload(this.getLanguageCode(), this.getTextEncoding(), value);
    };

    ndefTextRecord.prototype.getText = function () {
        if (this.getPayload() == null || this.getPayload().length == 0)
            return ""; // Default

        // var encoding = (this.getTextEncoding() == NdefLibrary.NdefTextRecord.TextEncodingType.Utf8) ? Encoding.UTF8 : Encoding.BigEndianUnicode;
        // Language code length
        var codeLength = (this.getPayload()[0] & 0x3f);

        var encoding = new Array();
        arrayCopy(this.getPayload(), 1 + codeLength, encoding, 0, this.getPayload().length - 1 - codeLength);

        //TODO UTF16 encoding

        return fromArray(encoding);
    };

    ndefTextRecord.prototype.setTextEncoding = function (value) {
        this.assemblePayload(this.getLanguageCode(), value, this.getText());
    };


    ndefTextRecord.prototype.getTextEncoding = function () {
        if (this.getPayload() == null || this.getPayload().length == 0)
            return NdefLibrary.NdefTextRecord.TextEncodingType.Utf8;

        return ((this.getPayload()[0] & 0x80) != 0) ? NdefLibrary.NdefTextRecord.TextEncodingType.Utf16 : NdefLibrary.NdefTextRecord.TextEncodingType.Utf8;
    };

    ndefTextRecord.prototype.assemblePayload = function (languageCode, textEncoding, text) {
        // Convert the language code to a byte array
        // var languageEncoding = Encoding.UTF8;
        var encodedLanguage = languageCode.getBytes();
        // Encode and convert the text to a byte array
        // var encoding = (textEncoding == NdefLibrary.NdefTextRecord.TextEncodingType.Utf8) ? Encoding.UTF8 : Encoding.BigEndianUnicode;
        // var encodedText = encoding.GetBytes(text);
        var encodedText = text.getBytes();

        // Calculate the length of the payload & create the array
        var payloadLength = 1 + encodedLanguage.length + encodedText.length;
        var payload = new Array(payloadLength);

        // Assemble the status byte
        payload[0] = 0; // Make sure also the RFU bit is set to 0
        // Text encoding
        if (textEncoding == NdefLibrary.NdefTextRecord.TextEncodingType.Utf8)
            payload[0] &= 0x7F; // ~0x80
        else
            payload[0] |= 0x80;

        // Language code length
        payload[0] |= (0x3f & encodedLanguage.length);

        // Language code
        arrayCopy(encodedLanguage, 0, payload, 1, encodedLanguage.length);

        // Text
        arrayCopy(encodedText, 0, payload, 1 + encodedLanguage.length, encodedText.length);

        this.setPayload(payload);
    };


}