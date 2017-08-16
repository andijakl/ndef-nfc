/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefGeoRecord(context) {
	'use strict';

	var NdefLibrary = context.NdefLibrary;


	/// <summary>
	/// Store longitude and latitude on a tag, to allow the user
	/// to view a map when tapping the tag.
	/// </summary>
	/// <remarks>
	/// Geo tags are not standardized by the NFC forum, therefore,
	/// this class supports three different types of writing the location
	/// to a tag.
	/// 
	/// * GeoUri: write URI based on the "geo:" URI scheme, as specified
	/// by RFC5870, available at: http://geouri.org/
	/// 
	/// * BingMaps: Uses the URI scheme defined by the Maps application
	/// on Windows 8.
	/// 
	/// * DriveTo / WalkTo: URI schemes supported by Windows Phone 8 and
	/// used in apps to launch an installed navigation app to navigate
	/// to a specified position. An app to handle DriveTo request should
	/// be present by default on all WP8 phones; WalkTo not necessarily.
	/// 
	/// * NokiaMapsUri: write URI based on a Nokia Maps link, following the
	/// "http://m.ovi.me/?c=..." scheme of the Nokia/Ovi Maps Rendering API.
	/// Depending on the target device, the phone / web service should then
	/// redirect to the best maps representation.
	/// On Symbian, the phone will launch the Nokia Maps client. On a
	/// desktop computer, the full Nokia Maps web experience will open.
	/// On other phones, the HTML 5 client may be available.
	/// 
	/// * WebRedirect: uses the web service at NfcInteractor.com to
	/// check the OS of the phone, and then redirect to the best way
	/// of showing maps to the user.
	/// Note the limitations and terms of use of the web service. For
	/// real world deployment, outside of development and testing, it's
	/// recommended to host the script on your own web server.
	/// Find more information at nfcinteractor.com.
	/// 
	/// As this class is based on the Smart URI base class, the
	/// payload is formatted as a URI record initially. When first
	/// adding Smart Poster information (like a title), the payload
	/// instantly transforms into a Smart Poster.
	/// </remarks>
	var ndefGeoRecord = NdefLibrary.NdefGeoRecord = function (opt_config) {

		/// <summary>
		/// Longitude of the coordinate to encode in the Geo URI.
		/// </summary>
		this.Longitude = "";

		/// <summary>
		/// Latitude of the coordinate to encode in the Geo URI.
		/// </summary>
		this.Latitude = "";

		/// <summary>
		/// Format to use for encoding the coordinate into a URI.
		/// </summary>
		this._geoType = "";


		/// Constructors
		if (arguments.length == 1) {
			NdefLibrary.NdefUriRecord.call(this, arguments[0]);
			this.updatePayload();
			//TODO get Latitude and longitude from URI
		}
		else {
			NdefLibrary.NdefUriRecord.call(this);
		}
	};

	//Derive from NdefUriRecord
	ndefGeoRecord.prototype = new NdefLibrary.NdefUriRecord();
	ndefGeoRecord.prototype.constructor = NdefLibrary.NdefGeoRecord;

	/// <summary>
	/// Format of the URI on the geo tag.
	/// </summary>
	NdefLibrary.NdefGeoRecord.NfcGeoType = {
		/// <summary>
		/// Geo URI scheme, as defined in RFC 5870 (http://tools.ietf.org/html/rfc5870).
		/// </summary>
		GeoUri: 0,
		/// <summary>
		/// Bing Maps URI scheme, used for Maps on Windows 8 (http://msdn.microsoft.com/en-us/library/windows/apps/jj635237.aspx)
		/// </summary>
		BingMaps: 1,
		/// <summary>
		/// Nokia Maps HTTP URL to show maps in the browser.
		/// </summary>
		NokiaMapsUri: 2,
		/// <summary>
		/// Web redirection script that uses the appropriate URI format depending on the browser's user agent.
		/// </summary>
		WebRedirect: 3,
		/// <summary>
		/// Drive-to URI scheme for Windows Phone 8 (http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj710324%28v=vs.105%29.aspx)
		/// </summary>
		MsDriveTo: 4,
		/// <summary>
		/// Walk-to URI scheme for Windows Phone 8 (http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj710324%28v=vs.105%29.aspx)
		/// </summary>
		MsWalkTo: 5
	};

	/// <summary>
	/// Default URIs for the different formats to store the geo tag.
	/// </summary>
	/// <remarks>
	/// URI that will be used for this record. Parameter {0} will be replaced with
	/// the latitude, {1} with the longitude.
	/// </remarks>
	NdefLibrary.NdefSocialRecord.GeoTagTypeUris = [
		"geo:{0},{1}",
		"bingmaps:?cp={0}~{1}",
		"http://m.ovi.me/?c={0},{1}",
		"http://NfcInteractor.com/m?c={0},{1}",
		"ms-drive-to:?destination.latitude={0}&destination.longitude={1}",
		"ms-walk-to:?destination.latitude={0}&destination.longitude={1}"
	];

	/// <summary>
	/// Format the URI of the SmartUri base class.
	/// </summary>
	ndefGeoRecord.prototype.updatePayload = function () {
		if (this.Latitude == null || this.Latitude.length == 0) {
			if (this.Longitude == null || this.Longitude.length == 0) {
				return;
			}
		}

		var base = NdefLibrary.NdefSocialRecord.GeoTagTypeUris[this.getGeoType()];

		// Make sure we always use the "en" culture to have "." as the decimal separator.
		var uri = String.format(base, this.Latitude, this.Longitude);
		this.setUri(uri);
	};

	ndefGeoRecord.prototype.getLatitude = function () {
		return this.Latitude;
	};

	ndefGeoRecord.prototype.setLatitude = function (value) {
		this.Latitude = value;
		this.updatePayload();
	};

	ndefGeoRecord.prototype.getLongitude = function () {
		return this.Longitude;
	};

	ndefGeoRecord.prototype.setLongitude = function (value) {
		this.Longitude = value;
		this.updatePayload();
	};

	ndefGeoRecord.prototype.getGeoType = function () {
		return this._geoType;
	};

	ndefGeoRecord.prototype.setGeoType = function (value) {
		this._geoType = value;
		this.updatePayload();
	};


}