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