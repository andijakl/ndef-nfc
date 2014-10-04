/****************************************************************************
**
** Copyright (C) 2012-2014 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Handover Select Record for the Connection Handover Standard. Its payload contains
    /// at least one reference to an Alternative Carrier record that defines a possible
    /// carrier for a connection handover.
    /// </summary>
    /// <remarks>
    /// This record is sent either in a connection handover process to select alternative carriers,
    /// or it can also be stored on an NFC tag to provide information about a connection handover
    /// to for example a Bluetooth carrier.
    /// 
    /// After the version information (this record is implemented according to version 1.3 of
    /// the NFC Forum Connection Handover specification from 2014-01-16), the payload of this record
    /// essentially contains a complete NDEF message (within its payload!) that identifies alternative
    /// carriers. These are then subrecords within this record.
    /// 
    /// At the end, one Handover error record may follow.
    /// </remarks>
    public class NdefHandoverSelectRecord : NdefRecord
    {
        /// <summary>
        /// Type of the Bluetooth Carrier Configuration Record.
        /// </summary>
        public static readonly byte[] BtHsType = Encoding.UTF8.GetBytes("Hs");

        private NdefHandoverVersion _handoverVersion;

        private List<NdefHandoverAlternativeCarrierRecord> _handoverAlternativeCarrierRecords;
        private NdefHandoverErrorRecord _handoverErrorRecord;

        /// <summary>
        /// Indicates the connection handover specification, split up into a Major and a Minor
        /// version number. This implementation conforms to the Connection Handover specification
        /// version 1.3 by the NFC Forum.
        /// </summary>
        public NdefHandoverVersion HandoverVersion
        {
            get { return _handoverVersion; }
            set
            {
                _handoverVersion = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// After the Handover Select record, usually one or more Alternative Carrier
        /// records follow that define the possible carriers for the handover.
        /// </summary>
        /// <remarks>
        /// Note: setting this list triggers updating the payload. If you modify the list directly,
        /// make sure you call AssemblePayload() to trigger another payload update. Or use
        /// the AddHandoverAlternativeCarrierRecord() method to automate this process.</remarks>
        public List<NdefHandoverAlternativeCarrierRecord> HandoverAlternativeCarrierRecords
        {
            get { return _handoverAlternativeCarrierRecords; }
            set
            {
                _handoverAlternativeCarrierRecords = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// Add another Alternative Carrier record as a child and create the storage list if
        /// necessary. Also automatically updates the payload.
        /// </summary>
        /// <param name="newRecord">Additional Alternative Carrier record to add to the list
        /// for use as children of the Handover Select record.</param>
        public void AddHandoverAlternativeCarrierRecord(NdefHandoverAlternativeCarrierRecord newRecord)
        {
            if (_handoverAlternativeCarrierRecords == null)
            {
                _handoverAlternativeCarrierRecords = new List<NdefHandoverAlternativeCarrierRecord>();
            }
            _handoverAlternativeCarrierRecords.Add(newRecord);
            AssemblePayload();
        }

        /// <summary>
        /// The optional error record needs to be last in the message and can contain
        /// details if there was a problem.
        /// </summary>
        public NdefHandoverErrorRecord HandoverErrorRecord
        {
            get { return _handoverErrorRecord; }
            set
            {
                _handoverErrorRecord = value;
                AssemblePayload();
            }
        }

        #region Constructors
        /// <summary>
        /// Create an empty Handover Select Record.
        /// </summary>
        public NdefHandoverSelectRecord()
            :base(TypeNameFormatType.NfcRtd, BtHsType)
        {
        }


        /// <summary>
        /// Create a Handover Select record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a Handover Select Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this Handover Select record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a Handover Select record
        /// based on an incompatible record type.</exception>
        public NdefHandoverSelectRecord(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);

            ParsePayloadToData(_payload);
        }

        /// <summary>
        /// Create a Handover Select record based on the specified version number.
        /// </summary>
        /// <remarks>
        /// To use the Handover Select record, you also need to supply at least
        /// one Alternative Carrier record.</remarks>
        /// <param name="version">Version to use for the handover select record.</param>
        public NdefHandoverSelectRecord(NdefHandoverVersion version)
            : base(TypeNameFormatType.NfcRtd, BtHsType)
        {
            _handoverVersion = version;
        }

        /// <summary>
        /// Create a Handover Select record based on the specified version number and 
        /// supplying an alternative carrier record.
        /// </summary>
        /// <param name="version">Version to use for the handover select record.</param>
        /// <param name="alternativeCarrier">A single alternative carrier record. If you want
        /// to add multiple alternative carrier records, use the AddHandoverAlternativeCarrierRecord()
        /// method.</param>
        public NdefHandoverSelectRecord(NdefHandoverVersion version, NdefHandoverAlternativeCarrierRecord alternativeCarrier)
            : base(TypeNameFormatType.NfcRtd, BtHsType)
        {
            _handoverVersion = version;
            AddHandoverAlternativeCarrierRecord(alternativeCarrier);
        }
        #endregion

        /// <summary>
        /// Create the payload based on the data stored in the properties. Usually called
        /// automatically when changing properties, but you might need to call this manually
        /// if you modify the list of alternative carrier records directly without using
        /// accessor method provided by this class.
        /// </summary>
        /// <exception cref="NdefException">Thrown if unable to assemble the payload.
        /// The exception message contains further details about the issue.</exception>
        public void AssemblePayload()
        {
            if (_handoverVersion == null)
            {
                throw new NdefException(NdefExceptionMessages.ExHandoverInvalidVersion);
            }

            // Convert child records to message
            var childMsg = new NdefMessage();
            if (_handoverAlternativeCarrierRecords != null) childMsg.AddRange(_handoverAlternativeCarrierRecords);
            if (_handoverErrorRecord != null) childMsg.Add(_handoverErrorRecord);

            var childBytes = childMsg.ToByteArray();

            var newPayload = new byte[childBytes.Length + 1];

            // Frist byte: handover version
            newPayload[0] = _handoverVersion.Version;

            // Rest of the payload: child message containing Alternative Carrier records + Error record
            Array.Copy(childBytes, 0, newPayload, 1, childBytes.Length);

            _payload = newPayload;
        }


        /// <summary>
        /// Deletes any details currently stored in the Handover Select record 
        /// and re-initializes them by parsing the contents of the payload.
        /// </summary>
        private void ParsePayloadToData(byte[] payload)
        {
            if (payload == null || payload.Length < 1)
            {
                throw new NdefException(NdefExceptionMessages.ExHandoverInvalidVersion);
            }
            // Parse Handover version (byte 0)
            _handoverVersion = new NdefHandoverVersion(payload[0]);
            
            // Parse child records (bytes 1+)
            var childRecordMsg = new Byte[payload.Length - 1];
            Array.Copy(payload, 1, childRecordMsg, 0, payload.Length - 1);
            SetAndAssignChildRecords(childRecordMsg);
        }

        /// <summary>
        /// Go through the records stored in this message, parse them and assign
        /// them to the individual properties. This also checks if the child records
        /// are actually valid for a Handover Select message.
        /// </summary>
        public void SetAndAssignChildRecords(byte[] payloadToParse)
        {
            // Create NDEF message based on payload
            var childRecordsMsg = NdefMessage.FromByteArray(payloadToParse);

            // Clear previous child records
            _handoverAlternativeCarrierRecords = null;
            _handoverErrorRecord = null;

            // Assign new child records from the source message
            if (childRecordsMsg.Count < 1)
            {
                throw new NdefException(NdefExceptionMessages.ExHandoverSelectMsgInvalidRecords);
            }

            foreach (var curChildRecord in childRecordsMsg)
            {
                // Alternate carrier record
                if (curChildRecord.CheckSpecializedType(false) == typeof(NdefHandoverAlternativeCarrierRecord))
                {
                    if (HandoverErrorRecord != null)
                    {
                        // Error record needs to be last - there can't be an AC record
                        // after we have already found an error record.
                        throw new NdefException(NdefExceptionMessages.ExHandoverSelectMsgInvalidRecords);
                    }
                    if (_handoverAlternativeCarrierRecords == null)
                    {
                        _handoverAlternativeCarrierRecords = new List<NdefHandoverAlternativeCarrierRecord>();
                    }
                    _handoverAlternativeCarrierRecords.Add(new NdefHandoverAlternativeCarrierRecord(curChildRecord));
                }
                else if (curChildRecord.CheckSpecializedType(false) == typeof(NdefHandoverErrorRecord))
                {
                    if (_handoverErrorRecord != null)
                    {
                        // Only one error record is allowed
                        throw new NdefException(NdefExceptionMessages.ExHandoverSelectMsgInvalidRecords);
                    }
                    _handoverErrorRecord = new NdefHandoverErrorRecord(curChildRecord);
                }
                else
                {
                    // Unknown record found that should not be in this message
                    throw new NdefException(NdefExceptionMessages.ExHandoverSelectMsgInvalidRecords);
                }
            }
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Handover Select record.
        /// Checks the type and type name format.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type, type name format and payload
        /// to be a Handover Select record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            return record.TypeNameFormat == TypeNameFormatType.NfcRtd && 
                record.Type != null && 
                record.Type.SequenceEqual(BtHsType) &&
                record.Payload != null &&
                record.Payload.Length > 1;
        }


    }
}
