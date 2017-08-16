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
