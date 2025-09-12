/****************************************************************************
**
** Copyright (C) 2012-2018 Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
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
using System.Collections.Generic;
using System.Linq;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Convenience class for formatting URI records that launch
    /// specific setting pages on Windows 10.
    /// </summary>
    /// <remarks>
    /// Tapping a tag with one of the custom URI schemes defined in this
    /// record will cause a Windows10 application to launch the settings
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
    /// This URI scheme is specific to Windows 10 devices and will only
    /// work on such devices.
    /// More information:
    /// https://msdn.microsoft.com/en-us/library/windows/apps/mt228342.aspx
    /// </remarks>
    public class NdefWindowsSettingsRecord : NdefUriRecord
    {
        /// <summary>
        /// Matches the setting scheme from the enumeration to the
        /// actual textual representation of the scheme.
        /// </summary>
        private static readonly Dictionary<NfcSettingsApp, string> SettingsSchemes = new Dictionary<NfcSettingsApp, string>
                                {
                                {NfcSettingsApp.SettingsHome, "ms-settings:"},

                                {NfcSettingsApp.SystemDisplay, "ms-settings:screenrotation"},
                                {NfcSettingsApp.SystemNotifications, "ms-settings:notifications"},
                                {NfcSettingsApp.SystemPhone, "ms-settings:phone"},        // Mobile only
                                {NfcSettingsApp.SystemMessaging, "ms-settings:messaging"},   // Mobile only
                                {NfcSettingsApp.SystemBatterySaver, "ms-settings:batterysaver"},
                                {NfcSettingsApp.SystemBatterySaverSettings, "ms-settings:batterysaver-settings"},
                                {NfcSettingsApp.SystemBatteryUse, "ms-settings:batterysaver-usagedetails"},
                                {NfcSettingsApp.SystemPower, "ms-settings:powersleep"},        // Desktop only
                                {NfcSettingsApp.SystemEncryption, "ms-settings:deviceencryption"},
                                {NfcSettingsApp.SystemOfflineMaps, "ms-settings:maps"},
                                {NfcSettingsApp.SystemAbout, "ms-settings:about"},

                                {NfcSettingsApp.DevicesCamera, "ms-settings:camera"},      // Mobile only
                                {NfcSettingsApp.DevicesBluetooth, "ms-settings:bluetooth"},   // Desktop only
                                {NfcSettingsApp.DevicesMouseTouchpad, "ms-settings:mousetouchpad"},
                                {NfcSettingsApp.DevicesNfc, "ms-settings:nfctransactions"},

                                {NfcSettingsApp.NetworkWifi, "ms-settings:network-wifi"},
                                {NfcSettingsApp.NetworkAirplaneMode, "ms-settings:network-airplanemode"},
                                {NfcSettingsApp.NetworkDataUsage, "ms-settings:datausage"},
                                {NfcSettingsApp.NetworkCellularSim, "ms-settings:network-cellular"},
                                {NfcSettingsApp.NetworkMobileHotspot, "ms-settings:network-mobilehotspot"},
                                {NfcSettingsApp.NetworkProxy, "ms-settings:network-proxy"},

                                {NfcSettingsApp.Personalization, "ms-settings:personalization"},
                                {NfcSettingsApp.PersonalizationBackground, "ms-settings:personalization-background"},  // Desktop only
                                {NfcSettingsApp.PersonalizationColors, "ms-settings:personalization-colors"},
                                {NfcSettingsApp.PersonalizationSounds, "ms-settings:sounds"},  // Mobile only
                                {NfcSettingsApp.PersonalizationLockScreen, "ms-settings:lockscreen"},

                                {NfcSettingsApp.AccountsEmail, "ms-settings:emailandaccounts"},
                                {NfcSettingsApp.AccountsWorkAccess, "ms-settings:workplace"},
                                {NfcSettingsApp.AccountsSync, "ms-settings:sync"},

                                {NfcSettingsApp.TimeLanguageDateTime, "ms-settings:dateandtime"},
                                {NfcSettingsApp.TimeLanguageRegion, "ms-settings:regionlanguage"},     // Desktop only

                                {NfcSettingsApp.EaseOfAccessNarrator, "ms-settings:easeofaccess-narrator"},
                                {NfcSettingsApp.EaseOfAccessMagnifier, "ms-settings:easeofaccess-magnifier"},
                                {NfcSettingsApp.EaseOfAccessHighContrast, "ms-settings:easeofaccess-highcontrast"},
                                {NfcSettingsApp.EaseOfAccessClosedCaptions, "ms-settings:easeofaccess-closedcaptioning"},
                                {NfcSettingsApp.EaseOfAccessKeyboard, "ms-settings:easeofaccess-keyboard"},
                                {NfcSettingsApp.EaseOfAccessMouse, "ms-settings:easeofaccess-mouse"},
                                {NfcSettingsApp.EaseOfAccessOther, "ms-settings:easeofaccess-otheroptions"},

                                {NfcSettingsApp.PrivacyLocation, "ms-settings:privacy-location"},
                                {NfcSettingsApp.PrivacyCamera, "ms-settings:privacy-webcam"},
                                {NfcSettingsApp.PrivacyMicrophone, "ms-settings:privacy-microphone"},
                                {NfcSettingsApp.PrivacyMotion, "ms-settings:privacy-motion"},
                                {NfcSettingsApp.PrivacySpeechInkingTyping, "ms-settings:privacy-speechtyping"},
                                {NfcSettingsApp.PrivacyAccountInfo, "ms-settings:privacy-accountinfo"},
                                {NfcSettingsApp.PrivacyContacts, "ms-settings:privacy-contacts"},
                                {NfcSettingsApp.PrivacyCalendar, "ms-settings:privacy-calendar"},
                                {NfcSettingsApp.PrivacyCallHistory, "ms-settings:privacy-callhistory"},
                                {NfcSettingsApp.PrivacyEmail, "ms-settings:privacy-email"},
                                {NfcSettingsApp.PrivacyMessaging, "ms-settings:privacy-messaging"},
                                {NfcSettingsApp.PrivacyRadios, "ms-settings:privacy-radios"},
                                {NfcSettingsApp.PrivacyBackgroundApps, "ms-settings:privacy-backgroundapps"},
                                {NfcSettingsApp.PrivacyOtherDevices, "ms-settings:privacy-customdevices"},
                                {NfcSettingsApp.PrivacyFeedback, "ms-settings:privacy-feedback"},

                                {NfcSettingsApp.UpdateSecurityDevelopers, "ms-settings:developers"}};

        /// <summary>
        /// The different setting schemes for launching setting app pages on
        /// Windows Phone, as defined by Microsoft.
        /// </summary>
        public enum NfcSettingsApp
        {
            SettingsHome,

            SystemDisplay,
            SystemNotifications,
            SystemPhone,        // Mobile only
            SystemMessaging,   // Mobile only
            SystemBatterySaver,
            SystemBatterySaverSettings,
            SystemBatteryUse,
            SystemPower,        // Desktop only
            SystemEncryption,
            SystemOfflineMaps,
            SystemAbout,

            DevicesCamera,      // Mobile only
            DevicesBluetooth,   // Desktop only
            DevicesMouseTouchpad,
            DevicesNfc,

            NetworkWifi,
            NetworkAirplaneMode,
            NetworkDataUsage,
            NetworkCellularSim,
            NetworkMobileHotspot,
            NetworkProxy,

            Personalization,
            PersonalizationBackground,  // Desktop only
            PersonalizationColors,
            PersonalizationSounds,  // Mobile only
            PersonalizationLockScreen,

            AccountsEmail,
            AccountsWorkAccess,
            AccountsSync,

            TimeLanguageDateTime,
            TimeLanguageRegion,     // Desktop only

            EaseOfAccessNarrator,
            EaseOfAccessMagnifier,
            EaseOfAccessHighContrast,
            EaseOfAccessClosedCaptions,
            EaseOfAccessKeyboard,
            EaseOfAccessMouse,
            EaseOfAccessOther,

            PrivacyLocation,
            PrivacyCamera,
            PrivacyMicrophone,
            PrivacyMotion,
            PrivacySpeechInkingTyping,
            PrivacyAccountInfo,
            PrivacyContacts,
            PrivacyCalendar,
            PrivacyCallHistory,
            PrivacyEmail,
            PrivacyMessaging,
            PrivacyRadios,
            PrivacyBackgroundApps,
            PrivacyOtherDevices,
            PrivacyFeedback,

            UpdateSecurityDevelopers
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
        /// Create an empty Windows Settings record.
        /// </summary>
        public NdefWindowsSettingsRecord()
        {
        }

        /// <summary>
        /// Create a Windows settings record based on another Windows settings record, or a URI
        /// record that has a Uri that corresponds to one of the allowed URI schemes
        /// for this record.
        /// </summary>
        /// <param name="other">Other record to copy the data from.</param>
        public NdefWindowsSettingsRecord(NdefRecord other)
            : base(other)
        {
            ParseUriToData(Uri);
        }

        /// <summary>
        /// Deletes any details currently stored in the Windows settings record
        /// and re-initializes them by parsing the contents of the provided URI.
        /// </summary>
        /// <remarks>The URI has to be formatted according to the one of the
        /// allowed Windows settings schemes.</remarks>
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
        /// Checks if the record sent via the parameter is indeed a Windows Settings record.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a Windows Settings record + if its URI equals one of the allowed
        /// scheme names. False if it's a different record.</returns>
        public new static bool IsRecordType(NdefRecord record)
        {
            if (!NdefUriRecord.IsRecordType(record)) return false;
            var testRecord = new NdefUriRecord(record);
            return testRecord.Uri != null && SettingsSchemes.Any(curScheme => testRecord.Uri.Equals(curScheme.Value));
        }


    }
}
