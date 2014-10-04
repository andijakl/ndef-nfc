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
    /// Convenience class for creating records that open the email
    /// app on the user's device, with a specified recipient address,
    /// subject and body text. 
    /// </summary>
    /// <remarks>
    /// Implements the mailto URI scheme, which is also widely known from 
    /// links on web pages. Most devices should react in the same way
    /// if they encounter this URI scheme from an NFC tag.
    /// 
    /// The record should always have the recipient address defined,
    /// which needs to be a valid email address. Optionally, you can also
    /// add a subject and body text.
    /// 
    /// The reading device will usually not send the email directly, but
    /// usually opens the user's email app with the contents you specified
    /// pre-defined. In general, the user can then still modify the contents
    /// and send the email.
    /// 
    /// This class takes care of correctly escaping the subject and body text,
    /// so that they form valid URLs.
    /// 
    /// The mailto: URI scheme is defined here:
    /// http://tools.ietf.org/html/rfc2368
    /// (this record only implements a subset of the complete URI scheme)
    /// </remarks>
    public class NdefMailtoRecord : NdefSmartUriRecord
    {
        /// <summary>
        /// URI scheme in use for this record.
        /// </summary>
        private const string MailtoScheme = "mailto:";

        private string _address;
        private string _subject;
        private string _body;

        /// <summary>
        /// Email address of the recipient.
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = value; UpdatePayload(); }
        }

        /// <summary>
        /// Optional pre-written subject to use for the email.
        /// </summary>
        /// <remarks>The property string should not be URL escaped.
        /// Escaping will be applied directly when creating the URI
        /// through System.Uri.EscapeDataString().</remarks>
        public string Subject
        {
            get { return _subject; }
            set { _subject = value; UpdatePayload(); }
        }

        /// <summary>
        /// Optional pre-written body to use for the email.
        /// </summary>
        /// <remarks>The property string should not be URL escaped.
        /// Escaping will be applied directly when creating the URI
        /// through System.Uri.EscapeDataString().</remarks>
        public string Body
        {
            get { return _body; }
            set { _body = value; UpdatePayload(); }
        }

        /// <summary>
        /// Create an empty Mailto record.
        /// </summary>
        public NdefMailtoRecord()
        {
        }

        /// <summary>
        /// Create a Mailto record based on another Mailto record, or a Smart Poster / URI
        /// record that has a Uri set that corresponds to the mailto: URI scheme.
        /// </summary>
        /// <param name="other">Other record to copy the data from.</param>
        public NdefMailtoRecord(NdefRecord other)
            : base(other)
        {
            ParseUriToData(Uri);
        }

        /// <summary>
        /// Deletes any details currently stored in the mailto record 
        /// and re-initializes them by parsing the contents of the provided URI.
        /// </summary>
        /// <remarks>The URI has to be formatted according to the mailto: URI scheme.</remarks>
        private void ParseUriToData(string uri)
        {
            var mailUri = new Uri(uri);

            // Check if scheme is right (mailUri.Scheme == scheme without : at the end)
            if (mailUri.Scheme != MailtoScheme.Substring(0, MailtoScheme.Length - 1))
                return;

            // Email address
            var questionmarkPos = uri.IndexOf('?');
            if (questionmarkPos >= MailtoScheme.Length)
            {
                // Found "?" - query parameters are present
                _address =
                    System.Uri.UnescapeDataString(uri.Substring(MailtoScheme.Length,
                                                                questionmarkPos - MailtoScheme.Length));
            } else if (questionmarkPos == -1)
            {
                // No ? - use complete text as address, no query parameters present
                _address = System.Uri.UnescapeDataString(uri.Substring(MailtoScheme.Length,
                                                                       uri.Length - MailtoScheme.Length));
            }

            // Query parameters
            var queryParameters = ParseQueryString(mailUri.Query);
            if (queryParameters.ContainsKey("subject"))
                _subject = System.Uri.UnescapeDataString(queryParameters["subject"]);
            if (queryParameters.ContainsKey("body"))
                _body = System.Uri.UnescapeDataString(queryParameters["body"]);

            UpdatePayload();
        }

        /// <summary>
        /// Split the query into a dictionary, containing the query keys and values.
        /// </summary>
        /// <param name="queryString">Query string to parse</param>
        /// <returns>Dictionary of parameters (key) and its values.</returns>
        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var queryParameters = new Dictionary<string, string>();
            var querySegments = queryString.Split('&');
            foreach (var segment in querySegments)
            {
                var parts = segment.Split('=');
                if (parts.Length > 1)
                {
                    var key = parts[0].Trim(new[] { '?', ' ' });
                    var val = parts[1].Trim();

                    queryParameters.Add(key, val);
                }
            }
            return queryParameters;
        }

        /// <summary>
        /// Format the URI of the SmartUri base class.
        /// </summary>
        private void UpdatePayload()
        {
            // UriBuilder adds / after host (= email address), and
            // adding another query would manually require to add & if needed anyway.
            // -> not using it here.
            var builder = new StringBuilder(MailtoScheme);
            if (!String.IsNullOrEmpty(Address)) builder.Append(Address);
            if (!String.IsNullOrEmpty(Subject)) AddQuery(builder, "subject=" + System.Uri.EscapeDataString(Subject), true);
            if (!String.IsNullOrEmpty(Body)) AddQuery(builder, "body=" + System.Uri.EscapeDataString(Body), String.IsNullOrEmpty(Subject));

            Uri = builder.ToString();
        }

        private static void AddQuery(StringBuilder builder, string argument, bool firstArgument)
        {
            builder.Append(firstArgument ? "?" : "&");
            builder.Append(argument);
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Mailto record.
        /// Checks the type and type name format and if the URI starts with the correct scheme.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type, type name format and payload
        /// to be a Mailto record, false if it's a different record.</returns>
        public new static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            if (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Payload != null)
            {
                if (record.Type.SequenceEqual(NdefUriRecord.UriType))
                {
                    var testRecord = new NdefUriRecord(record);
                    return testRecord.Uri.StartsWith(MailtoScheme);
                }
                if (record.Type.SequenceEqual(SmartPosterType))
                {
                    var testRecord = new NdefSpRecord(record);
                    return testRecord.Uri.StartsWith(MailtoScheme);
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
            if (string.IsNullOrEmpty(Address))
            {
                throw new NdefException(NdefExceptionMessages.ExMailtoAddressEmpty);
            }
            // Check if recipient address appears to be valid
            var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,6})+)$");
            var match = regex.Match(Address);
            if (!match.Success)
                throw new NdefException(NdefExceptionMessages.ExMailtoAddressNotValid);
            return true;
        }

    }
}
