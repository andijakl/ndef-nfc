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

function initLibraryNdefRecord(context) {
    'use strict';

    var NdefLibrary = context.NdefLibrary;


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
    var ndefRecord = NdefLibrary.NdefRecord = function (opt_config) {

        /// <summary>
        /// Indicates the structure of the value of the TYPE field.
        /// </summary>
        this._typeNameFormat = NdefLibrary.NdefRecord.TypeNameFormatType.Empty;

        /// <summary>
        /// Byte array storing raw contents of the type.
        /// Use Type property to access it whenever possible.
        /// </summary>
        /// <remarks>Direct byte array access provided as virtual
        /// property can't be accessed from constructor.</remarks>
        this._type = [];

        /// <summary>
        /// An identifier in the form of a URI reference.
        /// </summary>
        this._id = [];

        /// <summary>
        /// Byte array storing raw contents of the payload.
        /// Use Payload property to access it whenever possible.
        /// </summary>
        /// <remarks>Direct byte array access provided as virtual
        /// property can't be accessed from constructor.</remarks>
        this._payload = [];


        ///Constructors
        if (arguments.length == 2) {
            /// <summary>
            /// Create a new record with the specified type name format and type.
            /// Doesn't set the payload and ID.
            /// </summary>
            /// <param name="tnf">Type name format to use, based on the tnf's standardized
            /// by the Nfc Forum.</param>
            /// <param name="type">Type string.</param>

            this._typeNameFormat = arguments[0];
            if (arguments[1] != null) {
                this.setType(arguments[1]);
            }
        } else if (arguments.length == 1) {
            /// <summary>
            /// Create a new record, copying the information of the record sent through the parameter.
            /// </summary>
            /// <param name="other">Record to copy.</param>
            var other = arguments[0];

            if (other.getType() != null) {
                this.setType(other.getType());
            }

            if (other.getId() != null) {
                this.setId(other.getId());
            }

            if (other.getPayload() != null) {
                this.setPayload(other.getPayload());
            }

            this._typeNameFormat = other.getTypeNameFormat();
        }
        else {
            /// <summary>
            /// Create an empty record, not setting any information.
            /// </summary>
            this._typeNameFormat = NdefLibrary.NdefRecord.TypeNameFormatType.Empty;
        }
    };


    /// <summary>
    /// Standardized type name formats, as defined by the NDEF record
    /// specification from the Nfc Forum.
    /// </summary>
    ndefRecord.TypeNameFormatType = {
        /// Empty indicates that no type or payload is associated with this record.
        "Empty": 0x00,
        /// The NFC Forum well-known type follows the RTD type name format defined in the NFC Forum RTD specification.
        "NfcRtd": 0x01,
        /// The media type indicates that the TYPE field contains a value that follows the media-type BNF construct defined by RFC 2046.
        "Mime": 0x02,
        /// Absolute-URI indicates that the TYPE field contains a value that follows the absolute-URI BNF construct defined by RFC 3986.
        "Uri": 0x03,
        /// NFC Forum external type indicates that the TYPE field contains a value that follows the type name format defined in [NFC RTD] for external type names.
        "ExternalRtd": 0x04,
        /// Unknown SHOULD be used to indicate that the type of the payload is unknown. This is similar to the "application/octet-stream" media type defined by MIME.
        /// When used, the TYPE_LENGTH field MUST be zero and thus the TYPE field is omitted from the NDEF record.
        "Unknown": 0x05,
        /// Unchanged MUST be used in all middle record chunks and the terminating record chunk used in chunked payloads.
        "Unchanged": 0x06,
        /// Any other type name format; should be treated as unknown. 
        "Reserved": 0x07
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.getId = function () {
        return this._id;
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.setId = function (value) {
        if (value == null) {
            this._id = null;
            return;
        }
        this._id = [];
        arrayCopy(value, this._id, value.length);
    };

    /// <summary>
    /// An identifier describing the type of the payload.
    /// </summary>
    ndefRecord.prototype.getType = function () {
        return this._type;
    };

    /// <summary>
    /// An identifier describing the type of the payload.
    /// </summary>
    ndefRecord.prototype.setType = function (value) {
        if (value == null) {
            this._type = null;
            return;
        }
        this._type = [];
        arrayCopy(value, this._type, value.length);
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.getPayload = function () {
        return this._payload;
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.setPayload = function (value) {
        if (value == null) {
            this._payload = null;
            return;
        }
        this._payload = [];
        arrayCopy(value, this._payload, value.length);
    };


    /// <summary>
    /// Indicates the structure of the value of the TYPE field.
    /// </summary>
    ndefRecord.prototype.getTypeNameFormat = function () {
        return this._typeNameFormat;
    };

    /// <summary>
    /// Indicates the structure of the value of the TYPE field.
    /// </summary>
    ndefRecord.prototype.setTypeNameFormat = function (value) {
        this._typeNameFormat = value;
    };

    /// Checks the type name format and type of this record and returns
    /// the appropriate specialized class, if one is available and known
    /// for this record type.
    /// </summary>
    /// <returns>Type name of the specialized class that can understand
    /// and manipulate the payload through convenience methods.</returns>
    ndefRecord.prototype.checkSpecializedType = function (checkForSubtypes) {
        //TODO
    };


    /// <summary>
    /// Checks if the contents of the record are valid; throws an exception if
    /// a problem is found, containing a textual description of the issue.
    /// </summary>
    /// <exception cref="NdefException">Thrown if no valid NDEF record can be
    /// created based on the record's current contents. The exception message 
    /// contains further details about the issue.</exception>
    /// <returns>True if the record contents are valid, or throws an exception
    /// if an issue is found.</returns>
    ndefRecord.prototype.checkIfValid = function () {
        // Check Type (according to TNF)
        if (this._typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged ||
            this._typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unknown) {
            // Unknown and unchanged TNF must have a type length of 0
            if (!(this._type == null || this._type.length == 0)) {
                throw "NdefExceptionMessages.ExRecordUnchangedTypeName";
            }
        }
        else {
            // All other TNF (except Empty) should have a type set
            // Proximity APIs unstable in some cases when no Type name is set
            if (this._typeNameFormat != NdefLibrary.NdefRecord.TypeNameFormatType.Empty && (this._type == null || this._type.length == 0)) {
                throw "NdefExceptionMessages.ExRecordNoType";
            }
        }
        // Check ID
        // Middle and terminating record chunks MUST not have an ID field
        if (this._typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged && !(this._id == null || this._id.length == 0))
            throw "NdefExceptionMessages.ExRecordUnchangedId";
        return true;
    };

}