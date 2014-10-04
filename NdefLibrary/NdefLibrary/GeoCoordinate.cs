/****************************************************************************
**
** Copyright (C) 2012-2013 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
** More information: http://ndef.codeplex.com/
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
namespace NdefLibrary
{
    /// <summary>
    /// Custom GeoCoordinate class to allow for cross-platform portability.
    /// </summary>
    /// <remarks>Windows Phone and Windows 8 use different namespaces &
    /// class name for the GeoCoordinate class.
    /// WP: System.Device.Location.GeoCoordinate,
    /// Win8: Windows.Devices.Geolocation.Geocoordinate</remarks>
    public class GeoCoordinate
    {
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        public double Longitude { get; set; }
    }
}
