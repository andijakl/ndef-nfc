
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace VcardLibrary
{

    /// <summary>
    ///     The type of a delivery address.
    /// </summary>
    [Flags]
    public enum vCardDeliveryAddressTypes
    {

        /// <summary>
        ///     Default address settings.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     A domestic delivery address.
        /// </summary>
        Domestic = 1,                   // Andreas Jakl: FIX - added numbers

        /// <summary>
        ///     An international delivery address.
        /// </summary>
        International = 2,

        /// <summary>
        ///     A postal delivery address.
        /// </summary>
        Postal = 4,

        /// <summary>
        ///     A parcel delivery address.
        /// </summary>
        Parcel = 8,

        /// <summary>
        ///     A home delivery address.
        /// </summary>
        Home = 16,

        /// <summary>
        ///     A work delivery address.
        /// </summary>
        Work = 32
    }

}