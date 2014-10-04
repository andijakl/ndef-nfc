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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// The URI record as specified by the NFC Forum URI record type definition.
    /// </summary>
    /// <remarks>
    /// The record stores a URI and can be stored on a tag or sent to another device.
    /// Several of the most common URI headers are automatically abbreviated in order
    /// to keep the record as small as possible. URIs will be encoded using UTF-8.
    /// 
    /// This record can either be used stand alone, or as part of another record
    /// like the Smart Poster (<see cref="NdefSpRecord"/>).
    /// </remarks>
    public class NdefUriRecord : NdefRecord
    {
        /// <summary>
        /// Type of the NDEF Text record (well-known, type 'U').
        /// </summary>
        public static readonly byte[] UriType = { (byte)'U' };

        /// <summary>
        /// URI abbreviations, as defined in NDEF URI record specifications.
        /// </summary>
        private static readonly string[] Abbreviations =
        {
            String.Empty,
            "http://www.",
            "https://www.",
            "http://",
            "https://",
            "tel:",
            "mailto:",
            "ftp://anonymous:anonymous@",
            "ftp://ftp.",
            "ftps://",
            "sftp://",
            "smb://",
            "nfs://",
            "ftp://",
            "dav://",
            "news:",
            "telnet://",
            "imap:",
            "rtsp://",
            "urn:",
            "pop:",
            "sip:",
            "sips:",
            "tftp:",
            "btspp://",
            "btl2cap://",
            "btgoep://",
            "tcpobex://",
            "irdaobex://",
            "file://",
            "urn:epc:id:",
            "urn:epc:tag:",
            "urn:epc:pat:",
            "urn:epc:raw:",
            "urn:epc:",
            "urn:nfc:"
        };

        /// <summary>
        /// Get the raw URI as stored in this record, excluding any abbreviations.
        /// </summary>
        /// <remarks>Gets the raw contents of the URI, exclusive the first byte of the
        /// record's payload that would contain the abbreviation code.
        /// If the URI has been abbreviated, this method returns retuns the actual URI
        /// text as stored on the tag. To get the full URI that has been expanded with
        /// the abbreviated URI scheme, use the normal Uri accessor.</remarks>
        public byte[] RawUri
        {
            get
            {
                if (Payload == null || Payload.Length == 0)
                    return null;

                var code = Payload.ElementAt(0);
                if (code != 0)
                {
                    Debug.WriteLine(NdefExceptionMessages.ExRawUriNoAbbreviation);
                }

                var rawUri = new byte[Payload.Length - 1];
                Array.Copy(Payload, 1, rawUri, 0, Payload.Length - 1);
                return rawUri;
            }
            set
            {
                Payload = new byte[value.Length + 1];
                Payload[0] = 0;
                Array.Copy(value, 0, Payload, 1, value.Length);
            }
        }

        /// <summary>
        /// URI stored in this record.
        /// The abbreviation will be handled behind the scenes - getting and 
        /// setting this property will always work on the full URI.
        /// </summary>
        /// <remarks>
        /// Note that this property / class does not escape the URI data
        /// string to avoid double-escaping and to allow storing unescaped
        /// URIs if allowed by the protocol.
        /// For generic URLs, it's recommended to escape the URL string when
        /// sending it to this class, e.g., with System.Uri.EscapeUriString().
        /// </remarks>
        public string Uri
        {
            get 
            {
                if (Payload == null || Payload.Length == 0)
                {
                    return String.Empty;
                }
                var encoding = Encoding.UTF8;
                byte code = Payload.ElementAt(0);
                if (code >= Abbreviations.Length)
                {
                    code = 0;
                }
                var uri = new byte[Payload.Length - 1];
                Array.Copy(Payload, 1, uri, 0, Payload.Length - 1);
                return (Abbreviations[code] + encoding.GetString(uri, 0, uri.Length));
            }
            set
            {
                var uriString = value;
                var encoding = Encoding.UTF8;
                var useAbbreviation = 0;
                for (var i = 1; i < Abbreviations.Length; i++)
                {
                    if (uriString.StartsWith(Abbreviations[i]))
                    {
                        useAbbreviation = i;
                        break;
                    }
                }

                // Can abbreviate the URI
                var abbrevLength = Abbreviations[useAbbreviation].Length;
                var encodedLength = encoding.GetByteCount(uriString.ToCharArray(), abbrevLength,
                                      uriString.Length - abbrevLength);
                Payload = new byte[encodedLength + 1];
                Payload[0] = (byte) useAbbreviation;

                encoding.GetBytes(uriString, abbrevLength, uriString.Length - abbrevLength, Payload, 1);
            }
        }

        /// <summary>
        /// Create an empty URI record.
        /// </summary>
        public NdefUriRecord()
            : base(TypeNameFormatType.NfcRtd, UriType)
        {
        }

        /// <summary>
        /// Create a URI record based on the record passed through the argument.
        /// </summary>
        /// <remarks> 
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a URI record as well.
        /// </remarks>
        /// <param name="other">Uri record to copy into this record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a Uri record
        /// based on an incompatible record type.</exception>
        public NdefUriRecord(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
        }


        /// <summary>
        /// Checks if the record sent via the parameter is indeed a URI record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a URI record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Type.SequenceEqual(UriType));
        }
    }
}
