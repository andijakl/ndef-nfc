/****************************************************************************
**
** Copyright (C) 2026 Andreas Jakl - https://www.nfcinteractor.com/
** All rights reserved.
**
** Conversion between the NDEF library classes and the Web NFC API
** (https://w3c.github.io/web-nfc/).
**
** More information: https://andijakl.github.io/ndef-nfc/
**
** GNU Lesser General Public License Usage
** This file may be used under the terms of the GNU Lesser General Public
** License version 3 or later as published by the Free Software Foundation.
**
****************************************************************************/

import { NdefRecord, TypeNameFormatType } from './NdefRecord.js';
import { NdefMessage } from './NdefMessage.js';
import { NdefUriRecord } from './NdefUriRecord.js';
import { NdefTextRecord } from './NdefTextRecord.js';

const textEncoder = new TextEncoder();
const textDecoder = new TextDecoder();

const SMART_POSTER_TYPE = 'Sp';

/**
 * Converts an NDEF message of this library to the `NDEFMessageInit` structure
 * expected by `NDEFReader.write()` of the Web NFC API.
 *
 * Web NFC does not allow writing raw NDEF bytes. Records are therefore mapped to the
 * Web NFC record types (`url`, `text`, `smart-poster`, `mime`, `absolute-url`,
 * `unknown`, `empty` and external types). Well-known types other than URI, Text and
 * Smart Poster are only supported inside a Smart Poster (as local types, e.g. `:act`).
 *
 * @example
 * const message = new NdefMessage([new NdefUriRecord("https://www.nfcinteractor.com")]);
 * await new NDEFReader().write(toWebNfcMessage(message));
 *
 * @param {NdefMessage | NdefRecord[]} message Message (or array of records) to convert.
 * @returns {{ records: object[] }} `NDEFMessageInit` for the Web NFC API.
 */
export function toWebNfcMessage(message) {
    const records = message instanceof NdefMessage ? message.records : message;
    return { records: records.map(record => toWebNfcRecord(record, false)) };
}

/**
 * Converts an `NDEFMessage` received from the Web NFC API (e.g., `event.message`
 * in the `reading` event of `NDEFReader`) to an NDEF message of this library.
 *
 * URI and Text records are returned as `NdefUriRecord` / `NdefTextRecord` instances;
 * all other records as generic `NdefRecord` instances with type name format, type
 * and payload as stored on the tag.
 *
 * @example
 * reader.onreading = event => {
 *     const message = fromWebNfcMessage(event.message);
 * };
 *
 * @param {{ records: ReadonlyArray<object> }} webNfcMessage `NDEFMessage` from the Web NFC API.
 * @returns {NdefMessage}
 */
export function fromWebNfcMessage(webNfcMessage) {
    return new NdefMessage(webNfcMessage.records.map(record => fromWebNfcRecord(record)));
}

/**
 * @param {NdefRecord} record
 * @param {boolean} nested True if the record is part of a Smart Poster payload.
 */
function toWebNfcRecord(record, nested) {
    const tnf = record.getTypeNameFormat();
    const type = bytesToString(record.getType());
    const payload = toBytes(record.getPayload());
    const id = record.getId() && record.getId().length > 0 ? bytesToString(record.getId()) : undefined;

    let init;
    switch (tnf) {
        case TypeNameFormatType.Empty:
            init = { recordType: 'empty' };
            break;
        case TypeNameFormatType.NfcRtd:
            init = wellKnownToWebNfc(record, type, payload, nested);
            break;
        case TypeNameFormatType.Mime:
            init = { recordType: 'mime', mediaType: type, data: payload };
            break;
        case TypeNameFormatType.Uri:
            init = { recordType: 'absolute-url', data: type };
            break;
        case TypeNameFormatType.ExternalRtd:
            init = { recordType: type, data: payload };
            break;
        case TypeNameFormatType.Unknown:
            init = { recordType: 'unknown', data: payload };
            break;
        default:
            throw new Error(`Records with type name format ${tnf} can't be written with Web NFC`);
    }
    if (id !== undefined) {
        init.id = id;
    }
    return init;
}

function wellKnownToWebNfc(record, type, payload, nested) {
    if (type === 'U') {
        const uriRecord = new NdefUriRecord();
        uriRecord.setPayload(payload);
        return { recordType: 'url', data: uriRecord.getUri() };
    }
    if (type === 'T') {
        const status = payload[0] ?? 0;
        const langLength = status & 0x3f;
        return {
            recordType: 'text',
            encoding: (status & 0x80) !== 0 ? 'utf-16be' : 'utf-8',
            lang: textDecoder.decode(payload.subarray(1, 1 + langLength)),
            data: payload.subarray(1 + langLength),
        };
    }
    if (type === SMART_POSTER_TYPE) {
        const nestedMessage = NdefMessage.fromByteArray(payload);
        return {
            recordType: 'smart-poster',
            data: { records: nestedMessage.records.map(nestedRecord => toWebNfcRecord(nestedRecord, true)) },
        };
    }
    if (nested) {
        // Local types (e.g., the action, size and type records of a Smart Poster)
        return { recordType: ':' + type, data: payload };
    }
    throw new Error(`Well-known record type "${type}" can't be written with Web NFC`);
}

/**
 * @param {object} webNfcRecord `NDEFRecord` from the Web NFC API.
 * @returns {NdefRecord}
 */
function fromWebNfcRecord(webNfcRecord) {
    const recordType = webNfcRecord.recordType;
    const data = webNfcRecord.data ? dataViewToBytes(webNfcRecord.data) : new Uint8Array(0);

    let record;
    switch (recordType) {
        case 'empty':
            record = new NdefRecord(TypeNameFormatType.Empty);
            break;
        case 'url':
            record = new NdefUriRecord(textDecoder.decode(data));
            break;
        case 'text':
            record = textFromWebNfc(webNfcRecord, data);
            break;
        case 'smart-poster':
            record = new NdefRecord(TypeNameFormatType.NfcRtd, textEncoder.encode(SMART_POSTER_TYPE), data);
            break;
        case 'absolute-url':
            record = new NdefRecord(TypeNameFormatType.Uri, data);
            break;
        case 'mime':
            record = new NdefRecord(TypeNameFormatType.Mime, textEncoder.encode(webNfcRecord.mediaType ?? ''), data);
            break;
        case 'unknown':
            record = new NdefRecord(TypeNameFormatType.Unknown, null, data);
            break;
        default:
            if (recordType.startsWith(':')) {
                // Local type: well-known type within the context of a parent record
                record = new NdefRecord(TypeNameFormatType.NfcRtd, textEncoder.encode(recordType.substring(1)), data);
            } else {
                // External type, e.g. "android.com:pkg"
                record = new NdefRecord(TypeNameFormatType.ExternalRtd, textEncoder.encode(recordType), data);
            }
            break;
    }
    if (webNfcRecord.id) {
        record.setId(textEncoder.encode(webNfcRecord.id));
    }
    return record;
}

function textFromWebNfc(webNfcRecord, data) {
    const lang = textEncoder.encode(webNfcRecord.lang ?? 'en');
    const isUtf16 = (webNfcRecord.encoding ?? 'utf-8').startsWith('utf-16');
    // Re-assemble the payload of the NFC Forum Text RTD: status byte, language code, text
    const payload = new Uint8Array(1 + lang.length + data.length);
    payload[0] = (isUtf16 ? 0x80 : 0) | (lang.length & 0x3f);
    payload.set(lang, 1);
    payload.set(data, 1 + lang.length);
    const record = new NdefTextRecord();
    record.setPayload(payload);
    return record;
}

function dataViewToBytes(dataView) {
    // Copy, as the underlying buffer may be reused by the browser
    return new Uint8Array(dataView.buffer, dataView.byteOffset, dataView.byteLength).slice();
}

function toBytes(value) {
    if (!value) {
        return new Uint8Array(0);
    }
    return value instanceof Uint8Array ? value : Uint8Array.from(value);
}

function bytesToString(value) {
    return value ? textDecoder.decode(toBytes(value)) : '';
}
