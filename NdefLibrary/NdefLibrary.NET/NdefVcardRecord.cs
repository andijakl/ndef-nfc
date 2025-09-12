/****************************************************************************
**
** Copyright (C) 2012-2018 Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
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

using System;
using System.IO;
using System.Text;
using NdefLibrary.Ndef;
using VcardLibrary;
using Thought.vCards;

namespace NdefLibrary.Ndef
{
    public class NdefVcardRecord : NdefVcardRecordBase
    {
        private NdefContact _contactData;

        public NdefContact ContactData
        {
            get { return _contactData; }
            set
            {
                _contactData = value;
                AssemblePayload();
            }
        }

        #region Constructors and Initialization
        public NdefVcardRecord() { }

        public NdefVcardRecord(NdefRecord other) : base(other)
        {
            ContactData = ParseDataToContact(_payload);
        }

        public NdefVcardRecord(NdefContact contact)
        {
            ContactData = contact;
        }

        public NdefVcardRecord(string vCardString)
        {
            ContactData = ParseDataToContact(Encoding.UTF8.GetBytes(vCardString));
        }

        public NdefVcardRecord(byte[] vCardByte)
        {
            ContactData = ParseDataToContact(vCardByte);
        }

        private void AssemblePayload()
        {
            if (ContactData == null) return;
            var vCard = ConvertContactToVCard(ContactData);
            var vcWriter = new vCardStandardWriter();

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            vcWriter.Write(vCard, writer);
            writer.Flush();
            Payload = ms.ToArray();
        }
        #endregion

        #region Convert vCard to NdefContact

        private NdefContact ParseDataToContact(byte[] vCardByte)
        {
            var vcReader = new vCardStandardReader();
            var vc = vcReader.Read(new StringReader(Encoding.UTF8.GetString(vCardByte, 0, vCardByte.Length)));

            var convertedContact = new NdefContact
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
                var birthDay = (DateTime)vc.BirthDate;
                convertedContact.ImportantDates.Add(new NdefContactDate
                {
                    Kind = NdefContactDateKind.Birthday,
                    Year = birthDay.Year,
                    Month = (uint?)birthDay.Month,
                    Day = (uint?)birthDay.Day
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
                var jobInfo = new NdefContactJobInfo();
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

        private NdefContactPhone ConvertVcardToPhone(vCardPhone phone)
        {
            var cp = new NdefContactPhone
            {
                Number = phone.FullNumber
            };
            if (phone.IsWork)
            {
                cp.Kind = NdefContactPhoneKind.Work;
            }
            else if (phone.IsCellular)
            {
                cp.Kind = NdefContactPhoneKind.Mobile;
            }
            else if (phone.IsHome)
            {
                cp.Kind = NdefContactPhoneKind.Home;
            }
            else
            {
                cp.Kind = NdefContactPhoneKind.Other;
            }
            return cp;
        }

        private NdefContactEmail ConvertVcardToEmail(vCardEmailAddress email)
        {
            var ce = new NdefContactEmail
            {
                Address = email.Address,
                Kind = NdefContactEmailKind.Other   // No useful types supported by vCard library
            };
            return ce;
        }

        private NdefContactAddress ConvertVcardToAddress(vCardDeliveryAddress address)
        {
            var ca = new NdefContactAddress
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

        private NdefContactAddressKind ConvertVcardToAddressType(vCardDeliveryAddressTypes addressType)
        {
            switch (addressType)
            {
                case vCardDeliveryAddressTypes.Default:
                    return NdefContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Domestic:
                    return NdefContactAddressKind.Other;
                case vCardDeliveryAddressTypes.International:
                    return NdefContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Postal:
                    return NdefContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Parcel:
                    return NdefContactAddressKind.Other;
                case vCardDeliveryAddressTypes.Home:
                    return NdefContactAddressKind.Home;
                case vCardDeliveryAddressTypes.Work:
                    return NdefContactAddressKind.Work;
                default:
                    throw new ArgumentOutOfRangeException("addressType");
            }
        }

        private NdefContactWebsite ConvertVcardToWebsite(vCardWebsite website)
        {
            return new NdefContactWebsite
            {
                Uri = new Uri(website.Url)
            };
        }

        #endregion

        #region Convert NdefContact to vCard

        public vCard ConvertContactToVCard(NdefContact contact)
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
                if (importantDate.Kind == NdefContactDateKind.Birthday)
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

        private vCardDeliveryAddress ConvertAddressToVcard(NdefContactAddress address)
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

        private vCardDeliveryAddressTypes ConvertAddressTypeToVcard(NdefContactAddressKind addressKind)
        {
            switch (addressKind)
            {
                case NdefContactAddressKind.Home:
                    return vCardDeliveryAddressTypes.Home;
                case NdefContactAddressKind.Work:
                    return vCardDeliveryAddressTypes.Work;
                case NdefContactAddressKind.Other:
                    return vCardDeliveryAddressTypes.Postal;
                default:
                    return vCardDeliveryAddressTypes.Postal;
            }
        }

        private vCardEmailAddressType ConvertEmailTypeToVcard(NdefContactEmailKind emailKind)
        {
            switch (emailKind)
            {
                case NdefContactEmailKind.Personal:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
                case NdefContactEmailKind.Work:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
                case NdefContactEmailKind.Other:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
                default:
                    return vCardEmailAddressType.Internet; // Not supported by vCard library
            }
        }

        private vCardPhoneTypes ConvertPhoneTypeToVcard(NdefContactPhoneKind phoneKind)
        {
            switch (phoneKind)
            {
                case NdefContactPhoneKind.Home:
                    return vCardPhoneTypes.HomeVoice;
                case NdefContactPhoneKind.Mobile:
                    return vCardPhoneTypes.CellularVoice;
                case NdefContactPhoneKind.Work:
                    return vCardPhoneTypes.WorkVoice;
                case NdefContactPhoneKind.Other:
                    return vCardPhoneTypes.Voice;
                default:
                    return vCardPhoneTypes.Voice;
            }
        }

        private vCardWebsite ConvertWebsiteToVcard(NdefContactWebsite website)
        {
            // Can't match description to website type
            return new vCardWebsite(website.Uri.ToString(), vCardWebsiteTypes.Default);
        }

        #endregion


    }
}
