
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using Thought.vCards;

namespace VcardLibrary
{

    /// <summary>
    ///     A formatted delivery label.
    /// </summary>
    /// <seealso cref="vCardDeliveryLabelCollection"/>
    /// <seealso cref="vCardDeliveryAddress"/>
    public class vCardDeliveryLabel
    {

        private vCardDeliveryAddressTypes addressType;
        private string text;


        /// <summary>
        ///     Initializes a new <see cref="vCardDeliveryLabel"/>.
        /// </summary>
        public vCardDeliveryLabel()
        {
        }


        /// <summary>
        ///     Initializes a new <see cref="vCardDeliveryLabel"/> to
        ///     the specified text.
        /// </summary>
        /// <param name="text">
        ///     The formatted text of a delivery label.  The label 
        ///     may contain carriage returns, line feeds, and other
        ///     control characters.
        /// </param>
        public vCardDeliveryLabel(string text)
        {
            this.text = text == null ? string.Empty : text;
        }


        /// <summary>
        ///     The type of delivery address for the label.
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
        ///     The formatted delivery text.
        /// </summary>
        public string Text
        {
            get
            {
                return this.text ?? string.Empty;
            }
            set
            {
                this.text = value;
            }
        }

    }

}