/****************************************************************************
**
** Copyright (C) 2012-2016 Andreas Jakl - http://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
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
using System;
using System.IO;
using System.Text;
using Windows.ApplicationModel.Contacts;
using NdefLibrary.Ndef;
using Thought.vCards;
using VcardLibrary;

namespace NdefLibraryWin.Ndef
{
    /// <summary>
    /// Work with contact / vCard / vcf information in an NDEF record, and 
    /// convert the WinRT Contact storage to / from a valid vCard representation.
    /// </summary>
    /// <remarks>
    /// WinRT is using the Contact class to store contact information for use
    /// in its address book. However, this class does not include conversion to
    /// and from the vCard standard that can be used to store the contact information
    /// in files (.vcf) or to include it as a payload for a contact NDEF record.
    /// 
    /// Furthermore, the Contact class does not actually match the vCard standard
    /// from its supported features and information details.
    /// 
    /// Therefore, this class depends on an external vCard library that is used
    /// so that the NFC library can convert between the WinRT Contact class and 
    /// the vCard library that can generate and parse vcf contents.
    /// 
    /// As that matching is not 1:1, some details might not be represented accurately
    /// after the conversion or might even get lost. But the class tries to retain
    /// as much information as possible and maps data between the WinRT Contact class
    /// and the vCard standard with care.
    /// 
    /// You can either create a class instance from a WinRT Contact, or from a
    /// vCard string / UTF-8 encoded byte array. The class will handle automatic
    /// conversion between the two formats upon construction.
    /// </remarks>
    public class NdefVcardRecord : NdefVcardRecordBase
    {
        private Contact _contactData;

        /// <summary>
        /// Contact from WinRT that is represented as a vCard by this class.
        /// </summary>
        /// <remarks>
        /// If you make any changes to this class instance, call 
        /// </remarks>
        public Contact ContactData
        {
            get { return _contactData; }
            set
            {
                _contactData = value;
                AssemblePayload();
            }
        }

        #region Constructors and Initialization
        /// <summary>
        /// Construct an emtpy vCard record with no contact set.
        /// </summary>
        public NdefVcardRecord() { }

        /// <summary>
        /// Creates a new instance of an NdefVcardRecord and imports
        /// the payload into the ContactData instance.
        /// </summary>
        /// <param name="other">An NDEF record that contains
        /// valid vCard data as its payload and has the right type information.</param>
        public NdefVcardRecord(NdefRecord other) : base(other)
        {
            ContactData = ParseDataToContact(_payload);
        }

        /// <summary>
        /// Construct a new vCard record based on the WinRT contact instance,
        /// which is automatically converted to a vCard for the payload of this record.
        /// </summary>
        /// <param name="contact">Contact instance that should be converted to a vCard
        /// for this payload's record.</param>
        public NdefVcardRecord(Contact contact)
        {
            ContactData = contact;
        }

        /// <summary>
        /// Create a new vCard record based on the string that contains valid vCard
        /// information. Automatically parses the vCard data and populates fields in
        /// the WinRT Contact instance accessible through ContactData.
        /// </summary>
        /// <param name="vCardString">String that contains vCard data.</param>
        public NdefVcardRecord(string vCardString)
        {
            ContactData = ParseDataToContact(Encoding.UTF8.GetBytes(vCardString));
        }

        /// <summary>
        /// Create a new vCard record based on the byte array that contains valid vCard
        /// information. Automatically parses the vCard data and populates fields in
        /// the WinRT Contact instance accessible through ContactData.
        /// </summary>
        /// <param name="vCardByte">Byte array that contains vCard data.</param>
        public NdefVcardRecord(byte[] vCardByte)
        {
            ContactData = ParseDataToContact(vCardByte);
        }

        /// <summary>
        /// Convert the ContactData instance into a vCard for the payload of this record.
        /// </summary>
        private void AssemblePayload()
        {
            var vCard = ConvertContactToVCard(ContactData);
            var vcWriter = new vCardStandardWriter();

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            vcWriter.Write(vCard, writer);
            writer.Flush();
            Payload = ms.ToArray();
        }
        #endregion

        #region Convert vCard to Windows 8 Contact

        /// <summary>
        /// Convert the byte array to a WinRT Contact.
        /// Matching is not 1:1, but this methods tries to convert as much of the data as possible.
        /// </summary>
        /// <param name="vCardByte">Business card vCard string as a UTF8 encoded byte array.</param>
        private Contact ParseDataToContact(byte[] vCardByte)
        {
            var vcReader = new vCardStandardReader();
            var vc = vcReader.Read(new StringReader(Encoding.UTF8.GetString(vCardByte, 0, vCardByte.Length)));

            var convertedContact = new Contact
            {
                FirstName = vc.GivenName, 
                LastName = vc.FamilyName,
                HonorificNamePrefix = vc.NamePrefix,
                HonorificNameSuffix = vc.NameSuffix
            };

            // Phone numbers
            foreach (var phone in vc.Phones)
            {
                convertedContact.Phones.Add(ConvertVcardToPhone(phone));
            }

            // Email
            foreach (var email in vc.EmailAddresses)
            {
                convertedContact.Emails.Add(ConvertVcardToEmail(email));
            }

            // Postal address
            foreach (var address in vc.DeliveryAddresses)
            {
                convertedContact.Addresses.Add(ConvertVcardToAddress(address));
            }

            // Notes
            foreach (var note in vc.Notes)
            {
                if (convertedContact.Notes != null)
                {
                    convertedContact.Notes += "\n" + note.Text;
                }
                else
                {
                    convertedContact.Notes = note.Text;
                }
            }

            // Dates
            if (vc.BirthDate != null)
            {
                var birthDay = (DateTime) vc.BirthDate;
                convertedContact.ImportantDates.Add(new ContactDate
                {
                    Kind = ContactDateKind.Birthday,
                    Year = birthDay.Year,
                    Month = (uint?) birthDay.Month,
                    Day = (uint?) birthDay.Day
                });
            }

            // Websites
            foreach (var website in vc.Websites)
            {
                convertedContact.Websites.Add(ConvertVcardToWebsite(website));
            }

            // Job info
            if (!String.IsNullOrEmpty(vc.Organization) ||
                !String.IsNullOrEmpty(vc.Department) ||
                !String.IsNullOrEmpty(vc.Title))
            {
                var jobInfo = new ContactJobInfo();
                if (!String.IsNullOrEmpty(vc.Organization))
                {
                    jobInfo.CompanyName = vc.Organization;
                }
                if (!String.IsNullOrEmpty(vc.Department))
                {
                    jobInfo.Department = vc.Department;
                }
                if (!String.IsNullOrEmpty(vc.Title))
                {
                    jobInfo.Title = vc.Title;
                }
                convertedContact.JobInfo.Add(jobInfo);
            }
            return convertedContact;
        }

        /// <summary>
        /// Utility method to convert phone information from the vCard library to a
        /// WinRT ContactPhone instance.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="phone">Phone information from the vCard library.</param>
        /// <returns>The phone information from the vCard library converted to a 
        /// WinRT ContactPhone instance.</returns>
        private ContactPhone ConvertVcardToPhone(vCardPhone phone)
        {
            var cp = new ContactPhone
            {
                Number = phone.FullNumber
            };
            if (phone.IsWork)
            {
                cp.Kind = ContactPhoneKind.Work;
            }
            else if (phone.IsCellular)
            {
                cp.Kind = ContactPhoneKind.Mobile;
            }
            else if (phone.IsHome)
            {
                cp.Kind = ContactPhoneKind.Home;
            }
            else
            {
                cp.Kind = ContactPhoneKind.Other;
            }
            return cp;
        }

        /// <summary>
        /// Utility method to convert email information from the vCard library to a
        /// WinRT ContactEmail instance.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="email">Email information from the vCard library.</param>
        /// <returns>The email information from the vCard library converted to a 
        /// WinRT ContactEmail instance.</returns>
        private ContactEmail ConvertVcardToEmail(vCardEmailAddress email)
        {
            var ce = new ContactEmail
            {
                Address = email.Address,
                Kind = ContactEmailKind.Other   // No useful types supported by vCard library
            };
            return ce;
        }

        /// <summary>
        /// Utility method to convert address information from the vCard library to a
        /// WinRT ContactAddress instance.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="address">Address information from the vCard library.</param>
        /// <returns>The address information from the vCard library converted to a 
        /// WinRT ContactAddress instance.</returns>
        private ContactAddress ConvertVcardToAddress(vCardDeliveryAddress address)
        {
            var ca = new ContactAddress
            {
                Country = address.Country,
                PostalCode = address.PostalCode,
                Region = address.Region,
                StreetAddress = address.Street,
                Locality = address.City,
                Kind = ConvertVcardToAddressType(address.AddressType)
            };
            return ca;
        }

        /// <summary>
        /// Utility method to convert address type information from the vCard library to a
        /// WinRT ContactAddressKind instance.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="addressType">Address type information from the vCard library.</param>
        /// <returns>The address type information from the vCard library converted to a 
        /// WinRT ContactAddressKind instance.</returns>
        private ContactAddressKind ConvertVcardToAddressType(vCardDeliveryAddressTypes addressType)
        {
            switch (addressType)
            {
                case vCardDeliveryAddressTypes.Default:
                    return ContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Domestic:
                    return ContactAddressKind.Other;
                case vCardDeliveryAddressTypes.International:
                    return ContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Postal:
                    return ContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Parcel:
                    return ContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Home:
                    return ContactAddressKind.Home;
                case vCardDeliveryAddressTypes.Work:
                    return ContactAddressKind.Work;
                default:
                    throw new ArgumentOutOfRangeException("addressType");
            }
        }

        /// <summary>
        /// Utility method to convert website information from the vCard library to a
        /// WinRT ContactWebsite instance.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="website">Website information from the vCard library.</param>
        /// <returns>The website information from the vCard library converted to a 
        /// WinRT ContactWebsite instance.</returns>
        private ContactWebsite ConvertVcardToWebsite(vCardWebsite website)
        {
            return new ContactWebsite
            {
                Uri = new Uri(website.Url)
            };
        }

        #endregion

        #region Convert Windows 8 Contact to vCard

        /// <summary>
        /// Map the Windows 8 Contact class to a vCard object from the vCard library.
        /// This mapping is not 1:1, as Contact has differnet properties than supported
        /// in the vCard standard.
        /// The method maps / converts as much information as possible.
        /// </summary>
        /// <param name="contact">Source contact to convert to a vCard object.</param>
        /// <returns>New vCard object with information from the source contact mapped
        /// as completely as possible.</returns>
        public vCard ConvertContactToVCard(Contact contact)
        {
            var vc = new vCard
            {
                FamilyName = contact.LastName,
                GivenName = contact.FirstName,
                NamePrefix = contact.HonorificNamePrefix,
                NameSuffix = contact.HonorificNameSuffix
            };

            // Phone numbers
            foreach (var phone in contact.Phones)
            {
                vc.Phones.Add(new vCardPhone(phone.Number, ConvertPhoneTypeToVcard(phone.Kind)));
            }

            // Email
            foreach (var email in contact.Emails)
            {
                vc.EmailAddresses.Add(new vCardEmailAddress(email.Address, ConvertEmailTypeToVcard(email.Kind)));
            }

            // Postal address
            foreach (var address in contact.Addresses)
            {
                vc.DeliveryAddresses.Add(ConvertAddressToVcard(address));
            }

            // Notes
            if (!String.IsNullOrEmpty(contact.Notes))
            {
                vc.Notes.Add(new vCardNote(contact.Notes));
            }

            // Dates
            foreach (var importantDate in contact.ImportantDates)
            {
                if (importantDate.Kind == ContactDateKind.Birthday)
                {
                    if (importantDate.Year != null && (importantDate.Month != null && importantDate.Day != null))
                    {
                        vc.BirthDate = new DateTime((int)importantDate.Year, (int)importantDate.Month,
                            (int)importantDate.Day);
                    }
                }
            }

            // Websites
            foreach (var website in contact.Websites)
            {
                vc.Websites.Add(ConvertWebsiteToVcard(website));
            }

            // Job info
            foreach (var jobInfo in contact.JobInfo)
            {
                // Set company name / organisation if not yet set and present in the source info
                if (!String.IsNullOrEmpty(jobInfo.CompanyName) && String.IsNullOrEmpty(vc.Organization))
                {
                    vc.Organization = jobInfo.CompanyName;
                }
                // Set department if not yet set and present in the source info
                if (!String.IsNullOrEmpty(jobInfo.Department) && String.IsNullOrEmpty(vc.Department))
                {
                    vc.Department = jobInfo.Department;
                }
                // Set position / job if not yet set and present in the source info
                if (!String.IsNullOrEmpty(jobInfo.Title) && String.IsNullOrEmpty(vc.Title))
                {
                    vc.Title = jobInfo.Title;
                }
            }

            return vc;
        }

        /// <summary>
        /// Utility method to convert address information from the WinRT ContactAddress 
        /// instance to the representation in the vCard library.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="address">Address information from WinRT.</param>
        /// <returns>The address information from WinRT library converted to a 
        /// vCard library class instance.</returns>
        private vCardDeliveryAddress ConvertAddressToVcard(ContactAddress address)
        {
            var newAddress = new vCardDeliveryAddress
            {
                Country = address.Country,
                PostalCode = address.PostalCode,
                Region = address.Region,
                Street = address.StreetAddress,
                City = address.Locality,
                AddressType = ConvertAddressTypeToVcard(address.Kind)
            };
            return newAddress;
        }

        /// <summary>
        /// Utility method to convert address type information from the WinRT ContactAddressKind 
        /// instance to the representation in the vCard library.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="addressKind">Address type information from WinRT.</param>
        /// <returns>The address type information from WinRT library converted to a 
        /// vCard library class instance.</returns>
        private vCardDeliveryAddressTypes ConvertAddressTypeToVcard(ContactAddressKind addressKind)
        {
            switch (addressKind)
            {
                case ContactAddressKind.Home:
                    return vCardDeliveryAddressTypes.Home;
                case ContactAddressKind.Work:
                    return vCardDeliveryAddressTypes.Work;
                case ContactAddressKind.Other:
                    return vCardDeliveryAddressTypes.Postal;
                default:
                    return vCardDeliveryAddressTypes.Postal;
            }
        }

        /// <summary>
        /// Utility method to convert email information from the WinRT ContactEmailKind 
        /// instance to the representation in the vCard library.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="emailKind">Email information from WinRT.</param>
        /// <returns>The email information from WinRT library converted to a 
        /// vCard library class instance.</returns>
        private vCardEmailAddressType ConvertEmailTypeToVcard(ContactEmailKind emailKind)
        {
            switch (emailKind)
            {
                case ContactEmailKind.Personal:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
                case ContactEmailKind.Work:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
                case ContactEmailKind.Other:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
                default:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
            }
        }

        /// <summary>
        /// Utility method to convert phone information from the WinRT ContactPhoneKind 
        /// instance to the representation in the vCard library.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="phoneKind">Phone information from WinRT.</param>
        /// <returns>The phone information from WinRT library converted to a 
        /// vCard library class instance.</returns>
        private vCardPhoneTypes ConvertPhoneTypeToVcard(ContactPhoneKind phoneKind)
        {
            switch (phoneKind)
            {
                case ContactPhoneKind.Home:
                    return vCardPhoneTypes.HomeVoice;
                case ContactPhoneKind.Mobile:
                    return vCardPhoneTypes.CellularVoice;
                case ContactPhoneKind.Work:
                    return vCardPhoneTypes.WorkVoice;
                case ContactPhoneKind.Other:
                    return vCardPhoneTypes.Voice;
                default:
                    return vCardPhoneTypes.Voice;
            }
        }

        /// <summary>
        /// Utility method to convert website information from the WinRT ContactWebsite 
        /// instance to the representation in the vCard library.
        /// No 1:1 matching is possible between both classes, the method tries to
        /// keep the conversion as accurate as possible.
        /// </summary>
        /// <param name="website">Website information from WinRT.</param>
        /// <returns>The website information from WinRT library converted to a 
        /// vCard library class instance.</returns>
        private vCardWebsite ConvertWebsiteToVcard(ContactWebsite website)
        {
            // Can't match description to website type
            return new vCardWebsite(website.Uri.ToString(), vCardWebsiteTypes.Default);
        }

        #endregion


    }
}
