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

function initLibraryNdefMessage(context) {
    'use strict';

    var NdefLibrary = context.NdefLibrary;


    /// <summary>
    /// An NDEF message is composed of one or more NDEF records.
    /// </summary>
    /// <remarks>
    /// This class is essentially just a list of records, and provides
    /// the necessary methods to convert all the records into a single
    /// byte array that can be written to a tag and has the correct
    /// flags set (e.g., message begin / end).
    /// NdefMessage can also parse a byte array into an NDEF message,
    /// separating and creating all the individual records out of the array.
    /// 
    /// From the NFC Forum specification document:
    /// NFC Forum Data Exchange Format is a lightweight binary message 
    /// format designed to encapsulate one or more application-defined 
    /// payloads into a single message construct.
    /// 
    /// An NDEF message contains one or more NDEF records, each carrying 
    /// a payload of arbitrary type and up to (2^32)-1 octets in size. 
    /// Records can be chained together to support larger payloads. 
    /// 
    /// An NDEF record carries three parameters for describing its payload: 
    /// the payload length, the payload type, and an optional payload identifier.
    /// </remarks>
    var ndefMessage = NdefLibrary.NdefMessage = function () {

        //Holds the NdefRecords added to the NdefMessge
        this._records = new Array();

        if (arguments != null && arguments.length > 0) {
            // console.log("Create NdefMessage with "+ arguments.length+" records");
            arrayCopy(arguments, this._records, arguments.length);
        }
    };


    ndefMessage.prototype.length = function () {
        if (this._records == null)
            return 0;
        return this._records.length;
    };

    ndefMessage.prototype.getRecords = function () {
        return this._records;
    };

    ndefMessage.prototype.push = function (value) {
        if (value == null) {
            return;
        }
        this._records.push(value);
    };

    ndefMessage.prototype.clear = function () {
        this._records = new Array();
    };


    /// <summary>
    /// Returns the NDEF message parsed from the contents of <paramref name="message"/>.
    /// </summary>
    /// <remarks>
    /// The <paramref name="message"/> parameter is interpreted as the raw message format 
    /// defined in the NFC Forum specifications.
    /// </remarks>
    /// <param name="message">Raw byte array containing the NDEF message, which consists
    /// of 0 or more NDEF records.</param>
    /// <exception cref="NdefException">Thrown if there is an error parsing the NDEF
    /// message out of the byte array.</exception>
    /// <returns>If parsing was successful, the NDEF message containing 0 or more NDEF
    /// records.</returns>
    NdefLibrary.NdefMessage.fromByteArray = function (message) {

        var result = new NdefLibrary.NdefMessage();

        var seenMessageBegin = false;
        var seenMessageEnd = false;

        var partialChunk = new Array();
        var record = new NdefLibrary.NdefRecord();

        var i = 0;
        while (i < message.length) {
            // console.log("Parsing byte[] to NDEF message. New record starts at {0}", i);

            // Parse flags out of NDEF message header
            var messageBegin = (message[i] & 0x80) != 0;
            var messageEnd = (message[i] & 0x40) != 0;
            var cf = (message[i] & 0x20) != 0;
            var sr = (message[i] & 0x10) != 0;
            var il = (message[i] & 0x08) != 0;

            var typeNameFormat = (message[i] & 0x07);

            // console.log("TypeNameFormat: " + typeNameFormat+" message[i]:"+message[i]);
            // console.log("ShortRecord: " + (sr ? "yes" : "no"));
            // console.log("Id Length present: " + (il ? "yes" : "no"));

            if (messageBegin && seenMessageBegin) {
                throw "NdefExceptionMessages.ExMessageBeginLate";
            }
            else if (!messageBegin && !seenMessageBegin) {
                throw "NdefExceptionMessages.ExMessageBeginMissing";
            }
            else if (messageBegin && !seenMessageBegin) {
                seenMessageBegin = true;
            }

            if (messageEnd && seenMessageEnd) {
                throw "NdefExceptionMessages.ExMessageEndLate";
            }
            else if (messageEnd && !seenMessageEnd) {
                seenMessageEnd = true;
            }

            if (cf && (typeNameFormat != NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) && partialChunk.length > 0) {
                throw "NdefExceptionMessages.ExMessagePartialChunk";
            }

            // Header length
            var headerLength = 1;
            headerLength += (sr) ? 1 : 4;
            headerLength += (il) ? 1 : 0;

            if (i + headerLength >= message.length) {
                throw "NdefExceptionMessages.ExMessageUnexpectedEnd";
            }

            // Type length
            var typeLength = message[++i];

            if ((typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) && (typeLength != 0)) {
                throw "NdefExceptionMessages.ExMessageInvalidChunkedType";
            }

            // Payload length (short record?)
            var payloadLength;
            if (sr) {
                // Short record - payload length is a single octet
                payloadLength = message[++i];
            }
            else {
                // No short record - payload length is four octets representing a 32 bit unsigned integer (MSB-first)
                payloadLength = ((message[++i]) << 24);
                payloadLength |= ((message[++i]) << 16);
                payloadLength |= ((message[++i]) << 8);
                payloadLength |= ((message[++i]) << 0);
            }

            // ID length
            var idLength;
            idLength = (il ? message[++i] : 0);

            // Total length of content (= type + payload + ID)
            var contentLength = typeLength + payloadLength + idLength;
            if (i + contentLength >= message.length) {
                throw "NdefExceptionMessages.ExMessageUnexpectedEnd";
            }


            if ((typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) && (idLength != 0)) {
                throw "NdefExceptionMessages.ExMessageInvalidChunkedId";
            }

            if (typeNameFormat != NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) {
                record.setTypeNameFormat(typeNameFormat);
            }

            // Read type
            if (typeLength > 0) {
                var type = new Array();
                arrayCopy(message, (++i), type, 0, typeLength);
                record.setType(type);

                i += typeLength - 1;
            }

            // Read ID
            if (idLength > 0) {
                var id = new Array();
                arrayCopy(message, (++i), id, 0, idLength);
                record.setId(id);

                i += idLength - 1;
            }

            // Read payload
            if (payloadLength > 0) {
                var payload = new Array();
                arrayCopy(message, (++i), payload, 0, payloadLength);
                record.setPayload(payload);

                if (cf) {
                    // chunked payload, except last
                    addToArray(partialChunk, payload);
                }
                else if (typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) {
                    // last chunk of chunked payload
                    addToArray(partialChunk, payload);
                    record.setPayload(partialChunk);
                }
                else {
                    // non-chunked payload
                    record.setPayload(payload);
                }

                i += payloadLength - 1;
            }

            if (!cf) {
                // Add record to the message and create a new record for the next loop iteration
                result.push(record);
                record = new NdefLibrary.NdefRecord();
            }

            if (!cf && seenMessageEnd)
                break;

            // move to start of next record
            ++i;
        }


        if (!seenMessageBegin && !seenMessageEnd) {
            throw "NdefExceptionMessages.ExMessageNoBeginOrEnd";
        }

        return result;

    };


    /// <summary>
    /// Convert all the NDEF records currently stored in the NDEF message to a byte
    /// array suitable for writing to a tag or sending to another device.
    /// </summary>
    /// <returns>The NDEF record(s) converted to an NDEF message.</returns>
    ndefMessage.prototype.toByteArray = function () {

        var count = this._records.length;

        // Empty message: single empty record
        if (count == 0) {
            var tmpNdefMessage = new NdefLibrary.NdefMessage(new NdefLibrary.NdefRecord());
            return tmpNdefMessage.toByteArray();
        }

        var m = new Array();

        for (var i = 0; i < count; i++) {
            var record = this._records[i];

            var flags = record.getTypeNameFormat();

            // Message begin / end flags. If there is only one record in the message,
            // both flags are set.
            if (i == 0)
                flags |= 0x80;      // MB (message begin = first record in the message)
            if (i == count - 1)
                flags |= 0x40;      // ME (message end = last record in the message)

            // cf (chunked records) not supported yet

            // SR (Short Record)?
            if (record.getPayload() == null || record.getPayload().length < 255)
                flags |= 0x10;

            // ID present?
            if (record.getId() != null && record.getId().length > 0)
                flags |= 0x08;

            m.push(flags);

            // Type length
            if (record.getType() != null) m.push(record.getType().length); else m.push(0);

            // Payload length 1 byte (SR) or 4 bytes
            if (record.getPayload() == null)
                m.push(0);
            else {
                if ((flags & 0x10) != 0) {
                    // SR
                    m.push(record.getPayload().length);
                }
                else {
                    // No SR (Short Record)
                    var payloadLength = record.getPayload().length;

                    m.push((payloadLength >> 24));
                    m.push((payloadLength >> 16));
                    m.push((payloadLength >> 8));
                    m.push((payloadLength & 0x000000ff));
                }
            }

            // ID length
            if (record.getId() != null && (flags & 0x08) != 0)
                m.push(record.getId().length);

            // Type length
            if (record.getType() != null && record.getType().length > 0)
                addToArray(m, record.getType());

            // ID data
            if (record.getId() != null && record.getId().length > 0)
                addToArray(m, record.getId());

            // Payload data
            if (record.getPayload() != null && record.getPayload().length > 0)
                addToArray(m, record.getPayload());

        }

        return m;
    };

}