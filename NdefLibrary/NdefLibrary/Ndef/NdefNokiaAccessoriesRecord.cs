/****************************************************************************
**
** Copyright (C) 2012-2013 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
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
using System.Text.RegularExpressions;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// URI NDEF Record with the custom nokia-accessories URI scheme, as used for example
    /// in the Nokia Wireless Charging Stand.
    /// </summary>
    /// <remarks>
    /// Custom URI schemes can be associated with custom apps on Windows Phone.
    /// On Nokia phones, a pre-installed default handler for the nokia-accessories
    /// URI protocol exists. The protocol is used for example in the Nokia Wireless Charging
    /// stand.
    /// 
    /// This record class is a specialization of the generic well-known URI NDEF record,
    /// and formats the correct URI string, based on a provided product name and serial
    /// number. See the documentation of the properties for details on the format of those.
    /// </remarks>
    public class NdefNokiaAccessoriesRecord : NdefUriRecord
    {
        private const string NokiaAccessoriesUriFormat = "nokia-accessories:s?p={0};sn={1}";
        private string _productName;
        private string _serialNumber;

        public static readonly Dictionary<string, string> NokiaAccessoriesNamePresets = new Dictionary<string, string> {
            { "dt910", "DT-910 Wireless Charging Stand" },
            { "cr200", "CR-200 Wireless Charging Car Holder" }
        };

        /// <summary>
        /// Create an empty Nokia Accessories record.
        /// </summary>
        public NdefNokiaAccessoriesRecord()
        {
        }

        /// <summary>
        /// Create a Nokia Accessories record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a Nokia Accessories Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this Text record.</param>
        public NdefNokiaAccessoriesRecord(NdefRecord other)
            : base(other)
        {
            ParsePayloadToData(_payload);
        }

        /// <summary>
        /// Deletes any details currently stored in the Nokia Accessories record 
        /// and re-initializes them by parsing the contents of the payload.
        /// </summary>
        private void ParsePayloadToData(byte[] payload)
        {
            // Extract product name and serial number from the payload
            var pattern = new Regex(@"nokia-accessories:s\?p=(?<productName>.*);sn=(?<serialNumber>\d+)");
            var match = pattern.Match(Encoding.UTF8.GetString(payload, 0, payload.Length));
            // Assign extracted data to member variables
            _productName = match.Groups["productName"].Value;
            _serialNumber = match.Groups["serialNumber"].Value;
            UpdatePayload();
        }

        /// <summary>
        /// Product name, e.g., dt910 (= Nokia Wireless Charging Stand)
        /// </summary>
        public string ProductName
        {
            get { return _productName; }
            set
            {
                _productName = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Serial number of accessory.
        /// 19 chars, all numbers [0-9]
        /// </summary>
        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                _serialNumber = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Format the URI of the record.
        /// </summary>
        private void UpdatePayload()
        {
            if (ProductName != null && SerialNumber != null)
            {
                Uri = String.Format(NokiaAccessoriesUriFormat, ProductName, SerialNumber);
            }
        }

        /// <summary>
        /// Check if the specified URI is a valid payload for a Nokia Accessories record.
        /// </summary>
        /// <param name="uriToCheck">URI to check for compliance to record specifications.</param>
        /// <returns>True if the URi can be parsed into a Nokia Accessories record.</returns>
        private static bool CheckIsValidUri(string uriToCheck)
        {
            var pattern = new Regex(@"nokia-accessories:s\?p=(.*?);sn=(\d{19})");
            return pattern.IsMatch(uriToCheck);
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Nokia Accessories record.
        /// Checks the type and type name format as well as if the payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type, type name format and payload
        /// to be a Nokia Accessories record, false if it's a different record.</returns>
        public new static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Type.SequenceEqual(UriType) &&
                   record.Payload != null &&
                   CheckIsValidUri(Encoding.UTF8.GetString(record.Payload, 0, record.Payload.Length));
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
            if (string.IsNullOrEmpty(ProductName))
            {
                throw new NdefException(NdefExceptionMessages.ExNokiaAccessoriesProductEmpty);
            }
            if (string.IsNullOrEmpty(SerialNumber))
            {
                throw new NdefException(NdefExceptionMessages.ExNokiaAccessoriesSerialEmpty);
            }
            if (!CheckIsValidUri(Uri))
            {
                throw new NdefException(NdefExceptionMessages.ExNokiaAccessoriesSerialFormat);
            }
            return true;
        }
    }
}
