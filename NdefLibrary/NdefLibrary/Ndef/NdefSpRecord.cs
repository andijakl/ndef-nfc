/****************************************************************************
**
** Copyright (C) 2012-2014 Andreas Jakl, Mopius - http://www.mopius.com/
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Handles the Smart Poster meta-record and is able to
    /// both parse and assemble a smart poster according to the specification.
    /// </summary>
    /// <remarks>
    /// The class is able to handle the mandatory URI, any number of
    /// text records (for storing the Smart Poster title in multiple
    /// languages), the action, size, type and image.
    ///
    /// As only the URI is mandatory, various ...InUse() methods can
    /// be queried to see if an optional detail was set in the Smart
    /// Poster that was read from a tag.
    ///
    /// The action, size and type records used within the Smart Poster
    /// are only valid within its context according to the specification.
    /// Therefore, classes for those record types are defined within the
    /// context of the Smart Poster record class.
    ///
    /// Due to the more complex nature of the Smart Poster record, which
    /// consists of multiple records, this class parses the payload
    /// contents and creates instances of the various record classes
    /// found within the Smart Poster. However, any changes to details
    /// are instantly commited to the raw payload as well.
    /// </remarks>
    public class NdefSpRecord : NdefRecord
    {
        /// <summary>
        /// Type of the NDEF Smart Poster record (well-known, type 'Sp').
        /// </summary>
        public static readonly byte[] SmartPosterType = { (byte)'S', (byte)'p' };

        /// <summary>
        /// Create an empty Smart Poster (Sp) record.
        /// </summary>
        /// <remarks>
        /// Note that in order to write a Smart Poster to a tag,
        /// you have to at least add the URL. An empty Smart Poster
        /// record is not valid according to the specs.
        /// </remarks>
        public NdefSpRecord()
            : base(TypeNameFormatType.NfcRtd, SmartPosterType)
        {
        }

        /// <summary>
        /// Create a Smart Poster record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record can be a Smart Poster or a URI record.
        /// </remarks>
        /// <param name="other">Record to copy into this smart poster record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a Smart Poster
        /// based on an incompatible record type.</exception>
        public NdefSpRecord(NdefRecord other)
            : base(other)
        {
            if (TypeNameFormat != TypeNameFormatType.NfcRtd)
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
            if (_type.SequenceEqual(SmartPosterType))
            {
                // Other record was a Smart Poster, so parse and internalize the Sp payload
                ParsePayloadToData(_payload);
            }
            else if (_type.SequenceEqual(NdefUriRecord.UriType))
            {
                // Create Smart Poster based on URI record
                RecordUri = new NdefUriRecord(other) {Id = null};
                // Set type of this instance to Smart Poster
                _type = new byte[SmartPosterType.Length];
                Array.Copy(SmartPosterType, _type, SmartPosterType.Length);
            }
            else
            {
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
            }

        }

        /// <summary>
        /// (Re)set all the stored sub records of the Smart Poster.
        /// </summary>
        private void InitializeData()
        {
            RecordUri = null;
            Titles = null;
            _recordAction = null;
            _recordSize = null;
            _recordMimeType = null;
            _recordImage = null;
        }

        /// <summary>
        /// Sets the payload of the base class to the byte array from 
        /// the parameter and optionally parses its contents.
        /// 
        /// The parsing needs to be done when the Smart Poster is read from
        /// a tag. If a detail of an existing Smart Poster class is modified,
        /// it will just update its internal payload, but doesn't need to
        /// parse it again (as the details are already stored in instances
        /// of the various record classes).
        /// </summary>
        /// <param name="payload">new payload</param>
        /// <param name="parseNewPayload">whether to parse the new payload to 
        /// update the internal records (url, title, type, etc.).</param>
        public void SetPayloadAndParse(byte[] payload, bool parseNewPayload)
        {
            Payload = payload;
            if (parseNewPayload) ParsePayloadToData(Payload);
        }

        /// <summary>
        /// Deletes any details currently stored in the Smart Poster 
        /// and re-initializes them by parsing the contents of the payload.
        /// </summary>
        private void ParsePayloadToData(byte[] payload)
        {
            InitializeData();
            if (payload == null || payload.Length == 0)
                return;

            var message = NdefMessage.FromByteArray(payload);

            foreach (NdefRecord record in message)
            {
                var specializedType = record.CheckSpecializedType(false);
                if (specializedType == null) continue;

                if (specializedType == typeof(NdefUriRecord))
                {
                    // URI
                    RecordUri = new NdefUriRecord(record);
                }
                else if (specializedType == typeof (NdefTextRecord))
                {
                    // Title
                    var textRecord = new NdefTextRecord(record);
                    if (Titles == null) Titles = new List<NdefTextRecord>();
                    Titles.Add(textRecord);
                }
                else if (specializedType == typeof (NdefSpActRecord))
                {
                    // Action
                    _recordAction = new NdefSpActRecord(record);
                }
                else if (specializedType == typeof (NdefSpSizeRecord))
                {
                    // Size
                    _recordSize = new NdefSpSizeRecord(record);
                }
                else if (specializedType == typeof (NdefSpMimeTypeRecord))
                {
                    // Mime Type
                    _recordMimeType = new NdefSpMimeTypeRecord(record);
                }
                else if (specializedType == typeof(NdefMimeImageRecordBase))
                {
                    // Image
                    _recordImage = new NdefMimeImageRecordBase(record);
                }
                else
                {
                    Debug.WriteLine("Sp: Don't know how to handle this record: " +
                                    BitConverter.ToString(record.Type));
                }
            }
        }

        /// <summary>
        /// Reverse function to parseRecords() - this one takes
        /// the information stored in the individual record instances and assembles
        /// it into the payload of the base class.
        /// </summary>
        /// <remarks>
        /// As the URI is mandatory, the payload will not be assembled
        /// if no URI is defined.
        /// </remarks>
        /// <returns>Whether assembling the payload was successful.</returns>
        private bool AssemblePayload()
        {
            // Uri is mandatory - don't assemble the payload if it's not set
            if (RecordUri == null) return false;

            // URI (mandatory)
            var message = new NdefMessage { RecordUri };

            // Title(s) (optional)
            if (Titles != null && Titles.Count > 0)
                message.AddRange(Titles);

            // Action (optional)
            if (ActionInUse())
                message.Add(_recordAction);

            // Size (optional)
            if (SizeInUse())
                message.Add(_recordSize);

            // Mime Type (optional)
            if (MimeTypeInUse())
                message.Add(_recordMimeType);

            // Image (optional)
            if (ImageInUse())
                message.Add(_recordImage);

            SetPayloadAndParse(message.ToByteArray(), false);

            return true;
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Smart Poster.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a Smart Poster, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Type.SequenceEqual(SmartPosterType));
        }

        /// <summary>
        /// Returns true if this record contains data that requires a Smart Poster
        /// and could not be stored in a simple URI record.
        /// </summary>
        /// <returns>true if the record contains one or more titles, an image, action, size
        /// or type information. It is false if the record only contains a URI.</returns>
        public bool HasSpData()
        {
            return (TitleCount() > 0 || ActionInUse() || SizeInUse() || MimeTypeInUse() || ImageInUse());
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
            if (RecordUri == null || string.IsNullOrEmpty(RecordUri.Uri))
            {
                throw new NdefException(NdefExceptionMessages.ExSpUriEmpty);
            }
            return true;
        }

        // -----------------------------------------------------------------------------
        // URI
        /// <summary>
        /// URI sub-record of this Smart Poster. Use Uri property to access if possible.
        /// </summary>
        protected NdefUriRecord RecordUri;

        /// <summary>
        /// Mandatory URI of the Smart Poster
        /// </summary>
        public string Uri
        {
            get
            {
                return (RecordUri != null && RecordUri.Uri != null) ? RecordUri.Uri : String.Empty;
            }
            set
            {
                RecordUri = new NdefUriRecord { Uri = value };
                AssemblePayload();
            }
        }

        /// <summary>
        /// Set the URI based on another URI record instead of specifying the URI itself.
        /// </summary>
        /// <param name="newUri"></param>
        public void SetUri(NdefUriRecord newUri)
        {
            RecordUri = new NdefUriRecord(newUri);
            AssemblePayload();
        }

        // -----------------------------------------------------------------------------
        // Title(s)
        /// <summary>
        /// List of all title texts for different languages used in the Smart Poster.
        /// </summary>
        /// <remarks>
        /// Can be no title at all, or multiple titles. Each title should have a different
        /// language.
        /// </remarks>
        public List<NdefTextRecord> Titles { get; private set; }

        /// <summary>
        /// Add an (optional) title to the Smart Poster.
        /// </summary>
        /// <remarks>
        /// It is possible to add more than one title as each title text
        /// record can have a different language. The phone is then recommended
        /// to choose the text record with the language that makes most sense
        /// to the user.
        /// </remarks>
        /// <param name="newTitle">(additional) title to be stored in the Smart Poster.</param>
        public void AddTitle(NdefTextRecord newTitle)
        {
            if (Titles == null)
                Titles = new List<NdefTextRecord>();
            Titles.Add(new NdefTextRecord(newTitle));
            AssemblePayload();
        }

        /// <summary>
        /// Returns how many title text records are stored in the Smart Poster.
        /// </summary>
        public int TitleCount()
        {
            return (Titles != null) ? Titles.Count : 0;
        }

        /// <summary>
        /// Retrieve a specific title text record from the list.
        /// </summary>
        /// <remarks>
        /// Returns an empty record in case the index is invalid.
        /// </remarks>
        /// <param name="index">number of the title record to return</param>
        /// <returns>title record at the specified index, or empty record if the index is invalid.</returns>
        public NdefTextRecord Title(int index)
        {
            return Titles.ElementAtOrDefault(index);
        }


        // -----------------------------------------------------------------------------
        // Action
        private NdefSpActRecord _recordAction;

        /// <summary>
        /// The action defines how the reader should handle the Smart Poster record.
        /// </summary>
        /// <remarks> 
        /// Make sure to check actionInUse() before retrieving the action;
        /// if the Smart Poster doesn't have an action defined, itd
        /// will return the default NdefNfcSpRecord::DoAction.
        /// </remarks>
        public NdefSpActRecord.NfcActionType NfcAction
        {
            get { return (_recordAction != null) ? _recordAction.NfcAction : NdefSpActRecord.NfcActionType.DoAction; }
            set
            {
                if (_recordAction != null)
                    _recordAction.NfcAction = value;
                else
                    _recordAction = new NdefSpActRecord { NfcAction = value };
                AssemblePayload();
            }
        }

        /// <summary>
        /// Returns if the action has been defined for this Smart Poster instance.
        /// </summary>
        public bool ActionInUse()
        {
            return _recordAction != null;
        }

        // -----------------------------------------------------------------------------
        // Size
        private NdefSpSizeRecord _recordSize;

        /// <summary>
        /// Size of the linked content.
        /// </summary>
        /// <remarks>
        /// This is useful if the reader device needs to decide in advance whether 
        /// it has the capability to process the referenced object.
        /// </remarks>
        public UInt32 NfcSize
        {
            get { return (_recordSize != null) ? _recordSize.NfcSize : 0; }
            set
            {
                if (_recordSize != null)
                    _recordSize.NfcSize = value;
                else
                    _recordSize = new NdefSpSizeRecord { NfcSize = value };
                AssemblePayload();
            }
        }

        /// <summary>
        /// Returns if the size has been defined for this Smart Poster instance.
        /// </summary>
        public bool SizeInUse()
        {
            return _recordSize != null;
        }

        // -----------------------------------------------------------------------------
        // Type
        private NdefSpMimeTypeRecord _recordMimeType;

        /// <summary>
        /// MIME type of the linked object.
        /// </summary>
        /// <remarks>
        /// This can be used to tell the mobile device what kind of an 
        /// object it can expect before it opens the connection.
        /// </remarks>
        public string NfcMimeType
        {
            get { return (_recordMimeType != null) ? _recordMimeType.NfcMimeType : String.Empty; }
            set
            {
                if (_recordMimeType != null)
                    _recordMimeType.NfcMimeType = value;
                else
                    _recordMimeType = new NdefSpMimeTypeRecord { NfcMimeType = value };
                AssemblePayload();
            }
        }

        /// <summary>
        /// Returns if the mime type has been defined for this Smart Poster instance.
        /// </summary>
        public bool MimeTypeInUse()
        {
            return _recordMimeType != null;
        }

        // -----------------------------------------------------------------------------
        // Image
        private NdefMimeImageRecordBase _recordImage;

        /// <summary>
        /// Icon / Image data contained in the Smart Poster.
        /// </summary>
        /// <remarks>
        /// A reader device should display the icon prior to acting
        /// on the URI record.
        /// Note that the Smart Poster standard would allow to use multiple
        /// icons. However, as no currently available phones actually show
        /// the image contained in the Smart Poster and the standard does
        /// not contain an indication as to what would be the use of having
        /// multiple images if the device should only show one anyway (and
        /// there is no distinction between the images), this class is
        /// deliberately limited to only one image record.
        /// </remarks>
        public NdefMimeImageRecordBase NfcImage
        {
            get { return _recordImage; }
            set
            {
                _recordImage = new NdefMimeImageRecordBase(value);
                AssemblePayload();
            }
        }

        // Not implemented yet (optional and very seldom due to tag size limitations of
        // usually only a few bytes)
        /// <summary>
        /// Returns if the Smart Poster contains an image (not supported yet).
        /// </summary>
        public bool ImageInUse()
        {
            return _recordImage != null;
        }
    }

    // -----------------------------------------------------------------------------
    // Action Record
    /// <summary>
    /// The Action record is a Local Type specific to the Smart Poster.
    /// </summary>
    /// <remarks>
    /// It suggests a course of action that the device should do with the content.
    /// The NFC Local Type Name for the action is "act" (0x61 0x63 0x74).
    /// The action record is defined as having a local scope only, and therefore
    /// it has meaning only within a Smart Poster record. A lone "act"-record
    /// SHALL be considered an error.
    /// 
    /// The device MAY ignore this suggestion. The default (i.e., the Action
    /// record is missing from the Smart Poster) is not defined. For example,
    /// the device might show a list of options to the user.
    /// 
    /// (Information taken from NFC Forum Smart Poster NDEF record specifications)
    /// </remarks>
    public class NdefSpActRecord : NdefRecord
    {
        /// <summary>
        /// Type of the action, according to the NFC Forum Smart Poster specification.
        /// </summary>
        public enum NfcActionType
        {
            /// <summary>
            /// Do the action (send the SMS, launch the browser, make the telephone call).
            /// </summary>
            DoAction = 0,
            /// <summary>
            /// Save for later (store the SMS in INBOX, put the URI in a bookmark, save the telephone number in contacts).
            /// </summary>
            SaveForLater,
            /// <summary>
            /// Open for editing (open an SMS in the SMS editor, open the URI in an URI editor, open the telephone number for editing).
            /// </summary>
            OpenForEditing,
            /// <summary>
            /// Reserved for future use.
            /// </summary>
            Rfu
        }

        /// <summary>
        /// Defines how the device should handle the linked content.
        /// DoAction per default.
        /// </summary>
        public NfcActionType NfcAction
        {
            get
            {
                if (Payload == null || Payload.Length != 1)
                    // Invalid action record
                    return NfcActionType.DoAction;

                return Enum.IsDefined(typeof(NfcActionType), (int)Payload[0]) ? (NfcActionType)Payload[0] : NfcActionType.DoAction;
            }
            set
            {
                Payload = new[] { (byte)value };
            }
        }

        /// <summary>
        /// Create an empty Action record, to be used within a Smart Poster message.
        /// Automatically creates the payload and sets it to the default DoAction.
        /// </summary>
        public NdefSpActRecord()
            : base(TypeNameFormatType.NfcRtd, new[] { (byte)'a', (byte)'c', (byte)'t' })
        {
            _payload = new[] { (byte)NfcActionType.DoAction };
        }

        /// <summary>
        /// Create an action record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks> 
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be an action record as well.
        /// </remarks>
        /// <param name="other">Action record to copy into this record.</param>
        public NdefSpActRecord(NdefRecord other) : base(other) { }


        /// <summary>
        /// Checks if the record sent via the parameter is indeed an Action record.
        /// </summary>
        /// <remarks>
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </remarks>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be an Action record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Type.SequenceEqual(new[] { (byte)'a', (byte)'c', (byte)'t' }));
        }
    }

    // -----------------------------------------------------------------------------
    // Size Record
    /// <summary>
    /// The Size Record contains a four-byte, 32-bit, unsigned integer,
    /// which contains the size of object that the URI field refers to.
    /// </summary>
    /// <remarks>
    /// Note that in practice this is limited to URLs (http://, ftp:// and similar).
    /// The Size Record's Local Type Name is "s".
    /// 
    /// The size is expressed in network byte order (most significant byte first).
    /// For example, if Byte 0 contains 0x12, Byte 1 contains 0x34, Byte 2
    /// contains 0x56, and Byte 3 0x78, the size of the referred object
    /// is 0x12345678 bytes.
    /// 
    /// The size record MAY be used by the device to determine whether it can
    /// accommodate the referenced file or not. For example, an NFC tag could
    /// trigger the download of an application to a cell phone. Using a combination
    /// of the Type Record and the Size Record, the mobile phone could determine
    /// whether it can accommodate such a program or not.
    /// 
    /// The Size Record is for informational purposes only. Since the object size
    /// in the network may vary (for example, due to updates), this value should
    /// be used as a guideline only.
    /// 
    /// The Size Record is optional to support.
    /// 
    /// (Information taken from NFC Forum Smart Poster NDEF record specifications)
    /// </remarks>
    public class NdefSpSizeRecord : NdefRecord
    {
        /// <summary>
        /// Size of the linked content.
        /// </summary>
        public UInt32 NfcSize
        {
            get
            {
                if (Payload == null || Payload.Length != 4)
                    // Invalid size record
                    return 0;

                // Make sure we're using big endian
                return (UInt32)((Payload[0]) << 24) | (UInt32)((Payload[1]) << 16) | (UInt32)((Payload[2]) << 8) | (UInt32)((Payload[3]) << 0);
            }
            set
            {
                Payload = new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0x000000ff) };
            }
        }

        /// <summary>
        /// Create an empty size record, to be used within a Smart Poster message.
        /// Automatically creates the payload and sets it to size 0.
        /// </summary>
        public NdefSpSizeRecord()
            : base(TypeNameFormatType.NfcRtd, new[] { (byte)'s' })
        {
            _payload = new byte[] { 0, 0, 0, 0 };
        }

        /// <summary>
        /// Create a size record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be an action record as well.
        /// </remarks>
        /// <param name="other">Size record to copy into this record.</param>
        public NdefSpSizeRecord(NdefRecord other) : base(other) { }


        /// <summary>
        /// Checks if the record sent via the parameter is indeed a size record.
        /// </summary>
        /// <remarks>
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </remarks>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a size record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Type.SequenceEqual(new[] { (byte)'s' }));
        }
    }

    // -----------------------------------------------------------------------------
    // Mime Type Record
    /// <summary>
    /// The Payload of the Type Record is a UTF-8-formatted string
    /// that describes a MIME type [RFC 2046] which describes the type of the
    /// object that can be reached through the URI.
    /// </summary>
    /// <remarks>
    /// (In practice this is limited to URLs only, much like the Size Record.)
    /// The Local Type Name for the Type Record is "t".
    /// 
    /// The length of the payload string is the same as the length of the
    /// payload, so there is no need for separate length information or
    /// termination.
    /// 
    /// The Type Record MAY be used by the device to determine whether it
    /// can process the referenced file or not. For example, an NFC tag
    /// could trigger a media file playback from an URL. If the Type Record
    /// references an unknown media type, the reader device (e.g. a cell phone)
    /// does not need to even initiate the playback.
    /// 
    /// The Type Record is optional to support.
    /// 
    /// (Information taken from NFC Forum Smart Poster NDEF record specifications)
    /// </remarks>
    public class NdefSpMimeTypeRecord : NdefRecord
    {
        /// <summary>
        /// MIME type of the linked content.
        /// </summary>
        public string NfcMimeType
        {
            get
            {
                return Payload == null ? string.Empty : Encoding.UTF8.GetString(Payload, 0, Payload.Length);
            }
            set
            {
                Payload = Encoding.UTF8.GetBytes(value);
            }
        }

        /// <summary>
        /// Create an empty mime type record, to be used within a Smart Poster message.
        /// </summary>
        public NdefSpMimeTypeRecord()
            : base(TypeNameFormatType.NfcRtd, new[] { (byte)'t' })
        {
            _payload = new byte[] { };
        }

        /// <summary>
        /// Create a mime type record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be an action record as well.
        /// </remarks>
        /// <param name="other">Mime type record to copy into this record.</param>
        public NdefSpMimeTypeRecord(NdefRecord other) : base(other) { }


        /// <summary>
        /// Checks if the record sent via the parameter is indeed a (mime) type record.
        /// </summary>
        /// <remarks>
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </remarks>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a (mime) type record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Type.SequenceEqual(new[] { (byte)'t' }));
        }
    }

}
