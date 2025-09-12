/****************************************************************************
**
** Copyright (C) 2012-2018 Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
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
using System.IO;
using System.Linq;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// The Error Record is used in the Handover Select Record to indicate that the Handover Selector failed
    /// to successfully process the most recently received Handover Request Message. It SHALL NOT be used elsewhere.
    /// </summary>
    public class NdefHandoverErrorRecord : NdefRecord
    {
        /// <summary>
        /// Type of the Handover Select / Error Record.
        /// </summary>
        public static readonly byte[] BtHandoverErrorType = { 0x65, 0x72, 0x72 };    // "err"

        private byte _errorReason;
        private byte[] _errorData;

        /// <summary>
        /// According to the current specification, four different error reasons are defined.
        /// Depending on the value set for the error reason, the error data has a different meaning.
        /// </summary>
        public enum ErrorReasonTypes : byte
        {
            Reserved = 0x00,
            /// <summary>
            /// The Handover Request Message could not be processed due to temporary memory constraints.
            /// Resending the unmodified Handover Request Message might be successful after a time
            /// interval of at least the number of milliseconds expressed in the error data field.
            /// </summary>
            TemporaryMemoryConstraints = 0x01,
            /// <summary>
            /// The Handover Request Message could not be processed due to permanent memory constraints.
            /// Resending the unmodified Handover Request Message will always yield the same error condition.
            /// </summary>
            PermanentMemoryConstraints = 0x02,
            /// <summary>
            /// The Handover Request Message could not be processed due to carrier-specific constraints.
            /// Resending the Handover Request Message might not be successful until after a time interval
            /// of at least the number of milliseconds expressed in the error data field.
            /// </summary>
            CarrierSpecificConstraints = 0x03
        }

        /// <summary>
        /// The actual byte used to store the error reason in the Handover Error record.
        /// </summary>
        public byte ErrorReason
        {
            get { return _errorReason; }
            set
            {
                _errorReason = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// Access the byte-based ErrorReason property through the enum variable for easier use within apps.
        /// </summary>
        public ErrorReasonTypes ErrorReasonAsType
        {
            get {
                return Enum.IsDefined(typeof (ErrorReasonTypes), ErrorReason)
                    ? (ErrorReasonTypes) ErrorReason
                    : ErrorReasonTypes.Reserved;
            }
            set { ErrorReason = (byte)value; }
        }

        /// <summary>
        /// Byte array that contains the error data - additional information about the error.
        /// Its contents depend on the error reason and are described in the error reason enum documentation.
        /// </summary>
        public byte[] ErrorData
        {
            get { return _errorData; }
            set
            {
                _errorData = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// Access the ErrorData byte array as an Integer.
        /// </summary>
        /// <remarks>
        /// Based on the current Connection Handover 1.3 specification, all possible error data values
        /// are time lengths with a different byte number / maximum duration. Therefore, this class
        /// provides this convenience accessor to the ErrorData property, which correctly converts
        /// a number stored in the ErrorData array based on the number of bytes as defined by the
        /// error reason.
        /// </remarks>
        public uint ErrorDataAsNumber {
            get
            {
                if (ErrorData == null || ErrorData.Length == 0)
                {
                    return 0;
                    //throw new NdefException(NdefExceptionMessages.ExHandoverErrorNoData);
                }
                switch (ErrorReasonAsType)
                {
                    case ErrorReasonTypes.TemporaryMemoryConstraints:
                    case ErrorReasonTypes.CarrierSpecificConstraints:
                        // 1 byte
                        return ErrorData.Length != 1 ? (uint)0 : ErrorData[0];
                        //throw new NdefException(NdefExceptionMessages.ExHandoverErrorInvalidData);
                    case ErrorReasonTypes.PermanentMemoryConstraints:
                        // 4 bytes
                        if (ErrorData.Length != 4)
                            return 0;
                            //throw new NdefException(NdefExceptionMessages.ExHandoverErrorInvalidData);
                        // Make sure we're using big endian
                        return (uint)((Payload[0]) << 24) | (uint)((Payload[1]) << 16) | (uint)((Payload[2]) << 8) | (uint)((Payload[3]) << 0);
                    default:
                        return 0;
                        //throw new NdefException(NdefExceptionMessages.ExHandoverErrorUnknownReason);
                }
            }
            set
            {
                switch (ErrorReasonAsType)
                {
                    case ErrorReasonTypes.TemporaryMemoryConstraints:
                    case ErrorReasonTypes.CarrierSpecificConstraints:
                        // Create a byte array with 1 byte
                        ErrorData = new[] {(byte)(value & 0xFF)};
                        break;
                    case ErrorReasonTypes.PermanentMemoryConstraints:
                        // Create a byte array with 4 bytes
                        ErrorData = new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0x000000ff) };
                        break;
                    //default:
                        //throw new NdefException(NdefExceptionMessages.ExHandoverErrorUnknownReason);
                }
            }
        }


        #region Constructors
        /// <summary>
        /// Create an empty Handover Error Record.
        /// </summary>
        /// <remarks>This record is only valid within the Handover Select record.</remarks>
        public NdefHandoverErrorRecord()
            :base(TypeNameFormatType.NfcRtd, BtHandoverErrorType)
        {
        }


        /// <summary>
        /// Create a Handover Error record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a Handover Error Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this Handover Error record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a Handover Error record
        /// based on an incompatible record type.</exception>
        public NdefHandoverErrorRecord(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);

            ParsePayloadToData(_payload);
        }

        /// <summary>
        /// Create a Handover Error record based on the supplied error reason
        /// and error data, coming from direct byte arrays.
        /// </summary>
        /// <remarks>This record is only valid within the Handover Select record.</remarks>
        /// <param name="errorReason">The reason why the handover request could not be
        /// processed by the target device. The value of this defines the possible
        /// contents and content length of the second (errorData) parameter.</param>
        /// <param name="errorData">Additional data for the error reason. The contents
        /// depend on the error reason, and is usually a time value.</param>
        public NdefHandoverErrorRecord(byte errorReason, byte[] errorData)
            : base(TypeNameFormatType.NfcRtd, BtHandoverErrorType)
        {
            _errorReason = errorReason;
            if (errorData != null)
            {
                _errorData = new byte[errorData.Length];
                Array.Copy(errorData, _errorData, errorData.Length);
            }
            AssemblePayload();
        }

        /// <summary>
        /// Create a Handover Error record based on the supplied error reason
        /// and error data, coming from higher level convenience types that this
        /// class will convert to the appropriate byte level representation.
        /// </summary>
        /// <remarks>This record is only valid within the Handover Select record.</remarks>
        /// <param name="errorReason">The reason why the handover request could not be
        /// processed by the target device. The value of this defines the possible
        /// contents and content length of the second (errorData) parameter.</param>
        /// <param name="errorData">Additional data for the error reason. The contents
        /// depend on the error reason, and is usually a time value.</param>
        public NdefHandoverErrorRecord(ErrorReasonTypes errorReason, uint errorData)
            : base(TypeNameFormatType.NfcRtd, BtHandoverErrorType)
        {
            ErrorReasonAsType = errorReason;
            ErrorDataAsNumber = errorData;
        }
#endregion


        /// <summary>
        /// Create the payload based on the data stored in the properties. Usually called
        /// automatically when changing properties.
        /// </summary>
        private void AssemblePayload()
        {
            var m = new MemoryStream();
            m.WriteByte(ErrorReason);
            if (ErrorData != null)
            {
                m.Write(ErrorData, 0, ErrorData.Length);
            }
            _payload = m.ToArray();
        }


        /// <summary>
        /// Deletes any properties currently stored in the record
        /// and re-initializes them by parsing the contents of the payload.
        /// </summary>
        private void ParsePayloadToData(byte[] payload)
        {
            if (payload == null || payload.Length < 1)
            {
                throw new NdefException(NdefExceptionMessages.ExHandoverErrorInvalidSourceData);
            }
            _errorReason = payload[0];
            _errorData = new byte[payload.Length - 1];
            Array.Copy(payload, 1, _errorData, 0, payload.Length - 1);
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Handover Error record.
        /// Checks the type and type name format.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type, type name format and payload
        /// to be a Handover Error record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            return record.TypeNameFormat == TypeNameFormatType.NfcRtd &&
                   record.Type != null &&
                   record.Type.SequenceEqual(BtHandoverErrorType) &&
                   record.Payload != null;
        }
    }
}
