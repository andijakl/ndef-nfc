/****************************************************************************
**
** Copyright (C) 2012 - 2014 Andreas Jakl.
** All rights reserved.
**
** Code example for the NDEF Library for Proximity APIs (NFC).
**
** Created by Andreas Jakl (2012).
** More information: http://andijakl.github.io/ndef-nfc/
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
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows;
using Windows.Phone.PersonalInformation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.UserData;
using NdefDemo.Resources;
using Windows.Networking.Proximity;
using NdefLibrary.Ndef;
using NdefLibraryWp.Ndef;

namespace NdefDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ProximityDevice _device;
        private long _subscriptionIdNdef;
        private long _publishingMessageId;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
            // Initialize app menu
            BuildLocalizedApplicationBar();
        }

        private void BtnInitNfc_Click(object sender, RoutedEventArgs e)
        {
            // Initialize NFC
            _device = ProximityDevice.GetDefault();
            // Update status text for UI
            SetStatusOutput(_device != null ? AppResources.StatusInitialized : AppResources.StatusInitFailed);
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
        }

        #region Subscribe for tags
        // ----------------------------------------------------------------------------------------------------

        private void BtnSubscribeNdef_Click(object sender, RoutedEventArgs e)
        {
            // Only subscribe for messages if no NDEF subscription is already active
            if (_subscriptionIdNdef != 0) return;
            // Ask the proximity device to inform us about any kind of NDEF message received from
            // another device or tag.
            // Store the subscription ID so that we can cancel it later.
            _subscriptionIdNdef = _device.SubscribeForMessage("NDEF", MessageReceivedHandler);
            // Update status text for UI
            SetStatusOutput(string.Format(AppResources.StatusSubscribed, _subscriptionIdNdef));
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
        }

        private async void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            // Get the raw NDEF message data as byte array
            var rawMsg = message.Data.ToArray();
            // Let the NDEF library parse the NDEF message out of the raw byte array
            var ndefMessage = NdefMessage.FromByteArray(rawMsg);

            // Analysis result
            var tagContents = new StringBuilder();

            // Loop over all records contained in the NDEF message
            foreach (NdefRecord record in ndefMessage)
            {
                // --------------------------------------------------------------------------
                // Print generic information about the record
                if (record.Id != null && record.Id.Length > 0)
                {
                    // Record ID (if present)
                    tagContents.AppendFormat("Id: {0}\n", Encoding.UTF8.GetString(record.Id, 0, record.Id.Length));
                }
                // Record type name, as human readable string
                tagContents.AppendFormat("Type name: {0}\n", ConvertTypeNameFormatToString(record.TypeNameFormat));
                // Record type
                if (record.Type != null && record.Type.Length > 0)
                {
                    tagContents.AppendFormat("Record type: {0}\n",
                                             Encoding.UTF8.GetString(record.Type, 0, record.Type.Length));
                }

                // --------------------------------------------------------------------------
                // Check the type of each record
                // Using 'true' as parameter for CheckSpecializedType() also checks for sub-types of records,
                // e.g., it will return the SMS record type if a URI record starts with "sms:"
                // If using 'false', a URI record will always be returned as Uri record and its contents won't be further analyzed
                // Currently recognized sub-types are: SMS, Mailto, Tel, Nokia Accessories, NearSpeak, WpSettings
                var specializedType = record.CheckSpecializedType(true);

                if (specializedType == typeof(NdefMailtoRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract Mailto record info
                    var mailtoRecord = new NdefMailtoRecord(record);
                    tagContents.Append("-> Mailto record\n");
                    tagContents.AppendFormat("Address: {0}\n", mailtoRecord.Address);
                    tagContents.AppendFormat("Subject: {0}\n", mailtoRecord.Subject);
                    tagContents.AppendFormat("Body: {0}\n", mailtoRecord.Body);
                }
                else if (specializedType == typeof(NdefUriRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract URI record info
                    var uriRecord = new NdefUriRecord(record);
                    tagContents.Append("-> URI record\n");
                    tagContents.AppendFormat("URI: {0}\n", uriRecord.Uri);
                }
                else if (specializedType == typeof (NdefLaunchAppRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract LaunchApp record info
                    var launchAppRecord = new NdefLaunchAppRecord(record);
                    tagContents.Append("-> LaunchApp record" + Environment.NewLine);
                    if (!string.IsNullOrEmpty(launchAppRecord.Arguments))
                        tagContents.AppendFormat("Arguments: {0}\n", launchAppRecord.Arguments);
                    if (launchAppRecord.PlatformIds != null)
                    {
                        foreach (var platformIdTuple in launchAppRecord.PlatformIds)
                        {
                            if (platformIdTuple.Key != null)
                                tagContents.AppendFormat("Platform: {0}\n", platformIdTuple.Key);
                            if (platformIdTuple.Value != null)
                                tagContents.AppendFormat("App ID: {0}\n", platformIdTuple.Value);
                        }
                    }
                }
                else if (specializedType == typeof(NdefVcardRecordBase))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract business card info
                    var vcardRecord = await NdefVcardRecord.CreateFromGenericBaseRecord(record);
                    tagContents.Append("-> Business Card record" + Environment.NewLine);
                    tagContents.AppendFormat("vCard Version: {0}" + Environment.NewLine,
                                             vcardRecord.VCardFormatToWrite == VCardFormat.Version2_1 ? "2.1" : "3.0");
                    var contact = vcardRecord.ContactData;
                    var contactInfo = await contact.GetPropertiesAsync();
                    foreach (var curProperty in contactInfo.OrderBy(i => i.Key))
                    {
                        tagContents.Append(String.Format("{0}: {1}" + Environment.NewLine, curProperty.Key, curProperty.Value));
                    }
                }
                else
                {
                    // Other type, not handled by this demo
                    tagContents.Append("NDEF record not parsed by this demo app" + Environment.NewLine);
                }
            }
            // Update status text for UI
            SetStatusOutput(string.Format(AppResources.StatusTagParsed, tagContents));
        }
        #endregion

        #region NFC Publishing
        // ----------------------------------------------------------------------------------------------------

        private void BtnWriteLaunchApp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create a new LaunchApp record to launch this app
            // The app will print the arguments when it is launched (see MainPage.OnNavigatedTo() method)
            var record = new NdefLaunchAppRecord {Arguments = "Hello World"};
            // WindowsPhone is the pre-defined platform ID for WP8.
            // You can get the application ID from the WMAppManifest.xml file
            record.AddPlatformAppId("WindowsPhone", "{544ec154-b521-4d73-9405-963830adb213}");
            // Publish the record using the proximity device
            PublishRecord(record, true);
        }

        private void BtnWriteBusinessCard_Click(object sender, RoutedEventArgs e)
        {
            SetStatusOutput(AppResources.LoadingFirstContact);
            var cons = new Microsoft.Phone.UserData.Contacts();
            cons.SearchCompleted += Contacts_SearchCompleted;
            cons.SearchAsync(String.Empty, FilterKind.None, "Contacts Import from Address Book");
        }

        private async void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            ContactInformation contactInfo;
            if (!e.Results.Any())
            {
                // No contacts in the address book
                // Create demo contact
                contactInfo = new ContactInformation
                {
                    FamilyName = "Jakl",
                    GivenName = "Andreas"
                };
                var contactProps = await contactInfo.GetPropertiesAsync();
                contactProps.Add(KnownContactProperties.Url, "http://www.nfcinteractor.com/");
            }
            else
            {
                // Found a contact in the address book?
                // Use first contact from the address book
                var contact = e.Results.First();
                // Use library utility class to convert the Contact class from the WP address book
                // to the WP ContactInformation class that can convert to vCard format.
                contactInfo = await NdefVcardRecord.ConvertContactToInformation(contact);
            }

            // Create new NDEF record based on selected contact
            var vcardRecord = await NdefVcardRecord.CreateFromContactInformation(contactInfo);

            PublishRecord(vcardRecord, true);
        }

        private void BtnWriteMailTo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create a new mailto record, set the relevant properties for the email
            var record = new NdefMailtoRecord
                             {
                                 Address = "andreas.jakl@live.com",
                                 Subject = "Feedback for the NDEF Library",
                                 Body = "I think the NDEF library is ..."
                             };
            // Publish the record using the proximity device
            PublishRecord(record, true);
        }

        private void BtnPublishUri_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create a URI record
            var record = new NdefUriRecord { Uri = "http://www.nfcinteractor.com/" };
            // Publish the record using the proximity device
            PublishRecord(record, false);
        }

        private void PublishRecord(NdefRecord record, bool writeToTag)
        {
            if (_device == null) return;
            // Make sure we're not already publishing another message
            StopPublishingMessage(false);
            // Wrap the NDEF record into an NDEF message
            var message = new NdefMessage { record };
            // Convert the NDEF message to a byte array
            var msgArray = message.ToByteArray();
            // Publish the NDEF message to a tag or to another device, depending on the writeToTag parameter
            // Save the publication ID so that we can cancel publication later
            _publishingMessageId = _device.PublishBinaryMessage((writeToTag ? "NDEF:WriteTag" : "NDEF"), msgArray.AsBuffer(), MessageWrittenHandler);
            // Update status text for UI
            SetStatusOutput(string.Format((writeToTag ? AppResources.StatusWriteToTag : AppResources.StatusWriteToDevice), msgArray.Length, _publishingMessageId));
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
        }

        private void MessageWrittenHandler(ProximityDevice sender, long messageid)
        {
            // Stop publishing the message
            StopPublishingMessage(false);
            // Update status text for UI
            SetStatusOutput(AppResources.StatusMessageWritten);
        }
        #endregion

        #region Managing Subscriptions
        private void BtnStopSubscription_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Stop NDEF subscription and print status update to the UI
            StopSubscription(true);
        }

        private void StopSubscription(bool writeToStatusOutput)
        {
            if (_subscriptionIdNdef != 0 && _device != null)
            {
                // Ask the proximity device to stop subscribing for NDEF messages
                _device.StopSubscribingForMessage(_subscriptionIdNdef);
                _subscriptionIdNdef = 0;
                // Update enabled / disabled state of buttons in the User Interface
                UpdateUiForNfcStatus();
                // Update status text for UI - only if activated
                if (writeToStatusOutput) SetStatusOutput(AppResources.StatusSubscriptionStopped);
            }
        }

        private void BtnStopPublication_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StopPublishingMessage(true);
        }

        private void StopPublishingMessage(bool writeToStatusOutput)
        {
            if (_publishingMessageId != 0 && _device != null)
            {
                // Stop publishing the message
                _device.StopPublishingMessage(_publishingMessageId);
                _publishingMessageId = 0;
                // Update enabled / disabled state of buttons in the User Interface
                UpdateUiForNfcStatus();
                // Update status text for UI - only if activated
                if (writeToStatusOutput) SetStatusOutput(AppResources.StatusPublicationStopped);
            }
        }
        #endregion

        #region UI Management
        private void SetStatusOutput(string newStatus)
        {
            // Update the status output UI element in the UI thread
            // (some of the callbacks are in a different thread that wouldn't be allowed
            // to modify the UI thread)
            Dispatcher.BeginInvoke(() => { if (StatusOutput != null) StatusOutput.Text = newStatus; });
        }

        private string ConvertTypeNameFormatToString(NdefRecord.TypeNameFormatType tnf)
        {
            // Each record contains a type name format, which defines which format
            // the type name is actually in.
            // This method converts the constant to a human-readable string.
            string tnfString;
            switch (tnf)
            {
                case NdefRecord.TypeNameFormatType.Empty:
                    tnfString = "Empty NDEF record (does not contain a payload)";
                    break;
                case NdefRecord.TypeNameFormatType.NfcRtd:
                    tnfString = "NFC RTD Specification";
                    break;
                case NdefRecord.TypeNameFormatType.Mime:
                    tnfString = "RFC 2046 (Mime)";
                    break;
                case NdefRecord.TypeNameFormatType.Uri:
                    tnfString = "RFC 3986 (Url)";
                    break;
                case NdefRecord.TypeNameFormatType.ExternalRtd:
                    tnfString = "External type name";
                    break;
                case NdefRecord.TypeNameFormatType.Unknown:
                    tnfString = "Unknown record type; should be treated similar to content with MIME type 'application/octet-stream' without further context";
                    break;
                case NdefRecord.TypeNameFormatType.Unchanged:
                    tnfString = "Unchanged (partial record)";
                    break;
                case NdefRecord.TypeNameFormatType.Reserved:
                    tnfString = "Reserved";
                    break;
                default:
                    tnfString = "Unknown";
                    break;
            }
            return tnfString;
        }
        private void UpdateUiForNfcStatus()
        {
            Dispatcher.BeginInvoke(() =>
                                       {
                                           BtnInitNfc.IsEnabled = (_device == null);

                                           // Subscription buttons
                                           BtnSubscribeNdef.IsEnabled = (_device != null && _subscriptionIdNdef == 0);
                                           BtnStopSubscription.IsEnabled = (_device != null && _subscriptionIdNdef != 0);

                                           // Publishing buttons
                                           BtnWriteLaunchApp.IsEnabled = (_device != null && _publishingMessageId == 0);
                                           BtnWriteBusinessCard.IsEnabled = (_device != null && _publishingMessageId == 0);
                                           BtnWriteMailTo.IsEnabled = (_device != null && _publishingMessageId == 0);
                                           BtnPublishUri.IsEnabled = (_device != null && _publishingMessageId == 0);
                                           BtnStopPublication.IsEnabled = (_device != null && _publishingMessageId != 0);
                                       });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Check if app was launched via LaunchApp tag
            if (NavigationContext.QueryString.ContainsKey("ms_nfp_launchargs"))
            {
                // Update status text for UI
                // Print arguments retrieved from LaunchApp tag
                SetStatusOutput(string.Format(AppResources.StatusLaunchedFromTag, NavigationContext.QueryString["ms_nfp_launchargs"]));
            }
        }


        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Minimized };

            // Create a new menu item with the localized string from AppResources.
            var appBarMenuItemAbout = new ApplicationBarMenuItem(AppResources.MenuAbout);
            ApplicationBar.MenuItems.Add(appBarMenuItemAbout);
            appBarMenuItemAbout.Click += CmdAbout;
        }

        private void CmdAbout(object sender, EventArgs e)
        {
            // Navigate to the about page
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }
        #endregion

    }
}