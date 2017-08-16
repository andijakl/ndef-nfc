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
