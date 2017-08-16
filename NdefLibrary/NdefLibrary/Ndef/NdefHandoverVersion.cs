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
using System.Globalization;

namespace NdefLibrary.Ndef
{

    /// <summary>
    /// Utility class that handles the correct format for storing version information
    /// in Handover records. The handover version contains a major and minor version.
    /// This implementation conforms to the specification with major version 1 and minor
    /// version 3.
    /// </summary>
    public class NdefHandoverVersion
    {
        private byte _version;
        private ushort _major = 0x01;
        private ushort _minor = 0x03;

        /// <summary>
        /// 4-bit field is the major version of the Connection Handover specification and
        /// SHALL be set to 1 by an implementation that conforms to this specification.
        /// </summary>
        /// <remarks>
        /// When an NDEF parser reads a different value, it SHALL NOT assume backwards
        /// compatibility. Changing this property automatically also updates the combined
        /// Version property.
        /// </remarks>
        public ushort Major
        {
            get { return _major; }
            set
            {
                if (_major == value) return;
                _major = value;
                UpdateVersion();
            }
        }

        /// <summary>
        /// 4-bit field is the minor version of the Connection Handover specification and
        /// SHALL be set to 3 by an implementation that conforms to this specification.
        /// </summary>
        /// <remarks>
        /// When an NDEF parser reads a different value, it SHALL NOT assume backwards
        /// compatibility. Changing this property automatically also updates the combined
        /// Version property.
        /// </remarks>
        public ushort Minor
        {
            get { return _minor; }
            set
            {
                if (_minor == value) return;
                _minor = value;
                UpdateVersion();
            }
        }

        /// <summary>
        /// The version as a byte that contains 4 bits for the major version and 4 bit
        /// for the minor version.
        /// </summary>
        /// <remarks>
        /// Changing this property automatically also udpates the major and minor accrssor
        /// properties.
        /// </remarks>
        public byte Version
        {
            get { return _version; }
            set
            {
                if (_version == value) return;
                _version = value;
                UpdateMajorMinor();
            }
        }


        /// <summary>
        /// Create a new handover version utility class instance, which can format
        /// the major and minor version according to the Handover specification 1.3.
        /// </summary>
        public NdefHandoverVersion()
        {
        }

        /// <summary>
        /// Create a new handover version utility class instance, which can format
        /// the major and minor version according to the Handover specification 1.3.
        /// </summary>
        /// <param name="combinedVersion">The field to use for the version, contains
        /// 4 bits for minor and 4 bits for the major version.</param>
        public NdefHandoverVersion(byte combinedVersion)
        {
            Version = combinedVersion;
        }


        /// <summary>
        /// Create a new handover version utility class instance, which can format
        /// the major and minor version according to the Handover specification 1.3.
        /// </summary>
        /// <param name="version">Parse the float version into a major and minor 
        /// component.</param>
        public NdefHandoverVersion(float version)
        {
            Major = (ushort)version;
            var minorSplit = version.ToString(CultureInfo.InvariantCulture).Split(new[] { NumberFormatInfo.InvariantInfo.NumberDecimalSeparator }, StringSplitOptions.None);
            Minor = minorSplit.Length > 1 ? Convert.ToUInt16(minorSplit[1]) : (ushort)0;
        }


        /// <summary>
        /// Update the major and minor numbers based on the version.
        /// </summary>
        private void UpdateMajorMinor()
        {
            Major = (ushort)(Version >> 4);
            Minor = (ushort)(Version & 0x0F);
        }

        /// <summary>
        /// Update the version byte based on the major and minor numbers.
        /// </summary>
        private void UpdateVersion()
        {
            Version = (byte)((Major << 4) | (Minor & 0x0F));
        }

    }
}
