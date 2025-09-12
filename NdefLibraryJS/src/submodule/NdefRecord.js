export const TypeNameFormatType = {
    Empty: 0x00,
    NfcRtd: 0x01,
    Mime: 0x02,
    Uri: 0x03,
    ExternalRtd: 0x04,
    Unknown: 0x05,
    Unchanged: 0x06,
    Reserved: 0x07,
};

export class NdefRecord {
    _typeNameFormat = TypeNameFormatType.Empty;
    _type = [];
    _id = [];
    _payload = [];

    constructor(tnf, type, payload, id) {
        if (tnf instanceof NdefRecord) {
            const other = tnf;
            this._typeNameFormat = other.getTypeNameFormat();
            this.setType(other.getType());
            this.setId(other.getId());
            this.setPayload(other.getPayload());
        } else {
            this._typeNameFormat = tnf || TypeNameFormatType.Empty;
            if (type) {
                this.setType(type);
            }
            if (payload) {
                this.setPayload(payload);
            }
            if (id) {
                this.setId(id);
            }
        }
    }

    getId() {
        return this._id;
    }

    setId(value) {
        if (value == null) {
            this._id = null;
            return;
        }
        this._id = value.slice();
    }

    getType() {
        return this._type;
    }

    setType(value) {
        if (value == null) {
            this._type = null;
            return;
        }
        this._type = value.slice();
    }

    getPayload() {
        return this._payload;
    }

    setPayload(value) {
        if (value == null) {
            this._payload = null;
            return;
        }
        this._payload = value.slice();
    }

    getTypeNameFormat() {
        return this._typeNameFormat;
    }

    setTypeNameFormat(value) {
        this._typeNameFormat = value;
    }

    checkSpecializedType(checkForSubtypes) {
        // TODO
    }

    checkIfValid() {
        if (
            this._typeNameFormat === TypeNameFormatType.Unchanged ||
            this._typeNameFormat === TypeNameFormatType.Unknown
        ) {
            if (this._type && this._type.length > 0) {
                throw new Error("Unchanged and Unknown TNF must have a type length of 0");
            }
        } else {
            if (this._typeNameFormat !== TypeNameFormatType.Empty && (!this._type || this._type.length === 0)) {
                throw new Error("All other TNF (except Empty) should have a type set");
            }
        }

        if (this._typeNameFormat === TypeNameFormatType.Unchanged && this._id && this._id.length > 0) {
            throw new Error("Unchanged TNF must not have an ID field");
        }
        return true;
    }
}
