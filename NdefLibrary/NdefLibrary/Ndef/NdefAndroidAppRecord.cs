/****************************************************************************
**
** Copyright (C) 2012-2018 Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
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
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Creates the Android-specific Android Application Record.
    /// </summary>
    /// <remarks>
    /// Through specifying the package name, this record directly launches an
    /// app on an Android phone (4.0+). If the app isn't installed on the phone,
    /// it will open the store and search for the app.
    /// 
    /// To pass custom data to the app, you would typically add other records
    /// to the NDEF message.
    /// 
    /// If creating a multi-record NDEF message, it's recommended to put this
    /// record to the end of the message.
    /// </remarks>
    /// <seealso cref="http://developer.android.com/guide/topics/connectivity/nfc/nfc.html#aar"/>
    public class NdefAndroidAppRecord : NdefRecord
    {
        /// <summary>
        /// Type name for the Android Application Record (TNF = External RTD)
        /// </summary>
        private static readonly byte[] AndroidAppRecordType = { (byte)'a', (byte)'n', (byte)'d', (byte)'r', (byte)'o', (byte)'i', (byte)'d', (byte)'.', (byte)'c', (byte)'o', (byte)'m', (byte)':', (byte)'p', (byte)'k', (byte)'g' };

        /// <summary>
        /// Name of the android package.
        /// </summary>
        public string PackageName
        {
            get
            {
                if (Payload == null || Payload.Length == 0)
                {
                    return string.Empty;
                }
                return Encoding.UTF8.GetString(Payload, 0, Payload.Length);
            }
            set
            {
                if (value == null)
                {
                    Payload = null;
                    return;
                }
                var encoding = Encoding.UTF8;
                var encodedLength = encoding.GetByteCount(value);
                Payload = new byte[encodedLength];
                encoding.GetBytes(value, 0, value.Length, Payload, 0);
            }
        }

        /// <summary>
        /// Create an empty Android Application Record.
        /// </summary>
        public NdefAndroidAppRecord()
            : base(TypeNameFormatType.ExternalRtd, AndroidAppRecordType)
        {
        }

        /// <summary>
        /// Create an Android Application Record record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be an Android Application Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this AAR record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create an Android
        /// Application Record based on an incompatible record type.</exception>
        public NdefAndroidAppRecord(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
        }
        
        /// <summary>
        /// Checks if the record sent via the parameter is indeed an Android
        /// Application Record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be an Android Application Record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record == null || record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.ExternalRtd && record.Type.SequenceEqual(AndroidAppRecordType));
        }
    }
}
