import { NdefUriRecord } from './NdefUriRecord.js';

export const NfcSocialType = {
    Twitter: 0,
    LinkedIn: 1,
    Facebook: 2,
    Xing: 3,
    VKontakte: 4,
    FoursquareWeb: 5,
    FoursquareApp: 6,
    Skype: 7,
    GooglePlus: 8
};

const SOCIAL_TAG_TYPE_URIS = [
    "http://twitter.com/{0}",
    "http://linkedin.com/in/{0}",
    "http://facebook.com/{0}",
    "http://xing.com/profile/{0}",
    "http://vkontakte.ru/{0}",
    "http://m.foursquare.com/v/{0}",
    "foursquare://venues/{0}",
    "skype:{0}?call",
    "https://plus.google.com/{0}"
];

export class NdefSocialRecord extends NdefUriRecord {
    constructor(socialUserName, socialType = NfcSocialType.Twitter) {
        super();
        this.socialUserName = socialUserName;
        this.socialType = socialType;
        this.updatePayload();
    }

    updatePayload() {
        const base = SOCIAL_TAG_TYPE_URIS[this.socialType];
        const uri = base.replace('{0}', this.socialUserName);
        this.setUri(uri);
    }

    checkIfValid() {
        super.checkIfValid();
        if (!this.socialUserName) {
            throw new Error("Social user name is empty");
        }
        return true;
    }

    getSocialUserName() {
        return this.socialUserName;
    }

    setSocialUserName(value) {
        this.socialUserName = value;
        this.updatePayload();
    }

    getSocialType() {
        return this.socialType;
    }

    setSocialType(value) {
        this.socialType = value;
        this.updatePayload();
    }
}
