/// <reference path="/scripts/ndeflibrary.js"/>
/// <reference path="~/scripts/ndeflibrary.js" />
(function () {
    "use strict";
    var waitingPublishToTag = false;
    var waitingPublishToDevice = false;
    var waitingPublishMsg = null;

    var subscribedToTags = false;

    // Array that contains the status messages
    var statusItems = [];
    // Create a list binding that can be used to bind the array to a WinJS UI element
    var statusListBinding = new WinJS.Binding.List(statusItems);
    // Define the binding in a namespace to make it accessible from the outside
    WinJS.Namespace.define("NdefDemo.StatusListView", {
        data: statusListBinding
    });

    // Add a status item to the beginning of the status list
    function addStatusItem(statusTitle, statusText) {
        statusListBinding.unshift({ title: statusTitle, text: statusText });
    }


    WinJS.UI.Pages.define("./pages/home/home.html", {
        // This function is called whenever a user navigates to this page. It
        // populates the page elements with the app's data.
        ready: function (element, options) {
            //alert("Running on platform: " + window.cordova.platformId);
            // Find all <a> links and execute the event handler when clicking
            WinJS.Utilities.query("a").listen("click",
                this.linkClickEventHandler, false);

            // Button event handlers
            var subscribeBtn = document.getElementById("subscribeBtn");
            subscribeBtn.addEventListener("click", this.subscribeBtnClick, false);

            var publishUriBtn = document.getElementById("publishUriBtn");
            publishUriBtn.addEventListener("click", this.publishUriBtnClick, false);

            var shareUriBtn = document.getElementById("shareUriBtn");
            shareUriBtn.addEventListener("click", this.shareUriBtnClick, false);

            var stopSharingBtn = document.getElementById("stopSharingBtn");
            stopSharingBtn.addEventListener("click", this.stopSharingBtnClick, false);

            // Bindings
            var statusListView = document.getElementById("statusListView");
            WinJS.Binding.processAll(statusListView, statusListBinding);


            // Check NFC
            if (window.nfc === undefined) {
                alert('NFC Plugin not found.');
            }
        },
        linkClickEventHandler: function (eventInfo) {
            // Stop default behaviour!
            eventInfo.preventDefault();
            // Use WinJS to navigate instead - loads the new page into the <div> fragment
            var link = eventInfo.target;
            WinJS.Navigation.navigate(link.href);
        },

        subscribeBtnClick: function () {
            if (!subscribedToTags) {
                startSubscribingToTags();
            } else {
                stopSubscribingToTags();
            }
        },

        publishUriBtnClick: function () {
            ensureReadyToWrite();
            prepareUriMsg(urlInput.value, true);
        },
        
        shareUriBtnClick: function () {
            if (!waitingPublishToDevice) {
                prepareUriMsg(urlInput.value, false);
            } else {
                stopSharingMsg();
            }
        },
    });

    function ensureReadyToWrite() {
        if (!subscribedToTags &&
            window.cordova.platformId === "android") {
            addStatusItem("Starting to listen", "Android only writes to tags in tag discovered call-back.");
            startSubscribingToTags();
        }
    }

    function startSubscribingToTags() {
        if (!subscribedToTags) {
            // Read NFC tag
            window.nfc.addNdefListener(
                nfcHandler,
                function () {
                    addStatusItem("NDEF subscription", "started");
                    subscribedToTags = true;
                    updateButtonLabels();
                },
                function (error) { addStatusItem("Starting subscription failed: ", JSON.stringify(error)); }
            );
        } else {
            addStatusItem("Already subscribed", "");
        }
    }

    function stopSubscribingToTags() {
        if (subscribedToTags) {
            window.nfc.removeNdefListener(
                function () {
                    // Note: this unnecessary call-back will be removed in NFC plug-in 0.6.0
                    console.log("Stop subscribing callback");
                },
                function () {
                    addStatusItem("NDEF subscription", "stopped");
                    subscribedToTags = false;
                    updateButtonLabels();
                },
                function(error) { addStatusItem("Stopping subscription failed: ", JSON.stringify(error)); }
            );
        }
    }
    
    function nfcHandler (nfcEvent) {
        var tag = nfcEvent.tag;
        console.log(JSON.stringify(tag));
        var ndefMessageBytes = tag.ndefMessage;
        var payload = nfc.bytesToString(ndefMessageBytes[0].payload);
        addStatusItem("NFC tag contents", payload);


        if (window.cordova.platformId === "android" &&
            waitingPublishToTag || waitingPublishToDevice) {
            startPublishingMsg();
        }
    };

    function prepareUriMsg(url, publishToTag) {
        if (waitingPublishToDevice || waitingPublishToTag) {
            addStatusItem("Already publishing", "Stop publishing old msg first");
            return;
        }

        // Prepare and save message
        var ndefRecord = new NdefLibrary.NdefUriRecord();
        ndefRecord.setUri(url);
        var ndefMessage = new window.NdefLibrary.NdefMessage(ndefRecord);
        // Convert message to raw NDEF byte array
        var ndefMsgBytes = ndefMessage.toByteArray();
        // Parse raw byte array to Cordova NFC Plugin format (JSON)
        waitingPublishMsg = window.ndef.decodeMessage(ndefMsgBytes);

        // Save status
        if (publishToTag) {
            waitingPublishToTag = true;
            addStatusItem("Publishing to tag", "URL: " + url);
        }
        else {
            waitingPublishToDevice = true;
            addStatusItem("Sharing to device", "URL: " + url);
        }

        if (!(window.cordova.platformId === "android" && publishToTag)) {
            // Write immediately except if we want to write a tag on Android
            // -> This is done in the NDEF tag found callback handler.
            startPublishingMsg();
        }
        updateButtonLabels();
    };

    function startPublishingMsg() {
        // Write expects a JSON array of NdefRecords
        if (waitingPublishMsg != null) {
            if (waitingPublishToTag) {
                window.nfc.write(
                    //records,
                    //ndefMessagePlugin,
                    waitingPublishMsg,
                    function (msg) {
                        addStatusItem("Publish Success", msg);
                        waitingPublishToTag = false;
                        waitingPublishMsg = null;
                        updateButtonLabels();
                    },
                    function (msg) {
                        addStatusItem("Publish Error", msg);
                        waitingPublishToTag = false;
                        waitingPublishMsg = null;
                        updateButtonLabels();
                    }
                );
            } else if (waitingPublishToDevice) {
                window.nfc.share(
                    waitingPublishMsg,
                    function (msg) {
                        addStatusItem("Share Success", msg);
                    },
                    function (msg) {
                        addStatusItem("Share Error", msg);
                    }
                    );
            }
        }
    };

    function stopSharingMsg() {
        if (waitingPublishToDevice) {
            window.nfc.unshare(
                function () {
                    addStatusItem("Sharing stopped", "");
                    waitingPublishToDevice = false;
                    updateButtonLabels();
                },
                function (error) { addStatusItem("Stopping Sharing failed: ", JSON.stringify(error)); });
        }
    }

    function updateButtonLabels() {
        var shareUriBtn = document.getElementById("shareUriBtn");
        shareUriBtn.innerText = (waitingPublishToDevice ? "Stop Sharing" : "Share URI to Device");
        var subscribeBtn = document.getElementById("subscribeBtn");
        subscribeBtn.innerText = (subscribedToTags ? "Stop NDEF Subscription" : "Subscribe for NDEF");
    }

})(window);


////  Write NFC tag
//var records = [
//    //ndef.textRecord("Hi there at " + new Date()),
//    ndef.uriRecord("http://www.mopius.com/apps")
//    //ndef.uriRecord("http://www.nfcinteractor.com/")
//    //ndef.mimeMediaRecord("text/blah", nfc.stringToBytes("Blah!"))
//];

//// Create NDEF message with the NDEF library
////var ndefRecord = new NdefLibrary.NdefUriRecord();
////ndefRecord.setUri("http://www.nfcinteractor.com/");

//var ndefRecord = new window.NdefLibrary.NdefSocialRecord();
//ndefRecord.setSocialType(window.NdefLibrary.NdefSocialRecord.NfcSocialType.Twitter);
//ndefRecord.setSocialUserName("mopius");

//var ndefMessage = new window.NdefLibrary.NdefMessage(ndefRecord);

//// Convert message to raw NDEF byte array
//var ndefMsgBytes = ndefMessage.toByteArray();
//// Parse raw byte array to Cordova NFC Plugin format (JSON)
//var ndefMessagePlugin = window.ndef.decodeMessage(ndefMsgBytes);

//// On Android this method must be called from within an NDEF Event Handler.
//// On Windows Phone this method should be called outside the NDEF Event Handler, otherwise Windows tries to read the tag contents as you are writing to the tag.


//var ndefMessage = NdefLibrary.NdefMessage.fromByteArray(ndefMessageBytes);