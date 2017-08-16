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



