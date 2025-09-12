using System;
using System.Collections.Generic;

namespace NdefLibrary.Ndef
{
    public class NdefContact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HonorificNamePrefix { get; set; }
        public string HonorificNameSuffix { get; set; }
        public string Notes { get; set; }
        public IList<NdefContactPhone> Phones { get; } = new List<NdefContactPhone>();
        public IList<NdefContactEmail> Emails { get; } = new List<NdefContactEmail>();
        public IList<NdefContactAddress> Addresses { get; } = new List<NdefContactAddress>();
        public IList<NdefContactDate> ImportantDates { get; } = new List<NdefContactDate>();
        public IList<NdefContactWebsite> Websites { get; } = new List<NdefContactWebsite>();
        public IList<NdefContactJobInfo> JobInfo { get; } = new List<NdefContactJobInfo>();
    }

    public class NdefContactPhone
    {
        public string Number { get; set; }
        public NdefContactPhoneKind Kind { get; set; }
    }

    public class NdefContactEmail
    {
        public string Address { get; set; }
        public NdefContactEmailKind Kind { get; set; }
    }

    public class NdefContactAddress
    {
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string StreetAddress { get; set; }
        public string Locality { get; set; }
        public NdefContactAddressKind Kind { get; set; }
    }

    public class NdefContactDate
    {
        public NdefContactDateKind Kind { get; set; }
        public int? Year { get; set; }
        public uint? Month { get; set; }
        public uint? Day { get; set; }
    }

    public class NdefContactJobInfo
    {
        public string CompanyName { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
    }

    public class NdefContactWebsite
    {
        public Uri Uri { get; set; }
    }

    public enum NdefContactAddressKind
    {
        Home,
        Work,
        Other
    }

    public enum NdefContactEmailKind
    {
        Personal,
        Work,
        Other
    }

    public enum NdefContactPhoneKind
    {
        Home,
        Mobile,
        Work,
        Other
    }

    public enum NdefContactDateKind
    {
        Birthday,
        Anniversary,
        Other
    }
}
