/****************************************************************************
**
** Copyright (C) 2012-2018 Andreas Jakl - https://www.nfcinteractor.com/
** Original version copyright (C) 2012 Nokia Corporation and/or its subsidiary(-ies).
** All rights reserved.
**
** This file is based on the respective class of the Connectivity module
** of Qt Mobility (http://qt.gitorious.org/qt-mobility).
**
** Ported to C# by Andreas Jakl (2012)
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NdefLibrary.Ndef
{
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
    [ComVisible(false)]
    public class NdefMessage : List<NdefRecord>
    {
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
        public static NdefMessage FromByteArray(byte[] message)
        {
            var result = new NdefMessage();

            var seenMessageBegin = false;
            var seenMessageEnd = false;

            var partialChunk = new MemoryStream();
            var record = new NdefRecord();

            uint i = 0;
            while (i < message.Length)
            {
                //Debug.WriteLine("Parsing byte[] to NDEF message. New record starts at {0}", i);

                // Parse flags out of NDEF message header

                // The MB flag is a 1-bit field that when set indicates the start of an NDEF message.
                bool messageBegin = (message[i] & 0x80) != 0;
                // The ME flag is a 1-bit field that when set indicates the end of an NDEF message.
                // Note, that in case of a chunked payload, the ME flag is set only in the terminating record chunk of that chunked payload.
                bool messageEnd = (message[i] & 0x40) != 0;
                // The CF flag is a 1-bit field indicating that this is either the first record chunk or a middle record chunk of a chunked payload.
                bool cf = (message[i] & 0x20) != 0;
                // The SR flag is a 1-bit field indicating, if set, that the PAYLOAD_LENGTH field is a single octet.
                bool sr = (message[i] & 0x10) != 0;
                // The IL flag is a 1-bit field indicating, if set, that the ID_LENGTH field is present in the header as a single octet. 
                // If the IL flag is zero, the ID_LENGTH field is omitted from the record header and the ID field is also omitted from the record.
                bool il = (message[i] & 0x08) != 0;
                var typeNameFormat = (NdefRecord.TypeNameFormatType)(message[i] & 0x07);

                //Debug.WriteLine("ShortRecord: " + (sr ? "yes" : "no"));
                //Debug.WriteLine("Id Length present: " + (il ? "yes" : "no"));

                if (messageBegin && seenMessageBegin)
                {
                    throw new NdefException(NdefExceptionMessages.ExMessageBeginLate);
                }
                else if (!messageBegin && !seenMessageBegin)
                {
                    throw new NdefException(NdefExceptionMessages.ExMessageBeginMissing);
                }
                else if (messageBegin && !seenMessageBegin)
                {
                    seenMessageBegin = true;
                }

                if (messageEnd && seenMessageEnd)
                {
                    throw new NdefException(NdefExceptionMessages.ExMessageEndLate);
                }
                else if (messageEnd && !seenMessageEnd)
                {
                    seenMessageEnd = true;
                }

                if (cf && (typeNameFormat != NdefRecord.TypeNameFormatType.Unchanged) && partialChunk.Length > 0)
                {
                    throw new NdefException(NdefExceptionMessages.ExMessagePartialChunk);
                }

                // Header length
                int headerLength = 1;
                headerLength += (sr) ? 1 : 4;
                headerLength += (il) ? 1 : 0;

                if (i + headerLength >= message.Length)
                {
                    throw new NdefException(NdefExceptionMessages.ExMessageUnexpectedEnd);
                }

                // Type length
                byte typeLength = message[++i];

                if ((typeNameFormat == NdefRecord.TypeNameFormatType.Unchanged) && (typeLength != 0))
                {
                    throw new NdefException(NdefExceptionMessages.ExMessageInvalidChunkedType);
                }

                // Payload length (short record?)
                uint payloadLength;
                if (sr)
                {
                    // Short record - payload length is a single octet
                    payloadLength = message[++i];
                }
                else
                {
                    // No short record - payload length is four octets representing a 32 bit unsigned integer (MSB-first)
                    payloadLength = (uint)((message[++i]) << 24);
                    payloadLength |= (uint)((message[++i]) << 16);
                    payloadLength |= (uint)((message[++i]) << 8);
                    payloadLength |= (uint)((message[++i]) << 0);
                }

                // ID length
                byte idLength;
                idLength = (byte)(il ? message[++i] : 0);

                // Total length of content (= type + payload + ID)
                uint contentLength = typeLength + payloadLength + idLength;
                if (i + contentLength >= message.Length)
                {
                    throw new NdefException(NdefExceptionMessages.ExMessageUnexpectedEnd);
                }


                if ((typeNameFormat == NdefRecord.TypeNameFormatType.Unchanged) && (idLength != 0))
                {
                    throw new NdefException(NdefExceptionMessages.ExMessageInvalidChunkedId);
                }

                if (typeNameFormat != NdefRecord.TypeNameFormatType.Unchanged)
                {
                    record.TypeNameFormat = typeNameFormat;
                }

                // Read type
                if (typeLength > 0)
                {
                    record.Type = new byte[typeLength];
                    Array.Copy(message, (int)(++i), record.Type, 0, typeLength);
                    i += (uint)typeLength - 1;
                }

                // Read ID
                if (idLength > 0)
                {
                    record.Id = new byte[idLength];
                    Array.Copy(message, (int)(++i), record.Id, 0, idLength);
                    i += (uint)idLength - 1;
                }

                // Read payload
                if (payloadLength > 0)
                {
                    var payload = new byte[payloadLength];
                    Array.Copy(message, (int)(++i), payload, 0, (int)payloadLength);

                    if (cf)
                    {
                        // chunked payload, except last
                        partialChunk.Write(payload, 0, payload.Length);
                    }
                    else if (typeNameFormat == NdefRecord.TypeNameFormatType.Unchanged)
                    {
                        // last chunk of chunked payload
                        partialChunk.Write(payload, 0, payload.Length);
                        record.Payload = partialChunk.ToArray();
                    }
                    else
                    {
                        // non-chunked payload
                        record.Payload = payload;
                    }

                    i += payloadLength - 1;
                }

                if (!cf)
                {
                    // Add record to the message and create a new record for the next loop iteration
                    result.Add(record);
                    record = new NdefRecord();
                }

                if (!cf && seenMessageEnd)
                    break;

                // move to start of next record
                ++i;
            }


            if (!seenMessageBegin && !seenMessageEnd)
            {
                throw new NdefException(NdefExceptionMessages.ExMessageNoBeginOrEnd);
            }

            return result;
        }

        /// <summary>
        /// Convert all the NDEF records currently stored in the NDEF message to a byte
        /// array suitable for writing to a tag or sending to another device.
        /// </summary>
        /// <returns>The NDEF record(s) converted to an NDEF message.</returns>
        public byte[] ToByteArray()
        {
            // Empty message: single empty record
            if (Count == 0)
            {
                var msg = new NdefMessage { new NdefRecord() };
                return msg.ToByteArray();
            }

            var m = new MemoryStream();
            for (int i = 0; i < Count; i++)
            {
                var record = this[i];

                var flags = (byte)record.TypeNameFormat;

                // Message begin / end flags. If there is only one record in the message,
                // both flags are set.
                if (i == 0)
                    flags |= 0x80;      // MB (message begin = first record in the message)
                if (i == Count - 1)
                    flags |= 0x40;      // ME (message end = last record in the message)

                // cf (chunked records) not supported yet

                // SR (Short Record)?
                if (record.Payload == null || record.Payload.Length < 255)
                    flags |= 0x10;

                // ID present?
                if (record.Id != null && record.Id.Length > 0)
                    flags |= 0x08;

                m.WriteByte(flags);

                // Type length
                if (record.Type != null) m.WriteByte((byte)record.Type.Length); else m.WriteByte(0);

                // Payload length 1 byte (SR) or 4 bytes
                if (record.Payload == null)
                    m.WriteByte(0);
                else
                {
                    if ((flags & 0x10) != 0)
                    {
                        // SR
                        m.WriteByte((byte)record.Payload.Length);
                    }
                    else
                    {
                        // No SR (Short Record)
                        var payloadLength = (uint)record.Payload.Length;
                        m.WriteByte((byte)(payloadLength >> 24));
                        m.WriteByte((byte)(payloadLength >> 16));
                        m.WriteByte((byte)(payloadLength >> 8));
                        m.WriteByte((byte)(payloadLength & 0x000000ff));
                    }
                }

                // ID length
                if (record.Id != null && (flags & 0x08) != 0)
                    m.WriteByte((byte)record.Id.Length);

                // Type length
                if (record.Type != null && record.Type.Length > 0)
                    m.Write(record.Type, 0, record.Type.Length);

                // ID data
                if (record.Id != null && record.Id.Length > 0)
                    m.Write(record.Id, 0, record.Id.Length);

                // Payload data
                if (record.Payload != null && record.Payload.Length > 0)
                    m.Write(record.Payload, 0, record.Payload.Length);

            }

            return m.ToArray();
        }
    }
}
