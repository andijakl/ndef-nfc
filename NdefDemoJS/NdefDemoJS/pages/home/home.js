/// <reference path="/scripts/ndeflibrary.js"/>
/// <reference path="~/scripts/ndeflibrary.js" />
(function () {
    "use strict";
    /** If the device is currently waiting for a tag to write an NDEF message. */
    var waitingPublishToTag = false;
    /** If the device is currently waiting for another device to share an NDEF message. */
    var waitingPublishToDevice = false;
    /** The NDEF message that should be published / shared to a tag / device. */
    var waitingPublishMsg = null;

    /** Whether a subscription to NDEF tags is currently active. */
    var subscribedToTags = false;

    // Array that contains the status messages
    var statusItems = [];
    // Create a list binding that can be used to bind the array to a WinJS UI element
    var statusListBinding = new WinJS.Binding.List(statusItems);
    // Define the binding in a namespace to make it accessible from the outside
    WinJS.Namespace.define("NdefDemo.StatusListView", {
        data: statusListBinding
    });

    /**
     * Add a status item to the beginning of the status list.
     * @param statusTitle first part / header of the status message, shown in bold font.
     * @param statusText second part / text of the status message, shown in normal font
     */
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

            var publishTwitterBtn = document.getElementById("publishTwitterBtn");
            publishTwitterBtn.addEventListener("click", this.publishTwitterBtnClick, false);

            var shareUriBtn = document.getElementById("shareUriBtn");
            shareUriBtn.addEventListener("click", this.shareUriBtnClick, false);

            // Bindings
            var statusListView = document.getElementById("statusListView");
            WinJS.Binding.processAll(statusListView, statusListBinding);


            // Check NFC
            if (window.nfc === undefined) {
                alert('NFC Plugin not found.');
            }
        },

        /**
         * Handler when the user clicked on the subscribed button.
         * Depending on the state of the UI, either starts or stops the subscription.
         */
        subscribeBtnClick: function () {
            if (!subscribedToTags) {
                startSubscribingToTags();
            } else {
                stopSubscribingToTags();
            }
        },

        /**
         * Handler for the button to publish the URI from the urlInput field to an NFC tag.
         */
        publishUriBtnClick: function () {
            ensureReadyToWrite();
            var urlInput = document.getElementById("urlInput");
            prepareUriMsg(urlInput.value, true);
        },

        /**
         * Handler for the button to share a Twitter URL message with the user name from the 
         * twitterInput field to another NFC tag.
         */
        publishTwitterBtnClick: function () {
            ensureReadyToWrite();
            var twitterInput = document.getElementById("twitterInput");
            prepareTwitterMsg(twitterInput.value, true);
        },

        /**
         * Handler for the button to share the URI from the urlInput field to another NFC device.
         */
        shareUriBtnClick: function () {
            if (!waitingPublishToDevice) {
                var urlInput = document.getElementById("urlInput");
                prepareUriMsg(urlInput.value, false);
            } else {
                stopSharingMsg();
            }
        },
    });

    /**
     * On Android, the Cordova NFC plug-in only writes messages to NFC tags when the
     * write function is called in the NFC callback handler.
     * Therefore, tag discovery needs to be activated on Android in order to write to tags.
     * This method ensures that tag discovery is activated on Android and should be called 
     * before publishing a message.
     */
    function ensureReadyToWrite() {
        if (!subscribedToTags &&
            window.cordova.platformId === "android") {
            addStatusItem("Starting to listen", "Android only writes to tags in tag discovered call-back.");
            startSubscribingToTags();
        }
    }

    /** Start subscribing to NFC tags that contain an NDEF message if no subscription is currently active. */
    function startSubscribingToTags() {
        if (!subscribedToTags) {
            // Read NFC tag
            window.nfc.addNdefListener(
                nfcHandler,
                function () {   // Success
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

    /** Stop a currently running subscription to read tags. */
    function stopSubscribingToTags() {
        if (subscribedToTags) {
            window.nfc.removeNdefListener(
                function () {
                    // Note: this unnecessary call-back will be removed in Cordova NFC plug-in 0.6.0
                    console.log("Stop subscribing callback");
                },
                function () {   // Success
                    addStatusItem("NDEF subscription", "stopped");
                    subscribedToTags = false;
                    updateButtonLabels();
                },
                function(error) { addStatusItem("Stopping subscription failed: ", JSON.stringify(error)); }
            );
        }
    }
    
    /** 
     * Call-back when the device found an NFC target that contains an NDEF message. 
     * @param the NFC event contents provided by the Cordova NFC plug-in. JSON formatted information
     * about the tag and message contents.
     */
    function nfcHandler(nfcEvent) {
        // Get NFC event and output full event info to debug console
        var tag = nfcEvent.tag;
        console.log(JSON.stringify(tag));

        // Get the bytes of the NDEF message and create a string dump of the payload
        var ndefMessageBytes = tag.ndefMessage;
        var payload = nfc.bytesToString(ndefMessageBytes[0].payload);

        // Output parsed tag contents
        addStatusItem("NFC tag contents", payload);

        if (window.cordova.platformId === "android" && waitingPublishToTag) {
            // Android: Only publish to tags when user is currently tapping the tags.
            startPublishingMsg();
        }
    };

    /**
     * Prepare a URI message with the specified URL and publish it to a tag or device.
     * @param url the URL that should be used for the NDEF message.
     * @param publishToTag true to publish to an NFC tag, false to share to another NFC device.
     */
    function prepareUriMsg(url, publishToTag) {
        // Create new URL record
        var ndefRecord = new NdefLibrary.NdefUriRecord();
        ndefRecord.setUri(url);
        // Create and publish NDEF message
        prepareMsg(ndefRecord, publishToTag, "URL: " + url);
    };

    /**
     * Prepare a Social Network message for Twitter with the specified Twitter user name and 
     * publish it to a tag or device.
     * @param twitterUser user name for the Twitter social network. Only provide the username, not
     * the complete Twitter URL.
     * @param publishToTag true to publish to an NFC tag, false to share to another NFC device.
     */
    function prepareTwitterMsg(twitterUser, publishToTag) {
        // Create new social / Twitter record
        var ndefRecord = new window.NdefLibrary.NdefSocialRecord();
        ndefRecord.setSocialType(window.NdefLibrary.NdefSocialRecord.NfcSocialType.Twitter);
        ndefRecord.setSocialUserName(twitterUser);
        // Create and publish NDEF message
        prepareMsg(ndefRecord, publishToTag, "Twitter: " + twitterUser);
    }

    /**
     * Create & publish an NDEF message based on the provided NDEF record.
     * @param ndefRecord NDEF record from the NDEF Library that should be published
     * @param publishToTag true to publish to an NFC tag, false to share to another NFC device.
     * @param statusMsg Status message that will be shown to the user, should contain information
     * about the contents of the message.
     */
    function prepareMsg(ndefRecord, publishToTag, statusMsg) {
        // Ensure that we only publish one message
        if (waitingPublishToDevice || waitingPublishToTag) {
            addStatusItem("Already publishing", "Stop publishing old msg first");
            return;
        }
        // Create NDEF message based on the NDEF record
        var ndefMessage = new window.NdefLibrary.NdefMessage(ndefRecord);
        // Convert message to raw NDEF byte array
        var ndefMsgBytes = ndefMessage.toByteArray();
        // Parse raw byte array to Cordova NFC Plugin format (JSON)
        waitingPublishMsg = window.ndef.decodeMessage(ndefMsgBytes);

        // Save status of what we are about to do
        if (publishToTag) {
            waitingPublishToTag = true;
            addStatusItem("Publishing to tag", statusMsg);
        }
        else {
            waitingPublishToDevice = true;
            addStatusItem("Sharing to device", statusMsg);
        }

        if (!(window.cordova.platformId === "android" && publishToTag)) {
            // Write immediately except if we want to write a tag on Android
            // -> This is done in the NDEF tag found callback handler.
            startPublishingMsg();
        }

        // Make sure the buttons are updated correctly to start / stop processes
        updateButtonLabels();
    }

    /**
     * If the app has a cached NDEF message, publish it to a tag or share it to another
     * device.
     */
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

    /**
     * Stop sharing an NDEF message to another device if this is currently ongoing.
     * (Note that the NFC plugin for Cordova currently doesn't provide means to stop publishing to NFC tags.)
     */
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

    /**
     * Ensure that the buttons in the UI reflect the current application state.
     */
    function updateButtonLabels() {
        var shareUriBtn = document.getElementById("shareUriBtn");
        shareUriBtn.innerText = (waitingPublishToDevice ? "Stop Sharing" : "Share URI to Device");
        var subscribeBtn = document.getElementById("subscribeBtn");
        subscribeBtn.innerText = (subscribedToTags ? "Stop NDEF Subscription" : "Subscribe for NDEF");
    }

})(window);
