/****************************************************************************
**
** Copyright (C) 2012-2014 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2013).
** More information: http://ndef.mopius.com/
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
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Stores a business card (vCard) in a MIME type record, allowing users
    /// to immediately add a contact to their address book.
    /// </summary>
    /// <remarks>
    /// This base class provides the general record layout as well as the functionality
    /// to store the vcf format as a payload with the correct NDEF record type.
    /// To interface with platform specific contact classes in order to directly
    /// import / export a contact from the address book, use the derived classes
    /// in the NDEF Extension Library.
    /// </remarks>
    public class NdefVcardRecordBase : NdefRecord
    {
        /// <summary>
        /// Type of the NDEF MIME / vCard record.
        /// </summary>
        public static readonly byte[] VcardType = Encoding.UTF8.GetBytes("text/x-vCard");
        
        /// <summary>
        /// Create an empty MIME/vCard Record.
        /// </summary>
        public NdefVcardRecordBase()
            : base(TypeNameFormatType.Mime, VcardType)
        {
        }

        /// <summary>
        /// Create a MIME/vCard record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a MIME/vCard Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this vCard record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a vCard record
        /// based on an incompatible record type.</exception>
        public NdefVcardRecordBase(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a MIME/vCard record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a MIME/vCard record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.Mime && record.Type.SequenceEqual(VcardType));
        }
    }
}
