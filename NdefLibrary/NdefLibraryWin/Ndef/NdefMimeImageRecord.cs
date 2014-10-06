/****************************************************************************
**
** Copyright (C) 2012-2014 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
** More information: http://ndef.mopius.com/
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
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using NdefLibrary.Ndef;

namespace NdefLibraryWin.Ndef
{
    /// <summary>
    /// Extended MIME / Image record for the WinRT APIs that integrates with
    /// the platform's image handling to support convenient conversion to and
    /// from various formats (WriteableBitmap, PNG, JPEG, etc.)
    /// </summary>
    /// <remarks>
    /// If you are working on a platform that supports the WinRT APIs and you
    /// need to interact with the image data, you can use this derived class 
    /// instead of the base class.
    /// 
    /// It adds several convenient methods to directly construct this record
    /// from a WriteableBitmap, from a stream containing image data or from
    /// a file. If loading from a stream / file, the class will automatically
    /// determine the correct MIME type and set it to the record.
    /// 
    /// When loading & parsing an existing record, just construct this class
    /// based on a generic NdefRecord. You can then use convenient getter methods
    /// to retrieve a WriteableBitmap from the image data payload, no matter
    /// which encoding the image data is actually using.
    /// </remarks>
    public class NdefMimeImageRecord : NdefMimeImageRecordBase
    {
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
        public NdefMimeImageRecord(NdefRecord other)
            : base(other)
        {
        }

        protected NdefMimeImageRecord()
        {
        }

        #region Public factory constructor methods
        
        /// <summary>
        /// Construct a new MIME / Image record based on a WriteableBitmap.
        /// Specify the MIME type and DPI, and the image data is encoded accordingly. 
        /// </summary>
        /// <param name="bmp">Source bitmap to use for conversion.</param>
        /// <param name="mimeType">MIME type to specify the target image encoding.</param>
        /// <param name="dpi">Optional parameter to set the DPI of the encoded image
        /// (if supported by the specific MIME type).</param>
        /// <returns>A newly constructed MIME / Image record with the WriteableBitmap
        /// encoded into the specified MIME type.</returns>
        public static async Task<NdefMimeImageRecord> CreateFromBitmap(WriteableBitmap bmp, ImageMimeType mimeType,
            double dpi = 96.0)
        {
            var imgRecord = new NdefMimeImageRecord();
            await imgRecord.SetImage(bmp, mimeType, dpi);
            return imgRecord;
        }

        /// <summary>
        /// Construct a new MIME / Image record based on a file.
        /// </summary>
        /// <param name="file">Reference to a file that will be opened and parsed
        /// by this class, to load its contents into the Payload of this record
        /// and set the MIME type correctly depending on the file contents.</param>
        /// <returns>A newly constructed MIME / Image record with the file data
        /// as payload and the MIME type automatically determined based on the 
        /// image data.</returns>
        public static async Task<NdefMimeImageRecord> CreateFromFile(StorageFile file)
        {
            var imgRecord = new NdefMimeImageRecord();
            await imgRecord.LoadFile(file);
            return imgRecord;
        }

        /// <summary>
        /// Construct a new MIME / Image record based on a stream.
        /// </summary>
        /// <param name="imgStream">Reference to a stream containing image data
        /// (encoded for example as a PNG or JPEG). The stream will be parsed
        /// by this class, to load its contents into the Payload of this record
        /// and set the MIME type correctly depending on the stream contents.</param>
        /// <returns>A newly constructed MIME / Image record with the stream data
        /// as payload and the MIME type automatically determined based on the 
        /// image data.</returns>
        public static async Task<NdefMimeImageRecord> CreateFromStream(IRandomAccessStream imgStream)
        {
            var imgRecord = new NdefMimeImageRecord();
            await imgRecord.LoadStream(imgStream);
            return imgRecord;
        }
        #endregion

        #region Set and modify the image data
        /// <summary>
        /// Set an image to this class by specifying the WritableBitmap and the target encoding and
        /// optionally the DPI to set for the target file (if supported by the target file type).
        /// </summary>
        /// <remarks>
        /// The method takes the bitmap as input and uses the BitmapEncoder to encode
        /// the image data into the specified MIME type and file format. The encoded contents are
        /// then stored to the payload of the record, the MIME type is set as the type of the record.
        /// </remarks>
        /// <param name="bmp">Input bitmap to encode and set as payload of this record.</param>
        /// <param name="mimeType">MIME type to use for encoding this image.</param>
        /// <param name="dpi">Target DPI if supported by the target file / MIME format.</param>
        /// <returns>Task to await completion of the asynchronous image encoding.</returns>
        public async Task SetImage(WriteableBitmap bmp, ImageMimeType mimeType, double dpi = 96.0)
        {
            var encoderId = GetBitmapEncoderIdForMimeType(mimeType);
            byte[] pixels;
            using (var stream = bmp.PixelBuffer.AsStream())
            {
                pixels = new byte[(uint)stream.Length];
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            using (var imgStream = new MemoryStream())
            {
                var raImgStream = imgStream.AsRandomAccessStream();
                var encoder = await BitmapEncoder.CreateAsync(encoderId, raImgStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                                    (uint)bmp.PixelWidth,
                                    (uint)bmp.PixelHeight,
                                    dpi,
                                    dpi,
                                    pixels);
                await encoder.FlushAsync();
                await raImgStream.FlushAsync();
                Payload = imgStream.ToArray();
                Type = ImageMimeTypes[mimeType];
            }
        }

        /// <summary>
        /// Set an image to this class by specifying an input file that has to be one of the 
        /// supported MIME types (e.g., a JPEG or PNG file).
        /// </summary>
        /// <remarks>
        /// Specify the reference to a file that is one of the supported MIME types - e.g.,
        /// a JPEG or PNG image. The file contents are set as the payload of this record.
        /// This method will try to find out the MIME type of the referenced
        /// file automatically and adapt the type of the NDEF record accordingly.
        /// </remarks>
        /// <returns>Task to await completion of the asynchronous image loading / parsing.</returns>
        public async Task LoadFile(StorageFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            using (var fileStream =
                await file.OpenAsync(FileAccessMode.Read))
            {
                await LoadStream(fileStream);
            }
        }

        /// <summary>
        /// Set an image to this class by specifying an input stream that contains image data
        /// in one of the supported MIME types (e.g., JPEG or PNG file contents).
        /// </summary>
        /// <remarks>
        /// The stream has to contain one of the supported MIME types - e.g.,
        /// a JPEG or PNG image. The stream contents are set as the payload of this record.
        /// This method will try to find out the MIME type of the referenced
        /// stream contents automatically and adapt the type of the NDEF record accordingly.
        /// </remarks>
        /// <returns>Task to await completion of the asynchronous image loading / parsing.</returns>
        public async Task LoadStream(IRandomAccessStream imgStream)
        {
            if (imgStream == null) throw new ArgumentNullException("imgStream");
            var decoder = await BitmapDecoder.CreateAsync(imgStream);
            var fileCodecId = decoder.DecoderInformation.CodecId;

            Payload = new byte[imgStream.Size];
            await imgStream.ReadAsync(Payload.AsBuffer(), (uint)imgStream.Size, InputStreamOptions.None);
            Type = ImageMimeTypes[GetMimeTypeForBitmapDecoderId(fileCodecId)];
        }

        /// <summary>
        /// Utility method to determine & update the MIME type of the record according
        /// to the current payload.
        /// </summary>
        /// <remarks>
        /// Mostly for internal use, but can also be used externally if you
        /// set the Payload directly and want the class to adapt its MIME type based
        /// on the new Payload contents.
        /// </remarks>
        /// <returns>Task to await completion of determining the payload MIME type.</returns>
        public async Task DetermineMimeTypeFromPayload()
        {
            if (Payload == null) return;

            // Determine image type of payload
            var payloadStream = ConvertArrayToStream(Payload);
            var decoder = await BitmapDecoder.CreateAsync(payloadStream);
            var fileCodecId = decoder.DecoderInformation.CodecId;
            Type = ImageMimeTypes[GetMimeTypeForBitmapDecoderId(fileCodecId)];
        }
        #endregion

        #region Retrieve the image
        /// <summary>
        /// Retrieve the image stored in this class as a WriteableBitmap. Automatically 
        /// decodes the image.
        /// </summary>
        /// <remarks>Run this method from the UI thread, as it needs to create
        /// a WriteableBitmap.
        /// If you need to access the encoded image data (e.g., if it contains JPEG or
        /// PNG data), use the Payload property directly.</remarks>
        /// <returns>A WriteableBitmap, created from the decoded payload.</returns>
        public async Task<WriteableBitmap> GetImageAsBitmap()
        {
            if (Payload == null) return null;

            // Get stream from the payload
            var payloadStream = ConvertArrayToStream(Payload);

            // Load the stream into the writable bitmap
            // As we don't know the dimensions of the image yet,
            // create the Writeable bitmap with a size of 1/1.
            // This will be set according to the source when loading it.
            // (as demonstrated in Microsoft examples)
            var bmp = new WriteableBitmap(1, 1);
            await bmp.SetSourceAsync(payloadStream);
            return bmp;
        }
        #endregion

        #region Utility methods
        /// <summary>
        /// Utility method to convert the MIME type enumeration used by this class to the
        /// platform's Bitmap Encoder GUID.
        /// </summary>
        /// <param name="mimeType">MIME type to convert.</param>
        /// <returns>GUID that can be used in the bitmap encoder to encode this MIME type.</returns>
        public static Guid GetBitmapEncoderIdForMimeType(ImageMimeType mimeType)
        {
            switch (mimeType)
            {
                case ImageMimeType.Png:
                    return BitmapEncoder.PngEncoderId;
                case ImageMimeType.Jpg:
                case ImageMimeType.Jpeg:
                    return BitmapEncoder.JpegEncoderId;
                case ImageMimeType.JpegXr:
                    return BitmapEncoder.JpegXREncoderId;
                case ImageMimeType.Bmp:
                    return BitmapEncoder.BmpEncoderId;
                case ImageMimeType.Gif:
                    return BitmapEncoder.GifEncoderId;
                case ImageMimeType.Tiff:
                    return BitmapEncoder.TiffEncoderId;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Utility method to convert the platform's Bitmap Decoder codec ID / GUID to the
        /// MIME type enumeration used by this class.
        /// </summary>
        /// <param name="codecId">Bitmap decoder codec ID / GUID.</param>
        /// <returns>MIME type enumeration that can be used by this class.</returns>
        public static ImageMimeType GetMimeTypeForBitmapDecoderId(Guid codecId)
        {
            if (codecId.Equals(BitmapDecoder.PngDecoderId))
            {
                return ImageMimeType.Png;
            }
            if (codecId.Equals(BitmapDecoder.JpegDecoderId))
            {
                return ImageMimeType.Jpeg;
            }
            if (codecId.Equals(BitmapDecoder.GifDecoderId))
            {
                return ImageMimeType.Gif;
            }
            if (codecId.Equals(BitmapDecoder.BmpDecoderId))
            {
                return ImageMimeType.Png;
            }
            if (codecId.Equals(BitmapDecoder.TiffDecoderId))
            {
                return ImageMimeType.Tiff;
            }
            if (codecId.Equals(BitmapDecoder.JpegXRDecoderId))
            {
                return ImageMimeType.JpegXr;
            }
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Utility method to access a byte array as a random access stream.
        /// </summary>
        /// <param name="arr">Byte array that you would like to access as a stream.</param>
        /// <returns>Random access stream based on the specified byte array.</returns>
        internal static IRandomAccessStream ConvertArrayToStream(byte[] arr)
        {
            var stream = new MemoryStream(arr);
            return stream.AsRandomAccessStream();
        }
        #endregion

    }
}