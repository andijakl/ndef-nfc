/****************************************************************************
**
** Copyright (C) 2012-2015 Andreas Jakl - http://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
** More information: http://andijakl.github.io/ndef-nfc/
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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Stores a MIME type and image data for individual use or
    /// as a part of another record like the Smart Poster.
    /// </summary>
    /// <remarks>
    /// This base class provides the general record layout and various
    /// common used MIME types. As the actual interaction with bitmaps
    /// and encoded images (PNG, JPEG, etc.) is not possible from within
    /// a portable class library and requires platform-specific functions,
    /// you can use the derived class in the extension library for
    /// convenience methods to work with the image.
    /// </remarks>
    public class NdefMimeImageRecordBase : NdefRecord
    {
        /// <summary>
        /// Common Image MIME type definitions, to be used when accessing
        /// the ImageMimeTypes dictionary to retrieve the correct MIME type
        /// string for the NDEF record.
        /// </summary>
        public enum ImageMimeType
        {
            Png,
            Jpg,
            Jpeg,
            JpegXr,
            Bmp,
            Gif,
            Tiff
        }

        /// <summary>
        /// Prefix for all the MIME types.
        /// </summary>
        private const string MimeTypeImage = "image/";

        /// <summary>
        /// Dictionary containing the most common MIME type definitions
        /// for various image formats.
        /// </summary>
        public static readonly Dictionary<ImageMimeType,byte[]> ImageMimeTypes = new Dictionary<ImageMimeType, byte[]>
        {
            {ImageMimeType.Png, Encoding.UTF8.GetBytes(MimeTypeImage + "png")},
            {ImageMimeType.Jpg, Encoding.UTF8.GetBytes(MimeTypeImage + "jpg")},
            {ImageMimeType.Jpeg, Encoding.UTF8.GetBytes(MimeTypeImage + "jpeg")},
            {ImageMimeType.JpegXr, Encoding.UTF8.GetBytes(MimeTypeImage + "vnd.ms-photo")},
            {ImageMimeType.Bmp, Encoding.UTF8.GetBytes(MimeTypeImage + "bmp")},
            {ImageMimeType.Gif, Encoding.UTF8.GetBytes(MimeTypeImage + "gif")},
            {ImageMimeType.Tiff, Encoding.UTF8.GetBytes(MimeTypeImage + "tiff")},
        };
        
        /// <summary>
        /// Create an empty MIME / Image Record with the type of PNG.
        /// </summary>
        protected NdefMimeImageRecordBase()
            : base(TypeNameFormatType.Mime, ImageMimeTypes[ImageMimeType.Png])
        {
        }

        /// <summary>
        /// Create a MIME / Image record based on the record passed
        /// through the argument.
        /// </summary>
        /// <remarks>
        /// The original record has to be a MIME / Image Record as well.
        /// </remarks>
        /// <param name="other">Record to copy into this Image record.</param>
        /// <exception cref="NdefException">Thrown if attempting to create an Image record
        /// based on an incompatible record type.</exception>
        public NdefMimeImageRecordBase(NdefRecord other)
            : base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessages.ExInvalidCopy);
        }


        /// <summary>
        /// Checks if the record sent via the parameter is indeed a MIME / Image record.
        /// Only checks the type and type name format, doesn't analyze if the
        /// payload is valid.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type and type name format
        /// to be a MIME / Image record, false if it's a different record.</returns>
        public static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;

            var foundMime = record.Type != null &&
                ImageMimeTypes.Values.Any(t => t.SequenceEqual(record.Type));
            return (record.TypeNameFormat == TypeNameFormatType.Mime && foundMime);
        }
    }
}
