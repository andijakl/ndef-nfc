/****************************************************************************
**
** Copyright (C) 2012-2016 Andreas Jakl - http://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
** More information: http://andijakl.github.io/ndef-nfc/
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
using System.Collections.Generic;
using System.Linq;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Convenience class for formatting URI records that launch
    /// specific setting pages on Windows Phone 8.
    /// Please use the NdefWindowsSettingsRecord for Windows 10 and
    /// Windows 10 Mobile.
    /// </summary>
    /// <remarks>
    /// Tapping a tag with one of the custom URI schemes defined in this
    /// record will cause a Windows Phone 8 application to launch the settings
    /// app on the specified page, e.g., to adjust the Bluetooth settings or
    /// to activate flight mode.
    /// 
    /// Unfortunately, this URI scheme does not allow to actually change the 
    /// system settings; you can only show the required page to the user so that
    /// he can apply the changes manually. This serves as a shortcut, if you
    /// for example need the user to activate Bluetooth. Due to the security
    /// model on Windows Phone 8, no app or tag could modify system settings;
    /// the user always has to do that. You can only make his life easier by
    /// redirecting him to the specific settings page.
    /// 
    /// This URI scheme is specific to Windows Phone 8 devices and will only
    /// work on such phones.
    /// More information:
    /// http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj662937%28v=vs.105%29.aspx
    /// </remarks>
    public class NdefWpSettingsRecord : NdefUriRecord
    {
        /// <summary>
        /// Matches the setting scheme from the enumeration to the
        /// actual textual representation of the scheme.
        /// </summary>
        private static readonly Dictionary<NfcSettingsApp, string> SettingsSchemes = new Dictionary<NfcSettingsApp, string>
                                                                                         {
                                                                               {NfcSettingsApp.AirplaneMode, "ms-settings-airplanemode:"},
                                                                               {NfcSettingsApp.Battery, "ms-battery:"},     // Undocumented
                                                                               {NfcSettingsApp.Bluetooth, "ms-settings-bluetooth:"},
                                                                               {NfcSettingsApp.Camera, "ms-settings-camera:"},
                                                                               {NfcSettingsApp.Cellular, "ms-settings-cellular:"},
                                                                               {NfcSettingsApp.EmailAndAccounts, "ms-settings-emailandaccounts:"},
                                                                               {NfcSettingsApp.Location, "ms-settings-location:"},
                                                                               {NfcSettingsApp.Lock, "ms-settings-lock:"},
                                                                               {NfcSettingsApp.Notifications, "ms-settings-notifications:"},
                                                                               {NfcSettingsApp.Power, "ms-settings-power:"},
                                                                               {NfcSettingsApp.Proximity, "ms-settings-proximity:"},
                                                                               {NfcSettingsApp.ScreenRotation, "ms-settings-screenrotation:"},
                                                                               {NfcSettingsApp.Wallet, "ms-wallet:"},       // Undocumented
                                                                               {NfcSettingsApp.Wifi, "ms-settings-wifi:"},
                                                                               {NfcSettingsApp.Workplace, "ms-settings-workplace:"},

                                                                               {NfcSettingsApp.NetworkProfileUpdate, "ms-settings-networkprofileupdate:"},  // Undocumented
                                                                               {NfcSettingsApp.NfcTransactions, "ms-settings-nfctransactions:"},            // Same as Proximity 
                                                                               {NfcSettingsApp.UiccToolkit, "ms-settings-uicctoolkit:"},                    // Undocumented
                                                                           };

        /// <summary>
        /// The different setting schemes for launching setting app pages on
        /// Windows Phone, as defined by Microsoft.
        /// </summary>
        public enum NfcSettingsApp
        {
            AirplaneMode,
            Battery,
            Bluetooth,
            Camera,
            Cellular,
            EmailAndAccounts,
            Location,
            Lock,
            Notifications,
            Power,
            Proximity,
            ScreenRotation,
            Wallet,
            Wifi,
            Workplace,

            NetworkProfileUpdate,
            NfcTransactions,
            UiccToolkit,
        }

        private NfcSettingsApp _settingsApp;
        /// <summary>
        /// Use this property to get/set which settings page is launched
        /// through the record.
        /// </summary>
        public NfcSettingsApp SettingsApp
        {
            get { return _settingsApp; }
            set
            {
                _settingsApp = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Create an empty WP8 Settings record.
        /// </summary>
        public NdefWpSettingsRecord()
        {
        }

        /// <summary>
        /// Create a WP8 settings record based on another WP8 settings record, or a URI
        /// record that has a Uri that corresponds to one of the allowed URI schemes
        /// for this record.
        /// </summary>
        /// <param name="other">Other record to copy the data from.</param>
        public NdefWpSettingsRecord(NdefRecord other)
            : base(other)
        {
            ParseUriToData(Uri);
        }

        /// <summary>
        /// Deletes any details currently stored in the WP8 settings record 
        /// and re-initializes them by parsing the contents of the provided URI.
        /// </summary>
        /// <remarks>The URI has to be formatted according to the one of the
        /// allowed WP8 settings schemes.</remarks>
        private void ParseUriToData(string uri)
        {
            foreach (var curScheme in SettingsSchemes.Where(curScheme => uri == curScheme.Value))
            {
                SettingsApp = curScheme.Key;
                break;
            }
            // UpdatePayload() does not need to be called here - 
            // already called by the property setter
        }


        /// <summary>
        /// Format the URI of the Uri base class.
        /// </summary>
        private void UpdatePayload()
        {
            if (SettingsSchemes.ContainsKey(SettingsApp))
                Uri = SettingsSchemes[SettingsApp];
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed a Windows Phone Settings record.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a WP Settings record + if its URI equals one of the allowed
        /// scheme names. False if it's a different record.</returns>
        public new static bool IsRecordType(NdefRecord record)
        {
            if (!NdefUriRecord.IsRecordType(record)) return false;
            var testRecord = new NdefUriRecord(record);
            return testRecord.Uri != null && SettingsSchemes.Any(curScheme => testRecord.Uri.Equals(curScheme.Value));
        }


    }
}
