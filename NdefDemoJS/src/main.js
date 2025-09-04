import { NdefMessage, NdefRecord, NdefUriRecord, NdefTextRecord, NdefGeoRecord, NdefSocialRecord, NdefTelRecord, NdefAndroidAppRecord } from 'ndef-library';

const statusList = document.getElementById("statusList");
const urlInput = document.getElementById("urlInput");
const twitterInput = document.getElementById("twitterInput");
const subscribeBtn = document.getElementById("subscribeBtn");
const publishUriBtn = document.getElementById("publishUriBtn");
const publishTwitterBtn = document.getElementById("publishTwitterBtn");
const shareUriBtn = document.getElementById("shareUriBtn");

let ndef = null;
let isSubscribed = false;

function addStatusItem(title, text) {
    const li = document.createElement("li");
    li.innerHTML = `<b>${title}</b>: ${text}`;
    statusList.insertBefore(li, statusList.firstChild);
}

async function subscribe() {
    if (!isSubscribed) {
        try {
            if (!ndef) {
                ndef = new NDEFReader();
            }
            await ndef.scan();
            ndef.onreading = event => {
                const message = new NdefMessage(event.message.records.map(r => new NdefRecord(r.recordType, r.mediaType, r.data, r.id)));
                addStatusItem("NDEF message read", JSON.stringify(message.records));
            };
            addStatusItem("NDEF subscription", "started");
            isSubscribed = true;
            subscribeBtn.textContent = "Stop NDEF Subscription";
        } catch (error) {
            addStatusItem("Starting subscription failed", error);
        }
    } else {
        try {
            await ndef.stop();
            addStatusItem("NDEF subscription", "stopped");
            isSubscribed = false;
            subscribeBtn.textContent = "Subscribe for NDEF";
        } catch (error) {
            addStatusItem("Stopping subscription failed", error);
        }
    }
}

async function publish(message) {
    if (!ndef) {
        ndef = new NDEFReader();
    }
    try {
        await ndef.write(message);
        addStatusItem("Publish Success", "");
    } catch (error) {
        addStatusItem("Publish Error", error);
    }
}

function publishUri() {
    const uriRecord = new NdefUriRecord(urlInput.value);
    const message = new NdefMessage([uriRecord]);
    publish(message);
}

function publishTwitter() {
    const twitterRecord = new NdefSocialRecord(twitterInput.value, 'twitter');
    const message = new NdefMessage([twitterRecord]);
    publish(message);
}

async function shareUri() {
    const uriRecord = new NdefUriRecord(urlInput.value);
    const message = new NdefMessage([uriRecord]);
    try {
        await navigator.share({
            title: 'NDEF URI',
            text: 'NDEF URI',
            url: 'data:application/octet-stream;base64,' + btoa(String.fromCharCode.apply(null, message.toByteArray()))
        });
        addStatusItem("Share Success", "");
    } catch (error) {
        addStatusItem("Share Error", error);
    }
}


subscribeBtn.addEventListener("click", subscribe);
publishUriBtn.addEventListener("click", publishUri);
publishTwitterBtn.addEventListener("click", publishTwitter);
shareUriBtn.addEventListener("click", shareUri);

if ('NDEFReader' in window) {
    addStatusItem("Web NFC", "is supported");
} else {
    addStatusItem("Web NFC", "is not supported on this browser.");
}
