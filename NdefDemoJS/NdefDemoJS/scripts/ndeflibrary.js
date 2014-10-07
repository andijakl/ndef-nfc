/*! lib-ndeflibrary - v1.0.0 - 2014-10-02 - Sebastian Höbarth / Andreas Jakl */
;(function (global) {

/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

var DEBUG = false;

var arrayCopy = function () {
    var src, srcPos = 0,
      dest, destPos = 0,
      length = 0;

    if (arguments.length === 2) {
        src = arguments[0];
        dest = arguments[1];
        length = src.length;
    } else if (arguments.length === 3) {
        src = arguments[0];
        dest = arguments[1];
        length = arguments[2];
    } else if (arguments.length === 5) {
        src = arguments[0];
        srcPos = arguments[1];
        dest = arguments[2];
        destPos = arguments[3];
        length = arguments[4];
    }
    for (var i = srcPos, j = destPos; i < length + srcPos; i++, j++) if (dest[j] !== null) dest[j] = src[i];
    else throw "array index out of bounds exception";
};

var addToArray = function (array, content) {
    var src, srcPos = 0,
      dest, destPos = 0,
      length;

    src = content;
    srcPos = 0;
    dest = array;
    destPos = array.length;
    length = content.length;

    for (var i = srcPos, j = destPos; i < length + srcPos; i++, j++) if (dest[j] !== null) dest[j] = src[i];
    else throw "array index out of bounds exception";
};


var getEncodeURILength = function (countMe) {
    var escapedStr = encodeURI(countMe);
    if (escapedStr.indexOf("%") != -1) {
        var count = escapedStr.split("%").length - 1;
        if (count == 0) count++; //perverse case; can't happen with real UTF-8
        var tmp = escapedStr.length - (count * 3);
        count = count + tmp;
    } else {
        count = escapedStr.length;
    }
    // console.log(escapedStr + ": size is " + count);
    return count;
};

var arraysEqual = function (a, b) {
    if (a === b) return true;
    if (a == null || b == null) return false;
    if (a.length != b.length) return false;

    // If you don't care about the order of the elements inside
    // the array, you should sort both arrays here.

    for (var i = 0; i < a.length; ++i) {
        if (a[i] !== b[i]) return false;
    }
    return true;
};

var getHexString = function (value) {
    if (value != null) {
        if (value.length <= 1) {
            return value.toString(16);
        } else {
            var output = "";
            for (var i = 0; i < value.length; i++) {
                if (i == value.length - 1) output += "0x" + value[i].toString(16).toUpperCase();
                else output += "0x" + value[i].toString(16).toUpperCase() + ",";
            };
            return output;
        }
    }
    return "";
};

//String extensions
String.prototype.startsWith = function (str) {
    return this.slice(0, str.length) == str;
};

String.prototype.getBytes = function () {
    var bytes = [];
    for (var i = 0; i < this.length; ++i) {
        bytes.push(this.charCodeAt(i));
    }
    return bytes;
};

String.format = function () {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }
    return s;
};

var fromArray = function (array) {
    var result = "";
    for (var i = 0; i < array.length; i++) {
        result += String.fromCharCode(parseInt(array[i]));
    }
    return result;
};

var fromArrayUTF16 = function (array) {
    var result = "";
    for (var i = 0; i < array.length - 1; i += 2) {
        result += String.fromCharCode(parseInt(array[i]), parseInt(array[i + 1]));
    }
    return result;
};




/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

// Compiler directive for UglifyJS.  See library.const.js for more info.
if (typeof DEBUG === 'undefined') {
    DEBUG = true;
}


// LIBRARY-GLOBAL CONSTANTS
//
// These constants are exposed to all library modules.


// GLOBAL is a reference to the global Object.
var Fn = Function, GLOBAL = new Fn('return this')();


// LIBRARY-GLOBAL METHODS
//
// The methods here are exposed to all library modules.  Because all of the
// source files are wrapped within a closure at build time, they are not
// exposed globally in the distributable binaries.


/**
 * A no-op function.  Useful for passing around as a default callback.
 */
function noop() { }


/**
 * Init wrapper for the core module.
 * @param {Object} The Object that the library gets attached to in
 * library.init.js.  If the library was not loaded with an AMD loader such as
 * require.js, this is the global Object.
 */
function initLibraryCore(context) {


    // It is recommended to use strict mode to help make mistakes easier to find.
    'use strict';



    /**
     * This is the constructor for the Library Object.  Please rename it to
     * whatever your library's name is.  Note that the constructor is also being
     * attached to the context that the library was loaded in.
     * @param {Object} opt_config Contains any properties that should be used to
     * configure this instance of the library.
     * @constructor
     */
    var NdefLibrary = context.NdefLibrary = function (opt_config) {

        opt_config = opt_config || {};

        return this;
    };


    // LIBRARY PROTOTYPE METHODS
    //
    // These methods define the public API.


    // DEBUG CODE
    //
    // With compiler directives, you can wrap code in a conditional check to
    // ensure that it does not get included in the compiled binaries.  This is
    // useful for exposing certain properties and methods that are needed during
    // development and testing, but should be private in the compiled binaries.


}

/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryModule(context) {

    'use strict';

    var NdefLibrary = context.NdefLibrary;


    if (DEBUG) {
    }

}

/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefAndroidAppRecord(context) {
    'use strict';

    var NdefLibrary = context.NdefLibrary;


    /// <summary>
    /// Creates the Android-specific Android Application Record.
    /// </summary>
    /// <remarks>
    /// Through specifying the package name, this record directly launches an
    /// app on an Android phone (4.0+). If the app isn't installed on the phone,
    /// it will open the store and search for the app.
    /// 
    /// To pass custom data to the app, you would typically add other records
    /// to the NDEF message.
    /// 
    /// If creating a multi-record NDEF message, it's recommended to put this
    /// record to the end of the message.
    /// </remarks>
    /// <seealso cref="http://developer.android.com/guide/topics/connectivity/nfc/nfc.html#aar"/>
    var ndefAndroidAppRecord = NdefLibrary.NdefAndroidAppRecord = function (opt_config) {

        /// <summary>
        /// Name of the android package.
        /// </summary>
        this.packageName = "";


        ///Constructors
        if (arguments.length == 1) {
            NdefLibrary.NdefRecord.call(this, arguments[0]);

            if (!NdefLibrary.NdefAndroidAppRecord.isRecordType(this))
                throw "NdefException(NdefExceptionMessages.ExInvalidCopy)";
        }
        else {
            /// <summary>
            /// Create an empty Android Application Record.
            /// </summary>
            NdefLibrary.NdefRecord.call(this, NdefLibrary.NdefRecord.TypeNameFormatType.ExternalRtd, NdefLibrary.NdefAndroidAppRecord.AndroidAppRecordType);
        }

    };


    /// <summary>
    /// Type name for the Android Application Record (TNF = External RTD)
    /// </summary>
    NdefLibrary.NdefAndroidAppRecord.AndroidAppRecordType = "android.com:pkg".getBytes();

    //Derive from NdefRecord
    ndefAndroidAppRecord.prototype = new NdefLibrary.NdefRecord();
    ndefAndroidAppRecord.prototype.constructor = NdefLibrary.NdefAndroidAppRecord;


    /// <summary>
    /// Checks if the record sent via the parameter is indeed an Android
    /// Application Record.
    /// Only checks the type and type name format, doesn't analyze if the
    /// payload is valid.
    /// </summary>
    /// <param name="record">Record to check.</param>
    /// <returns>True if the record has the correct type and type name format
    /// to be an Android Application Record, false if it's a different record.</returns>
    NdefLibrary.NdefAndroidAppRecord.isRecordType = function (record) {
        if (record.getType() == null || record.getType().length == 0) return false;
        return (record.getTypeNameFormat() == NdefLibrary.NdefRecord.TypeNameFormatType.ExternalRtd && arraysEqual(record.getType(), NdefLibrary.NdefAndroidAppRecord.AndroidAppRecordType));
    };

    ndefAndroidAppRecord.prototype.setPackageName = function (value) {
        if (value == null) {
            this.setPayload(null);
            return;
        }
        var encodedString = encodeURI(value);
        var encodedStringArray = encodedString.getBytes();
        this.setPayload(encodedStringArray);
    };

    ndefAndroidAppRecord.prototype.getPackageName = function (value) {

        if (this.getPayload() == null || this.getPayload().length == 0) {
            return "";
        }

        return decodeURI(fromArray(this.getPayload()));
    };

}
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefSocialRecord(context) {
	'use strict';

	var NdefLibrary = context.NdefLibrary;


	/// <summary>
	/// Link to one of the supported social networks by
	/// simply selecting the network and specifying the username.
	/// </summary>
	/// <remarks>
	/// Tapping a tag written with this record type will take
	/// the user to the social network web site, where he can then
	/// for example start following you on Twitter.
	/// 
	/// As this class is based on the Smart URI base class, the
	/// payload is formatted as a URI record initially. When first
	/// adding Smart Poster information (like a title), the payload
	/// instantly transforms into a Smart Poster.
	/// </remarks>
	var ndefSocialRecord = NdefLibrary.NdefSocialRecord = function (opt_config) {

		/// <summary>
		/// Username / id of the social network.
		/// </summary>
		this.socialUserName = "";

		/// <summary>
		/// Format to use for encoding the social network URL.
		/// </summary>
		this.socialType = NdefLibrary.NdefSocialRecord.NfcSocialType.Twitter;

		///Constructors
		if (arguments.length == 1) {
			NdefLibrary.NdefUriRecord.call(this, arguments[0]);
		}
		else {
			NdefLibrary.NdefUriRecord.call(this);
		}
	};

	//Derive from NdefUriRecord
	ndefSocialRecord.prototype = new NdefLibrary.NdefUriRecord();
	ndefSocialRecord.prototype.constructor = NdefLibrary.NdefSocialRecord;

	/// <summary>
	/// List of social networks this class can be used to generate a link for.
	/// </summary>
	NdefLibrary.NdefSocialRecord.NfcSocialType = {
		Twitter: 0,
		LinkedIn: 1,
		Facebook: 2,
		Xing: 3,
		VKontakte: 4,
		FoursquareWeb: 5,
		FoursquareApp: 6,
		Skype: 7,
		GooglePlus: 8
	};

	/// <summary>
	/// Supported social network types and the respective format strings to create the URIs.
	/// </summary>
	NdefLibrary.NdefSocialRecord.SocialTagTypeUris = [
		"http://twitter.com/{0}",
		"http://linkedin.com/in/{0}",
		"http://facebook.com/{0}",
		"http://xing.com/profile/{0}",
		"http://vkontakte.ru/{0}",
		"http://m.foursquare.com/v/{0}",
		"foursquare://venues/{0}",
		"skype:{0}?call",
		"https://plus.google.com/{0}"
	];


	/// <summary>
	/// Format the URI of the SmartUri base class.
	/// </summary>
	ndefSocialRecord.prototype.updatePayload = function () {
		var base = NdefLibrary.NdefSocialRecord.SocialTagTypeUris[this.socialType];
		var uri = String.format(base, this.socialUserName);
		this.setUri(uri);
	};

	/// <summary>
	/// Checks if the contents of the record are valid; throws an exception if
	/// a problem is found, containing a textual description of the issue.
	/// </summary>
	/// <exception cref="NdefException">Thrown if no valid NDEF record can be
	/// created based on the record's current contents. The exception message 
	/// contains further details about the issue.</exception>
	/// <returns>True if the record contents are valid, or throws an exception
	/// if an issue is found.</returns>
	ndefSocialRecord.checkIfValid = function () {

		// First check the basics
		if (!NdefLibrary.NdefUriRecord.prototype.checkIfValid()) return false;

		// Check specific content of this record
		if (this.socialUserName == null || this.socialUserName.length == 0) {
			throw new "NdefException(NdefExceptionMessages.ExTelNumberEmpty)";
		}

		return true;
	};

	ndefSocialRecord.prototype.getSocialUserName = function () {
		return this.socialUserName;
	};

	ndefSocialRecord.prototype.setSocialUserName = function (value) {
		this.socialUserName = value;
		this.updatePayload();
	};

	ndefSocialRecord.prototype.getSocialType = function () {
		return this.socialType;
	};

	ndefSocialRecord.prototype.setSocialType = function (value) {
		this.socialType = value;
		this.updatePayload();
	};


}
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefTelRecord(context) {
	'use strict';

	var NdefLibrary = context.NdefLibrary;


	/// <summary>
	/// Convenience class for formatting telephone call information into
	/// an NDEF record, depending on added info either URI or Smart Poster.
	/// </summary>
	/// <remarks>
	/// Tapping a tag with telephone information stored on it should trigger
	/// a dialog in the phone to call the specified number. This can for
	/// example be used to get in touch with customer service or support,
	/// or to book a hotel.
	/// 
	/// To create and write the record, specify the target phone number
	/// (in international format). This class will take care of properly
	/// encoding the information.
	/// 
	/// As this class is based on the Smart URI base class, the
	/// payload is formatted as a URI record initially.
	/// </remarks>
	var ndefTelRecord = NdefLibrary.NdefTelRecord = function (opt_config) {

		/// <summary>
		/// The number the reading phone is supposed to call.
		/// Recommended to store in international format, e.g., +431234...
		/// </summary>
		this.telNumber = "";

		///Constructors
		if (arguments.length == 1) {
			/// <summary>
			/// Create a telephone record based on another telephone record, or Smart Poster / URI
			/// record that have a Uri set that corresponds to the tel: URI scheme.
			/// </summary>
			/// <param name="other">Other record to copy the data from.</param>
			NdefLibrary.NdefUriRecord.call(this, arguments[0]);
			this.parseUriToData(this.getUri());
		}
		else {
			/// <summary>
			/// Create an empty telephone record. You need to set the number 
			/// to create a URI and make this record valid.
			/// </summary>
			NdefLibrary.NdefUriRecord.call(this);
		}
	};

	/// <summary>
	/// URI scheme in use for this record.
	/// </summary>
	NdefLibrary.NdefTelRecord.TelScheme = "tel:";

	//Derive from NdefRecord
	ndefTelRecord.prototype = new NdefLibrary.NdefUriRecord();
	ndefTelRecord.prototype.constructor = NdefLibrary.NdefTelRecord;

	/// <summary>
	/// Checks if the record sent via the parameter is indeed an Sms record.
	/// Checks the type and type name format and if the URI starts with the correct scheme.
	/// </summary>
	/// <param name="record">Record to check.</param>
	/// <returns>True if the record has the correct type, type name format and payload
	/// to be an Sms record, false if it's a different record.</returns>
	NdefLibrary.NdefTelRecord.isRecordType = function (record) {

		if (record.getType() == null || record.getType().length == 0) return false;

		if (record.getTypeNameFormat() == NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd && record.getPayload() != null) {
			if (arraysEqual(record.getType(), NdefLibrary.NdefUriRecord.UriType)) {
				var testRecord = new NdefLibrary.NdefUriRecord(record);
				return testRecord.getUri().startsWith(NdefLibrary.NdefTelRecord.TelScheme);
			}
		}
		return false;
	};

	/// <summary>
	/// Checks if the contents of the record are valid; throws an exception if
	/// a problem is found, containing a textual description of the issue.
	/// </summary>
	/// <exception cref="NdefException">Thrown if no valid NDEF record can be
	/// created based on the record's current contents. The exception message 
	/// contains further details about the issue.</exception>
	/// <returns>True if the record contents are valid, or throws an exception
	/// if an issue is found.</returns>
	ndefTelRecord.checkIfValid = function () {

		// First check the basics
		if (!NdefLibrary.NdefUriRecord.prototype.checkIfValid()) return false;

		// Check specific content of this record
		if (this.telNumber == null || this.telNumber.length == 0) {
			throw new "NdefException(NdefExceptionMessages.ExTelNumberEmpty)";
		}

		return true;
	};

	ndefTelRecord.prototype.getTelNumber = function () {
		return this.telNumber;
	};

	ndefTelRecord.prototype.setTelNumber = function (value) {
		this.telNumber = value;
		this.updatePayload();
	};

	/// <summary>
	/// Deletes any details currently stored in the telephone record 
	/// and re-initializes them by parsing the contents of the provided URI.
	/// </summary>
	/// <remarks>The URI has to be formatted according to the tel: URI scheme,
	/// and include the number.</remarks>
	ndefTelRecord.prototype.parseUriToData = function (uri) {
		// Extract telephone number from the payload        
		var tel = uri.replace("tel:", "");
		this.telNumber = tel;
		this.updatePayload();
	};

	/// <summary>
	/// Format the URI of the SmartUri base class.
	/// </summary>
	ndefTelRecord.prototype.updatePayload = function () {
		if (this.telNumber != null && this.telNumber.length > 0) {
			this.setUri(NdefLibrary.NdefTelRecord.TelScheme + this.telNumber);
		}
	};

}
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefUriRecord(context) {
	'use strict';

	var NdefLibrary = context.NdefLibrary;


	/// <summary>
	/// The URI record as specified by the NFC Forum URI record type definition.
	/// </summary>
	/// <remarks>
	/// The record stores a URI and can be stored on a tag or sent to another device.
	/// Several of the most common URI headers are automatically abbreviated in order
	/// to keep the record as small as possible. URIs will be encoded using UTF-8.
	/// </remarks>
	var ndefUriRecord = NdefLibrary.NdefUriRecord = function (opt_config) {

		/// <summary>
		/// Get the raw URI as stored in this record, excluding any abbreviations.
		/// </summary>
		/// <remarks>Gets the raw contents of the URI, exclusive the first byte of the
		/// record's payload that would contain the abbreviation code.
		/// If the URI has been abbreviated, this method returns retuns the actual URI
		/// text as stored on the tag. To get the full URI that has been expanded with
		/// the abbreviated URI scheme, use the normal Uri accessor.</remarks>
		this.RawUri = new Array();

		/// <summary>
		/// URI stored in this record.
		/// The abbreviation will be handled behind the scenes - getting and 
		/// setting this property will always work on the full URI.
		/// </summary>
		/// <remarks>
		/// Note that this property / class does not escape the URI data
		/// string to avoid double-escaping and to allow storing unescaped
		/// URIs if allowed by the protocol.
		/// For generic URLs, it's recommended to escape the URL string when
		/// sending it to this class, e.g., with System.Uri.EscapeUriString().
		/// </remarks>
		this.Uri = "";


		///Constructors
		if (arguments.length == 1) {
			NdefLibrary.NdefRecord.call(this, arguments[0]);
		}
		else {
			/// <summary>
			/// Create an empty URI record.
			/// </summary>
			NdefLibrary.NdefRecord.call(this, NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd, NdefLibrary.NdefUriRecord.UriType);
		}
	};


	// <summary>
	/// Type of the NDEF Text record (well-known, type 'U').
	/// </summary>
	NdefLibrary.NdefUriRecord.UriType = "U".getBytes(); // U

	/// <summary>
	/// URI abbreviations, as defined in NDEF URI record specifications.
	/// </summary>
	NdefLibrary.NdefUriRecord.Abbreviations = [
		"",
		"http://www.",
		"https://www.",
		"http://",
		"https://",
		"tel:",
		"mailto:",
		"ftp://anonymous:anonymous@",
		"ftp://ftp.",
		"ftps://",
		"sftp://",
		"smb://",
		"nfs://",
		"ftp://",
		"dav://",
		"news:",
		"telnet://",
		"imap:",
		"rtsp://",
		"urn:",
		"pop:",
		"sip:",
		"sips:",
		"tftp:",
		"btspp://",
		"btl2cap://",
		"btgoep://",
		"tcpobex://",
		"irdaobex://",
		"file://",
		"urn:epc:id:",
		"urn:epc:tag:",
		"urn:epc:pat:",
		"urn:epc:raw:",
		"urn:epc:",
		"urn:nfc:"
	];


	/// <summary>
	/// Checks if the record sent via the parameter is indeed a URI record.
	/// Only checks the type and type name format, doesn't analyze if the
	/// payload is valid.
	/// </summary>
	/// <param name="record">Record to check.</param>
	/// <returns>True if the record has the correct type and type name format
	/// to be a URI record, false if it's a different record.</returns>
	NdefLibrary.NdefUriRecord.isRecordType = function (record) {
		if (record.getType() == null || record.getType().length == 0) return false;
		return (record.getTypeNameFormat() == NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd && arraysEqual(record.getType(), NdefLibrary.NdefUriRecord.UriType));
	};

	//Derive from NdefRecord
	ndefUriRecord.prototype = new NdefLibrary.NdefRecord();
	ndefUriRecord.prototype.constructor = NdefLibrary.NdefUriRecord;

	ndefUriRecord.prototype.getRawUri = function () {
		if (this.getPayload() == null || this.getPayload().length == 0)
			return null;

		var code = this.getPayload()[0];
		if (code != 0) {
			console.log("NdefExceptionMessages.ExRawUriNoAbbreviation");
		}

		var rawUri = new Array(this.getPayload().length - 1);
		var payload = this.getPayload();
		arrayCopy(payload, 1, rawUri, 0, payload.length - 1);

		return rawUri;
	};

	ndefUriRecord.prototype.setRawUri = function (value) {
		var payload = new Array(value.length + 1);
		payload[0] = 0;
		arrayCopy(value, 0, payload, 1, value.length);
		this.setPayload(payload);
	};

	ndefUriRecord.prototype.setUri = function (value) {
		var uriString = value;
		// var encoding = Encoding.UTF8;
		var useAbbreviation = 0;

		for (var i = 1; i < NdefLibrary.NdefUriRecord.Abbreviations.length; i++) {
			if (uriString.startsWith(NdefLibrary.NdefUriRecord.Abbreviations[i])) {
				useAbbreviation = i;
				break;
			}
		}

		// Can abbreviate the URI
		var abbrevLength = NdefLibrary.NdefUriRecord.Abbreviations[useAbbreviation].length;

		var plainUri = uriString.replace(NdefLibrary.NdefUriRecord.Abbreviations[useAbbreviation], "");
		var encodedLength = getEncodeURILength(plainUri);

		var payload = new Array(encodedLength + 1);
		payload[0] = useAbbreviation;

		var escapedStr = encodeURI(plainUri);
		var escapedStrBytes = escapedStr.getBytes();
		arrayCopy(escapedStrBytes, 0, payload, 1, escapedStrBytes.length);

		this.setPayload(payload);
	};

	ndefUriRecord.prototype.getUri = function (value) {
		var payload = this.getPayload();
		if (payload == null || payload.length == 0) {
			return "";
		}

		// var encoding = Encoding.UTF8;
		var code = payload[0];
		if (code >= NdefLibrary.NdefUriRecord.Abbreviations.length) {
			code = 0;
		}

		var uri = new Array(payload.length - 1);

		arrayCopy(payload, 1, uri, 0, payload.length - 1);

		return NdefLibrary.NdefUriRecord.Abbreviations[code].concat(decodeURI(fromArray(uri)));
	};

}
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefTextRecord(context) {
    'use strict';

    var NdefLibrary = context.NdefLibrary;


    /// <summary>
    /// The Text record as specified by the NFC Forum URI record type definition.
    /// </summary>
    /// <remarks>
    /// Stores an arbritary string. Multiple text records can be part of a message,
    /// each of those should have a different language so that the reading device
    /// can choose the most appropriate language. Text can be encoded either in
    /// Utf-8 or Utf-16.
    /// 
    /// The text record is usually used within a Smart Poster record or other meta
    /// records, where the text contained here describes the properties of the
    /// other record, in order to for example guide the user.
    /// </remarks>
    var ndefTextRecord = NdefLibrary.NdefTextRecord = function (opt_config) {

        /// <summary>
        /// Language code that corresponds to the text.
        /// All language codes MUST be done according to RFC 3066 [RFC3066]. The language code MAY NOT be omitted. 
        /// </summary>
        this.LanguageCode = null;

        /// <summary>
        /// The text stored in the text record as a string.
        /// </summary>
        this.text = null;

        ///Constructors
        if (arguments.length == 1) {
            /// <summary>
            /// Create a Text record based on the record passed through the argument.
            /// </summary>
            /// <remarks> 
            /// Internalizes and parses the payload of the original record.
            /// The original record has to be a Text record as well.
            /// </remarks>
            /// <param name="other">Record to copy into this Text record.</param>
            /// <exception cref="NdefException">Thrown if attempting to create a Text record
            /// based on an incompatible record type.</exception>
            NdefLibrary.NdefRecord.call(this, arguments[0]);

            if (!NdefLibrary.NdefTextRecord.isRecordType(this))
                throw "NdefException(NdefExceptionMessages.ExInvalidCopy)";
        }
        else {
            /// <summary>
            /// Create an empty Text record.
            /// </summary>
            NdefLibrary.NdefRecord.call(this, NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd, NdefLibrary.NdefTextRecord.TextType);
        }
    };

    //Derive from NdefRecord
    ndefTextRecord.prototype = new NdefLibrary.NdefRecord();
    ndefTextRecord.prototype.constructor = NdefLibrary.NdefTextRecord;

    /// <summary>
    /// Type of the NDEF Text record (well-known, type 'T').
    /// </summary>
    NdefLibrary.NdefTextRecord.TextType = "T".getBytes(); // U

    /// <summary>
    /// Text encoding to use - either Utf8 or Utf16 (big endian)
    /// </summary>
    NdefLibrary.NdefTextRecord.TextEncodingType = {
        Utf8: 0,
        Utf16: 1
    };

    /// <summary>
    /// Checks if the record sent via the parameter is indeed a Text record.
    /// Only checks the type and type name format, doesn't analyse if the
    /// payload is valid.
    /// </summary>
    /// <param name="record">Record to check.</param>
    /// <returns>True if the record has the correct type and type name format
    /// to be a Text record, false if it's a different record.</returns>
    NdefLibrary.NdefTextRecord.isRecordType = function (record) {
        if (record.getType() == null || record.getType().length == 0) {
            return false;
        }
        return (record.getTypeNameFormat() == NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd && arraysEqual(record.getType(), NdefLibrary.NdefTextRecord.TextType));
    };


    ndefTextRecord.prototype.setLanguageCode = function (value) {
        this.assemblePayload(value, this.getTextEncoding(), this.getText());
    };

    ndefTextRecord.prototype.getLanguageCode = function () {
        if (this.getPayload() == null || this.getPayload().length == 0)
            return "en"; // Default

        var status = this.getPayload()[0];
        var codeLength = (status & 0x3f);

        // US-Ascii encoding, starting at byte 1, length codeLength
        // No ASCII encoding seems to be available, so use UTF8 instead
        // There shouldn't be any special chars in the language codes anyway.
        // var encoding = Encoding.UTF8;
        // return encoding.GetString(Payload, 1, codeLength);

        var encoding = new Array(codeLength);
        arrayCopy(this.getPayload(), 1, encoding, 0, codeLength);

        return decodeURI(fromArray(encoding));
    };


    ndefTextRecord.prototype.setText = function (value) {
        this.assemblePayload(this.getLanguageCode(), this.getTextEncoding(), value);
    };

    ndefTextRecord.prototype.getText = function () {
        if (this.getPayload() == null || this.getPayload().length == 0)
            return ""; // Default

        // var encoding = (this.getTextEncoding() == NdefLibrary.NdefTextRecord.TextEncodingType.Utf8) ? Encoding.UTF8 : Encoding.BigEndianUnicode;
        // Language code length
        var codeLength = (this.getPayload()[0] & 0x3f);

        var encoding = new Array();
        arrayCopy(this.getPayload(), 1 + codeLength, encoding, 0, this.getPayload().length - 1 - codeLength);

        //TODO UTF16 encoding

        return fromArray(encoding);
    };

    ndefTextRecord.prototype.setTextEncoding = function (value) {
        this.assemblePayload(this.getLanguageCode(), value, this.getText());
    };


    ndefTextRecord.prototype.getTextEncoding = function () {
        if (this.getPayload() == null || this.getPayload().length == 0)
            return NdefLibrary.NdefTextRecord.TextEncodingType.Utf8;

        return ((this.getPayload()[0] & 0x80) != 0) ? NdefLibrary.NdefTextRecord.TextEncodingType.Utf16 : NdefLibrary.NdefTextRecord.TextEncodingType.Utf8;
    };

    ndefTextRecord.prototype.assemblePayload = function (languageCode, textEncoding, text) {
        // Convert the language code to a byte array
        // var languageEncoding = Encoding.UTF8;
        var encodedLanguage = languageCode.getBytes();
        // Encode and convert the text to a byte array
        // var encoding = (textEncoding == NdefLibrary.NdefTextRecord.TextEncodingType.Utf8) ? Encoding.UTF8 : Encoding.BigEndianUnicode;
        // var encodedText = encoding.GetBytes(text);
        var encodedText = text.getBytes();

        // Calculate the length of the payload & create the array
        var payloadLength = 1 + encodedLanguage.length + encodedText.length;
        var payload = new Array(payloadLength);

        // Assemble the status byte
        payload[0] = 0; // Make sure also the RFU bit is set to 0
        // Text encoding
        if (textEncoding == NdefLibrary.NdefTextRecord.TextEncodingType.Utf8)
            payload[0] &= 0x7F; // ~0x80
        else
            payload[0] |= 0x80;

        // Language code length
        payload[0] |= (0x3f & encodedLanguage.length);

        // Language code
        arrayCopy(encodedLanguage, 0, payload, 1, encodedLanguage.length);

        // Text
        arrayCopy(encodedText, 0, payload, 1 + encodedLanguage.length, encodedText.length);

        this.setPayload(payload);
    };


}
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefRecord(context) {
    'use strict';

    var NdefLibrary = context.NdefLibrary;


    /// <summary>
    /// An NDEF record contains a payload described by a type, a length, and an optional identifier.
    /// </summary>
    /// <remarks>
    /// This class is generic and can hold the data of any kind of record.
    /// It follows the specification from the NFC forum for the data format.
    /// 
    /// Ndef Records should usually be placed within an Ndef Message, which will
    /// make sure that for example the message begin / end flags are set correctly.
    /// 
    /// While the NdefRecord class only offers access to the payload as a byte array,
    /// you should rather use specialized sub classes, which offer convenient ways
    /// to handle data stored in the payload through easy access methods.
    /// Such classes are provided for several standardized types.
    /// </remarks>
    var ndefRecord = NdefLibrary.NdefRecord = function (opt_config) {

        /// <summary>
        /// Indicates the structure of the value of the TYPE field.
        /// </summary>
        this._typeNameFormat = NdefLibrary.NdefRecord.TypeNameFormatType.Empty;

        /// <summary>
        /// Byte array storing raw contents of the type.
        /// Use Type property to access it whenever possible.
        /// </summary>
        /// <remarks>Direct byte array access provided as virtual
        /// property can't be accessed from constructor.</remarks>
        this._type = [];

        /// <summary>
        /// An identifier in the form of a URI reference.
        /// </summary>
        this._id = [];

        /// <summary>
        /// Byte array storing raw contents of the payload.
        /// Use Payload property to access it whenever possible.
        /// </summary>
        /// <remarks>Direct byte array access provided as virtual
        /// property can't be accessed from constructor.</remarks>
        this._payload = [];


        ///Constructors
        if (arguments.length == 2) {
            /// <summary>
            /// Create a new record with the specified type name format and type.
            /// Doesn't set the payload and ID.
            /// </summary>
            /// <param name="tnf">Type name format to use, based on the tnf's standardized
            /// by the Nfc Forum.</param>
            /// <param name="type">Type string.</param>

            this._typeNameFormat = arguments[0];
            if (arguments[1] != null) {
                this.setType(arguments[1]);
            }
        } else if (arguments.length == 1) {
            /// <summary>
            /// Create a new record, copying the information of the record sent through the parameter.
            /// </summary>
            /// <param name="other">Record to copy.</param>
            var other = arguments[0];

            if (other.getType() != null) {
                this.setType(other.getType());
            }

            if (other.getId() != null) {
                this.setId(other.getId());
            }

            if (other.getPayload() != null) {
                this.setPayload(other.getPayload());
            }

            this._typeNameFormat = other.getTypeNameFormat();
        }
        else {
            /// <summary>
            /// Create an empty record, not setting any information.
            /// </summary>
            this._typeNameFormat = NdefLibrary.NdefRecord.TypeNameFormatType.Empty;
        }
    };


    /// <summary>
    /// Standardized type name formats, as defined by the NDEF record
    /// specification from the Nfc Forum.
    /// </summary>
    ndefRecord.TypeNameFormatType = {
        /// Empty indicates that no type or payload is associated with this record.
        "Empty": 0x00,
        /// The NFC Forum well-known type follows the RTD type name format defined in the NFC Forum RTD specification.
        "NfcRtd": 0x01,
        /// The media type indicates that the TYPE field contains a value that follows the media-type BNF construct defined by RFC 2046.
        "Mime": 0x02,
        /// Absolute-URI indicates that the TYPE field contains a value that follows the absolute-URI BNF construct defined by RFC 3986.
        "Uri": 0x03,
        /// NFC Forum external type indicates that the TYPE field contains a value that follows the type name format defined in [NFC RTD] for external type names.
        "ExternalRtd": 0x04,
        /// Unknown SHOULD be used to indicate that the type of the payload is unknown. This is similar to the "application/octet-stream" media type defined by MIME.
        /// When used, the TYPE_LENGTH field MUST be zero and thus the TYPE field is omitted from the NDEF record.
        "Unknown": 0x05,
        /// Unchanged MUST be used in all middle record chunks and the terminating record chunk used in chunked payloads.
        "Unchanged": 0x06,
        /// Any other type name format; should be treated as unknown. 
        "Reserved": 0x07
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.getId = function () {
        return this._id;
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.setId = function (value) {
        if (value == null) {
            this._id = null;
            return;
        }
        this._id = [];
        arrayCopy(value, this._id, value.length);
    };

    /// <summary>
    /// An identifier describing the type of the payload.
    /// </summary>
    ndefRecord.prototype.getType = function () {
        return this._type;
    };

    /// <summary>
    /// An identifier describing the type of the payload.
    /// </summary>
    ndefRecord.prototype.setType = function (value) {
        if (value == null) {
            this._type = null;
            return;
        }
        this._type = [];
        arrayCopy(value, this._type, value.length);
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.getPayload = function () {
        return this._payload;
    };

    /// <summary>
    /// An identifier in the form of a URI reference.
    /// </summary>
    ndefRecord.prototype.setPayload = function (value) {
        if (value == null) {
            this._payload = null;
            return;
        }
        this._payload = [];
        arrayCopy(value, this._payload, value.length);
    };


    /// <summary>
    /// Indicates the structure of the value of the TYPE field.
    /// </summary>
    ndefRecord.prototype.getTypeNameFormat = function () {
        return this._typeNameFormat;
    };

    /// <summary>
    /// Indicates the structure of the value of the TYPE field.
    /// </summary>
    ndefRecord.prototype.setTypeNameFormat = function (value) {
        this._typeNameFormat = value;
    };

    /// Checks the type name format and type of this record and returns
    /// the appropriate specialized class, if one is available and known
    /// for this record type.
    /// </summary>
    /// <returns>Type name of the specialized class that can understand
    /// and manipulate the payload through convenience methods.</returns>
    ndefRecord.prototype.checkSpecializedType = function (checkForSubtypes) {
        //TODO
    };


    /// <summary>
    /// Checks if the contents of the record are valid; throws an exception if
    /// a problem is found, containing a textual description of the issue.
    /// </summary>
    /// <exception cref="NdefException">Thrown if no valid NDEF record can be
    /// created based on the record's current contents. The exception message 
    /// contains further details about the issue.</exception>
    /// <returns>True if the record contents are valid, or throws an exception
    /// if an issue is found.</returns>
    ndefRecord.prototype.checkIfValid = function () {
        // Check Type (according to TNF)
        if (this._typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged ||
            this._typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unknown) {
            // Unknown and unchanged TNF must have a type length of 0
            if (!(this._type == null || this._type.length == 0)) {
                throw "NdefExceptionMessages.ExRecordUnchangedTypeName";
            }
        }
        else {
            // All other TNF (except Empty) should have a type set
            // Proximity APIs unstable in some cases when no Type name is set
            if (this._typeNameFormat != NdefLibrary.NdefRecord.TypeNameFormatType.Empty && (this._type == null || this._type.length == 0)) {
                throw "NdefExceptionMessages.ExRecordNoType";
            }
        }
        // Check ID
        // Middle and terminating record chunks MUST not have an ID field
        if (this._typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged && !(this._id == null || this._id.length == 0))
            throw "NdefExceptionMessages.ExRecordUnchangedId";
        return true;
    };

}
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

function initLibraryNdefMessage(context) {
    'use strict';

    var NdefLibrary = context.NdefLibrary;


    /// <summary>
    /// An NDEF message is composed of one or more NDEF records.
    /// </summary>
    /// <remarks>
    /// This class is essentially just a list of records, and provides
    /// the necessary methods to convert all the records into a single
    /// byte array that can be written to a tag and has the correct
    /// flags set (e.g., message begin / end).
    /// NdefMessage can also parse a byte array into an NDEF message,
    /// separating and creating all the individual records out of the array.
    /// 
    /// From the NFC Forum specification document:
    /// NFC Forum Data Exchange Format is a lightweight binary message 
    /// format designed to encapsulate one or more application-defined 
    /// payloads into a single message construct.
    /// 
    /// An NDEF message contains one or more NDEF records, each carrying 
    /// a payload of arbitrary type and up to (2^32)-1 octets in size. 
    /// Records can be chained together to support larger payloads. 
    /// 
    /// An NDEF record carries three parameters for describing its payload: 
    /// the payload length, the payload type, and an optional payload identifier.
    /// </remarks>
    var ndefMessage = NdefLibrary.NdefMessage = function () {

        //Holds the NdefRecords added to the NdefMessge
        this._records = new Array();

        if (arguments != null && arguments.length > 0) {
            // console.log("Create NdefMessage with "+ arguments.length+" records");
            arrayCopy(arguments, this._records, arguments.length);
        }
    };


    ndefMessage.prototype.length = function () {
        if (this._records == null)
            return 0;
        return this._records.length;
    };

    ndefMessage.prototype.getRecords = function () {
        return this._records;
    };

    ndefMessage.prototype.push = function (value) {
        if (value == null) {
            return;
        }
        this._records.push(value);
    };

    ndefMessage.prototype.clear = function () {
        this._records = new Array();
    };


    /// <summary>
    /// Returns the NDEF message parsed from the contents of <paramref name="message"/>.
    /// </summary>
    /// <remarks>
    /// The <paramref name="message"/> parameter is interpreted as the raw message format 
    /// defined in the NFC Forum specifications.
    /// </remarks>
    /// <param name="message">Raw byte array containing the NDEF message, which consists
    /// of 0 or more NDEF records.</param>
    /// <exception cref="NdefException">Thrown if there is an error parsing the NDEF
    /// message out of the byte array.</exception>
    /// <returns>If parsing was successful, the NDEF message containing 0 or more NDEF
    /// records.</returns>
    NdefLibrary.NdefMessage.fromByteArray = function (message) {

        var result = new NdefLibrary.NdefMessage();

        var seenMessageBegin = false;
        var seenMessageEnd = false;

        var partialChunk = new Array();
        var record = new NdefLibrary.NdefRecord();

        var i = 0;
        while (i < message.length) {
            // console.log("Parsing byte[] to NDEF message. New record starts at {0}", i);

            // Parse flags out of NDEF message header
            var messageBegin = (message[i] & 0x80) != 0;
            var messageEnd = (message[i] & 0x40) != 0;
            var cf = (message[i] & 0x20) != 0;
            var sr = (message[i] & 0x10) != 0;
            var il = (message[i] & 0x08) != 0;

            var typeNameFormat = (message[i] & 0x07);

            // console.log("TypeNameFormat: " + typeNameFormat+" message[i]:"+message[i]);
            // console.log("ShortRecord: " + (sr ? "yes" : "no"));
            // console.log("Id Length present: " + (il ? "yes" : "no"));

            if (messageBegin && seenMessageBegin) {
                throw "NdefExceptionMessages.ExMessageBeginLate";
            }
            else if (!messageBegin && !seenMessageBegin) {
                throw "NdefExceptionMessages.ExMessageBeginMissing";
            }
            else if (messageBegin && !seenMessageBegin) {
                seenMessageBegin = true;
            }

            if (messageEnd && seenMessageEnd) {
                throw "NdefExceptionMessages.ExMessageEndLate";
            }
            else if (messageEnd && !seenMessageEnd) {
                seenMessageEnd = true;
            }

            if (cf && (typeNameFormat != NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) && partialChunk.length > 0) {
                throw "NdefExceptionMessages.ExMessagePartialChunk";
            }

            // Header length
            var headerLength = 1;
            headerLength += (sr) ? 1 : 4;
            headerLength += (il) ? 1 : 0;

            if (i + headerLength >= message.length) {
                throw "NdefExceptionMessages.ExMessageUnexpectedEnd";
            }

            // Type length
            var typeLength = message[++i];

            if ((typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) && (typeLength != 0)) {
                throw "NdefExceptionMessages.ExMessageInvalidChunkedType";
            }

            // Payload length (short record?)
            var payloadLength;
            if (sr) {
                // Short record - payload length is a single octet
                payloadLength = message[++i];
            }
            else {
                // No short record - payload length is four octets representing a 32 bit unsigned integer (MSB-first)
                payloadLength = ((message[++i]) << 24);
                payloadLength |= ((message[++i]) << 16);
                payloadLength |= ((message[++i]) << 8);
                payloadLength |= ((message[++i]) << 0);
            }

            // ID length
            var idLength;
            idLength = (il ? message[++i] : 0);

            // Total length of content (= type + payload + ID)
            var contentLength = typeLength + payloadLength + idLength;
            if (i + contentLength >= message.length) {
                throw "NdefExceptionMessages.ExMessageUnexpectedEnd";
            }


            if ((typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) && (idLength != 0)) {
                throw "NdefExceptionMessages.ExMessageInvalidChunkedId";
            }

            if (typeNameFormat != NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) {
                record.setTypeNameFormat(typeNameFormat);
            }

            // Read type
            if (typeLength > 0) {
                var type = new Array();
                arrayCopy(message, (++i), type, 0, typeLength);
                record.setType(type);

                i += typeLength - 1;
            }

            // Read ID
            if (idLength > 0) {
                var id = new Array();
                arrayCopy(message, (++i), id, 0, idLength);
                record.setId(id);

                i += idLength - 1;
            }

            // Read payload
            if (payloadLength > 0) {
                var payload = new Array();
                arrayCopy(message, (++i), payload, 0, payloadLength);
                record.setPayload(payload);

                if (cf) {
                    // chunked payload, except last
                    addToArray(partialChunk, payload);
                }
                else if (typeNameFormat == NdefLibrary.NdefRecord.TypeNameFormatType.Unchanged) {
                    // last chunk of chunked payload
                    addToArray(partialChunk, payload);
                    record.setPayload(partialChunk);
                }
                else {
                    // non-chunked payload
                    record.setPayload(payload);
                }

                i += payloadLength - 1;
            }

            if (!cf) {
                // Add record to the message and create a new record for the next loop iteration
                result.push(record);
                record = new NdefLibrary.NdefRecord();
            }

            if (!cf && seenMessageEnd)
                break;

            // move to start of next record
            ++i;
        }


        if (!seenMessageBegin && !seenMessageEnd) {
            throw "NdefExceptionMessages.ExMessageNoBeginOrEnd";
        }

        return result;

    };


    /// <summary>
    /// Convert all the NDEF records currently stored in the NDEF message to a byte
    /// array suitable for writing to a tag or sending to another device.
    /// </summary>
    /// <returns>The NDEF record(s) converted to an NDEF message.</returns>
    ndefMessage.prototype.toByteArray = function () {

        var count = this._records.length;

        // Empty message: single empty record
        if (count == 0) {
            var tmpNdefMessage = new NdefLibrary.NdefMessage(new NdefLibrary.NdefRecord());
            return tmpNdefMessage.toByteArray();
        }

        var m = new Array();

        for (var i = 0; i < count; i++) {
            var record = this._records[i];

            var flags = record.getTypeNameFormat();

            // Message begin / end flags. If there is only one record in the message,
            // both flags are set.
            if (i == 0)
                flags |= 0x80;      // MB (message begin = first record in the message)
            if (i == count - 1)
                flags |= 0x40;      // ME (message end = last record in the message)

            // cf (chunked records) not supported yet

            // SR (Short Record)?
            if (record.getPayload() == null || record.getPayload().length < 255)
                flags |= 0x10;

            // ID present?
            if (record.getId() != null && record.getId().length > 0)
                flags |= 0x08;

            m.push(flags);

            // Type length
            if (record.getType() != null) m.push(record.getType().length); else m.push(0);

            // Payload length 1 byte (SR) or 4 bytes
            if (record.getPayload() == null)
                m.push(0);
            else {
                if ((flags & 0x10) != 0) {
                    // SR
                    m.push(record.getPayload().length);
                }
                else {
                    // No SR (Short Record)
                    var payloadLength = record.getPayload().length;

                    m.push((payloadLength >> 24));
                    m.push((payloadLength >> 16));
                    m.push((payloadLength >> 8));
                    m.push((payloadLength & 0x000000ff));
                }
            }

            // ID length
            if (record.getId() != null && (flags & 0x08) != 0)
                m.push(record.getId().length);

            // Type length
            if (record.getType() != null && record.getType().length > 0)
                addToArray(m, record.getType());

            // ID data
            if (record.getId() != null && record.getId().length > 0)
                addToArray(m, record.getId());

            // Payload data
            if (record.getPayload() != null && record.getPayload().length > 0)
                addToArray(m, record.getPayload());

        }

        return m;
    };

}
/****************************************************************************
**
** Copyright (C) 2012-2014 Sebastian Höbarth, http://www.mobilefactory.at/
** Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
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

/*global initLibraryCore initLibraryModule initLibrarySubmodule */
var initNdefLibrary = function (context) {

    //Basic library
    initLibraryCore(context);

    // Add a similar line as above for each module that you have.  If you have a
    // module named "Awesome Module," it should live in the file
    // "src/library.awesome-module.js" with a wrapper function named
    // "initAwesomeModule".  That function should then be invoked here with:
    initLibraryModule(context);
    initLibraryNdefRecord(context);
    initLibraryNdefMessage(context);

    initLibraryNdefUriRecord(context);
    initLibraryNdefAndroidAppRecord(context);
    initLibraryNdefTelRecord(context);
    initLibraryNdefSocialRecord(context);
    initLibraryNdefGeoRecord(context);
    initLibraryNdefTextRecord(context);


    return context.NdefLibrary;
};


if (typeof define === 'function' && define.amd) {
    // Expose Library as an AMD module if it's loaded with RequireJS or
    // similar.
    define(function () {
        return initNdefLibrary({});
    });
} else {
    // Load Library normally (creating a Library global) if not using an AMD
    // loader.
    initNdefLibrary(this);
}

} (this));
