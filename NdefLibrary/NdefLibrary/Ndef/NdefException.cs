/****************************************************************************
**
** Copyright (C) 2012-2016 Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
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

using System;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Exception occured when parsing or assembling NDEF messages, records and their
    /// respective payloads.
    /// </summary>
    public class NdefException : Exception
    {
        public NdefException() { }
        public NdefException(string message) : base(message) { }
        public NdefException(string message, Exception inner) : base(message, inner) { }
    }
}
