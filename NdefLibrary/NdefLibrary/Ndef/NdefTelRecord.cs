/****************************************************************************
**
** Copyright (C) 2012-2013 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
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
using System.Text.RegularExpressions;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Convenience class for formatting telephone call information into
    /// an NDEF record, depending on added info either URI or Smart Poster.
    /// </summary>
    /// <remarks>
    /// Tapping a tag with telephone information stored on it should trigger
    /// a dialog in the phone to call the specified number. This can for
    /// example be used to get in touch with customer service or support,
    /// or to book a hotel.
    /// 
    /// To create and write the record, specify the target phone number
    /// (in international format). This class will take care of properly
    /// encoding the information.
    /// 
    /// As this class is based on the Smart URI base class, the
    /// payload is formatted as a URI record initially. When first
    /// adding Smart Poster information (like a title), the payload
    /// instantly transforms into a Smart Poster.
    /// </remarks>
    public class NdefTelRecord : NdefSmartUriRecord
    {
        /// <summary>
        /// URI scheme in use for this record.
        /// </summary>
        private const string TelScheme = "tel:";

        private string _telNumber;
        
        /// <summary>
        /// The number the reading phone is supposed to call.
        /// Recommended to store in international format, e.g., +431234...
        /// </summary>
        public string TelNumber
        {
            get { return _telNumber; }
            set
            {
                _telNumber = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Create an empty telephone record. You need to set the number 
        /// to create a URI and make this record valid.
        /// </summary>
        public NdefTelRecord()
        {
        }

        /// <summary>
        /// Create a telephone record based on another telephone record, or Smart Poster / URI
        /// record that have a Uri set that corresponds to the tel: URI scheme.
        /// </summary>
        /// <param name="other">Other record to copy the data from.</param>
        public NdefTelRecord(NdefRecord other)
            : base(other)
        {
            ParseUriToData(Uri);
        }

        /// <summary>
        /// Deletes any details currently stored in the telephone record 
        /// and re-initializes them by parsing the contents of the provided URI.
        /// </summary>
        /// <remarks>The URI has to be formatted according to the tel: URI scheme,
        /// and include the number.</remarks>
        private void ParseUriToData(string uri)
        {
            // Extract product name and serial number from the payload
            var pattern = new Regex(@"tel:(?<telNumber>.*)");
            var match = pattern.Match(uri);
            // Assign extracted data to member variables
            _telNumber = match.Groups["telNumber"].Value;
            UpdatePayload();
        }

        /// <summary>
        /// Format the URI of the SmartUri base class.
        /// </summary>
        private void UpdatePayload()
        {
            if (TelNumber != null)
                Uri = TelScheme + TelNumber;
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Telephone record.
        /// Checks the type and type name format and if the URI starts with the correct scheme.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type, type name format and payload
        /// to be a Telephone record, false if it's a different record.</returns>
        public new static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            if (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Payload != null)
            {
                if (record.Type.SequenceEqual(NdefUriRecord.UriType))
                {
                    var testRecord = new NdefUriRecord(record);
                    return testRecord.Uri.StartsWith(TelScheme);
                }
                if (record.Type.SequenceEqual(SmartPosterType))
                {
                    var testRecord = new NdefSpRecord(record);
                    return testRecord.Uri.StartsWith(TelScheme);
                }
            }
            return false;
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
            if (string.IsNullOrEmpty(TelNumber))
            {
                throw new NdefException(NdefExceptionMessages.ExTelNumberEmpty);
            }
            return true;
        }
    }
}
