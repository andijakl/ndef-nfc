/****************************************************************************
**
** Copyright (C) 2012-2016 Andreas Jakl - https://www.nfcinteractor.com/
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Create an NDEF record for Bluetooth Carrier Handover according to the 
    /// "Secure Simple Pairing" mechanism defined by the Bluetooth SIG.
    /// </summary>
    /// <remarks>
    /// This record is usually written on static NFC tags that facilitate Bluetooth
    /// handover, as found in speakers or headsets. It enables devices to 
    /// establish a Bluetooth connection by tapping the NFC tag, without
    /// having to manually start a device & service search.
    /// 
    /// The record has to define at least the Bluetooth device address. 
    /// In addition, it usually contains additional optional Bluetooth OOB
    /// (Out-Of-Band) data - for example the friendly device name, information
    /// about the device class as well as supported services.
    /// 
    /// In some cases, also a pairing hash and randomizer can be supplied.
    /// 
    /// The class represents a complete implementation, including all
    /// currently standardized services and device classes as per May 20th, 2014
    /// and defined at: https://www.bluetooth.org/en-us/specification/assigned-numbers
    /// In addition, it provides convenient look-up functions and definitions
    /// to work with the numerous different defined classes and services.
    /// 
    /// The record is implemented according to the specification published by
    /// the NFC Forum. The record is defined as a Media-type per RFC2046, its 
    /// payload by the Extended Inquiry Response (EIR) format specified in
    /// the Bluetooth Core Specification, Volume 3, Part C, Section 8.
    /// 
    /// For information regards to compatibility with the Windows platform:
    /// http://msdn.microsoft.com/en-us/library/windows/hardware/dn482419%28v=vs.85%29.aspx
    /// Supported protocols in Windows:
    /// http://msdn.microsoft.com/en-us/library/windows/hardware/jj866066%28v=vs.85%29.aspx
    /// </remarks>
    public class NdefBtSecureSimplePairingRecord : NdefRecord
    {
        #region Properties and data

        /// <summary>
        /// Type of the Bluetooth Carrier Configuration Record.
        /// </summary>
        public static readonly byte[] BtOobType = Encoding.UTF8.GetBytes("application/vnd.bluetooth.ep.oob");

        /// <summary>
        /// IDs of OOB Data according to the Bluetooth Generic Access Profile (GAP).
        /// </summary>
        /// <remarks>
        /// The OOB (Bluetooth Out Of Band) data consists of several data blocks.
        /// Each data block first specifies the data block length, then this ID, and
        /// then the data type specific payload data (e.g., the local device name).
        /// https://www.bluetooth.org/en-us/specification/assigned-numbers/generic-access-profile
        /// </remarks>
        public enum OobDataTypes : byte
        {
            IncompleteList16BitServiceClassUuids = (byte)0x02,
            CompleteList16BitServiceClassUuids = (byte)0x03,
            IncompleteList32BitServiceClassUuids = (byte)0x04,
            CompleteList32BitServiceClassUuids = (byte)0x05,
            IncompleteList128BitServiceClassUuids = (byte)0x06,
            CompleteList128BitServiceClassUuids = (byte)0x07,
            ShortenedLocalName = (byte)0x08,
            CompleteLocalName = (byte)0x09,
            ClassOfDevice = (byte)0x0D,
            SimplePairingHashC = (byte)0x0E,
            SimplePairingRandomizerR = (byte)0x0F
        }

        /// <summary>
        /// Combination of many other OOB Optional data items - including the Bluetooth Local Name,
        /// simple pairing hash, class of device or service class.
        /// </summary>
        private byte[] BtOobData { get; set; }


        private byte[] _btDeviceAddress;

        /// <summary>
        /// Bluetooth device address.
        /// </summary>
        /// <remarks>
        /// Encoded in little endian order.
        /// For example, the Bluetooth Address 00:0c:78:51:c4:06 would be encoded as 0x06 0xC4 0x51 0x78 0x0C 0x00.
        /// </remarks>
        public byte[] BtDeviceAddress
        {
            get { return _btDeviceAddress; }
            set
            {
                _btDeviceAddress = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// Get / set the Bluetooth device address as a hex string instead of the byte array.
        /// The address string is in a format like "00:0c:78:51:c4:06".
        /// </summary>
        public string BtDeviceAddressString
        {
            get
            {
                // Convert the byte array to a string
                return ByteArrayToHexString(BtDeviceAddress);
            }
            set
            {
                // Set the string to the byte array
                BtDeviceAddress = HexStringToByteArray(value);
            }
        }

        private string _completeLocalName;
        /// <summary>
        /// The complete local device name / friendly name of the Bluetooth device.
        /// This is usually shown to the user during a Bluetooth search or 
        /// when attempting to establish a connection to the device.
        /// </summary>
        public string CompleteLocalName
        {
            get { return _completeLocalName; }
            set
            {
                _completeLocalName = value;
                AssemblePayload();
            }
        }

        private BtClassOfDevice _classOfDevice;
        /// <summary>
        /// The Bluetooth Class of Device information, which is used to provide a 
        /// graphical representation to the user as part of UI involving operations with
        /// Bluetooth devices.
        /// </summary>
        /// <remarks>For example, the UI may show a particular icon to represent the device,
        /// which is chosen based on the class of device defined by this property.</remarks>
        public BtClassOfDevice ClassOfDevice
        {
            get { return _classOfDevice; }
            set
            {
                _classOfDevice = value;
                AssemblePayload();
            }
        }

        private BtServiceClasses _serviceClasses;
        /// <summary>
        /// Defines the supported Bluetooth services of the device.
        /// </summary>
        /// <remarks>
        /// The service classes can be represented as
        /// 16 / 32 / 128 bit UUIDs. For standardized services,
        /// usually 16 bit UUIDs are used. The class contains a
        /// convenient enumeration for the currently standardized
        /// service classes.
        /// </remarks>
        public BtServiceClasses ServiceClasses
        {
            get { return _serviceClasses; }
            set
            {
                _serviceClasses = value;
                AssemblePayload();
            }
        }

        private byte[] _simplePairingHashC;
        /// <summary>
        /// The optional Simple Pairing Hash C can be used to facilitate the pairing process.
        /// </summary>
        /// <remarks>
        /// The Hash C should be generated anew for each pairing - this is usually
        /// not possible on passive & static NFC Forum tags.
        /// </remarks>
        public byte[] SimplePairingHashC
        {
            get { return _simplePairingHashC; }
            set
            {
                if (value != null && value.Length != 16)
                {
                    throw new NdefException(NdefExceptionMessages.ExBtInvalidLengthSimplePairingHashC);
                }
                _simplePairingHashC = value;
                AssemblePayload();
            }
        }

        private byte[] _simplePairingRandomizerR;
        /// <summary>
        /// The optional Simple Pairing Randomizer to facilitate pairing.
        /// </summary>
        public byte[] SimplePairingRandomizerR
        {
            get { return _simplePairingRandomizerR; }
            set
            {
                if (value != null && value.Length != 16)
                {
                    throw new NdefException(NdefExceptionMessages.ExBtInvalidLengthSimplePairingRandomizerR);
                }
                _simplePairingRandomizerR = value;
                AssemblePayload();
            }
        }

        #endregion


        #region Constructors
        /// <summary>
        /// Create an empty Bluetooth Secure Simple Pairing / Carrier Configuration record.
        /// </summary>
        public NdefBtSecureSimplePairingRecord()
            : base(TypeNameFormatType.Mime, BtOobType)
        {
        }

        /// <summary>
        /// Create a Bluetooth Secure Simple Pairing / Carrier Configuration record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a Bluetooth Carrier Configuration Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this Secure Simple Pairing record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create an Secure Simple Pairing
        /// Record based on an incompatible record type.</exception>
        public NdefBtSecureSimplePairingRecord(NdefRecord other)
            : base(other)
        {
            ParsePayloadToData(_payload);
        }

        #endregion


        #region Parse data

        /// <summary>
        /// Deletes any details currently stored in the record 
        /// and re-initializes them by parsing the contents of the payload.
        /// </summary>
        private void ParsePayloadToData(byte[] payload)
        {
            if (payload == null || payload.Length < 8)
            {
                throw new NdefException(NdefExceptionMessages.ExBtInvalidMinimumLength);
            }

            // OOB Data length (2 bytes) - little endian order
            var oobLength = (payload[1] << 8) | payload[0];

            if (oobLength != payload.Length)
            {
                // Don't be strict if the encoded length does not match the payload length.
                // According to the NFC Forum Bluetooth Secure Simple Pairing Using NFC specification,
                // there was an inconsistency in the Bluetooth definitions as to what the 
                // length contains (whether to include mandatory fields). This has only
                // been cleared up with Bluetooth 4.0.
                Debug.WriteLine(NdefExceptionMessages.ExBtInvalidLength);
            }

            // Bluetooth Device address (6 bytes)
            _btDeviceAddress = new byte[6];
            Array.Copy(payload, 2, _btDeviceAddress, 0, 6);

            // OOB Optional data (xx bytes)
            BtOobData = new byte[payload.Length - 8];
            Array.Copy(payload, 8, BtOobData, 0, payload.Length - 8);
            ParseOobData();
        }

        /// <summary>
        /// Utility method to parse the optional OOB data that follows after the record header
        /// and the Bluetooth device address.
        /// </summary>
        private void ParseOobData()
        {
            var i = 0;
            while (i < BtOobData.Length)
            {
                // First byte: EIR data length
                var eirDataLength = BtOobData[i];

                if (eirDataLength > 0)
                {
                    // Second byte: EIR Data Type
                    var eirDataType = BtOobData[++i];

                    // 3+ bytes: Data
                    var eirData = new byte[eirDataLength - 1];
                    Array.Copy(BtOobData, ++i, eirData, 0, eirDataLength - 1);
                    i += eirDataLength - 1;

                    // Call utility function to parse the data according
                    // to its type.
                    InternalizeOobDataElement(eirDataType, eirData);
                }
            }
        }

        /// <summary>
        /// Utility method to parse the provided OOB optional data block /
        /// EIR (Extended Inquirey Response).
        /// </summary>
        /// <param name="eirDataType">The data type of the OOB block.</param>
        /// <param name="eirData">Payload of the OOB block.</param>
        private void InternalizeOobDataElement(byte eirDataType, byte[] eirData)
        {
            // Warning: use _member variables and don't set property to avoid
            // triggering a payload assembly process while parsing the payload!
            switch (eirDataType)
            {
                case (byte)OobDataTypes.CompleteLocalName:
                    _completeLocalName = Encoding.UTF8.GetString(eirData, 0, eirData.Length);
                    break;
                case (byte)OobDataTypes.ShortenedLocalName:
                    if (string.IsNullOrEmpty(_completeLocalName))
                    {
                        _completeLocalName = Encoding.UTF8.GetString(eirData, 0, eirData.Length);
                    }
                    break;
                case (byte)OobDataTypes.ClassOfDevice:
                    _classOfDevice = new BtClassOfDevice(eirData);
                    break;
                case (byte)OobDataTypes.SimplePairingHashC:
                    if (eirData.Length != 16)
                    {
                        throw new NdefException(NdefExceptionMessages.ExBtInvalidLengthSimplePairingHashC);
                    }
                    _simplePairingHashC = new byte[eirData.Length];
                    Array.Copy(eirData, _simplePairingHashC, eirData.Length);
                    break;
                case (byte)OobDataTypes.SimplePairingRandomizerR:
                    if (eirData.Length != 16)
                    {
                        throw new NdefException(NdefExceptionMessages.ExBtInvalidLengthSimplePairingRandomizerR);
                    }
                    _simplePairingRandomizerR = new byte[eirData.Length];
                    Array.Copy(eirData, _simplePairingRandomizerR, eirData.Length);
                    break;
                case (byte)OobDataTypes.CompleteList128BitServiceClassUuids:
                case (byte)OobDataTypes.IncompleteList128BitServiceClassUuids:
                case (byte)OobDataTypes.IncompleteList16BitServiceClassUuids:
                case (byte)OobDataTypes.CompleteList16BitServiceClassUuids:
                case (byte)OobDataTypes.IncompleteList32BitServiceClassUuids:
                case (byte)OobDataTypes.CompleteList32BitServiceClassUuids:
                {
                    _serviceClasses = new BtServiceClasses(eirDataType, eirData);
                }
                    break;
                default:
                    Debug.WriteLine("BT NDEF record: Did not parse optional OOB data with type: " + (int)eirDataType);
                    break;
            }
        }


        #endregion

        #region Assemble data

        /// <summary>
        /// Takes the information stored in the individual properties and assembles
        /// them into the payload of the base class.
        /// </summary>
        private void AssemblePayload()
        {
            if (BtDeviceAddress == null || BtDeviceAddress.Length != 6)
            {
                throw new NdefException(NdefExceptionMessages.ExBtNoDeviceAddress);
            }

            // Make sure the optional data is correctly committed to the internal byte array
            AssembleOobData();

            // Get length of optional data
            var oobOptionalDataLength = (BtOobData != null) ? BtOobData.Length : 0;

            // Get complete length of the payload
            var oobLength = (ushort)(oobOptionalDataLength + BtDeviceAddress.Length + 2);

            using (var m = new MemoryStream())
            {
                // OOB Data length (2 bytes) - little endian order
                m.WriteByte((byte)(oobLength & 0x000000ff));
                m.WriteByte((byte)((oobLength >> 8) & 0xff));

                // Bluetooth Device address (6 bytes)
                m.Write(BtDeviceAddress, 0, BtDeviceAddress.Length);

                // OOB Optional data (xx bytes)
                if (BtOobData != null)
                {
                    m.Write(BtOobData, 0, BtOobData.Length);
                }

                // Assign generated byte array stream to the payload
                Payload = m.ToArray();
            }
        }

        /// <summary>
        /// Utility method to convert the optional Bluetooth OOB data into the instance
        /// variable byte array (BtOobData), so that it can be included into the payload.
        /// </summary>
        private void AssembleOobData()
        {
            using (var oob = new MemoryStream())
            {
                // Bluetooth local name
                if (!string.IsNullOrEmpty(CompleteLocalName))
                {
                    AddOobDataElement(oob, OobDataTypes.CompleteLocalName, Encoding.UTF8.GetBytes(CompleteLocalName));
                }
                // Class of Device
                if (ClassOfDevice != null)
                {
                    AddOobDataElement(oob, OobDataTypes.ClassOfDevice, ClassOfDevice.ToByteArray());
                }
                // Simple Pairing Hash C
                if (SimplePairingHashC != null)
                {
                    if (SimplePairingHashC.Length != 16)
                    {
                        throw new NdefException(NdefExceptionMessages.ExBtInvalidLengthSimplePairingHashC);
                    }
                    AddOobDataElement(oob, OobDataTypes.SimplePairingHashC, SimplePairingHashC);
                }
                // Simple Pairing Randomizer R
                if (SimplePairingRandomizerR != null)
                {
                    if (SimplePairingRandomizerR.Length != 16)
                    {
                        throw new NdefException(NdefExceptionMessages.ExBtInvalidLengthSimplePairingRandomizerR);
                    }
                    AddOobDataElement(oob, OobDataTypes.SimplePairingRandomizerR, SimplePairingRandomizerR);
                }
                // Service Class
                if (ServiceClasses != null)
                {
                    AddOobDataElement(oob, ServiceClasses.DataType, ServiceClasses.ToByteArray());
                }

                BtOobData = oob.ToArray();
            }

        }


        /// <summary>
        /// Write a new OOB data element to the provided data stream. Takes care of the correct
        /// format, including measuring the data length.
        /// </summary>
        /// <param name="oobData">Target stream which should then contain the OOB data block.</param>
        /// <param name="type">Data type of the OOB element.</param>
        /// <param name="data">Payload of the OOB element to write (does not include its
        /// type or length - both properties will be prepended by this method!)</param>
        private static void AddOobDataElement(Stream oobData, OobDataTypes type, byte[] data)
        {
            if (data == null) return;

            var dataLength = 1 + data.Length;
            // 1 Byte: data length
            oobData.WriteByte((byte)dataLength);
            // 2 Byte: Type
            oobData.WriteByte((byte)type);
            // xx bytes: OOB Data
            oobData.Write(data, 0, data.Length);
        }

        #endregion


        #region Data processing tools

        /// <summary>
        /// Convert the supplied byte array to a Hex string in the format of for
        /// example "00:0c:78:51:c4:06". Used for the Bluetooth Device Address.
        /// </summary>
        /// <param name="array">Byte array to convert to a hex string.</param>
        /// <returns>The byte array converted to a hex string, with ":" as separator.</returns>
        private static string ByteArrayToHexString(IEnumerable<byte> array)
        {
            // TODO check length of the array
            return BitConverter.ToString(array.Reverse().ToArray()).Replace("-", ":");
        }

        /// <summary>
        /// Convert pure hex string or ":"-separated hex string to a byte array. Format example: "00:0c:78:51:c4:06".
        /// </summary>
        /// <param name="hex">Hex string to convert to a byte array. In case it contains
        /// ":" as the separator character, the separators will be removed automatically for conversion. Also, any
        /// '(' and ')' characters will be trimmed from the parameter string.</param>
        /// <returns>The hex string converted to a byte array.</returns>
        private static byte[] HexStringToByteArray(string hex)
        {
            // Remove all ":" characters
            // Trim away opening and closing '(' and ')' characters
            var hexPure = hex.Replace(":", "").Trim(new[] {'(', ')'});
            if (hexPure.Length != 12)
            {
                // No correct length of the string
                throw new NdefException(NdefExceptionMessages.ExBtNoValidHexAddress);
            }

            var numberChars = hexPure.Length / 2;
            var bytes = new byte[numberChars];
            var sr = new StringReader(hexPure);
            try
            {
                for (var i = 0; i < numberChars; i++)
                {
                    bytes[i] = Convert.ToByte(new string(new[] { (char)sr.Read(), (char)sr.Read() }), 16);
                }
            }
            catch (Exception)
            {
                throw new NdefException(NdefExceptionMessages.ExBtNoValidHexAddress);
            }
            finally
            {
                sr.Dispose();
            }
            return bytes.Reverse().ToArray();
        }
        #endregion

        #region Generic record identification

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Bluetooth Secure Simple Pairing / Carrier Configuration Record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a Bluetooth Secure Simple Pairing / Carrier Configuration Record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record?.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.Mime && record.Type.SequenceEqual(BtOobType));
        }
        #endregion


        #region Subclasses

        /// <summary>
        /// Defines the Class of Device information data. This is to be used to provide a graphical
        /// representation to the user as part of UI involving operations with Bluetooth devices.
        /// For example, it may provide a particular icon to represent the device.
        /// </summary>
        /// <remarks>
        /// For more information regarding the standardized values that are contained
        /// in the class of device, see:
        /// https://www.bluetooth.org/en-us/specification/assigned-numbers/baseband
        /// </remarks>
        public class BtClassOfDevice
        {
            #region Format
            // --------------------------------------------------------------------------
            // Format

            /// <summary>
            /// Defines the format of the class of device information. Only two bits are used.
            /// </summary>
            /// <remarks>Currently, only the format #1 is defined / standardized.</remarks>
            public enum ClassOfDeviceFormatTypes : uint
            {
                Format1 = 0x00
            }


            private ClassOfDeviceFormatTypes _classOfDeviceFormat = ClassOfDeviceFormatTypes.Format1;

            /// <summary>
            /// The class of device format currently in use by the class of device definition.
            /// </summary>
            public ClassOfDeviceFormatTypes ClassOfDeviceFormat
            {
                get { return _classOfDeviceFormat; }
                set { _classOfDeviceFormat = value; }
            }

            #endregion

            #region Major Service Class
            // --------------------------------------------------------------------------
            // Major Service Class

            /// <summary>
            /// The different major service class flags, including the two reserved bits not yet in use.
            /// The actual service class in use can be a combination of one or more of these flags.
            /// </summary>
            [Flags]
            public enum MajorServiceClassTypes : uint
            {
                LimitedDiscoverableMode = 1 << 0,
                ReservedBit14 = 1 << 1,
                ReservedBit15 = 1 << 2,
                Positioning = 1 << 3,
                Networking = 1 << 4,
                Rendering = 1 << 5,
                Capturing = 1 << 6,
                ObjectTransfer = 1 << 7,
                Audio = 1 << 8,
                Telephony = 1 << 9,
                Information = 1 << 10,
            }

            /// <summary>
            /// Defines the major service class of the Bluetooth device.
            /// Bitmask containing one or more flags from the MajorServiceClassTypes enum.
            /// </summary>
            public uint MajorServiceClass { get; set; }

            /// <summary>
            /// Set the MajorServiceClass property by providing a single service class as specified
            /// by the MajorServiceClassTypes enum.
            /// </summary>
            /// <param name="majorClass">The major service class to set.</param>
            public void SetMajorServiceClassFromEnum(MajorServiceClassTypes majorClass)
            {
                MajorServiceClass = (uint) majorClass;
            }

            /// <summary>
            /// Set the major service class by providing a list of service class types.
            /// The method takes care of combining those into the single MajorServiceClass property.
            /// </summary>
            /// <param name="majorClasses">A list of different major service classes.</param>
            public void SetMajorServiceClassFromEnum(IEnumerable<MajorServiceClassTypes> majorClasses)
            {
                MajorServiceClass = 0;
                if (majorClasses != null)
                {
                    foreach (var curMajorClass in majorClasses)
                    {
                        MajorServiceClass |= (uint) curMajorClass;
                    }
                }
            }

            /// <summary>
            /// Get the current major service class(es) as a list of enum values, which makes
            /// parsing easier than having to manually check for flags.
            /// </summary>
            /// <returns></returns>
            public List<MajorServiceClassTypes> MajorServiceClassesAsList()
            {
                return Enum.GetValues(typeof (MajorServiceClassTypes)).Cast<MajorServiceClassTypes>().Where(curMsc => (MajorServiceClass & (uint) curMsc) == (uint) curMsc).ToList();
            }

            #endregion

            #region Major Device Class
            // --------------------------------------------------------------------------
            // Major Device Class

            /// <summary>
            /// Different major device class types currently standardized.
            /// </summary>
            public enum MajorDeviceClassTypes : uint // Max 5 bit
            {
                Miscellaneous = (uint)0x00,
                Computer = (uint)0x01,
                Phone = (uint)0x02,
                LanNetworkAccessPoint = (uint)0x03,
                AudioVideo = (uint)0x04,
                Peripheral = (uint)0x05,
                Imaging = (uint)0x06,
                Wearable = (uint)0x07,
                Toy = (uint)0x08,
                Health = (uint)0x09,
                Uncategorized = (uint)0x1F,
            }

            /// <summary>
            /// Major device class of the Bluetooth device.
            /// Can equal one of the types defined in the MajorDeviceClassTypes enum.
            /// </summary>
            public uint MajorDeviceClass { get; set; }

            /// <summary>
            /// Set the major device class by directly specifying the enum value.
            /// </summary>
            /// <param name="deviceClass">The major device class as enum value.</param>
            public void SetMajorDeviceClassFromEnum(MajorDeviceClassTypes deviceClass)
            {
                MajorDeviceClass = (uint) deviceClass;
            }
            #endregion

            #region Minor Device Class
            // --------------------------------------------------------------------------
            // Minor Device Class
            // Note: there can be 0-2 minor classes, depending on the major class in use!

            /// <summary>
            /// Lookup table that defines how many minor classes a major class defines.
            /// </summary>
            private static readonly Dictionary<MajorDeviceClassTypes, ushort> NumMinorDeviceClassesForMajorClassTable = new Dictionary<MajorDeviceClassTypes, ushort>
            {
                {MajorDeviceClassTypes.Miscellaneous, 0},
                {MajorDeviceClassTypes.Computer, 1},
                {MajorDeviceClassTypes.Phone, 1},
                {MajorDeviceClassTypes.LanNetworkAccessPoint, 2},
                {MajorDeviceClassTypes.AudioVideo, 1},
                {MajorDeviceClassTypes.Peripheral, 2},
                {MajorDeviceClassTypes.Imaging, 2},
                {MajorDeviceClassTypes.Wearable, 1},
                {MajorDeviceClassTypes.Toy, 1},
                {MajorDeviceClassTypes.Health, 1},
                {MajorDeviceClassTypes.Uncategorized, 0},
            };

            /// <summary>
            /// Get how many minor classes the supplied major class has. A major class can
            /// have 0 to 2 minor classes, depending on its type. See the Bluetooth specifications
            /// for more details.
            /// </summary>
            /// <param name="majorType">The major type to retrieve the number of minor classes for.</param>
            /// <returns>Number of minor classes in use for the specified major class.</returns>
            public static int NumDeviceClassesForMajorClass(MajorDeviceClassTypes majorType)
            {
                return NumMinorDeviceClassesForMajorClassTable.ContainsKey(majorType)
                    ? NumMinorDeviceClassesForMajorClassTable[majorType]
                    : -1;
            }

            /// <summary>
            /// Check whether the major type is using flags for its minor type (primary / secondary).
            /// If yes, multiple minor values can be set for a minor type. If no, only a single minor value applies.
            /// </summary>
            /// <remarks>Currently, only the primary minor class of the Imaging major class is using flags.</remarks>
            /// <param name="majorType">Major device class type to retrieve the information for.</param>
            /// <param name="primarySecondary">Use the primary or secondary bits.</param>
            /// <returns>True if the minor bits should be interpreted as flags (multiple
            /// minor classes per major class); false if they should be interpreted as value 
            /// (only one minor class per major class)</returns>
            public static bool UsesFlagsForMinorClass(MajorDeviceClassTypes majorType, int primarySecondary)
            {
                // Hard-coded: only the primary imaging type uses flags to combine
                // multiple properties into its bits.
                return majorType == MajorDeviceClassTypes.Imaging && primarySecondary == 1;
            }

            /// <summary>
            /// Definition list of the minor device classes currently standardized for Bluetooth.
            /// </summary>
            /// <remarks>The values are not mapped directly to bit-values stored in the payload.
            /// They are used to interact with the class and to conveniently set one or more
            /// minor device classes. The implementation will take care of setting the correct
            /// bits when you supply the minor device classes. Of course, you need to make sure
            /// the minor device classes fit to the major device class.</remarks>
            public enum MinorDeviceClassList : uint
            {
                // ** Computer
                ComputerUncategorized,
                ComputerDesktopWorkstation,
                ComputerServerClassComputer,
                ComputerLaptop,
                ComputerHandheldPcPdaClamshell,
                ComputerPalmSizePcPda,
                ComputerWearableComputerWatchSize,
                ComputerTablet,

                // ** Phone
                PhoneUncategorized,
                PhoneCellular,
                PhoneCordless,
                PhoneSmartphone,
                PhoneWiredModemOrVoiceGateway,
                PhoneCommonIsdnAccess,

                // ** LAN/Network Access Point 
                Lan1FullyAvailable,
                Lan1_1To17PercentUtilized,
                Lan1_17To33PercentUtilized,
                Lan1_33To50PercentUtilized,
                Lan1_50To67PercentUtilized,
                Lan1_67To83PercentUtilized,
                Lan1_83To99PercentUtilized,
                Lan1NoServiceAvailable,
                Lan2Uncategorized,

                // ** Audio Video
                AudioVideoUncategorized,
                AudioVideoWearableHeadsetDevice,
                AudioVideoHandsFreeDevice,
                AudioVideoMicrophone,
                AudioVideoLoudspeaker,
                AudioVideoHeadphones,
                AudioVideoPortableAudio,
                AudioVideoCarAudio,
                AudioVideoSetTopBox,
                AudioVideoHiFiAudioDevice,
                AudioVideoVcr,
                AudioVideoVideoCamera,
                AudioVideoCamcorder,
                AudioVideoVideoMonitor,
                AudioVideoVideoDisplayAndLoudspeaker,
                AudioVideoVideoConferencing,
                AudioVideoGamingToy,

                // ** Peripheral 1
                Peripheral1NotKeyboardNotPointing,
                Peripheral1Keyboard,
                Peripheral1Pointing,
                Peripheral1ComboKeyboardPointing,
                // ** Peripheral 2
                Peripheral2Uncategorized,
                Peripheral2Joystick,
                Peripheral2Gamepad,
                Peripheral2RemoteControl,
                Peripheral2SensingDevice,
                Peripheral2DigitizerTablet,
                Peripheral2CardReader,
                Peripheral2DigitalPen,
                Peripheral2HandheldScanner,
                Peripheral2HandheldGesturalInput,

                // ** Imaging 1 (Flags!)
                ImagingDisplay,
                ImagingCamera,
                ImagingScanner,
                ImagingPrinter,
                // ** Imaging 2
                ImagingUncategorized,

                // ** Wearable
                WearableWristwatch,
                WearablePager,
                WearableJacket,
                WearableHelmet,
                WearableGlasses,

                // ** Toy
                ToyRobot,
                ToyVehicle,
                ToyDollActionFigure,
                ToyController,
                ToyGame,

                // ** Health
                HealthUndefined,
                HealthBloodPressureMonitor,
                HealthThermometer,
                HealthWeighingScale,
                HealthGlucoseMeter,
                HealthPulseOximeter,
                HealthHeartPulseRateMonitor,
                HealthHealthDataDisplay,
                HealthStepCounter,
                HealthBodyCompositionAnalyzer,
                HealthPeakFlowMonitor,
                HealthMedicationMonitor,
                HealthKneeProsthesis,
                HealthAnkleProsthesis,
                HealthGenericHealthManager,
                HealthPersonalMobilityDevice,

                Unknown
            }

            /// <summary>
            /// Minor device class of the Bluetooth of the device.
            /// </summary>
            /// <remarks>This is the original value, where multiple minor device classes
            /// can be encoded. Note that the value retrieved here does not directly correspond
            /// to the MinorDeviceClassList enum definition. To de-encode the values that are actually
            /// set to the enum, use the MinorDeviceClassAsEnum() method.</remarks>
            public uint MinorDeviceClass { get; set; }

            /// <summary>
            /// Get all currently set minor device class values as a list for the specified
            /// primary or secondary minor device class.
            /// </summary>
            /// <param name="primarySecondary">Set to 1 to retrieve the minor device class
            /// values for the first minor class, or set to 2 for the second minor class
            /// (if applicable).</param>
            /// <returns>A list containing Unknown if no minor type was found, or otherwise
            /// a list of enums that are encoded into the specified minor device class.</returns>
            public List<MinorDeviceClassList> MinorDeviceClassAsEnum(int primarySecondary)
            {
                var minorType = MinorDeviceClassAsType(primarySecondary);
                if (minorType == null || minorType.Count == 0)
                {
                    // No minor types found
                    return new List<MinorDeviceClassList> {MinorDeviceClassList.Unknown};
                }
                // Minor types found - extract enum values
                return minorType.Select(curMinor => curMinor.MinorList).ToList();
            }

            /// <summary>
            /// Get all currently set minor device class values as extended type info for the specified
            /// primary or secondary minor device class.
            /// </summary>
            /// <remarks>In contrast to the MinorDeviceClassAsEnum() method, this will retrieve a list
            /// of MinorDeviceClassType objects; these contain not just the actual minor device class value(s),
            /// but also additional information about each minor device class in use. See the definition of the
            /// MinorDeviceClassType class for more details</remarks>
            /// <param name="primarySecondary">Set to 1 to retrieve the minor device class
            /// values for the first minor class, or set to 2 for the second minor class
            /// (if applicable).</param>
            /// <returns>A list of all minor device class types that are currently set for the specified
            /// primary or secondary minor class. Returns null or an empty list for invalid input or
            /// if no minor classes are set.</returns>
            public List<MinorDeviceClassType> MinorDeviceClassAsType(int primarySecondary)
            {
                List<MinorDeviceClassType> minorList = null;
                if (NumDeviceClassesForMajorClass((MajorDeviceClassTypes)MajorDeviceClass) >= primarySecondary)
                {
                    var usesFlags = UsesFlagsForMinorClass((MajorDeviceClassTypes) MajorDeviceClass, primarySecondary);
                    minorList = new List<MinorDeviceClassType>();

                    foreach (var curMinor in MinorDeviceClasses.Where(curMinor => curMinor.MajorFieldByte == MajorDeviceClass && curMinor.PrimarySecondary == primarySecondary))
                    {
                        var bitMask = (1 << curMinor.NumBits) - 1;
                        var processedMinor = (uint) ((MinorDeviceClass >> curMinor.BitOffset) & bitMask);
                        if (usesFlags)
                        {
                            if ((processedMinor & curMinor.MinorFieldByte) == curMinor.MinorFieldByte)
                            {
                                minorList.Add(curMinor);
                            }
                        }
                        else
                        {
                            if (processedMinor == curMinor.MinorFieldByte)
                            {
                                minorList.Add(curMinor);
                                return minorList;
                            }
                        }
                    }
                }
                return minorList;
            }

            /// <summary>
            /// Conveniently set a minor device class by supplying enum values, letting the implementation
            /// to figure out the exact bit encoding.
            /// </summary>
            /// <param name="minorClassType">The minor device class value to set.
            /// If the minor device class supports setting multiple values as flags, the minorClassType list
            /// parameter can contain multiple minor device classes. Otherwise, it can only contain a single minor
            /// device class. Find out how if the minor device class uses flags, use UsesFlagsForMinorClass().</param>
            /// <param name="primarySecondary">Whether to assign the minor device class value as a primary or
            /// secondary minor device class. Set to 1 to retrieve the minor device class
            /// values for the first minor class, or set to 2 for the second minor class
            /// (if applicable).</param>
            /// <param name="clearPrevious">Set to true to delete all minor device classes that are currently
            /// assigned before applying the new minor device class. This essentially sets the MinorDeviceClass
            /// property to 0 before using the logical OR operator to add the new minor device class.</param>
            public void SetMinorDeviceClassFromEnum(List<MinorDeviceClassList> minorClassType, int primarySecondary, bool clearPrevious)
            {
                if (clearPrevious)
                {
                    MinorDeviceClass = 0;
                }
                if (NumDeviceClassesForMajorClass((MajorDeviceClassTypes) MajorDeviceClass) >= primarySecondary)
                {
                    foreach (var curNewMinor in minorClassType)
                    {
                        foreach (var curReferenceMinor in MinorDeviceClasses)
                        {
                            if (curReferenceMinor.MajorFieldByte == MajorDeviceClass &&
                                curReferenceMinor.MinorList == curNewMinor &&
                                curReferenceMinor.PrimarySecondary == primarySecondary)
                            {
                                MinorDeviceClass |= (curReferenceMinor.MinorFieldByte << curReferenceMinor.BitOffset);
                            }
                        }
                    }

                }
            }

            /// <summary>
            /// Information class that specifies required information to correclty parse and create a minor device class
            /// based on one or more encoded values. The exact format of the minor device class depends on the major
            /// device class in use, and this class enables to look up the encoding and lets this implementation
            /// take care of most of the complex underlying work that needs to be done.
            /// </summary>
            public class MinorDeviceClassType
            {
                /// <summary>
                /// The major device class this instance is describing - 
                /// specified as a uint value, but can be casted to the MajorDeviceClassTypes enum.
                /// </summary>
                public uint MajorFieldByte { get; set; }
                /// <summary>
                /// The value of the minor device class defined by this instance.
                /// Note that only a certain number of bits are used from this definition (specified
                /// in NumBits) and that it might be necessary to bit-shift the value for the
                /// minor device class overall definition (specified by BitOffset).
                /// </summary>
                public uint MinorFieldByte { get; set; }
                /// <summary>
                /// The minor device class represented as the enum value. Note that this enum value is
                /// not related to the actual bits set in the minor device class - these are specified
                /// by the MinorFieldByte property.
                /// </summary>
                public MinorDeviceClassList MinorList { get; set; }
                /// <summary>
                /// If this instance describes the primary (1) or secondary (2) minor device class.
                /// How many device classes are available depends on the major device class.
                /// </summary>
                public int PrimarySecondary { get; set; }
                /// <summary>
                /// How many bits are used by the MinorFieldByte according to the definition.
                /// Especially if multiple minor device classes are encoded, the number of bits in
                /// use for the minor device class can be less than usual in order to leave room for
                /// other values (in the secondary class).
                /// </summary>
                public int NumBits { get; set; }
                /// <summary>
                /// Offset to be used for bit shifting to get the MinorFieldByte to the correct
                /// position in the overall minor device class field in the payload of the OOB record.
                /// </summary>
                public int BitOffset { get; set; }
            }

            /// <summary>
            /// Minor device class definitions. Used by the implementation to assemble and to parse
            /// the minor device class information encoded into the record.
            /// </summary>
            /// <remarks>For details about the individual values, see the MinorDeviceClassType class definition.
            /// The actual definitions correspond to the Bluetooth specification, as defined by:
            /// https://www.bluetooth.org/en-us/specification/assigned-numbers/baseband
            /// </remarks>
            public static readonly List<MinorDeviceClassType> MinorDeviceClasses = new List<MinorDeviceClassType>
            {
                // ** Computer
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x00, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerUncategorized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x01, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerDesktopWorkstation},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x02, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerServerClassComputer},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x03, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerLaptop},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x04, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerHandheldPcPdaClamshell},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x05, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerPalmSizePcPda},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x06, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerWearableComputerWatchSize},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Computer, MinorFieldByte = 0x07, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ComputerTablet},
            
                // ** Phone
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Phone, MinorFieldByte = 0x00, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.PhoneUncategorized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Phone, MinorFieldByte = 0x01, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.PhoneCellular},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Phone, MinorFieldByte = 0x02, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.PhoneCordless},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Phone, MinorFieldByte = 0x03, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.PhoneSmartphone},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Phone, MinorFieldByte = 0x04, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.PhoneWiredModemOrVoiceGateway},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Phone, MinorFieldByte = 0x05, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.PhoneCommonIsdnAccess},
                
                // ** LAN/Network Access Point 
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x00, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1FullyAvailable},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x01, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1_1To17PercentUtilized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x02, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1_17To33PercentUtilized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x03, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1_33To50PercentUtilized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x04, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1_50To67PercentUtilized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x05, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1_67To83PercentUtilized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x06, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1_83To99PercentUtilized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x07, NumBits = 3, BitOffset = 3, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Lan1NoServiceAvailable},

                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.LanNetworkAccessPoint, MinorFieldByte = 0x00, NumBits = 3, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Lan2Uncategorized},

                // ** Audio Video
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x00, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoUncategorized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x01, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoWearableHeadsetDevice},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x02, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoHandsFreeDevice},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x04, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoMicrophone},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x05, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoLoudspeaker},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x06, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoHeadphones},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x07, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoPortableAudio},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x08, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoCarAudio},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x09, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoSetTopBox},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x0A, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoHiFiAudioDevice},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x0B, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoVcr},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x0C, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoVideoCamera},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x0D, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoCamcorder},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x0E, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoVideoMonitor},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x0F, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoVideoDisplayAndLoudspeaker},
                // (Reserved)
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x10, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoVideoConferencing},
                // (Reserved)
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.AudioVideo, MinorFieldByte = 0x12, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.AudioVideoGamingToy},
                
                // ** Peripheral 
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x00, NumBits = 2, BitOffset = 4, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Peripheral1NotKeyboardNotPointing},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x01, NumBits = 2, BitOffset = 4, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Peripheral1Keyboard},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x02, NumBits = 2, BitOffset = 4, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Peripheral1Pointing},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x03, NumBits = 2, BitOffset = 4, PrimarySecondary = 1, MinorList = MinorDeviceClassList.Peripheral1ComboKeyboardPointing},
                
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x00, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2Uncategorized},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x01, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2Joystick},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x02, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2Gamepad},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x03, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2RemoteControl},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x04, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2SensingDevice},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x05, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2DigitizerTablet},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x06, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2CardReader},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x07, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2DigitalPen},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x08, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2HandheldScanner},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Peripheral, MinorFieldByte = 0x09, NumBits = 4, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.Peripheral2HandheldGesturalInput},
            
                // ** Imaging
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Imaging, MinorFieldByte = 0x01, NumBits = 4, BitOffset = 2, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ImagingDisplay},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Imaging, MinorFieldByte = 0x02, NumBits = 4, BitOffset = 2, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ImagingCamera},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Imaging, MinorFieldByte = 0x04, NumBits = 4, BitOffset = 2, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ImagingScanner},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Imaging, MinorFieldByte = 0x08, NumBits = 4, BitOffset = 2, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ImagingPrinter},
                
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Imaging, MinorFieldByte = 0x00, NumBits = 2, BitOffset = 0, PrimarySecondary = 2, MinorList = MinorDeviceClassList.ImagingUncategorized},

                // ** Wearable 
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Wearable, MinorFieldByte = 0x01, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.WearableWristwatch},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Wearable, MinorFieldByte = 0x02, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.WearablePager},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Wearable, MinorFieldByte = 0x03, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.WearableJacket},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Wearable, MinorFieldByte = 0x04, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.WearableHelmet},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Wearable, MinorFieldByte = 0x05, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.WearableGlasses},

                // ** Toy
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Toy, MinorFieldByte = 0x01, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ToyRobot},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Toy, MinorFieldByte = 0x02, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ToyVehicle},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Toy, MinorFieldByte = 0x03, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ToyDollActionFigure},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Toy, MinorFieldByte = 0x04, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ToyController},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Toy, MinorFieldByte = 0x05, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.ToyGame},

                // ** Health
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x01, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthUndefined},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x02, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthBloodPressureMonitor},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x03, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthThermometer},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x04, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthWeighingScale},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x05, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthGlucoseMeter},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x06, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthPulseOximeter},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x07, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthHeartPulseRateMonitor},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x08, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthHealthDataDisplay},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x09, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthStepCounter},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x0A, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthBodyCompositionAnalyzer},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x0B, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthPeakFlowMonitor},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x0C, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthMedicationMonitor},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x0D, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthKneeProsthesis},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x0E, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthAnkleProsthesis},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x0F, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthGenericHealthManager},
                new MinorDeviceClassType { MajorFieldByte = (uint)MajorDeviceClassTypes.Health, MinorFieldByte = 0x10, NumBits = 6, BitOffset = 0, PrimarySecondary = 1, MinorList = MinorDeviceClassList.HealthPersonalMobilityDevice},
            };

            #endregion


            #region Constructons and Parsing
            // --------------------------------------------------------------------------
            // Constructors & Parsing

            /// <summary>
            /// Create a new class of device instance and parse the supplied class of device data
            /// into the instance properties.
            /// </summary>
            /// <param name="codData">Class of device payload data to parse and to internalize
            /// for use in this class through properties.</param>
            public BtClassOfDevice(byte[] codData)
            {
                ParseClassOfDevice(codData);
            }

            /// <summary>
            /// Create an empty class of device instance.
            /// </summary>
            public BtClassOfDevice()
            {
                
            }

            /// <summary>
            /// Parse the supplied class of device data and set the properties of the current instance.
            /// </summary>
            /// <param name="codData">Class of device data to parse</param>
            private void ParseClassOfDevice(byte[] codData)
            {
                if (codData.Length != 3)
                {
                    throw new NdefException(NdefExceptionMessages.ExBtCodUnknownFormat);
                }
                // "s": Major service class
                // "M": major device class
                // "m": minor device class
                // "F": Format
                // ssssssss sssMMMMM mmmmmmFF
                var cod = (uint) ((codData[2] << 16)
                                   | (codData[1] << 8)
                                   | codData[0]);

                // Check format ("F", 2x bits 0-1, 00 = format #1)
                if ((cod & 0x03) == (uint)ClassOfDeviceFormatTypes.Format1)
                {
                    ClassOfDeviceFormat = ClassOfDeviceFormatTypes.Format1;

                    // Major service class ("s", 11x bits 13 - 23, bit mask)
                    MajorServiceClass = (cod >> 13) & 0x7ff;

                    // Major device class ("M", 5x bits 8-12)
                    MajorDeviceClass = (cod >> 8) & 0x1f;
                    //if (Enum.IsDefined(typeof(MajorDeviceClassTypes), MajorDeviceClass))
                    //{
                    //    var majorEnum = (MajorDeviceClassTypes)MajorDeviceClass;
                    //    //Debug.WriteLine("Major device class: " + majorEnum);
                    //}

                    // Minor device class ("m", 6x bits 2-7, variable, some eg only 5-7)
                    MinorDeviceClass = (cod >> 2) & 0x3f;
                    //var numMinorDeviceClasses = NumDeviceClassesForMajorClass((MajorDeviceClassTypes) MajorDeviceClass);
                    //for (var i = 1; i <= numMinorDeviceClasses; i++)
                    //{
                    //    Debug.WriteLine("Minor device class ({0}): {1}", i, MinorDeviceClassAsEnum(i));
                    //}
                }
                else
                {
                    Debug.WriteLine("BT NDEF record: Did not parse Class of device due to unknown format.");
                    throw new NdefException(NdefExceptionMessages.ExBtCodUnknownFormat);
                }

            }

            /// <summary>
            /// Convert the data stored in this class of device instance to a byte array, for use
            /// in the payload of the parent record.
            /// </summary>
            /// <returns>The class of device information as a byte array with 3 bytes.</returns>
            public byte[] ToByteArray()
            {
                // Format
                var assembled = (uint) ClassOfDeviceFormat;

                // Major service class
                assembled |= MajorServiceClass << 13;

                // Major device class
                assembled |= MajorDeviceClass << 8;

                // Minor device class
                assembled |= MinorDeviceClass << 2;
                
                // Combine everything into a 3 byte array
                var b = new byte[3];
                b[0] = (byte)assembled;
                b[1] = (byte)((assembled >> 8) & 0xFF);
                b[2] = (byte)((assembled >> 16) & 0xFF);
                return b;
            }
            #endregion
        }

        /// <summary>
        /// Utility class to handle the Bluetooth Service Classes defined in the OOB data
        /// of the Secure Simple Pairing Record.
        /// </summary>
        /// <remarks>
        /// The service class UUIDs are defined as per the Bluetooth specificiation:
        /// https://www.bluetooth.org/en-us/specification/assigned-numbers/service-discovery
        /// The values can either be defined as 16 / 32 / 128 bit values. Most common are
        /// 16 bit values, as these are currently enough to cover all standardized Bluetooth
        /// use cases.
        /// It is possible to create a higher-bit value from a lower bit value, e.g. to
        /// create a 128 bit value from a 128 bit value.
        /// From the Bluetooth Specification 4.1 [Vol 3], page 228, section 2.5.1
        /// 128_bit_value = 16_bit_value * 2^96 + Bluetooth_Base_UUID
        /// 128_bit_value = 32_bit_value * 2^96 + Bluetooth_Base_UUID
        /// A 16-bit UUID may be converted to 32-bit UUID format by zero-extending the
        /// 16-bit value to 32-bits. An equivalent method is to add the 16-bit UUID value to
        /// a zero-valued 32-bit UUID.
        /// </remarks>
        public class BtServiceClasses
        {
            /// <summary>
            ///  The Bluetooth Base UUID for 128 bits, defined as a string.
            /// </summary>
            private const string BtBaseUuid128 = "{0,8:X}-0000-1000-8000-00805F9B34FB";

            /// <summary>
            /// The Bluetooth base UUID for 128 bits, defined as a byte array.
            /// </summary>
            private static readonly byte[] BtUuid128 =
            {
                0x00, 0x00, 0x00, 0x00, // Service Class itself (2 or 4 bytes)
                0x00, 0x00, 0x10, 0x00,     
                0x80, 0x00, 0x00, 0x80,
                0x5F, 0x9B, 0x34, 0xFB
            };

            /// <summary>
            /// Contains the list of raw byte values specifying the individual service
            /// classes assigned. The number of bits for each contained byte array 
            /// corresponds to the service list type.
            /// </summary>
            public List<byte[]> ServiceClasses { get; set; }

            /// <summary>
            /// Return the currently set service class values as a list of enum values
            /// for easier use within your own application.
            /// </summary>
            public List<ServiceClassTypes> ServiceClassesAsTypes
            {
                get
                {
                    var scList = new List<ServiceClassTypes>();
                    foreach (var converted in ServiceClasses.Select(ConvertServiceClassToType))
                    {
                        if (converted != null) scList.Add((ServiceClassTypes)converted);
                    }
                    return scList;
                }
            }

            private OobDataTypes _dataType = OobDataTypes.CompleteList16BitServiceClassUuids;
            /// <summary>
            /// Defines the data structure of the service classes (complete / incomplete,
            /// 16, 32 or 128 bit per service class definition)
            /// </summary>
            /// <remarks>If you change this value, all the currently stored service classes
            /// will be automatically recreated based on the new bit length.</remarks>
            public OobDataTypes DataType
            {
                get { return _dataType; }
                set
                {
                    _dataType = value;
                    EnsureServiceClassesHaveCorrectByteLength();
                }
            }


            /// <summary>
            /// Contains a list of currently valid Bluetooth service classes according to the
            /// specification.
            /// </summary>
            /// <remarks>
            /// The enum defines the byte values that can be directly used for the service classes.
            /// For the complete list, see: 
            /// https://www.bluetooth.org/en-us/specification/assigned-numbers/service-discovery
            /// </remarks>
            public enum ServiceClassTypes : ushort
            {
                ServiceDiscoveryServerServiceClassID = 0x1000,
                BrowseGroupDescriptorServiceClassID = 0x1001,
                SerialPort = 0x1101,
                LANAccessUsingPPP = 0x1102,
                DialupNetworking = 0x1103,
                IrMCSync = 0x1104,
                OBEXObjectPush = 0x1105,
                OBEXFileTransfer = 0x1106,
                IrMCSyncCommand = 0x1107,
                Headset = 0x1108,
                CordlessTelephony = 0x1109,
                AudioSource = 0x110A,
                AudioSink = 0x110B,
                AV_RemoteControlTarget = 0x110C,
                AdvancedAudioDistribution = 0x110D,
                AV_RemoteControl = 0x110E,
                AV_RemoteControlController = 0x110F,
                Intercom = 0x1110,
                Fax = 0x1111,
                Headset_AudioGateway = 0x1112,
                WAP = 0x1113,
                WAP_CLIENT = 0x1114,
                PANU = 0x1115,
                NAP = 0x1116,
                GN = 0x1117,
                DirectPrinting = 0x1118,
                ReferencePrinting = 0x1119,
                BasicImagingProfile = 0x111A,
                ImagingResponder = 0x111B,
                ImagingAutomaticArchive = 0x111C,
                ImagingReferencedObjects = 0x111D,
                Handsfree = 0x111E,
                HandsfreeAudioGateway = 0x111F,
                DirectPrintingReferenceObjectsService = 0x1120,
                ReflectedUI = 0x1121,
                BasicPrinting = 0x1122,
                PrintingStatus = 0x1123,
                HumanInterfaceDeviceService = 0x1124,
                HardcopyCableReplacement = 0x1125,
                HCR_Print = 0x1126,
                HCR_Scan = 0x1127,
                Common_ISDN_Access = 0x1128,
                SIM_Access = 0x112D,
                PhonebookAccess_PCE = 0x112E,
                PhonebookAccess_PSE = 0x112F,
                PhonebookAccess = 0x1130,
                HeadsetHS = 0x1131,
                MessageAccessServer = 0x1132,
                MessageNotificationServer = 0x1133,
                MessageAccessProfile = 0x1134,
                GNSS = 0x1135,
                GNSS_Server = 0x1136,
                ThreeD_Display = 0x1137,
                ThreeD_Glasses = 0x1138,
                ThreeD_Synchronization = 0x1139,
                MPS_Profile_UUID = 0x113A,
                MPS_SC_UUID = 0x113B,
                PnPInformation = 0x1200,
                GenericNetworking = 0x1201,
                GenericFileTransfer = 0x1202,
                GenericAudio = 0x1203,
                GenericTelephony = 0x1204,
                UPNP_Service = 0x1205,
                UPNP_IP_Service = 0x1206,
                ESDP_UPNP_IP_PAN = 0x1300,
                ESDP_UPNP_IP_LAP = 0x1301,
                ESDP_UPNP_L2CAP = 0x1302,
                VideoSource = 0x1303,
                VideoSink = 0x1304,
                VideoDistribution = 0x1305,
                HDP = 0x1400,
                HDPSource = 0x1401,
                HDPSink = 0x1402
            }

            /// <summary>
            /// Format the specified service class type enum value as a string that represents
            /// the 128 bit format conversion.
            /// </summary>
            /// <param name="serviceClassType">The service class type to represent as a string.</param>
            /// <returns>Service class type converted to a string that contains the 128 bit service class
            /// UUID.</returns>
            public static string UuidAs128BitString(ServiceClassTypes serviceClassType)
            {
                return string.Format(BtBaseUuid128, serviceClassType);
            }

            /// <summary>
            /// Convert the specified service class string to a service class type enum value.
            /// </summary>
            /// <remarks>Only works for UUIDs that are based on the Bluetooth standard base UUID
            /// where only the first 16 / 32 bit are customized. The string needs to correspond
            /// to the schema: "xxxx-0000-1000-8000-00805f9b34fb", where xxxx is the 32-bit value
            /// used for the service class.</remarks>
            /// <param name="uuid">String-based UUID to parse</param>
            /// <returns>The converted service class type.</returns>
            public static ServiceClassTypes? UuidFrom128String(string uuid)
            {
                var pattern = new Regex(@"(<uuid>[0-9a-fA-F]{8})-0000-1000-8000-00805f9b34fb");
                var match = pattern.Match(uuid, 0);
                var uuidComponent = match.Groups["uuid"].Value;
                var uuidValue = Convert.ToInt32(uuidComponent, 16);
                return Enum.IsDefined(typeof (ServiceClassTypes), uuidValue) ? (ServiceClassTypes) uuidValue : (ServiceClassTypes?) null;
            }

            /// <summary>
            /// How many bit are used for each service class according to the current DataType setting
            /// in this instance.
            /// </summary>
            /// <returns>Number of bits used for each service class - 16, 32 or 128.</returns>
            public int BitsPerServiceClass()
            {
                return BytesPerServiceClass(DataType) * 8;
            }

            /// <summary>
            /// How many bytes are used for each service class according to the current DataType setting
            /// in this instance.
            /// </summary>
            /// <returns>Number of bytes used for each service class - 2, 4 or 16.</returns>
            public int BytesPerServiceClass()
            {
                return BytesPerServiceClass(DataType);
            }

            /// <summary>
            /// Determine the number of bytes per service class, according to the service class type
            /// supplied as a parameter.
            /// </summary>
            /// <param name="serviceClassType">Service class type to return the number of bytes for.</param>
            /// <returns>Number of bytes that each service class needs to have to correspond to the
            /// specified service class type.</returns>
            private static int BytesPerServiceClass(OobDataTypes serviceClassType)
            {
                if (serviceClassType == OobDataTypes.CompleteList16BitServiceClassUuids ||
                    serviceClassType == OobDataTypes.IncompleteList16BitServiceClassUuids)
                    return 2;
                if (serviceClassType == OobDataTypes.CompleteList32BitServiceClassUuids ||
                    serviceClassType == OobDataTypes.IncompleteList32BitServiceClassUuids)
                    return 4;
                if (serviceClassType == OobDataTypes.CompleteList128BitServiceClassUuids ||
                    serviceClassType == OobDataTypes.IncompleteList128BitServiceClassUuids)
                    return 16; 
                return -1;
            }

            /// <summary>
            /// Utility method to check all currently stored service class UUIDs for their bit length and
            /// correct if necessary.
            /// </summary>
            /// <remarks>If a service class has too little / many bytes used based on the definition
            /// of this class, replace it with a new byte array that has the correct number of bytes.</remarks>
            private void EnsureServiceClassesHaveCorrectByteLength()
            {
                if (ServiceClasses == null) return;
                for (var i = 0; i < ServiceClasses.Count; i++)
                {
                    if (ServiceClasses[i].Length != BytesPerServiceClass())
                    {
                        // Have to recreate this one with a new bit/byte length!
                        var curType = ConvertServiceClassToType(ServiceClasses[i]);
                        if (curType != null)
                        {
                            ServiceClasses[i] = ConvertServiceClassToByte((ServiceClassTypes)curType, DataType);
                        }
                    }
                }
            }


            /// <summary>
            /// Add a new service class to the list, specified as an enum value instead of a raw byte array.
            /// The method will take care of using the correct bit length to store the UUID, according to the
            /// setting in use for this instance.
            /// </summary>
            /// <param name="newSc">New service class type to add.</param>
            public void AddServiceClassType(ServiceClassTypes newSc)
            {
                if (ServiceClasses == null)
                    ServiceClasses = new List<byte[]>();
                ServiceClasses.Add(ConvertServiceClassToByte(newSc, DataType));
            }

            /// <summary>
            /// Convert the specified service class byte array to the enum type for more convenient use in your app.
            /// </summary>
            /// <param name="scBytes">Byte array that contains the service class. Has to have a length of 2, 4 or 16 bytes.</param>
            /// <returns>The service class byte array converted the the enum value definition.</returns>
            /// <exception cref="NdefException">Thrown if the supplied byte array does not have a valid length.</exception>
            public static ServiceClassTypes? ConvertServiceClassToType(byte[] scBytes)
            {
                var scNumBytes = scBytes.Length;
                if (!(scNumBytes == 2 || scNumBytes == 4 || scNumBytes == 16))
                {
                    throw new NdefException(NdefExceptionMessages.ExBtNoValidServiceClassLength);
                }
                ushort scAsNum = 0;
                switch (scNumBytes)
                {
                    case 4:
                    case 2:
                        scAsNum = (ushort)((scBytes[1] << 8) | scBytes[0]);
                        break;
                    case 16:
                        scAsNum = (ushort)((scBytes[2] << 8) | scBytes[3]);
                        break;
                }
                return Enum.IsDefined(typeof(ServiceClassTypes), scAsNum) ? (ServiceClassTypes)scAsNum : (ServiceClassTypes?)null;
            }

            /// <summary>
            /// Convert the specified service class enum value to a byte array, based on the specified OOB datatype
            /// which influences the bit length.
            /// </summary>
            /// <param name="newSc">Service class enum to convert to a byte array.</param>
            /// <param name="dataType">Data type to use for the conversion - defines the byte length of the resulting
            /// byte array (2 / 4 / 16 bytes)</param>
            /// <returns>The service class enum value converted to a byte array with the specified length.</returns>
            private static byte[] ConvertServiceClassToByte(ServiceClassTypes newSc, OobDataTypes dataType)
            {
                return ConvertServiceClassToByte(newSc, BytesPerServiceClass(dataType));
            }

            /// <summary>
            /// Convert the specified service class enum value to a byte array, based on the specified byte length.
            /// </summary>
            /// <param name="newSc">Service class enum to convert to a byte array.</param>
            /// <param name="numBytes">How many bytes to use for the conversion of the service class (2 / 4 / 16 bytes)</param>
            /// <returns>The service class enum value converted to a byte array with the specified length.</returns>
            private static byte[] ConvertServiceClassToByte(ServiceClassTypes newSc, int numBytes)
            {
                if (!(numBytes == 2 || numBytes == 4 || numBytes == 16))
                {
                    return null;
                }
                var scByte = new byte[numBytes];
                var scOrig = (ushort) newSc;
                switch (numBytes)
                {
                    case 2:
                        scByte[0] = (byte)newSc;
                        scByte[1] = (byte)((scOrig >> 8) & 0xFF);
                        break;
                    case 4:
                        scByte[0] = (byte)newSc;
                        scByte[1] = (byte)((scOrig >> 8) & 0xFF);
                        scByte[2] = 0;
                        scByte[3] = 0;
                        break;
                    case 16:
                        Array.Copy(BtUuid128, scByte, 16);
                        scByte[0] = 0;
                        scByte[1] = 0;
                        scByte[2] = (byte)newSc;
                        scByte[3] = (byte)((scOrig >> 8) & 0xFF);
                        break;
                }
                return scByte;
            }

            #region Constructons and Parsing
            // --------------------------------------------------------------------------
            // Constructors & Parsing

            /// <summary>
            /// Create a new instance of the Bluetooth Service Class and parse the
            /// supplied service class type (defines the UUID lengths) and data (contains
            /// the actual service class UUIDs).
            /// </summary>
            /// <param name="scType">Specifies the service class type, which has to be
            /// defined in the OobDataTypes enum and defines the number of bits used
            /// for each service class UUID (16 / 32 / 128 bits)</param>
            /// <param name="scData">Byte array continaing one or more service class UUIDs,
            /// each in the bit length specified by the scType parameter.</param>
            public BtServiceClasses(byte scType, byte[] scData)
            {
                ParseServiceClass(scType, scData);
            }

            /// <summary>
            /// Create an empty instance of the Bluetooth Service Class helper.
            /// </summary>
            public BtServiceClasses()
            {
                
            }

            /// <summary>
            /// Parse the supplied service class information and internalize it into the
            /// properties of this class instance - the DataType and the ServiceClasses list.
            /// </summary>
            /// <param name="scType">Specifies the service class type, which has to be
            /// defined in the OobDataTypes enum and defines the number of bits used
            /// for each service class UUID (16 / 32 / 128 bits)</param>
            /// <param name="scData">Byte array continaing one or more service class UUIDs,
            /// each in the bit length specified by the scType parameter.</param>
            private void ParseServiceClass(byte scType, byte[] scData)
            {
                // Check service class type
                if (!Enum.IsDefined(typeof (OobDataTypes), scType))
                {
                    throw new NdefException(NdefExceptionMessages.ExBtNoValidServiceClassId);
                }
                var scTypeConv = (OobDataTypes) scType;
                var bytesPerClass = BytesPerServiceClass(scTypeConv);
                if (bytesPerClass <= 0)
                {
                    throw new NdefException(NdefExceptionMessages.ExBtNoValidServiceClassId);
                }
                DataType = scTypeConv;

                // Convert values
                var numClasses = scData.Length / bytesPerClass;
                ServiceClasses = new List<byte[]>(numClasses);
                for (var i = 0; i < scData.Length; i += bytesPerClass)
                {
                    var curSc = new byte[bytesPerClass];
                    Array.Copy(scData, i, curSc, 0, bytesPerClass);
                    ServiceClasses.Add(curSc);
                }
            }

            /// <summary>
            /// Convert the properties currently stored in this class instance to a byte array,
            /// which can then be used to embed this information into OOB data payload of the parent
            /// Bluetooth Secure Simple Pairing record.
            /// </summary>
            /// <returns>The service class list converted to a byte array. The actual size of the byte
            /// array depends on the number of service classes set, as well as the number of bits used
            /// for each service class.</returns>
            public byte[] ToByteArray()
            {
                if (ServiceClasses == null) return new byte[0];

                var bytesPerClass = BytesPerServiceClass();

                var b = new byte[bytesPerClass * ServiceClasses.Count];
                for (var i = 0; i < ServiceClasses.Count; i++)
                {
                    Array.Copy(ServiceClasses[i], 0, b, i * bytesPerClass, bytesPerClass);
                }
                return b;
            }
            #endregion

        }
        #endregion
    }
}
