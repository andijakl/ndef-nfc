/****************************************************************************
**
** Copyright (C) 2012-2016 Andreas Jakl - http://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// The Alternative Carrier record is used within the payload of the Handover Select record
    /// to describe a single alternative carrier.
    /// </summary>
    /// <remarks>
    /// This record essentially describes a single alternative carrier, including its power state
    /// and a reference to where further details can be found - this is done by referencing the ID
    /// of the other NDEF record within the same handover message. The pointed record may be
    /// a Handover Carrier record or a Carrier Configuration record.
    /// 
    /// Optional Auxiliary Data reference(s) are pointers to NDEF records that contain 
    /// additional information about the alternative carrier. There are no limitations on the
    /// type of record that can be pointed to.
    /// </remarks>
    public class NdefHandoverAlternativeCarrierRecord : NdefRecord
    {
        /// <summary>
        /// Type of the Alternative Carrier Record.
        /// </summary>
        public static readonly byte[] BtAcType = Encoding.UTF8.GetBytes("ac");

        private CarrierPowerStates _carrierPowerState;
        private byte[] _carrierDataReference;
        private List<byte[]> _auxiliaryDataReferenceList;

        /// <summary>
        /// Enum to conveniently access the 2-bit field that indicates the carrier power state.
        /// </summary>
        public enum CarrierPowerStates : byte
        {
            Inactive = 0x00,
            Active = 0x01,
            Activating = 0x02,
            Unknown = 0x03
        }

        /// <summary>
        /// 2-bit field that indicates the carrier power state.
        /// </summary>
        public CarrierPowerStates CarrierPowerState
        {
            get { return _carrierPowerState; }
            set
            {
                if (_carrierPowerState == value) return;
                _carrierPowerState = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// Access the Carrier Power State property as a byte instead of the enum. Note
        /// that according to the current Bluetooth specification (1.3), only two bits
        /// of the byte are in use for the Carrier Power State.
        /// </summary>
        /// <remarks>If setting a Carrier power state that is not known by the enum,
        /// the internal value is set to Unknown.</remarks>
        public byte CarrierPowerStateAsByte
        {
            get { return (byte)CarrierPowerState; }
            set {
                CarrierPowerState = Enum.IsDefined(typeof (CarrierPowerStates), value)
                    ? (CarrierPowerStates) value
                    : CarrierPowerStates.Unknown;
            }
        }

        /// <summary>
        /// The Carrier Data Reference is a pointer to an NDEF record that 
        /// uniquely identifies the carrier technology.
        /// </summary>
        /// <remarks>Reference is the record ID of a Handover Carrier record
        /// or a Carrier Configuration Record (can also be a Bluetooth
        /// Secure Simple Pairing record)</remarks>
        public byte[] CarrierDataReference
        {
            get { return _carrierDataReference; }
            set
            {
                _carrierDataReference = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// Access the byte array that should contain chars forming a number as a string for
        /// easier manipulation from within an app.
        /// </summary>
        public string CarrierDataReferenceAsString
        {
            get
            {
                if (CarrierDataReference == null || CarrierDataReference.Length == 0)
                {
                    return null;
                    //throw new NdefException(NdefExceptionMessages.ExHandoverAcCarrierNoData);
                }
                return Encoding.UTF8.GetString(CarrierDataReference, 0, CarrierDataReference.Length);
            }
            set {
                CarrierDataReference = value == null ? null : Encoding.UTF8.GetBytes(value);
            }
        }

        /// <summary>
        /// An Auxiliary Data Reference is a pointer to an NDEF record
        /// that gives additional information about the alternative carrier.
        /// </summary>
        public List<byte[]> AuxiliaryDataReferenceList
        {
            get { return _auxiliaryDataReferenceList; }
            set
            {
                _auxiliaryDataReferenceList = value;
                AssemblePayload();
            }
        }


        /// <summary>
        /// Convenience method to add another auxiliary data reference to the list.
        /// Using this method ensures that the list is already created and updates
        /// the payload according to the changes.
        /// </summary>
        /// <remarks>If you manually modify the AuxiliaryDataReferenceList, you need
        /// to call AssemblePayload() manually when you are finished with changes
        /// and want the payload to be synchronized to the properties.</remarks>
        /// <param name="auxiliaryDataReference">The auxiliary data reference to
        /// add. This is the record ID of another record within the Handover Select
        /// payload which contains additional information about this alternative
        /// carrier.</param>
        public void AddAuxiliaryDataReference(byte[] auxiliaryDataReference)
        {
            if (_auxiliaryDataReferenceList == null)
            {
                _auxiliaryDataReferenceList = new List<byte[]>();
            }
            _auxiliaryDataReferenceList.Add(auxiliaryDataReference);
            AssemblePayload();
        }

        #region Constructors
        /// <summary>
        /// Create an empty Alternative Carrier Record.
        /// </summary>
        public NdefHandoverAlternativeCarrierRecord()
            : base(TypeNameFormatType.NfcRtd, BtAcType)
        {

        }

        /// <summary>
        /// Create an Alternative Carrier Record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be an Alternative Carrier Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this Alternative Carrier Record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a Alternative Carrier Record
        /// based on an incompatible record type.</exception>
        public NdefHandoverAlternativeCarrierRecord(NdefRecord other)
            : base(other)
        {
            ParsePayloadToData(_payload);
        }
        #endregion

        /// <summary>
        /// Create the payload based on the data stored in the properties. Usually called
        /// automatically when changing properties, but you might need to call this manually
        /// if you modify the list of auxiliary data references directly without using
        /// accessor method provided by this class.
        /// </summary>
        /// <exception cref="NdefException">Thrown if unable to assemble the payload.
        /// The exception message contains further details about the issue.</exception>
        public bool AssemblePayload()
        {
            var m = new MemoryStream();

            if (CarrierDataReference == null) return false;

            // Byte 0: last two bit = CPS (Carrier power state), others RFU
            m.WriteByte((byte)CarrierPowerState);

            // Carrier Data Reference: pointer to NDEF record that uniquely identifies the carrier technology
            // May be a Handover Carrier record or a Carrier Configuration record
            // Byte 1: Carrier Data Reference Length
            m.WriteByte((byte)CarrierDataReference.Length);
            // Bytes 2+: Carrier Data Reference
            m.Write(CarrierDataReference, 0, CarrierDataReference.Length);

            // Auxiliary Data Reference(s)
            if (AuxiliaryDataReferenceList == null ||
                AuxiliaryDataReferenceList.Count == 0)
            {
                // No auxiliary data references
                m.WriteByte(0);
            }
            else
            {
                // Byte 1: Number of auxiliary data references
                m.WriteByte((byte)AuxiliaryDataReferenceList.Count);
                // Bytes 2+: the auxiliary data references
                foreach (var curAuxiliaryDataReference in AuxiliaryDataReferenceList)
                {
                    // Length of this data reference (1 byte)
                    m.WriteByte((byte)curAuxiliaryDataReference.Length);
                    // Data reference
                    m.Write(curAuxiliaryDataReference, 0, curAuxiliaryDataReference.Length);
                }
                
            }

            Payload = m.ToArray();
            return true;
        }

        /// <summary>
        /// Deletes any details currently stored in the record 
        /// and re-initializes them by parsing the contents of the payload.
        /// </summary>
        private void ParsePayloadToData(byte[] payload)
        {
            if (payload == null || payload.Length < 2)
            {
                //throw new NdefException(NdefExceptionMessages.ExHandoverErrorInvalidSourceData);
            }
            var m = new MemoryStream(payload);

            // Reset values
            _carrierDataReference = null;
            _auxiliaryDataReferenceList = null;
            
            // Byte 0: Carrier power state (last two bits)
            var cps = (byte)(m.ReadByte() & 0x03);
            _carrierPowerState = Enum.IsDefined(typeof(CarrierPowerStates), cps)
                ? (CarrierPowerStates)cps
                : CarrierPowerStates.Unknown;

            // Carrier data reference
            // Byte 1: Carrier Data Reference Length
            var cdrLength = m.ReadByte();

            // Bytes 2+: Carrier Data Reference
            var cdrBytes = new byte[cdrLength];
            m.Read(cdrBytes, 0, cdrLength);
            _carrierDataReference = cdrBytes;

            // Auxiliary Data Reference(s)
            // Byte 1: Number of auxiliary data references
            var numAdr = m.ReadByte();

            if (numAdr > 0)
            {
                _auxiliaryDataReferenceList = new List<byte[]>(numAdr);
                // Bytes 2+: the auxiliary data references
                for (int i = 0; i < numAdr; i++)
                {
                    // Length of this data reference (1 byte)
                    var curAdrLength = m.ReadByte();
                    // Data reference
                    var curAdrBytes = new byte[curAdrLength];
                    m.Read(curAdrBytes, 0, curAdrLength);
                    _auxiliaryDataReferenceList.Add(curAdrBytes);
                }
            }
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
        public override bool CheckIfValid()
        {
            // First check the basics
            if (!base.CheckIfValid()) return false;

            // Check specific content of this record
            if (CarrierDataReference == null || CarrierDataReference.Count() == 0)
            {
                throw new NdefException(NdefExceptionMessages.ExHandoverNoCarrierDataReference);
            }
            if (CarrierDataReference.Length > 255)
            {
                throw new NdefException(NdefExceptionMessages.ExHandoverDataReferenceTooLong);
            }
            if (AuxiliaryDataReferenceList != null && AuxiliaryDataReferenceList.Count > 255)
            {
                throw new NdefException(NdefExceptionMessages.ExHandoverDataTooManyAuxiliaryReferences);
            }
            return true;
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Handover Alternate Carrier record.
        /// Checks the type and type name format.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type, type name format and payload
        /// to be a Handover Alternate Carrier record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            return record.TypeNameFormat == TypeNameFormatType.NfcRtd &&
                   record.Type != null &&
                   record.Type.SequenceEqual(BtAcType) &&
                   record.Payload != null;
        }
    }
}
