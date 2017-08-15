/****************************************************************************
**
** Copyright (C) 2012-2016 Andreas Jakl - http://www.nfcinteractor.com/
** Original version copyright (C) 2012 Nokia Corporation and/or its subsidiary(-ies).
** All rights reserved.
**
** This file is based on the respective class of the Connectivity module
** of Qt Mobility (http://qt.gitorious.org/qt-mobility).
**
** Ported to C# by Andreas Jakl (2012)
** More information: http://andijakl.github.io/ndef-nfc/
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

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// An NDEF record contains a payload described by a type, a length, and an optional identifier.
    /// </summary>
    /// <remarks>
    /// This class is generic and can hold the data of any kind of record.
    /// It follows the specification from the NFC forum for the data format.
    /// 
    /// Ndef Records should usually be placed within an Ndef Message, which will
    /// make sure that for example the message begin / end flags are set correctly.
    /// 
    /// While the NdefRecord class only offers access to the payload as a byte array,
    /// you should rather use specialized sub classes, which offer convenient ways
    /// to handle data stored in the payload through easy access methods.
    /// Such classes are provided for several standardized types.
    /// </remarks>
    public class NdefRecord
    {

        /// <summary>
        /// Standardized type name formats, as defined by the NDEF record
        /// specification from the Nfc Forum.
        /// </summary>
        public enum TypeNameFormatType
        {
            /// <summary>
            /// Empty indicates that no type or payload is associated with this record.
            /// </summary>
            Empty = 0x00,
            /// <summary>
            /// The NFC Forum well-known type follows the RTD type name format defined in the NFC Forum RTD specification.
            /// </summary>
            NfcRtd = 0x01,
            /// <summary>
            /// The media type indicates that the TYPE field contains a value that follows the media-type BNF construct defined by RFC 2046.
            /// </summary>
            Mime = 0x02,
            /// <summary>
            /// Absolute-URI indicates that the TYPE field contains a value that follows the absolute-URI BNF construct defined by RFC 3986.
            /// </summary>
            Uri = 0x03,
            /// <summary>
            /// NFC Forum external type indicates that the TYPE field contains a value that follows the type name format defined in [NFC RTD] for external type names.
            /// </summary>
            ExternalRtd = 0x04,
            /// <summary>
            /// Unknown SHOULD be used to indicate that the type of the payload is unknown. This is similar to the "application/octet-stream" media type defined by MIME.
            /// When used, the TYPE_LENGTH field MUST be zero and thus the TYPE field is omitted from the NDEF record.
            /// </summary>
            Unknown = 0x05,
            /// <summary>
            /// Unchanged MUST be used in all middle record chunks and the terminating record chunk used in chunked payloads.
            /// </summary>
            Unchanged = 0x06,
            /// <summary>
            /// Any other type name format; should be treated as unknown. 
            /// </summary>
            Reserved = 0x07
        };

        /// <summary>
        /// Indicates the structure of the value of the TYPE field.
        /// </summary>
        public TypeNameFormatType TypeNameFormat { get; set; }

        /// <summary>
        /// Byte array storing raw contents of the type.
        /// Use Type property to access it whenever possible.
        /// </summary>
        /// <remarks>Direct byte array access provided as virtual
        /// property can't be accessed from constructor.</remarks>
        protected byte[] _type;
        /// <summary>
        /// An identifier describing the type of the payload.
        /// </summary>
        public virtual byte[] Type
        {
            get { return _type; }
            set
            {
                if (value == null)
                {
                    _type = null;
                    return;
                }
                _type = new byte[value.Length];
                Array.Copy(value, _type, value.Length);
            }
        }

        private byte[] _id;
        /// <summary>
        /// An identifier in the form of a URI reference.
        /// </summary>
        public byte[] Id
        {
            get { return _id; }
            set
            {
                if (value == null)
                {
                    _id = null;
                    return;
                }
                _id = new byte[value.Length];
                Array.Copy(value, _id, value.Length);
            }
        }

        /// <summary>
        /// Byte array storing raw contents of the payload.
        /// Use Payload property to access it whenever possible.
        /// </summary>
        /// <remarks>Direct byte array access provided as virtual
        /// property can't be accessed from constructor.</remarks>
        protected byte[] _payload;
        /// <summary>
        /// The application data carried within an NDEF record.
        /// </summary>
        public virtual byte[] Payload
        {
            get { return _payload; }
            set
            {
                if (value == null)
                {
                    _payload = null;
                    return;
                }
                _payload = new byte[value.Length];
                Array.Copy(value, _payload, value.Length);
            }
        }

        /// <summary>
        /// Create an empty record, not setting any information.
        /// </summary>
        public NdefRecord()
        {
            TypeNameFormat = TypeNameFormatType.Empty;
        }

        /// <summary>
        /// Create a new record with the specified type name format and type.
        /// Doesn't set the payload and ID.
        /// </summary>
        /// <param name="tnf">Type name format to use, based on the tnf's standardized
        /// by the Nfc Forum.</param>
        /// <param name="type">Type string.</param>
        public NdefRecord(TypeNameFormatType tnf, byte[] type)
        {
            TypeNameFormat = tnf;
            if (type != null)
            {
                // Can't call Type property set method from constructor, as it's virtual
                _type = new byte[type.Length];
                Array.Copy(type, _type, type.Length);
            }
        }

        /// <summary>
        /// Create a new record, copying the information of the record sent through the parameter.
        /// </summary>
        /// <param name="other">Record to copy.</param>
        public NdefRecord(NdefRecord other)
        {
            TypeNameFormat = other.TypeNameFormat;
            if (other.Type != null)
            {
                // Can't call Type property set method from constructor, as it's virtual
                _type = new byte[other.Type.Length];
                Array.Copy(other.Type, _type, other.Type.Length);
            }
            if (other.Id != null) Id = other.Id;
            if (other.Payload != null)
            {
                // Can't call Type property set method from constructor, as it's virtual
                _payload = new byte[other.Payload.Length];
                Array.Copy(other.Payload, _payload, other.Payload.Length);
            }
        }

        /// <summary>
        /// Checks the type name format and type of this record and returns
        /// the appropriate specialized class, if one is available and known
        /// for this record type.
        /// </summary>
        /// <param name="checkForSubtypes">If set to true, also checks for
        /// subtypes of the URL / SmartPoster record where the library offers
        /// a convenient handling class - e.g. for SMS or Mailto records,
        /// which are actually URL schemes.</param>
        /// <returns>Type name of the specialized class that can understand
        /// and manipulate the payload through convenience methods.</returns>
        public Type CheckSpecializedType(bool checkForSubtypes)
        {
            // Note: can't check for specialized types like the geo record
            // or the SMS record yet, as these are just convenience classes
            // for creating URI / Smart Poster records.
            if (checkForSubtypes)
            {
                // Need to check specialized URI / Sp records before checking for base types.
                if (NdefSmsRecord.IsRecordType(this))
                    return typeof(NdefSmsRecord);
                if (NdefMailtoRecord.IsRecordType(this))
                    return typeof(NdefMailtoRecord);
                if (NdefTelRecord.IsRecordType(this))
                    return typeof(NdefTelRecord);
                if (NdefWindowsSettingsRecord.IsRecordType(this))
                    return typeof(NdefWindowsSettingsRecord);
            }
            // Unique / base record types
            if (NdefUriRecord.IsRecordType(this))
                return typeof(NdefUriRecord);
            if (NdefSpRecord.IsRecordType(this))
                return typeof(NdefSpRecord);
            if (NdefTextRecord.IsRecordType(this))
                return typeof(NdefTextRecord);
            if (NdefSpActRecord.IsRecordType(this))
                return typeof(NdefSpActRecord);
            if (NdefSpSizeRecord.IsRecordType(this))
                return typeof(NdefSpSizeRecord);
            if (NdefSpMimeTypeRecord.IsRecordType(this))
                return typeof(NdefSpMimeTypeRecord);
            if (NdefLaunchAppRecord.IsRecordType(this))
                return typeof(NdefLaunchAppRecord);
            if (NdefAndroidAppRecord.IsRecordType(this))
                return typeof(NdefAndroidAppRecord);
            if (NdefVcardRecordBase.IsRecordType(this))
                return typeof(NdefVcardRecordBase);
            if (NdefIcalendarRecordBase.IsRecordType(this))
                return typeof(NdefIcalendarRecordBase);
            if (NdefBtSecureSimplePairingRecord.IsRecordType(this))
                return typeof(NdefBtSecureSimplePairingRecord);
            if (NdefHandoverSelectRecord.IsRecordType(this))
                return typeof(NdefHandoverSelectRecord);
            if (NdefHandoverErrorRecord.IsRecordType(this))
                return typeof(NdefHandoverErrorRecord);
            if (NdefHandoverAlternativeCarrierRecord.IsRecordType(this))
                return typeof(NdefHandoverAlternativeCarrierRecord);
            if (NdefMimeImageRecordBase.IsRecordType(this))
                return typeof(NdefMimeImageRecordBase);
            return typeof(NdefRecord);
        }

        /// <summary>
        /// Checks if the contents of the record are valid; throws an exception if
        /// a problem is found, containing a textual description of the issue.
        /// </summary>
        /// <exception cref="NdefException">Thrown if no valid NDEF record can be
        /// created based on the record's current contents. The exception message 
        /// contains further details about the issue.</exception>
        /// <returns>True if the record contents are valid, or throws an exception
        /// if an issue is found.</returns>
        public virtual bool CheckIfValid()
        {
            // Check Type (according to TNF)
            if (TypeNameFormat == TypeNameFormatType.Unchanged ||
                TypeNameFormat == TypeNameFormatType.Unknown)
            {
                // Unknown and unchanged TNF must have a type length of 0
                if (!(Type == null || Type.Length == 0))
                {
                    throw new NdefException(NdefExceptionMessages.ExRecordUnchangedTypeName);
                }
            }
            else if (TypeNameFormat == TypeNameFormatType.Empty)
            {
                // The value 0x00 (Empty) indicates that there is no type or payload associated 
                // with this record. When used, the TYPE_LENGTH, ID_LENGTH, and PAYLOAD_LENGTH 
                // fields MUST be zero and the TYPE, ID, and PAYLOAD fields are thus omitted from the record.
                if (Type != null || Payload != null)
                {
                    throw new NdefException(NdefExceptionMessages.ExRecordEmptyWithTypeOrPayload);
                }
            }
            else
            {
                // All other TNF should have a type set
                // Proximity APIs unstable in some cases when no Type name is set
                if (Type == null || Type.Length == 0)
                {
                    throw new NdefException(NdefExceptionMessages.ExRecordNoType);
                }
            }
            // Check ID
            // Middle and terminating record chunks MUST not have an ID field
            if (TypeNameFormat == TypeNameFormatType.Unchanged && !(Id == null || Id.Length == 0))
                throw new NdefException(NdefExceptionMessages.ExRecordUnchangedId);
            return true;
        }
    }
}
