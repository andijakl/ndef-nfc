import { NdefUriRecord } from './NdefUriRecord.js';

export const NfcSocialType = {
    X: 0,           // Formerly Twitter
    LinkedIn: 1,
    Facebook: 2,
    Instagram: 3,
    Threads: 4,
    TikTok: 5
};

const SOCIAL_TAG_TYPE_URIS = [
    "https://x.com/{0}",                    // X (formerly Twitter)
    "https://linkedin.com/in/{0}",         // LinkedIn
    "https://facebook.com/{0}",            // Facebook
    "https://instagram.com/{0}",           // Instagram
    "https://threads.net/@{0}",            // Threads
    "https://tiktok.com/@{0}"              // TikTok
];

export class NdefSocialRecord extends NdefUriRecord {
    constructor(socialUserName, socialType = NfcSocialType.X) {
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
