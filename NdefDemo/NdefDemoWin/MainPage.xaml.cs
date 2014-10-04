/****************************************************************************
**
** Copyright (C) 2012 - 2014 Andreas Jakl.
** All rights reserved.
**
** Code example for the NDEF Library for Proximity APIs (NFC).
**
** Created by Andreas Jakl (2013).
** More information: http://ndef.codeplex.com/
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
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Proximity;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using NdefDemoWin.Common;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NdefLibrary.Ndef;
using NdefLibraryWin.Ndef;

namespace NdefDemoWin
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly NavigationHelper _navigationHelper;

        private ProximityDevice _device;
        private long _subscriptionIdNdef;
        private long _publishingMessageId;
        private readonly CoreDispatcher _dispatcher;
        private readonly ResourceLoader _loader = new ResourceLoader();

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }


        public MainPage()
        {
            InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += NavigationHelper_LoadState;
            _navigationHelper.SaveState += NavigationHelper_SaveState;

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return;
            }
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void BtnInitNfc_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Initialize NFC
            _device = ProximityDevice.GetDefault();
            // Update status text for UI
            SetStatusOutput(_loader.GetString(_device != null ? "StatusInitialized" : "StatusInitFailed"));
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
        }

        #region Subscribe for tags
        // ----------------------------------------------------------------------------------------------------
        private void BtnSubscribeNdef_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Only subscribe for messages if no NDEF subscription is already active
            if (_subscriptionIdNdef != 0) return;
            // Ask the proximity device to inform us about any kind of NDEF message received from
            // another device or tag.
            // Store the subscription ID so that we can cancel it later.
            _subscriptionIdNdef = _device.SubscribeForMessage("NDEF", MessageReceivedHandler);
            // Update status text for UI
            SetStatusOutput(string.Format(_loader.GetString("StatusSubscribed"), _subscriptionIdNdef));
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

            // Parse tag contents
            try
            {
                // Clear bitmap if the last tag contained an image
                SetStatusImage(null);

                // Parse the contents of the tag
                await ParseTagContents(ndefMessage, tagContents);

                // Update status text for UI
                SetStatusOutput(string.Format(_loader.GetString("StatusTagParsed"), tagContents));
            }
            catch (Exception ex)
            {
                SetStatusOutput(string.Format(_loader.GetString("StatusNfcParsingError"), ex.Message));
            }

        }

        private async Task ParseTagContents(NdefMessage ndefMessage, StringBuilder tagContents)
        {
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
                else if (specializedType == typeof(NdefSpRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract Smart Poster info
                    var spRecord = new NdefSpRecord(record);
                    tagContents.Append("-> Smart Poster record\n");
                    tagContents.AppendFormat("URI: {0}", spRecord.Uri);
                    tagContents.AppendFormat("Titles: {0}", spRecord.TitleCount());
                    if (spRecord.TitleCount() > 1)
                        tagContents.AppendFormat("1. Title: {0}", spRecord.Titles[0].Text);
                    tagContents.AppendFormat("Action set: {0}", spRecord.ActionInUse());
                    // You can also check the action (if in use by the record), 
                    // mime type and size of the linked content.
                }
                else if (specializedType == typeof(NdefVcardRecordBase))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract business card info
                    var vcardRecord = new NdefVcardRecord(record);
                    tagContents.Append("-> Business Card record" + Environment.NewLine);
                    var contact = vcardRecord.ContactData;

                    // Contact has phone or email info set? Use contact manager to show the contact card
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (contact.Emails.Any() || contact.Phones.Any())
                        {
                            var rect = GetElementRect(StatusOutput);
                            ContactManager.ShowContactCard(contact, rect, Placement.Below);
                        }
                        else
                        {
                            // No phone or email set - contact manager would not show the contact card.
                            // -> parse manually
                            tagContents.AppendFormat("Name: {0}\n", contact.DisplayName);
                            tagContents.Append("[not parsing other values in the demo app]");
                        }
                    });
                }
                else if (specializedType == typeof(NdefIcalendarRecordBase))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract iCalendar info
                    var icalendarRecord = new NdefIcalendarRecord(record);
                    tagContents.Append("-> iCalendar record" + Environment.NewLine);
                    var ap = icalendarRecord.AppointmentData;
                    if (!String.IsNullOrEmpty(ap.Subject))
                        tagContents.AppendFormat("Subject: {0}\n", ap.Subject);
                    if (!String.IsNullOrEmpty(ap.Details))
                        tagContents.AppendFormat("Details: {0}\n", ap.Details);
                    if (!String.IsNullOrEmpty(ap.Organizer.Address))
                        tagContents.AppendFormat("Organizer Address: {0}\n", ap.Organizer.Address);
                    if (!String.IsNullOrEmpty(ap.Location))
                        tagContents.AppendFormat("Location: {0}\n", ap.Location);
                    tagContents.AppendFormat("Start time: {0}\n", ap.StartTime);
                    tagContents.AppendFormat("Duration: {0}\n", ap.Duration);
                    tagContents.AppendFormat("AllDay? {0}\n", ap.AllDay ? "yes" : "no");
                    if (ap.Reminder != null)
                        tagContents.AppendFormat("Reminder: {0}\n", ap.Reminder);
                }
                else if (specializedType == typeof(NdefLaunchAppRecord))
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
                else if (specializedType == typeof (NdefMimeImageRecordBase))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract Image record info
                    var imgRecord = new NdefMimeImageRecord(record);
                    tagContents.Append("-> MIME / Image record" + Environment.NewLine);
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => SetStatusImage(await imgRecord.GetImageAsBitmap()));
                    
                }
                else
                {
                    // Other type, not handled by this demo
                    tagContents.Append("NDEF record not parsed by this demo app" + Environment.NewLine);
                }
            }
        }

        private Rect GetElementRect(FrameworkElement element)
        {
            var elementTransform = element.TransformToVisual(null);
            var point = elementTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }
        #endregion

        #region NFC Publishing
        // ----------------------------------------------------------------------------------------------------

        private void BtnWriteLaunchApp_Click(object sender, RoutedEventArgs e)
        {
            // Create a new LaunchApp record to launch this app
            // The app will print the arguments when it is launched (see MainPage.OnNavigatedTo() method)
            var record = new NdefLaunchAppRecord { Arguments = "Hello World" };
            // WindowsPhone is the pre-defined platform ID for WP8.
            // You can get the application ID from the WMAppManifest.xml file
            record.AddPlatformAppId("WindowsPhone", "{544ec154-b521-4d73-9405-963830adb213}");
            // The app platform for a Windows 8 computer is Windows. 
            // The format of the proximity app Id is <package family name>!<app Id>. 
            // You can get the package family name from the Windows.ApplicationModel.Package.Current.Id.FamilyName property. 
            // You must copy the app Id value from the Id attribute of the Application element in the 
            // package manifest for your app.
            record.AddPlatformAppId("Windows", Windows.ApplicationModel.Package.Current.Id.FamilyName + "!" + "NdefDemoWin");

            // Publish the record using the proximity device
            PublishRecord(record, true);
        }

        private void BtnWriteBusinessCard_Click(object sender, RoutedEventArgs e)
        {
            var contact = new Contact
            {
                FirstName = "Andreas",
                LastName = "Jakl"
            };
            // Add the personal email address to the Contact object’s emails vector
            var personalEmail = new ContactEmail { Address = "andreas.jakl@mopius.com", Kind = ContactEmailKind.Work };
            contact.Emails.Add(personalEmail);

            // Adds the home phone number to the Contact object’s phones vector
            var homePhone = new ContactPhone { Number = "+1234", Kind = ContactPhoneKind.Home };
            contact.Phones.Add(homePhone);

            // Adds the address to the Contact object’s addresses vector
            var workAddress = new ContactAddress
            {
                StreetAddress = "Technologiestrasse 10",
                Locality = "Vienna",
                Region = "Austria",
                PostalCode = "1120",
                Kind = ContactAddressKind.Work
            };
            contact.Addresses.Add(workAddress);

            contact.Websites.Add(new ContactWebsite { Uri = new Uri("http://www.mopius.com/") });

            contact.JobInfo.Add(new ContactJobInfo
            {
                CompanyName = "Mopius",
                Title = "Chief Technology Wizard"
            });
            contact.Notes = "Developer of the NFC Library";

            var record = new NdefVcardRecord(contact);

            // Publish the record using the proximity device
            PublishRecord(record, true);
        }


        private void BtnWriteMailTo_Click(object sender, RoutedEventArgs e)
        {
            // Create a new mailto record, set the relevant properties for the email
            var record = new NdefMailtoRecord
            {
                Address = "andreas.jakl@mopius.com",
                Subject = "Feedback for the NDEF Library",
                Body = "I think the NDEF library is ..."
            };
            // Publish the record using the proximity device
            PublishRecord(record, true);
        }

        private async void BtnWriteImageTo_Click(object sender, RoutedEventArgs e)
        {
            // Load an image

            var openPicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
            };

            // Filter to include a sample subset of file types.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");

            // Open the file picker.
            var file = await openPicker.PickSingleFileAsync();

            // file is null if user cancels the file picker.
            if (file != null)
            {
                var record = await NdefMimeImageRecord.CreateFromFile(file);
                // Publish the record using the proximity device
                PublishRecord(record, true);
            }
        }

        private void BtnPublishUri_Click(object sender, RoutedEventArgs e)
        {
            // Create a URI record
            var record = new NdefUriRecord { Uri = "http://www.nfcinteractor.com/" };
            // Publish the record using the proximity device
            PublishRecord(record, false);
        }

        private void BtnPublishIcalendar_Click(object sender, RoutedEventArgs e)
        {
            // Warning: iCalendar support is currently still in alpha in the library!
            // Functionality and APIs might change in future updates.
            var ap = new Appointment
            {
                Subject = "Spread the word",
                Details = "Inform other developers about the open source NFC / NDEF Library",
                Organizer = new AppointmentOrganizer
                {
                    Address = "andreas.jakl@mopius.com",
                    DisplayName = "Andreas Jakl"
                },
                Location = "Email, Facebook, Twitter",
                AllDay = true,
                StartTime = DateTimeOffset.UtcNow.AddDays(1),
                Reminder = new TimeSpan(0, 0, -30, 0)
            };

            var record = new NdefIcalendarRecord(ap);

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
            try
            {
                // Publish the NDEF message to a tag or to another device, depending on the writeToTag parameter
                // Save the publication ID so that we can cancel publication later
                _publishingMessageId = _device.PublishBinaryMessage((writeToTag ? "NDEF:WriteTag" : "NDEF"), msgArray.AsBuffer(), MessageWrittenHandler);
                // Update status text for UI
                SetStatusOutput(string.Format(_loader.GetString(writeToTag ? "StatusWriteToTag" : "StatusWriteToDevice"), msgArray.Length, _publishingMessageId));
                // Update enabled / disabled state of buttons in the User Interface
                UpdateUiForNfcStatus();
            }
            catch (Exception e)
            {
                SetStatusOutput(string.Format(_loader.GetString("StatusPublicationError"), e.Message));
            }
        }

        private void MessageWrittenHandler(ProximityDevice sender, long messageid)
        {
            // Stop publishing the message
            StopPublishingMessage(false);
            // Update status text for UI
            SetStatusOutput(_loader.GetString("StatusMessageWritten"));
        }
        #endregion

        #region Managing Subscriptions
        private void BtnStopSubscription_Click(object sender, RoutedEventArgs e)
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
                if (writeToStatusOutput) SetStatusOutput(_loader.GetString("StatusSubscriptionStopped"));
            }
        }

        private void BtnStopPublication_Click(object sender, RoutedEventArgs e)
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
                if (writeToStatusOutput) SetStatusOutput(_loader.GetString("StatusPublicationStopped"));
            }
        }
        #endregion

        #region UI Management

        private void SetStatusOutput(string newStatus)
        {
            // Update the status output UI element in the UI thread
            // (some of the callbacks are in a different thread that wouldn't be allowed
            // to modify the UI thread)
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { if (StatusOutput != null) StatusOutput.Text = newStatus; });
        }

        private void SetStatusImage(WriteableBitmap newImg)
        {
            // Update the status output UI element in the UI thread
            // (some of the callbacks are in a different thread that wouldn't be allowed
            // to modify the UI thread)
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusImg.Source = newImg;
                if (newImg != null)
                {
                    StatusImg.Width = newImg.PixelWidth;
                    StatusImg.Height = newImg.PixelHeight;   
                }
            });
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
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                BtnInitNfc.IsEnabled = (_device == null);

                // Subscription buttons
                BtnSubscribeNdef.IsEnabled = (_device != null && _subscriptionIdNdef == 0);
                BtnStopSubscription.IsEnabled = (_device != null && _subscriptionIdNdef != 0);

                // Publishing buttons
                BtnWriteLaunchApp.IsEnabled = (_device != null && _publishingMessageId == 0);
                BtnWriteBusinessCard.IsEnabled = (_device != null && _publishingMessageId == 0);
                BtnPublishIcalendar.IsEnabled = (_device != null && _publishingMessageId == 0);
                BtnWriteMailTo.IsEnabled = (_device != null && _publishingMessageId == 0);
                BtnWriteImage.IsEnabled = (_device != null && _publishingMessageId == 0);
                BtnPublishUri.IsEnabled = (_device != null && _publishingMessageId == 0);
                BtnStopPublication.IsEnabled = (_device != null && _publishingMessageId != 0);
            });
        }
        #endregion

    }
}
