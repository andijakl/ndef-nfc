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