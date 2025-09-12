import { NdefUriRecord } from './NdefUriRecord.js';

export const NfcGeoType = {
    GeoUri: 0,
    BingMaps: 1,
    NokiaMapsUri: 2,
    WebRedirect: 3,
    MsDriveTo: 4,
    MsWalkTo: 5
};

const GEO_TAG_TYPE_URIS = [
    "geo:{0},{1}",
    "bingmaps:?cp={0}~{1}",
    "http://m.ovi.me/?c={0},{1}",
    "http://NfcInteractor.com/m?c={0},{1}",
    "ms-drive-to:?destination.latitude={0}&destination.longitude={1}",
    "ms-walk-to:?destination.latitude={0}&destination.longitude={1}"
];

export class NdefGeoRecord extends NdefUriRecord {
    constructor(latitude, longitude, geoType = NfcGeoType.GeoUri) {
        super();
        this.latitude = latitude;
        this.longitude = longitude;
        this.geoType = geoType;
        this.updatePayload();
    }

    updatePayload() {
        if (!this.latitude || !this.longitude) {
            return;
        }
        const base = GEO_TAG_TYPE_URIS[this.geoType];
        const uri = base.replace('{0}', this.latitude).replace('{1}', this.longitude);
        this.setUri(uri);
    }

    getLatitude() {
        return this.latitude;
    }

    setLatitude(value) {
        this.latitude = value;
        this.updatePayload();
    }

    getLongitude() {
        return this.longitude;
    }

    setLongitude(value) {
        this.longitude = value;
        this.updatePayload();
    }

    getGeoType() {
        return this.geoType;
    }

    setGeoType(value) {
        this.geoType = value;
        this.updatePayload();
    }
}
