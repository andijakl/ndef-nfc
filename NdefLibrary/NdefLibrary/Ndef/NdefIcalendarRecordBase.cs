/****************************************************************************
**
** Copyright (C) 2012-2014 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
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
    public class NdefIcalendarRecordBase : NdefRecord
    {
        /// <summary>
        /// Type of the NDEF MIME / iCal record.
        /// </summary>
        public static readonly byte[] IcalType = Encoding.UTF8.GetBytes("text/calendar");
        
        /// <summary>
        /// Create an empty MIME/iCal Record.
        /// </summary>
        public NdefIcalendarRecordBase()
            : base(TypeNameFormatType.Mime, IcalType)
        {
        }

        /// <summary>
        /// Create a MIME/iCal record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a MIME/iCal Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this iCal record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a iCal record
        /// based on an incompatible record type.</exception>
        protected NdefIcalendarRecordBase(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a MIME/iCal record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a MIME/iCal record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.Mime && record.Type.SequenceEqual(IcalType));
        }
    }
}
