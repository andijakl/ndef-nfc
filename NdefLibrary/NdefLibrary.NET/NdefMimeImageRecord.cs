/****************************************************************************
**
** Copyright (C) 2012-2018 Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
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
using System.IO;
using System.Threading.Tasks;
using NdefLibrary.Ndef;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Extended MIME / Image record that integrates with
    /// the image handling to support convenient conversion to and
    /// from various formats (PNG, JPEG, etc.)
    /// </summary>
    public class NdefMimeImageRecord : NdefMimeImageRecordBase
    {
        public NdefMimeImageRecord(NdefRecord other)
            : base(other)
        {
        }

        protected NdefMimeImageRecord()
        {
        }

        #region Public factory constructor methods

        public static NdefMimeImageRecord CreateFromImage(NdefImage image, ImageMimeType mimeType)
        {
            var imgRecord = new NdefMimeImageRecord();
            imgRecord.SetImage(image, mimeType);
            return imgRecord;
        }

        public static NdefMimeImageRecord CreateFromFile(string filePath)
        {
            var imgRecord = new NdefMimeImageRecord();
            imgRecord.LoadFile(filePath);
            return imgRecord;
        }

        public static NdefMimeImageRecord CreateFromStream(Stream imgStream)
        {
            var imgRecord = new NdefMimeImageRecord();
            imgRecord.LoadStream(imgStream);
            return imgRecord;
        }
        #endregion

        #region Set and modify the image data
        public void SetImage(NdefImage image, ImageMimeType mimeType)
        {
            using (var imgStream = new MemoryStream())
            {
                var encoder = GetEncoder(mimeType);
                image.Image.Save(imgStream, encoder);
                Payload = imgStream.ToArray();
                Type = ImageMimeTypes[mimeType];
            }
        }

        public void LoadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            using (var fileStream = File.OpenRead(filePath))
            {
                LoadStream(fileStream);
            }
        }

        public void LoadStream(Stream imgStream)
        {
            if (imgStream == null) throw new ArgumentNullException("imgStream");
            var image = Image.Load(imgStream);
            var mimeType = GetMimeType(image.Metadata.DecodedImageFormat);

            imgStream.Position = 0;
            using (var memoryStream = new MemoryStream())
            {
                imgStream.CopyTo(memoryStream);
                Payload = memoryStream.ToArray();
            }

            Type = ImageMimeTypes[mimeType];
        }

        public void DetermineMimeTypeFromPayload()
        {
            if (Payload == null) return;
            using (var stream = new MemoryStream(Payload))
            {
                var image = Image.Load(stream);
                Type = ImageMimeTypes[GetMimeType(image.Metadata.DecodedImageFormat)];
            }
        }
        #endregion

        #region Retrieve the image
        public NdefImage GetImage()
        {
            if (Payload == null) return null;
            using (var stream = new MemoryStream(Payload))
            {
                var image = Image.Load<Rgba32>(stream);
                return new NdefImage { Image = image };
            }
        }
        #endregion

        #region Utility methods
        private IImageEncoder GetEncoder(ImageMimeType mimeType)
        {
            switch (mimeType)
            {
                case ImageMimeType.Png:
                    return new SixLabors.ImageSharp.Formats.Png.PngEncoder();
                case ImageMimeType.Jpg:
                case ImageMimeType.Jpeg:
                    return new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                case ImageMimeType.Bmp:
                    return new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder();
                case ImageMimeType.Gif:
                    return new SixLabors.ImageSharp.Formats.Gif.GifEncoder();
                case ImageMimeType.Tiff:
                    return new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ImageMimeType GetMimeType(IImageFormat format)
        {
            if (format == SixLabors.ImageSharp.Formats.Png.PngFormat.Instance) return ImageMimeType.Png;
            if (format == SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance) return ImageMimeType.Jpeg;
            if (format == SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance) return ImageMimeType.Bmp;
            if (format == SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance) return ImageMimeType.Gif;
            if (format == SixLabors.ImageSharp.Formats.Tiff.TiffFormat.Instance) return ImageMimeType.Tiff;
            throw new ArgumentOutOfRangeException();
        }
        #endregion
    }
}