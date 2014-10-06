/****************************************************************************
**
** Copyright (C) 2012-2014 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2013).
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Phone.PersonalInformation;
using Windows.Storage.Streams;
using Microsoft.Phone.UserData;
using NdefLibrary.Ndef;

namespace NdefLibraryWp.Ndef
{
    /// <summary>
    /// Work with contact / vCard / vcf information in an NDEF record, and 
    /// convert the Windows Phone Contact or ContactInformation classes
    /// to / from a valid vCard representation.
    /// </summary>
    /// <remarks>
    /// Windows Phone supports two contact storage related classes: Contact and
    /// ContactInformation.
    /// 
    /// Contact is used in the address book to represent a contact. However,
    /// only the ContactInformation class supports the vCard standard and is
    /// able to export / import vCard data.
    /// 
    /// Unfortunately, the Microsoft APIs do not provide conversion between
    /// the two contact storage classes present in Windows Phone. Most times,
    /// you have to work with Contact instances, as this is what you need when
    /// interacting with the address book.
    /// 
    /// Therefore, this class adds conversation between these two classes, so that
    /// you can create a vCard also based on the Contact class, to enable real-world
    /// use of the NFC library.
    /// 
    /// As the information those two classes can store does not exactly match,
    /// this class is doing its best to support a great conversation and aims to retain
    /// as much information as possible. This works very well for the most common fields,
    /// but of course some additional information might get lost or changed during the
    /// transformation.
    /// 
    /// Using the NdefVcardRecord for Windows Phone
    /// 
    /// As most operations when working with contacts on WP are asynchronous, you can 
    /// only construct a class instance using static constructor methods (factory pattern).
    /// These methods will then parse the supplied contact information (both Contact as well
    /// as ContactInformation) and convert that to a vCard payload suitable to be stored
    /// on an NFC tag.
    /// 
    /// If you read an NDEF record from a tag, use CreateFromGenericBaseRecord() to 
    /// convert the NdefRecord base class into a NdefVcardRecord and to immediately parse
    /// the vCard data into the ContactData property.
    /// 
    /// To convert from Contact to ContactInformation in your app, the conversion
    /// method of this class have been made public: 
    /// Contact -> ContactInformation: ConvertContactToInformation()
    /// </remarks>
    public class NdefVcardRecord : NdefVcardRecordBase
    {
        public VCardFormat VCardFormatToWrite { set; get; }

        // Blocks UI thread, so don't parse payload here, let the user assemble payload manually
        // in AssemblePayload()
        public ContactInformation ContactData { get; protected set; }

        /// <summary>
        /// Creates a new instance of an NdefVcardRecord and imports
        /// the payload into contact information.
        /// </summary>
        /// <param name="record">An NDEF record of type NdefRecord that contains
        /// valid vCard data as its payload and has the right type information.</param>
        /// <returns>A new instance of an NdefVcardRecord that internalized the original
        /// payload into its internal contact data storage.</returns>
        public async static Task<NdefVcardRecord> CreateFromGenericBaseRecord(NdefRecord record)
        {
            var vcardRecord = new NdefVcardRecord(record);
            await vcardRecord.ParsePayloadToContact();
            return vcardRecord;
        }

        /// <summary>
        /// Create a new NdefVcardRecord based on an already supplied contact information
        /// from Windows Phone 8 ContactInformation data. The ContactInformation type is able
        /// to convert data into the vCard format.
        /// </summary>
        /// <param name="contactInfo">ContactInformation to be used for creating this record.</param>
        /// <param name="vCardFormat">Optionally specify the VCardFormat to use for creating
        /// the textual payload data.</param>
        /// <returns>A new instance of an NdefVcardRecord that uses the supplied contact information.</returns>
        public async static Task<NdefVcardRecord> CreateFromContactInformation(ContactInformation contactInfo, VCardFormat vCardFormat = VCardFormat.Version2_1)
        {
            var vcardRecord = new NdefVcardRecord
            {
                VCardFormatToWrite = vCardFormat,
                ContactData = contactInfo
            };
            await vcardRecord.AssemblePayload();
            return vcardRecord;
        }

        /// <summary>
        /// Create a new NdefVcardRecord based on an already supplied contact information
        /// from Windows Phone 8 Contact data. This data type is used by the WP address book.
        /// Internally, this class will convert the Contact class instance to a ContactInformation
        /// instance using an own mapping algorithm, as only ContactInformation can be used
        /// for working with vCard data.
        /// </summary>
        /// <param name="contact">Contact to be used for creating this record.</param>
        /// <param name="vCardFormat">Optionally specify the VCardFormat to use for creating
        /// the textual payload data.</param>
        /// <returns>A new instance of an NdefVcardRecord that uses the supplied contact information.</returns>
        public async static Task<NdefVcardRecord> CreateFromContact(Contact contact,
            VCardFormat vCardFormat = VCardFormat.Version2_1)
        {
            var contactInfo = await ConvertContactToInformation(contact);
            return await CreateFromContactInformation(contactInfo, vCardFormat);
        }

        /// <summary>
        /// Hide normal constructor - create instance of this class using the factory method
        /// CreateFromContactInformation().
        /// </summary>
        private NdefVcardRecord() {}

        /// <summary>
        /// Hide normal constructor - create instance of this class using the factory method
        /// CreateFromContactInformation().
        /// </summary>
        private NdefVcardRecord(NdefRecord other) : base(other) { }

        /// <summary>
        /// Takes the information stored in the payload of the record and converts that
        /// to a Contact.
        /// </summary>
        /// <returns>Task to await completion of the operation.</returns>
        public async Task ParsePayloadToContact()
        {
            if (Payload != null)
            {
                await ParseDataToContact(Payload);
            }
        }


        /// <summary>
        /// Takes the ContactData instance and converts it to a vCard representation that
        /// is assigned to the payload of this record instance.
        /// </summary>
        /// <returns>Task to await completion of the operation.</returns>
        public async Task AssemblePayload()
        {
            if (ContactData.FamilyName == null) ContactData.FamilyName = String.Empty;
            if (ContactData.GivenName == null) ContactData.GivenName = String.Empty;
            var vCard = await ContactData.ToVcardAsync(VCardFormatToWrite);

            var vCardBytes = new byte[vCard.Size];

            await vCard.ReadAsync(vCardBytes.AsBuffer(), (uint)vCard.Size, InputStreamOptions.None);
            Payload = vCardBytes;
        }

        /// <summary>
        /// Parse the supplied string containing vCard information (= vcf data) into a
        /// Windows Phone ContactInformation class (the ContactData property).
        /// </summary>
        /// <param name="vCardString">String that contains the vCard data to parse.</param>
        /// <returns>Task to await completion of the operation.</returns>
        public async Task ParseDataToContact(string vCardString)
        {
            await ParseDataToContact(Encoding.UTF8.GetBytes(vCardString));
        }

        /// <summary>
        /// Parse the supplied byte array containing UTF-8 encoded vCard information (= vcf data) into a
        /// Windows Phone ContactInformation class (the ContactData property).
        /// </summary>
        /// <param name="vCardByte">UTF-8 encoded byte array that contains the vCard data to parse.</param>
        /// <returns>Task to await completion of the operation.</returns>
        public async Task ParseDataToContact(byte[] vCardByte)
        {
            var ms = new System.IO.MemoryStream(vCardByte);
            var input = System.IO.WindowsRuntimeStreamExtensions.AsInputStream(ms);
            // Find out vCard version (not done automatically)
            var vCardVersionPattern = new Regex(@"^VERSION:(?<vcvers>.+)$", RegexOptions.Multiline);
            var vCardVersionMatch = vCardVersionPattern.Match(Encoding.UTF8.GetString(vCardByte, 0, vCardByte.Length));
            if (vCardVersionMatch.Success)
            {
                var vCardVersion = vCardVersionMatch.Groups["vcvers"].Value;
                VCardFormatToWrite = vCardVersion.Trim().Equals("2.1") ? VCardFormat.Version2_1 : VCardFormat.Version3;
            }
            // Parse vCard
            ContactData = await ContactInformation.ParseVcardAsync(input);
        }

        /// <summary>
        /// Called when reading a contact from the address book (Contact instance) 
        /// and we need to convert it to a ContactInformation instance that
        /// supports conversion to a vCard.
        /// </summary>
        /// <remarks>
        /// Note that there is no 1:1 conversion possible between those two formats,
        /// as they have a very different structure. This method does its best to
        /// map and retain as much information as possible.
        /// 
        /// Currently, this method is only working with KnownContactProperties of
        /// the ContactInformation object, and doesn't define custom properties.
        /// </remarks>
        /// <param name="contact">the contact from the address book to convert.</param>
        /// <returns>Converted contact information.</returns>
        public static async Task<ContactInformation> ConvertContactToInformation(Contact contact)
        {
            var contactInfo = new ContactInformation();
            var contactProps = await contactInfo.GetPropertiesAsync();

            contactProps.Add(KnownContactProperties.DisplayName, contact.DisplayName);

            // CompleteName
            var completeName = contact.CompleteName;
            AddContactPropertyIfPossible(KnownContactProperties.GivenName, completeName.FirstName, contactProps);
            AddContactPropertyIfPossible(KnownContactProperties.FamilyName, completeName.LastName, contactProps);
            AddContactPropertyIfPossible(KnownContactProperties.AdditionalName, completeName.MiddleName, contactProps);
            AddContactPropertyIfPossible(KnownContactProperties.Nickname, completeName.Nickname, contactProps);
            AddContactPropertyIfPossible(KnownContactProperties.HonorificPrefix, completeName.Title, contactProps);
            AddContactPropertyIfPossible(KnownContactProperties.HonorificSuffix, completeName.Suffix, contactProps);
            AddContactPropertyIfPossible(KnownContactProperties.YomiFamilyName, completeName.YomiLastName, contactProps);
            AddContactPropertyIfPossible(KnownContactProperties.YomiGivenName, completeName.YomiFirstName, contactProps);

            // Addresses
            foreach (var curAddress in contact.Addresses)
            {
                switch (curAddress.Kind)
                {
                    case AddressKind.Home:
                        AddAddressPropertyIfPossible(KnownContactProperties.Address, curAddress.PhysicalAddress, contactProps);
                        break;
                    case AddressKind.Work:
                        AddAddressPropertyIfPossible(KnownContactProperties.WorkAddress, curAddress.PhysicalAddress, contactProps);
                        break;
                    case AddressKind.Other:
                        AddAddressPropertyIfPossible(KnownContactProperties.OtherAddress, curAddress.PhysicalAddress, contactProps);
                        break;
                }
            }

            // Birthdays
            foreach (var curBirthday in contact.Birthdays)
            {
                AddContactPropertyIfPossible(KnownContactProperties.Birthdate, new DateTimeOffset(curBirthday), contactProps);
            }

            // Children
            AddStringContactProperties(contact.Children, KnownContactProperties.Children, contactProps);

            // Companies
            foreach (var curCompany in contact.Companies)
            {
                AddContactPropertyIfPossible(KnownContactProperties.CompanyName, curCompany.CompanyName, contactProps);
                AddContactPropertyIfPossible(KnownContactProperties.JobTitle, curCompany.JobTitle, contactProps);
                AddContactPropertyIfPossible(KnownContactProperties.OfficeLocation, curCompany.OfficeLocation, contactProps);
                AddContactPropertyIfPossible(KnownContactProperties.YomiCompanyName, curCompany.YomiCompanyName, contactProps);
            }

            // EmailAddresses
            foreach (var curEmail in contact.EmailAddresses)
            {
                switch (curEmail.Kind)
                {
                    case EmailAddressKind.Personal:
                        AddContactPropertyIfPossible(KnownContactProperties.Email, curEmail.EmailAddress, contactProps);
                        break;
                    case EmailAddressKind.Work:
                        AddContactPropertyIfPossible(KnownContactProperties.WorkEmail, curEmail.EmailAddress, contactProps);
                        break;
                    case EmailAddressKind.Other:
                        AddContactPropertyIfPossible(KnownContactProperties.OtherEmail, curEmail.EmailAddress, contactProps);
                        break;
                }
            }

            // Notes
            AddStringContactProperties(contact.Notes, KnownContactProperties.Notes, contactProps);

            // PhoneNumbers
            foreach (var curNumber in contact.PhoneNumbers)
            {
                switch (curNumber.Kind)
                {
                    case PhoneNumberKind.Mobile:
                        AddContactPropertyIfPossible(KnownContactProperties.MobileTelephone, curNumber.PhoneNumber, contactProps);
                        break;
                    case PhoneNumberKind.Home:
                        AddContactPropertyIfPossible(KnownContactProperties.Telephone, curNumber.PhoneNumber, contactProps);
                        break;
                    case PhoneNumberKind.Work:
                        AddContactPropertyIfPossible(KnownContactProperties.WorkTelephone, curNumber.PhoneNumber, contactProps);
                        break;
                    case PhoneNumberKind.Company:
                        AddContactPropertyIfPossible(KnownContactProperties.CompanyTelephone, curNumber.PhoneNumber, contactProps);
                        break;
                    case PhoneNumberKind.Pager:
                        // N/A
                        break;
                    case PhoneNumberKind.HomeFax:
                        AddContactPropertyIfPossible(KnownContactProperties.HomeFax, curNumber.PhoneNumber, contactProps);
                        break;
                    case PhoneNumberKind.WorkFax:
                        AddContactPropertyIfPossible(KnownContactProperties.WorkFax, curNumber.PhoneNumber, contactProps);
                        break;
                }
            }

            // SignificantOthers
            AddStringContactProperties(contact.SignificantOthers, KnownContactProperties.SignificantOther, contactProps);

            // Websites
            AddStringContactProperties(contact.Websites, KnownContactProperties.Url, contactProps);

            // Display name
            contactInfo.DisplayName = contact.DisplayName;

            return contactInfo;

        }

        private static void AddAddressPropertyIfPossible(string contactPropertyName, System.Device.Location.CivicAddress civicAddress, IDictionary<string, object> contactProps)
        {
            // Mapping is very difficult and not 1:1

            // *** VCard 2.1 specs -> Windows.Phone.PersonalInformation.ContactAddress
            // The property value is a concatenation of the 
            // Post Office Address (first field) = ? (Missing)
            // Extended Address (second field)  = ? (Missing)
            // Street (third field)             = contactAddress.StreetAddress
            // Locality (fourth field)          = contactAddress.Locality
            // Region (fifth field)             = contactAddress.Region
            // Postal Code (six field)          = contactAddress.PostalCode
            // Country (seventh field)          = contactAddress.Country 
            // An example of this property follows:
            // ADR;DOM;HOME:P.O. Box 101;Suite 101;123 Main Street;Any Town;CA;91921-1234;

            // *** System.Device.Location.CivicAddress
            // * CivicAddress *                                                         * Mapping to ContactAddress*
            // AddressLine1:	Gets or sets the first line of the address.             -> Street
            // AddressLine2:	Gets or sets the second line of the address.            -> +Street (?)
            // Building:    	Gets or sets the building name or number.               -> +Street (??)
            // City:        	Gets or sets the name of the city.                      -> Locality (?)
            // CountryRegion:	Gets or sets the country or region of the location.     -> Country
            // FloorLevel:  	Gets or sets the floor level of the location.           -> +Street (??)
            // PostalCode:  	Gets or sets the postal code of the location.           -> PostalCode
            // StateProvince:	Gets or sets the state or province of the location.     -> Region

            // IsUnknown:   	Gets a value that indicates whether the CivicAddress contains data.

            // Linefeeds seem to work fine in the mapping - understood by WP + Symbian. But trigger quoted printable encoding, increasing vCard size.

            var contactAddress = new Windows.Phone.PersonalInformation.ContactAddress();
            contactAddress.StreetAddress = civicAddress.AddressLine1;
            if (!String.IsNullOrEmpty(civicAddress.AddressLine2))
            {
                contactAddress.StreetAddress += "\n" + civicAddress.AddressLine2;
            }
            if (!String.IsNullOrEmpty(civicAddress.Building))
            {
                contactAddress.StreetAddress += "\n" + civicAddress.Building;
            }
            if (!String.IsNullOrEmpty(civicAddress.FloorLevel))
            {
                contactAddress.StreetAddress += "\n" + civicAddress.FloorLevel;
            }
            contactAddress.Locality = civicAddress.City;
            contactAddress.PostalCode = civicAddress.PostalCode;
            contactAddress.Region = civicAddress.StateProvince;
            contactAddress.Country = civicAddress.CountryRegion;

            // Finally, add our constructed address to the contact properties
            AddContactPropertyIfPossible(contactPropertyName, contactAddress, contactProps);
        }

        private static void AddStringContactProperties(IEnumerable<string> listOfValues, string knownContactProperty, IDictionary<string, object> contactProps)
        {
            foreach (var curItem in listOfValues)
            {
                AddContactPropertyIfPossible(knownContactProperty, curItem, contactProps);
            }
        }

        private static bool AddContactPropertyIfPossible(string contactPropertyName, object property, IDictionary<string, object> contactProps)
        {
            // First, assign to original contact property if not yet present
            if (!contactProps.ContainsKey(contactPropertyName))
            {
                contactProps.Add(contactPropertyName, property);
                return true;
            }
            // Check if there is an alternative property available for that property name
            var alternateProperty = AlternativeContactProperty(contactPropertyName);
            if (alternateProperty != null)
            {
                // Check if that property has not yet been assigned
                if (!contactProps.ContainsKey(alternateProperty))
                {
                    contactProps.Add(alternateProperty, property);
                    return true;
                }
            }
            // TODO: maybe add custom properties if no known are available? (e.g., "MobilePhone #2")
            return false;
        }

        private static string AlternativeContactProperty(string originalProperty)
        {
            if (originalProperty.Equals(KnownContactProperties.MobileTelephone))
                return KnownContactProperties.AlternateMobileTelephone;
            if (originalProperty.Equals(KnownContactProperties.WorkTelephone))
                return KnownContactProperties.AlternateWorkTelephone;
            if (originalProperty.Equals(KnownContactProperties.Email))
                return KnownContactProperties.OtherEmail;
            if (originalProperty.Equals(KnownContactProperties.Address))
                return KnownContactProperties.OtherAddress;

            return null;
        }
    }
}
