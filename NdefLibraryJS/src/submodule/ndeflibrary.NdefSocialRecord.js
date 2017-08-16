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