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

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Smart class that uses the smallest possible NDEF record type
    /// for storing the requested information - either a simple URI record,
    /// or a Smart Poster record.
    /// 
    /// At construction and when only setting the URI, this class will stay a
    /// URI record. As soon as you set a Smart Poster feature, the payload and
    /// type will transform into a Smart Poster.
    /// </summary>
    public class NdefSmartUriRecord : NdefSpRecord
    {
        /// <summary>
        /// Create an empty Smart Uri Record.
        /// </summary>
        public NdefSmartUriRecord()
        {
        }

        /// <summary>
        /// Create a Smart Uri record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record can be a Smart Poster or a URI record.
        /// </remarks>
        /// <param name="other">Record to copy into this SmartUri record.</param>
        public NdefSmartUriRecord(NdefRecord other) : base(other)
        {
            // Type compatibility checks done in base class
        }
        
        /// <summary>
        /// An identifier describing the type of the payload.
        /// Override to return correct type depending on data stored - either
        /// U(RI) or Sp (Smart Poster).
        /// </summary>
        public override byte[] Type
        {
            get { return HasSpData() ? base.Type : RecordUri.Type; }
        }

        /// <summary>
        /// The application data carried within an NDEF record.
        /// </summary>
        /// <remarks>Override because this can either be the whole
        /// Smart Poster payload, or just the URI record payload.</remarks>
        public override byte[] Payload
        {
            get { return HasSpData() ? base.Payload : RecordUri.Payload; }
        }

    }
}
