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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Handles the Windows LaunchApp record format.
    /// </summary>
    /// <remarks>
    /// The Proximity APIs can directly write LaunchApp tags using
    /// a specific API call. However, this doesn't allow flexibility
    /// in including this record together with other records on a tag,
    /// to allow for greater flexibility.
    /// 
    /// A LaunchApp record created using this class can be put into a 
    /// multi-record NDEF message. This allows creating a tag that contains
    /// both the Windows LaunchApp tag, as well as the Android Application
    /// Record.
    /// 
    /// Note that for default handling by the OS, the Windows LaunchApp tag
    /// needs to be the first record in the message. Android recommends to put
    /// its Android Application Record (AAR) as the last record of the message.
    /// 
    /// To create a LaunchApp tag using the Windows Proximity APIs, you have
    /// to pass a string containing the arguments and platform/app ID tuples
    /// in a special format (separated by tabs). The APIs then re-format the
    /// text into the actual payload of the record - meaning that the string
    /// you send to the Proximity LaunchApp Write API does NOT get directly 
    /// written to the tag.
    /// 
    /// In contrast, this class provides a more convenient way to set the
    /// arguments and to add any number of platforms; all using properties or
    /// methods, without the need to worry about formatting a special string.
    /// This class will then directly create the required raw payload that
    /// is suitable to be written to the tag.
    /// </remarks>
    public class NdefLaunchAppRecord : NdefRecord
    {
        /// <summary>
        /// Type of the record (of TNF Absolute URI)
        /// </summary>
        private static readonly byte[] WindowsLaunchAppType = { (byte)'w', (byte)'i', (byte)'n', (byte)'d', (byte)'o', (byte)'w', (byte)'s', (byte)'.', (byte)'c', (byte)'o', (byte)'m', (byte)'/', (byte)'L', (byte)'a', (byte)'u', (byte)'n', (byte)'c', (byte)'h', (byte)'A', (byte)'p', (byte)'p' };

        private string _arguments;
        /// <summary>
        /// Arguments that will be passed to the launched application.
        /// </summary>
        /// <remarks>
        /// The exact format is up to the app itself.
        /// The arguments string must not be empty; otherwise,
        /// the reading device might ignore the LaunchApp record.
        /// </remarks>
        public string Arguments
        {
            get { return _arguments ?? String.Empty; }
            set
            {
                _arguments = value;
                AssemblePayload();
            }
        }

        /// <summary>
        /// Platform names and respective app IDs.
        /// </summary>
        /// <remarks>
        /// The key is the platform name, the value the app ID for this specific platform.
        /// A valid LaunchApp tag needs to contain at least one platform / app ID
        /// tuple.
        /// The platform name needs to be unique. Each platform name + app ID has
        /// to be smaller or equal to 255 characters.
        /// </remarks>
        public Dictionary<string, string> PlatformIds { get; private set; }

        /// <summary>
        /// Create an empty LaunchApp Record.
        /// </summary>
        public NdefLaunchAppRecord()
            : base(TypeNameFormatType.Uri, WindowsLaunchAppType)
        {
        }

        /// <summary>
        /// Create a LaunchApp record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// Internalizes and parses the payload of the original record.
        /// The original record has to be a LaunchApp Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this LaunchApp record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create a LaunchApp record
        /// based on an incompatible record type.</exception>
        public NdefLaunchAppRecord(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
            ParsePayloadToData(_payload);
        }

        /// <summary>
        /// (Re)set the stored data of this launch app record.
        /// </summary>
        private void InitializeData()
        {
            _arguments = String.Empty;
            PlatformIds = null;
        }

        /// <summary>
        /// Add another platform and app ID.
        /// </summary>
        /// <remarks>
        /// A valid LaunchApp tag needs to contain at least one platform / app ID
        /// tuple.
        /// The platform name needs to be unique. Each platform name + app ID has
        /// to be smaller or equal to 255 characters.
        /// If known, the appID will be checked for semantic correctness.
        /// </remarks>
        /// <param name="platform">Name of the platform, e.g., Windows or WindowsPhone.</param>
        /// <param name="appId">ID of the platform. See platform documentation on how
        /// to format the platform ID.</param>
        public void AddPlatformAppId(string platform, string appId)
        {
            // Check appIds
            if (platform.Equals("Windows"))
            {
                // Windows 8
                // TODO: Optional check if supplied arguments are valid
                // You must specify at least one app platform and app name.
                // The app platform for a Windows 8 computer is Windows.
                // The format of the proximity app Id is <package family name>!<app Id>. 
                // You can get the package family name from the Windows.ApplicationModel.Package.Current.Id.FamilyName property. 
                // You must copy the app Id value from the Id attribute of the Application element in the package manifest for your app.
                // "Example.Proximity.JS_8wekyb3d8bbwe!Proximity.App"
            }
            else if (platform.Equals("WindowsPhone"))
            {
                // WP: Product ID {<36 chars>}
                var wpAppId = new Regex(@"^\{[a-fA-F0-9-]{36}\}$");
                if (!wpAppId.IsMatch(appId))
                {
                    // Check and auto-correct if the app id was specified without {}
                    var wpAppIdPure = new Regex(@"^[a-fA-F0-9-]{36}$");
                    if (!wpAppIdPure.IsMatch(appId))
                    {
                        // Invalid app id
                        throw new NdefException(NdefExceptionMessages.ExLaunchAppWpId);
                    }
                    // Supplied valid app id, but without {}
                    appId = "{" + appId + "}";
                }
            }

            // Add platform + app id to the dictionary
            if (PlatformIds == null) PlatformIds = new Dictionary<string, string>();
            PlatformIds.Add(platform, appId);
            AssemblePayload();
        }

        /// <summary>
        /// Sets the payload of the base class to the byte array from 
        /// the parameter and optionally parses its contents.
        /// </summary>
        /// <remarks>
        /// The parsing needs to be done when the LaunchApp record is read from
        /// a tag. If a detail of an existing LaunchApp class is modified,
        /// it will just update its internal payload, but doesn't need to
        /// parse it again (as the details are already stored in the respecitve
        /// member variables).
        /// </remarks>
        /// <param name="payload">new payload</param>
        /// <param name="parseNewPayload">whether to parse the new payload to 
        /// update the internal data properties</param>
        public void SetPayloadAndParse(byte[] payload, bool parseNewPayload)
        {
            Payload = payload;
            if (parseNewPayload) ParsePayloadToData(Payload);
        }

        /// <summary>
        /// Deletes any details currently stored in the LaunchApp record 
        /// and re-initializes them by parsing the contents of the payload.
        /// </summary>
        private void ParsePayloadToData(byte[] payload)
        {
            InitializeData();

            // Minimum legal length: 5 bytes for the lengths
            if (payload == null || payload.Length < 5)
                return;

            // General: UTF-8 encoding for the payload
            var encoding = Encoding.UTF8;

            // Create reader based on the payload
            var payloadStream = new MemoryStream(payload, false);
            var reader = new BinaryReader(payloadStream);

            // Number of platforms stored in the record
            // reader.ReadUInt16() would use little-endian encoding
            var platformIdsCount = (ushort)((reader.ReadByte()) << 8);
            platformIdsCount |= (ushort)((reader.ReadByte()) << 0);

            // At least one platform found?
            if (platformIdsCount > 0 && PlatformIds == null) PlatformIds = new Dictionary<string, string>();
            for (var i = 0; i < platformIdsCount; i++)
            {
                // Length of the platform name
                var platformLength = (int)reader.ReadByte();
                // Platform name
                var platformBinary = reader.ReadBytes(platformLength);
                var platformString = encoding.GetString(platformBinary, 0, platformBinary.Length);

                // Length of the App Id
                var appIdLength = (int)reader.ReadByte();
                // App Id
                var appIdBinary = reader.ReadBytes(appIdLength);
                var appIdString = encoding.GetString(appIdBinary, 0, appIdBinary.Length);

                // Add platform / app ID tuple to the dictionary
                PlatformIds.Add(platformString, appIdString);
            }

            // Length of the arguments string (big-endian)
            var argumentsLength = (ushort)((reader.ReadByte()) << 8);
            argumentsLength |= (ushort)((reader.ReadByte()) << 0);

            // Arguments string
            var argumentsBinary = reader.ReadBytes(argumentsLength);
            _arguments = encoding.GetString(argumentsBinary, 0, argumentsBinary.Length);
        }

        /// <summary>
        /// Create the payload based on the data stored in the properties.
        /// </summary>
        /// <exception cref="NdefException">Thrown if unable to assemble the payload.
        /// The exception message contains further details about the issue.</exception>
        /// <returns>Whether assembling the payload was successful.</returns>
        private bool AssemblePayload()
        {
            if (PlatformIds == null || PlatformIds.Count == 0)
            {
                Debug.WriteLine(NdefExceptionMessages.ExLaunchAppPlatformMissing);
                return false;
            }

            // General: UTF-8 encoding for the payload
            var encoding = Encoding.UTF8;
            var m = new MemoryStream();

            // First USHORT must contain the number of platform / AppID tuples in big-endian encoding
            // (BitConverter.GetBytes() would use little endian)
            m.WriteByte((byte)(((ushort)PlatformIds.Count) >> 8));
            m.WriteByte((byte)(((ushort)PlatformIds.Count) & 0x000000ff));

            // For each platform / AppID tuple
            foreach (var platformIdTuple in PlatformIds)
            {
                var encodedPlatform = encoding.GetBytes(platformIdTuple.Key);
                var encodedAppId = encoding.GetBytes(platformIdTuple.Value);

                // Check if length <= 255
                if (encodedPlatform.Length + encodedAppId.Length > 255)
                    throw new NdefException(NdefExceptionMessages.ExLaunchAppPlatformLength);

                // Add a byte with the length of the platform string itself
                m.WriteByte((byte)encodedPlatform.Length);

                // followed by the platform string itself
                m.Write(encodedPlatform, 0, encodedPlatform.Length);

                // followed by a byte with the length of the AppId string 
                m.WriteByte((byte)encodedAppId.Length);

                // followed by the AppID string itself
                m.Write(encodedAppId, 0, encodedAppId.Length);
            }

            // Argument string
            var encodedArguments = encoding.GetBytes(Arguments);

            // USHORT containing the length of the argument string
            //m.Write(BitConverter.GetBytes((ushort)encodedArguments.Length), 0, 2);
            m.WriteByte((byte)(((ushort)encodedArguments.Length) >> 8));
            m.WriteByte((byte)(((ushort)encodedArguments.Length) & 0x000000ff));

            // followed by the argument string itself
            m.Write(encodedArguments, 0, encodedArguments.Length);

            // Set payload
            SetPayloadAndParse(m.ToArray(), false);
            return true;
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a LaunchApp record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a LaunchApp record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            return (record.TypeNameFormat == TypeNameFormatType.Uri && record.Type.SequenceEqual(WindowsLaunchAppType));
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
            if (string.IsNullOrEmpty(Arguments))
            {
                throw new NdefException(NdefExceptionMessages.ExLaunchAppArgumentsEmpty);
            }
            if (PlatformIds == null || PlatformIds.Count < 1)
            {
                throw new NdefException(NdefExceptionMessages.ExLaunchAppPlatformsEmpty);
            }
            return true;
        }
    }
}
