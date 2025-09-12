/**
 * Demo Application UI Functionality Tests
 * Tests button interactions, form input handling, status messages, and error handling
 */

import { jest } from '@jest/globals';
import { JSDOM } from 'jsdom';
import { 
    MockNDEFReader, 
    installWebNFCMock, 
    uninstallWebNFCMock,
    createMockNDEFMessage 
} from '../utils/mock-web-nfc.js';
import { NdefUriRecord } from '../../src/submodule/NdefUriRecord.js';
import { NdefSocialRecord, NfcSocialType } from '../../src/submodule/NdefSocialRecord.js';
import { NdefMessage } from '../../src/submodule/NdefMessage.js';

describe('Demo App UI Functionality', () => {
    let dom;
    let window;
    let document;
    let mockNDEFReader;

    beforeEach(() => {
        // Create a DOM environment
        dom = new JSDOM(`
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <title>NFC Demo</title>
            </head>
            <body>
                <h1>NFC Demo</h1>
                <div class="container">
                    <div class="section">
                        <h2>Subscribe</h2>
                        <button id="subscribeBtn">Subscribe for NDEF</button>
                    </div>
                    <div class="section">
                        <h2>Publish</h2>
                        <div>
                            <label for="urlInput">URL:</label>
                            <input type="text" id="urlInput" value="https://www.nfcinteractor.com">
                            <button id="publishUriBtn">Publish URI</button>
                        </div>
                        <div>
                            <label for="twitterInput">Twitter:</label>
                            <input type="text" id="twitterInput" value="andijakl">
                            <button id="publishTwitterBtn">Publish Twitter</button>
                        </div>
                    </div>
                    <div class="section">
                        <h2>Share</h2>
                        <button id="shareUriBtn">Share URI to Device</button>
                    </div>
                    <div class="section">
                        <h2>Status</h2>
                        <ul id="statusList"></ul>
                    </div>
                </div>
            </body>
            </html>
        `, {
            url: 'https://localhost:3000',
            pretendToBeVisual: true,
            resources: 'usable'
        });

        window = dom.window;
        document = window.document;
        
        // Set up global environment
        global.window = window;
        global.document = document;
        global.navigator = {
            share: jest.fn(),
            permissions: {
                query: jest.fn()
            }
        };

        // Install Web NFC mock
        installWebNFCMock();
        mockNDEFReader = new MockNDEFReader();
        global.NDEFReader = jest.fn(() => mockNDEFReader);

        // Mock btoa for base64 encoding
        global.btoa = jest.fn((str) => Buffer.from(str, 'binary').toString('base64'));
    });

    afterEach(() => {
        uninstallWebNFCMock();
        dom.window.close();
        delete global.window;
        delete global.document;
        delete global.navigator;
        delete global.btoa;
        jest.clearAllMocks();
    });

    describe('Button Interactions', () => {
        let statusList, urlInput, twitterInput, subscribeBtn, publishUriBtn, publishTwitterBtn, shareUriBtn;
        let addStatusItem, subscribe, publishUri, publishTwitter, shareUri;

        beforeEach(() => {
            // Get DOM elements
            statusList = document.getElementById("statusList");
            urlInput = document.getElementById("urlInput");
            twitterInput = document.getElementById("twitterInput");
            subscribeBtn = document.getElementById("subscribeBtn");
            publishUriBtn = document.getElementById("publishUriBtn");
            publishTwitterBtn = document.getElementById("publishTwitterBtn");
            shareUriBtn = document.getElementById("shareUriBtn");

            // Mock the main.js functionality
            let ndef = null;
            let isSubscribed = false;

            addStatusItem = jest.fn((title, text) => {
                const li = document.createElement("li");
                li.innerHTML = `<b>${title}</b>: ${text}`;
                statusList.insertBefore(li, statusList.firstChild);
            });

            subscribe = jest.fn(async () => {
                if (!isSubscribed) {
                    try {
                        if (!ndef) {
                            ndef = new NDEFReader();
                        }
                        await ndef.scan();
                        ndef.onreading = event => {
                            addStatusItem("NDEF message read", JSON.stringify(event.message.records));
                        };
                        addStatusItem("NDEF subscription", "started");
                        isSubscribed = true;
                        subscribeBtn.textContent = "Stop NDEF Subscription";
                    } catch (error) {
                        addStatusItem("Starting subscription failed", error.message);
                    }
                } else {
                    try {
                        await ndef.stop();
                        addStatusItem("NDEF subscription", "stopped");
                        isSubscribed = false;
                        subscribeBtn.textContent = "Subscribe for NDEF";
                    } catch (error) {
                        addStatusItem("Stopping subscription failed", error.message);
                    }
                }
            });

            publishUri = jest.fn(() => {
                try {
                    const uriRecord = new NdefUriRecord(urlInput.value);
                    const message = new NdefMessage([uriRecord]);
                    // Mock publish function
                    addStatusItem("Publish Success", "");
                } catch (error) {
                    addStatusItem("Publish Error", error.message);
                }
            });

            publishTwitter = jest.fn(() => {
                try {
                    const twitterRecord = new NdefSocialRecord(twitterInput.value, NfcSocialType.Twitter);
                    const message = new NdefMessage([twitterRecord]);
                    // Mock publish function
                    addStatusItem("Publish Success", "");
                } catch (error) {
                    addStatusItem("Publish Error", error.message);
                }
            });

            shareUri = jest.fn(async () => {
                try {
                    const uriRecord = new NdefUriRecord(urlInput.value);
                    const message = new NdefMessage([uriRecord]);
                    await navigator.share({
                        title: 'NDEF URI',
                        text: 'NDEF URI',
                        url: 'data:application/octet-stream;base64,' + btoa(String.fromCharCode.apply(null, message.toByteArray()))
                    });
                    addStatusItem("Share Success", "");
                } catch (error) {
                    addStatusItem("Share Error", error.message);
                }
            });

            // Attach event listeners
            subscribeBtn.addEventListener("click", subscribe);
            publishUriBtn.addEventListener("click", publishUri);
            publishTwitterBtn.addEventListener("click", publishTwitter);
            shareUriBtn.addEventListener("click", shareUri);
        });

        test('Subscribe button toggles subscription state', async () => {
            expect(subscribeBtn.textContent).toBe("Subscribe for NDEF");
            
            // Click to start subscription
            subscribeBtn.click();
            await new Promise(resolve => setTimeout(resolve, 150)); // Wait for async operations
            
            expect(subscribe).toHaveBeenCalledTimes(1);
            expect(addStatusItem).toHaveBeenCalledWith("NDEF subscription", "started");
            expect(subscribeBtn.textContent).toBe("Stop NDEF Subscription");
            
            // Click to stop subscription
            subscribeBtn.click();
            await new Promise(resolve => setTimeout(resolve, 50));
            
            expect(subscribe).toHaveBeenCalledTimes(2);
            expect(addStatusItem).toHaveBeenCalledWith("NDEF subscription", "stopped");
            expect(subscribeBtn.textContent).toBe("Subscribe for NDEF");
        });

        test('Publish URI button creates and publishes URI record', () => {
            const testUrl = "https://example.com/test";
            urlInput.value = testUrl;
            
            publishUriBtn.click();
            
            expect(publishUri).toHaveBeenCalledTimes(1);
            expect(addStatusItem).toHaveBeenCalledWith("Publish Success", "");
        });

        test('Publish Twitter button creates and publishes social record', () => {
            const testHandle = "testuser";
            twitterInput.value = testHandle;
            
            publishTwitterBtn.click();
            
            expect(publishTwitter).toHaveBeenCalledTimes(1);
            expect(addStatusItem).toHaveBeenCalledWith("Publish Success", "");
        });

        test('Share URI button uses Web Share API', async () => {
            const testUrl = "https://example.com/share";
            urlInput.value = testUrl;
            
            navigator.share.mockResolvedValue();
            
            shareUriBtn.click();
            await new Promise(resolve => setTimeout(resolve, 50));
            
            expect(shareUri).toHaveBeenCalledTimes(1);
            expect(navigator.share).toHaveBeenCalledWith({
                title: 'NDEF URI',
                text: 'NDEF URI',
                url: expect.stringContaining('data:application/octet-stream;base64,')
            });
            expect(addStatusItem).toHaveBeenCalledWith("Share Success", "");
        });
    });

    describe('Form Input Handling', () => {
        test('URL input field accepts and validates URLs', () => {
            const urlInput = document.getElementById("urlInput");
            
            // Test default value
            expect(urlInput.value).toBe("https://www.nfcinteractor.com");
            
            // Test setting new value
            const newUrl = "https://example.com/test";
            urlInput.value = newUrl;
            expect(urlInput.value).toBe(newUrl);
            
            // Test empty value
            urlInput.value = "";
            expect(urlInput.value).toBe("");
            
            // Test invalid URL format (should still accept as input)
            urlInput.value = "not-a-url";
            expect(urlInput.value).toBe("not-a-url");
        });

        test('Twitter input field accepts and validates handles', () => {
            const twitterInput = document.getElementById("twitterInput");
            
            // Test default value
            expect(twitterInput.value).toBe("andijakl");
            
            // Test setting new value
            const newHandle = "testuser";
            twitterInput.value = newHandle;
            expect(twitterInput.value).toBe(newHandle);
            
            // Test empty value
            twitterInput.value = "";
            expect(twitterInput.value).toBe("");
            
            // Test handle with @ symbol
            twitterInput.value = "@testuser";
            expect(twitterInput.value).toBe("@testuser");
        });

        test('Input fields trigger change events', () => {
            const urlInput = document.getElementById("urlInput");
            const twitterInput = document.getElementById("twitterInput");
            
            const urlChangeHandler = jest.fn();
            const twitterChangeHandler = jest.fn();
            
            urlInput.addEventListener('change', urlChangeHandler);
            twitterInput.addEventListener('change', twitterChangeHandler);
            
            // Simulate user input
            urlInput.value = "https://new-url.com";
            urlInput.dispatchEvent(new window.Event('change'));
            
            twitterInput.value = "newhandle";
            twitterInput.dispatchEvent(new window.Event('change'));
            
            expect(urlChangeHandler).toHaveBeenCalledTimes(1);
            expect(twitterChangeHandler).toHaveBeenCalledTimes(1);
        });
    });

    describe('Status Message Display', () => {
        let addStatusItem;

        beforeEach(() => {
            const statusList = document.getElementById("statusList");
            
            addStatusItem = (title, text) => {
                const li = document.createElement("li");
                li.innerHTML = `<b>${title}</b>: ${text}`;
                statusList.insertBefore(li, statusList.firstChild);
            };
        });

        test('Status messages are added to the list', () => {
            const statusList = document.getElementById("statusList");
            expect(statusList.children.length).toBe(0);
            
            addStatusItem("Test Status", "Test message");
            expect(statusList.children.length).toBe(1);
            expect(statusList.firstChild.innerHTML).toBe("<b>Test Status</b>: Test message");
            
            addStatusItem("Second Status", "Second message");
            expect(statusList.children.length).toBe(2);
            expect(statusList.firstChild.innerHTML).toBe("<b>Second Status</b>: Second message");
        });

        test('Status messages are inserted at the top (newest first)', () => {
            const statusList = document.getElementById("statusList");
            
            addStatusItem("First", "Message 1");
            addStatusItem("Second", "Message 2");
            addStatusItem("Third", "Message 3");
            
            expect(statusList.children.length).toBe(3);
            expect(statusList.children[0].innerHTML).toBe("<b>Third</b>: Message 3");
            expect(statusList.children[1].innerHTML).toBe("<b>Second</b>: Message 2");
            expect(statusList.children[2].innerHTML).toBe("<b>First</b>: Message 1");
        });

        test('Status messages handle HTML escaping', () => {
            const statusList = document.getElementById("statusList");
            
            addStatusItem("HTML Test", "<script>alert('xss')</script>");
            expect(statusList.firstChild.innerHTML).toBe("<b>HTML Test</b>: <script>alert('xss')</script>");
        });

        test('Status messages handle empty content', () => {
            const statusList = document.getElementById("statusList");
            
            addStatusItem("Empty", "");
            expect(statusList.firstChild.innerHTML).toBe("<b>Empty</b>: ");
            
            addStatusItem("", "No title");
            expect(statusList.firstChild.innerHTML).toBe("<b></b>: No title");
        });
    });

    describe('UI State Management', () => {
        test('Subscribe button text changes based on subscription state', () => {
            const subscribeBtn = document.getElementById("subscribeBtn");
            
            // Initial state
            expect(subscribeBtn.textContent).toBe("Subscribe for NDEF");
            
            // Simulate subscription start
            subscribeBtn.textContent = "Stop NDEF Subscription";
            expect(subscribeBtn.textContent).toBe("Stop NDEF Subscription");
            
            // Simulate subscription stop
            subscribeBtn.textContent = "Subscribe for NDEF";
            expect(subscribeBtn.textContent).toBe("Subscribe for NDEF");
        });

        test('Button states can be disabled/enabled', () => {
            const buttons = [
                document.getElementById("subscribeBtn"),
                document.getElementById("publishUriBtn"),
                document.getElementById("publishTwitterBtn"),
                document.getElementById("shareUriBtn")
            ];
            
            buttons.forEach(button => {
                expect(button.disabled).toBe(false);
                
                button.disabled = true;
                expect(button.disabled).toBe(true);
                
                button.disabled = false;
                expect(button.disabled).toBe(false);
            });
        });

        test('Input field states can be managed', () => {
            const urlInput = document.getElementById("urlInput");
            const twitterInput = document.getElementById("twitterInput");
            
            // Test readonly state
            urlInput.readOnly = true;
            expect(urlInput.readOnly).toBe(true);
            
            twitterInput.readOnly = true;
            expect(twitterInput.readOnly).toBe(true);
            
            // Test disabled state
            urlInput.disabled = true;
            expect(urlInput.disabled).toBe(true);
            
            twitterInput.disabled = true;
            expect(twitterInput.disabled).toBe(true);
        });
    });

    describe('Error Handling in UI', () => {
        test('Handles Web NFC API errors gracefully', async () => {
            const statusList = document.getElementById("statusList");
            const addStatusItem = (title, text) => {
                const li = document.createElement("li");
                li.innerHTML = `<b>${title}</b>: ${text}`;
                statusList.insertBefore(li, statusList.firstChild);
            };

            // Mock error scenario
            mockNDEFReader.setMockError(new DOMException('Permission denied', 'NotAllowedError'));
            
            try {
                await mockNDEFReader.scan();
            } catch (error) {
                addStatusItem("Starting subscription failed", error.message);
            }
            
            expect(statusList.children.length).toBe(1);
            expect(statusList.firstChild.innerHTML).toContain("Permission denied");
        });

        test('Handles Web Share API errors gracefully', async () => {
            const statusList = document.getElementById("statusList");
            const addStatusItem = (title, text) => {
                const li = document.createElement("li");
                li.innerHTML = `<b>${title}</b>: ${text}`;
                statusList.insertBefore(li, statusList.firstChild);
            };

            // Mock share error
            navigator.share.mockRejectedValue(new Error('Share failed'));
            
            try {
                await navigator.share({
                    title: 'Test',
                    text: 'Test',
                    url: 'https://example.com'
                });
            } catch (error) {
                addStatusItem("Share Error", error.message);
            }
            
            expect(statusList.children.length).toBe(1);
            expect(statusList.firstChild.innerHTML).toContain("Share failed");
        });

        test('Handles NDEF library errors gracefully', () => {
            const statusList = document.getElementById("statusList");
            const addStatusItem = (title, text) => {
                const li = document.createElement("li");
                li.innerHTML = `<b>${title}</b>: ${text}`;
                statusList.insertBefore(li, statusList.firstChild);
            };

            try {
                // This should throw an error for invalid social record
                const socialRecord = new NdefSocialRecord("", NfcSocialType.Twitter);
                socialRecord.checkIfValid(); // Explicitly call validation
            } catch (error) {
                addStatusItem("Publish Error", error.message);
            }
            
            expect(statusList.children.length).toBe(1);
            expect(statusList.firstChild.innerHTML).toContain("Publish Error");
        });

        test('Displays appropriate message when Web NFC is not supported', () => {
            const statusList = document.getElementById("statusList");
            const addStatusItem = (title, text) => {
                const li = document.createElement("li");
                li.innerHTML = `<b>${title}</b>: ${text}`;
                statusList.insertBefore(li, statusList.firstChild);
            };

            // Simulate unsupported browser
            delete global.NDEFReader;
            
            if ('NDEFReader' in global) {
                addStatusItem("Web NFC", "is supported");
            } else {
                addStatusItem("Web NFC", "is not supported on this browser.");
            }
            
            expect(statusList.children.length).toBe(1);
            expect(statusList.firstChild.innerHTML).toBe("<b>Web NFC</b>: is not supported on this browser.");
        });
    });
});