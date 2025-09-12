
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using Thought.vCards;

namespace VcardLibrary
{

    /// <summary>
    ///     A postal address.
    /// </summary>
    /// <seealso cref="vCardDeliveryAddressCollection"/>
    public class vCardDeliveryAddress
    {

        private vCardDeliveryAddressTypes addressType;
        private string city;
        private string country;
        private string postalCode;
        private string region;
        private string street;


        /// <summary>
        ///     Creates a new delivery address object.
        /// </summary>
        public vCardDeliveryAddress()
        {
            this.city = string.Empty;
            this.country = string.Empty;
            this.postalCode = string.Empty;
            this.region = string.Empty;
            this.street = string.Empty;
        }


        /// <summary>
        ///     The type of postal address.
        /// </summary>
        public vCardDeliveryAddressTypes AddressType
        {
            get
            {
                return this.addressType;
            }
            set
            {
                this.addressType = value;
            }
        }


        /// <summary>
        ///     The city or locality of the address.
        /// </summary>
        public string City
        {
            get
            {
                return this.city ?? string.Empty;
            }
            set
            {
                this.city = value;
            }
        }


        /// <summary>
        ///     The country name of the address.
        /// </summary>
        public string Country
        {
            get
            {
                return this.country ?? string.Empty;
            }
            set
            {
                this.country = value;
            }
        }


        /// <summary>
        ///     Indicates a domestic delivery address.
        /// </summary>
        public bool IsDomestic
        {
            get
            {
                return (this.addressType & vCardDeliveryAddressTypes.Domestic) ==
                    vCardDeliveryAddressTypes.Domestic;
            }
            set
            {

                if (value)
                {
                    this.addressType |= vCardDeliveryAddressTypes.Domestic;
                }
                else
                {
                    this.addressType &= ~vCardDeliveryAddressTypes.Domestic;
                }

            }
        }


        /// <summary>
        ///     Indicates a home address.
        /// </summary>
        public bool IsHome
        {
            get
            {
                return (this.addressType & vCardDeliveryAddressTypes.Home) ==
                    vCardDeliveryAddressTypes.Home;
            }
            set
            {
                if (value)
                {
                    this.addressType |= vCardDeliveryAddressTypes.Home;
                }
                else
                {
                    this.addressType &= ~vCardDeliveryAddressTypes.Home;
                }

            }
        }


        /// <summary>
        ///     Indicates an international address.
        /// </summary>
        public bool IsInternational
        {
            get
            {
                return (this.addressType & vCardDeliveryAddressTypes.International) ==
                    vCardDeliveryAddressTypes.International;
            }
            set
            {
                if (value)
                {
                    this.addressType |= vCardDeliveryAddressTypes.International;
                }
                else
                {
                    this.addressType &= ~vCardDeliveryAddressTypes.International;
                }
            }
        }


        /// <summary>
        ///     Indicates a parcel delivery address.
        /// </summary>
        public bool IsParcel
        {
            get
            {
                return (this.addressType & vCardDeliveryAddressTypes.Parcel) ==
                    vCardDeliveryAddressTypes.Parcel;
            }
            set
            {
                if (value)
                {
                    this.addressType |= vCardDeliveryAddressTypes.Parcel;
                }
                else
                {
                    this.addressType &= ~vCardDeliveryAddressTypes.Parcel;
                }
            }
        }


        /// <summary>
        ///     Indicates a postal address.
        /// </summary>
        public bool IsPostal
        {
            get
            {
                return (this.addressType & vCardDeliveryAddressTypes.Postal) ==
                    vCardDeliveryAddressTypes.Postal;
            }
            set
            {
                if (value)
                {
                    this.addressType |= vCardDeliveryAddressTypes.Postal;
                }
                else
                {
                    this.addressType &= ~vCardDeliveryAddressTypes.Postal;
                }
            }
        }


        /// <summary>
        ///     Indicates a work address.
        /// </summary>
        public bool IsWork
        {
            get
            {
                return (this.addressType & vCardDeliveryAddressTypes.Work) ==
                    vCardDeliveryAddressTypes.Work;
            }
            set
            {
                if (value)
                {
                    this.addressType |= vCardDeliveryAddressTypes.Work;
                }
                else
                {
                    this.addressType &= ~vCardDeliveryAddressTypes.Work;
                }
            }
        }


        /// <summary>
        ///     The postal code (e.g. ZIP code) of the address.
        /// </summary>
        public string PostalCode
        {
            get
            {
                return this.postalCode ?? string.Empty;
            }
            set
            {
                this.postalCode = value;
            }
        }


        /// <summary>
        ///     The region (state or province) of the address.
        /// </summary>
        public string Region
        {
            get
            {
                return this.region ?? string.Empty;
            }
            set
            {
                this.region = value;
            }
        }


        /// <summary>
        ///     The street of the delivery address.
        /// </summary>
        public string Street
        {
            get
            {
                return this.street ?? string.Empty;
            }
            set
            {
                this.street = value;
            }
        }

    }

}