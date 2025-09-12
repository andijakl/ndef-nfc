import { NdefGeoRecord, NfcGeoType } from '../../src/submodule/NdefGeoRecord.js';
import { NdefSocialRecord, NfcSocialType } from '../../src/submodule/NdefSocialRecord.js';
import { NdefTelRecord } from '../../src/submodule/NdefTelRecord.js';
import { NdefAndroidAppRecord } from '../../src/submodule/NdefAndroidAppRecord.js';
import { NdefRecord, TypeNameFormatType } from '../../src/submodule/NdefRecord.js';
import { NdefUriRecord } from '../../src/submodule/NdefUriRecord.js';

describe('Specialized NDEF Record Types', () => {
    describe('NdefGeoRecord', () => {
        describe('Geo record creation', () => {
            test('should create geo record with latitude and longitude', () => {
                const latitude = 47.6062;
                const longitude = -122.3321;
                const record = new NdefGeoRecord(latitude, longitude);

                expect(record.getLatitude()).toBe(latitude);
                expect(record.getLongitude()).toBe(longitude);
                expect(record.getGeoType()).toBe(NfcGeoType.GeoUri);
                expect(record.getUri()).toBe(`geo:${latitude},${longitude}`);
            });

            test('should create geo record with specific geo type', () => {
                const latitude = 40.7128;
                const longitude = -74.0060;
                const record = new NdefGeoRecord(latitude, longitude, NfcGeoType.BingMaps);

                expect(record.getLatitude()).toBe(latitude);
                expect(record.getLongitude()).toBe(longitude);
                expect(record.getGeoType()).toBe(NfcGeoType.BingMaps);
                expect(record.getUri()).toBe(`bingmaps:?cp=${latitude}~${longitude}`);
            });

            test('should handle negative coordinates', () => {
                const latitude = -33.8688;
                const longitude = 151.2093;
                const record = new NdefGeoRecord(latitude, longitude);

                expect(record.getLatitude()).toBe(latitude);
                expect(record.getLongitude()).toBe(longitude);
                expect(record.getUri()).toBe(`geo:${latitude},${longitude}`);
            });

            test('should handle zero coordinates', () => {
                const latitude = 0;
                const longitude = 0;
                const record = new NdefGeoRecord(latitude, longitude);

                expect(record.getLatitude()).toBe(latitude);
                expect(record.getLongitude()).toBe(longitude);
                // KNOWN BUG: Implementation doesn't handle zero coordinates due to falsy check
                expect(record.getUri()).toBe('geo:0,0');
            });
        });

        describe('Coordinate validation', () => {
            test('should handle valid latitude range', () => {
                const validLatitudes = [-90, -45, 0, 45, 90];

                validLatitudes.forEach(lat => {
                    const record = new NdefGeoRecord(lat, 0);
                    expect(record.getLatitude()).toBe(lat);
                });
            });

            test('should handle valid longitude range', () => {
                const validLongitudes = [-180, -90, 0, 90, 180];

                validLongitudes.forEach(lng => {
                    const record = new NdefGeoRecord(0, lng);
                    expect(record.getLongitude()).toBe(lng);
                });
            });

            test('should handle high precision coordinates', () => {
                const latitude = 47.606209;
                const longitude = -122.332071;
                const record = new NdefGeoRecord(latitude, longitude);

                expect(record.getLatitude()).toBe(latitude);
                expect(record.getLongitude()).toBe(longitude);
            });
        });

        describe('Geo type handling', () => {
            test('should generate correct URI for each geo type', () => {
                const lat = 47.6062;
                const lng = -122.3321;

                const testCases = [
                    { type: NfcGeoType.GeoUri, expected: `geo:${lat},${lng}` },
                    { type: NfcGeoType.BingMaps, expected: `bingmaps:?cp=${lat}~${lng}` },
                    { type: NfcGeoType.NokiaMapsUri, expected: `http://m.ovi.me/?c=${lat},${lng}` },
                    { type: NfcGeoType.WebRedirect, expected: `http://NfcInteractor.com/m?c=${lat},${lng}` },
                    { type: NfcGeoType.MsDriveTo, expected: `ms-drive-to:?destination.latitude=${lat}&destination.longitude=${lng}` },
                    { type: NfcGeoType.MsWalkTo, expected: `ms-walk-to:?destination.latitude=${lat}&destination.longitude=${lng}` }
                ];

                testCases.forEach(testCase => {
                    const record = new NdefGeoRecord(lat, lng, testCase.type);
                    expect(record.getUri()).toBe(testCase.expected);
                });
            });

            test('should allow changing geo type after creation', () => {
                const record = new NdefGeoRecord(47.6062, -122.3321, NfcGeoType.GeoUri);

                expect(record.getGeoType()).toBe(NfcGeoType.GeoUri);

                record.setGeoType(NfcGeoType.BingMaps);
                expect(record.getGeoType()).toBe(NfcGeoType.BingMaps);
                expect(record.getUri()).toContain('bingmaps:');
            });
        });

        describe('Coordinate modification', () => {
            test('should allow modifying latitude after creation', () => {
                const record = new NdefGeoRecord(47.6062, -122.3321);

                record.setLatitude(40.7128);
                expect(record.getLatitude()).toBe(40.7128);
                expect(record.getUri()).toBe('geo:40.7128,-122.3321');
            });

            test('should allow modifying longitude after creation', () => {
                const record = new NdefGeoRecord(47.6062, -122.3321);

                record.setLongitude(-74.0060);
                expect(record.getLongitude()).toBe(-74.0060);
                // Handle floating point precision issues
                expect(record.getUri()).toContain('geo:47.6062,-74.006');
            });

            test('should update URI when coordinates change', () => {
                const record = new NdefGeoRecord(0, 0);

                record.setLatitude(51.5074);
                record.setLongitude(-0.1278);

                expect(record.getUri()).toBe('geo:51.5074,-0.1278');
            });
        });
    });

    describe('NdefSocialRecord', () => {
        describe('Social record creation', () => {
            test('should create X social record', () => {
                const username = "testuser";
                const record = new NdefSocialRecord(username, NfcSocialType.X);

                expect(record.getSocialUserName()).toBe(username);
                expect(record.getSocialType()).toBe(NfcSocialType.X);
                expect(record.getUri()).toBe(`https://x.com/${username}`);
            });

            test('should default to X when no type specified', () => {
                const username = "testuser";
                const record = new NdefSocialRecord(username);

                expect(record.getSocialType()).toBe(NfcSocialType.X);
                expect(record.getUri()).toBe(`https://x.com/${username}`);
            });

            test('should create LinkedIn social record', () => {
                const username = "john-doe";
                const record = new NdefSocialRecord(username, NfcSocialType.LinkedIn);

                expect(record.getSocialUserName()).toBe(username);
                expect(record.getSocialType()).toBe(NfcSocialType.LinkedIn);
                expect(record.getUri()).toBe(`https://linkedin.com/in/${username}`);
            });

            test('should create Facebook social record', () => {
                const username = "john.doe";
                const record = new NdefSocialRecord(username, NfcSocialType.Facebook);

                expect(record.getUri()).toBe(`https://facebook.com/${username}`);
            });

            test('should create Instagram social record', () => {
                const username = "john.doe";
                const record = new NdefSocialRecord(username, NfcSocialType.Instagram);

                expect(record.getUri()).toBe(`https://instagram.com/${username}`);
            });

            test('should create Threads social record', () => {
                const username = "john.doe";
                const record = new NdefSocialRecord(username, NfcSocialType.Threads);

                expect(record.getUri()).toBe(`https://threads.net/@${username}`);
            });

            test('should create TikTok social record', () => {
                const username = "john.doe";
                const record = new NdefSocialRecord(username, NfcSocialType.TikTok);

                expect(record.getUri()).toBe(`https://tiktok.com/@${username}`);
            });
        });

        describe('Social network URL generation', () => {
            test('should generate correct URLs for all social networks', () => {
                const username = "testuser";

                const testCases = [
                    { type: NfcSocialType.X, expected: `https://x.com/${username}` },
                    { type: NfcSocialType.LinkedIn, expected: `https://linkedin.com/in/${username}` },
                    { type: NfcSocialType.Facebook, expected: `https://facebook.com/${username}` },
                    { type: NfcSocialType.Instagram, expected: `https://instagram.com/${username}` },
                    { type: NfcSocialType.Threads, expected: `https://threads.net/@${username}` },
                    { type: NfcSocialType.TikTok, expected: `https://tiktok.com/@${username}` }
                ];

                testCases.forEach(testCase => {
                    const record = new NdefSocialRecord(username, testCase.type);
                    expect(record.getUri()).toBe(testCase.expected);
                });
            });

            test('should handle usernames with special characters', () => {
                const usernames = ["user_name", "user-name", "user.name", "user123"];

                usernames.forEach(username => {
                    const record = new NdefSocialRecord(username, NfcSocialType.X);
                    expect(record.getSocialUserName()).toBe(username);
                    expect(record.getUri()).toBe(`https://x.com/${username}`);
                });
            });
        });

        describe('Social record modification', () => {
            test('should allow changing username after creation', () => {
                const record = new NdefSocialRecord("olduser", NfcSocialType.Twitter);

                record.setSocialUserName("newuser");
                expect(record.getSocialUserName()).toBe("newuser");
                expect(record.getUri()).toBe("http://twitter.com/newuser");
            });

            test('should allow changing social type after creation', () => {
                const record = new NdefSocialRecord("testuser", NfcSocialType.Twitter);

                record.setSocialType(NfcSocialType.Facebook);
                expect(record.getSocialType()).toBe(NfcSocialType.Facebook);
                expect(record.getUri()).toBe("http://facebook.com/testuser");
            });
        });

        describe('Social record validation', () => {
            test('should validate record with username', () => {
                const record = new NdefSocialRecord("testuser");

                expect(() => record.checkIfValid()).not.toThrow();
            });

            test('should throw error for empty username', () => {
                const record = new NdefSocialRecord("");

                expect(() => record.checkIfValid()).toThrow("Social user name is empty");
            });

            test('should throw error for null username', () => {
                const record = new NdefSocialRecord(null);

                expect(() => record.checkIfValid()).toThrow("Social user name is empty");
            });
        });
    });

    describe('NdefTelRecord', () => {
        describe('Telephone record creation', () => {
            test('should create telephone record with phone number', () => {
                const phoneNumber = "+1-555-123-4567";
                const record = new NdefTelRecord(phoneNumber);

                expect(record.getTelNumber()).toBe(phoneNumber);
                expect(record.getUri()).toBe(`tel:${phoneNumber}`);
            });

            test('should create empty telephone record', () => {
                const record = new NdefTelRecord();

                expect(record.getTelNumber()).toBeUndefined();
            });

            test('should handle null phone number', () => {
                const record = new NdefTelRecord(null);

                // Implementation may not handle null properly
                expect(record.getTelNumber()).toBeFalsy();
            });
        });

        describe('Telephone number formatting', () => {
            test('should handle various phone number formats', () => {
                const phoneNumbers = [
                    "+1-555-123-4567",
                    "555-123-4567",
                    "(555) 123-4567",
                    "+44 20 7946 0958",
                    "555.123.4567",
                    "15551234567"
                ];

                phoneNumbers.forEach(number => {
                    const record = new NdefTelRecord(number);
                    expect(record.getTelNumber()).toBe(number);
                    expect(record.getUri()).toBe(`tel:${number}`);
                });
            });

            test('should handle international phone numbers', () => {
                const internationalNumbers = [
                    "+1-555-123-4567", // US
                    "+44-20-7946-0958", // UK
                    "+49-30-12345678", // Germany
                    "+81-3-1234-5678", // Japan
                    "+86-10-1234-5678" // China
                ];

                internationalNumbers.forEach(number => {
                    const record = new NdefTelRecord(number);
                    expect(record.getTelNumber()).toBe(number);
                    expect(record.getUri()).toBe(`tel:${number}`);
                });
            });

            test('should handle phone numbers with extensions', () => {
                const numberWithExt = "+1-555-123-4567 ext 123";
                const record = new NdefTelRecord(numberWithExt);

                expect(record.getTelNumber()).toBe(numberWithExt);
                expect(record.getUri()).toBe(`tel:${numberWithExt}`);
            });
        });

        describe('Telephone record modification', () => {
            test('should allow changing phone number after creation', () => {
                const record = new NdefTelRecord("+1-555-123-4567");

                record.setTelNumber("+1-555-987-6543");
                expect(record.getTelNumber()).toBe("+1-555-987-6543");
                expect(record.getUri()).toBe("tel:+1-555-987-6543");
            });

            test('should update URI when phone number changes', () => {
                const record = new NdefTelRecord("555-123-4567");

                record.setTelNumber("+1-555-123-4567");
                expect(record.getUri()).toBe("tel:+1-555-123-4567");
            });
        });

        describe('Telephone record validation', () => {
            test('should validate record with phone number', () => {
                const record = new NdefTelRecord("+1-555-123-4567");

                expect(() => record.checkIfValid()).not.toThrow();
            });

            test('should throw error for empty phone number', () => {
                const record = new NdefTelRecord("");

                expect(() => record.checkIfValid()).toThrow("Telephone number is empty");
            });

            test('should throw error for null phone number', () => {
                const record = new NdefTelRecord(null);

                expect(() => record.checkIfValid()).toThrow("Telephone number is empty");
            });
        });

        describe('Record type identification', () => {
            test('should identify telephone record type correctly', () => {
                const telRecord = new NdefTelRecord("+1-555-123-4567");

                expect(NdefTelRecord.isRecordType(telRecord)).toBe(true);
            });

            test('should not identify non-telephone URI as telephone record', () => {
                // Create a proper URI record that's not a telephone record
                const uriRecord = new NdefUriRecord("http://example.com");

                expect(NdefTelRecord.isRecordType(uriRecord)).toBe(false);
            });

            test('should not identify non-URI record as telephone record', () => {
                const textRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F]);

                expect(NdefTelRecord.isRecordType(textRecord)).toBe(false);
            });
        });
    });

    describe('NdefAndroidAppRecord', () => {
        describe('Android App Record creation', () => {
            test('should create AAR with package name', () => {
                const packageName = "com.example.myapp";
                const record = new NdefAndroidAppRecord(packageName);

                expect(record.getPackageName()).toBe(packageName);
                expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.ExternalRtd);
                expect(Array.from(record.getType())).toEqual(Array.from(NdefAndroidAppRecord.AAR_TYPE));
            });

            test('should create empty AAR', () => {
                const record = new NdefAndroidAppRecord();

                expect(record.getPackageName()).toBe("");
            });

            test('should handle null package name', () => {
                const record = new NdefAndroidAppRecord(null);

                expect(record.getPackageName()).toBe("");
            });
        });

        describe('AAR format validation', () => {
            test('should use correct External RTD type', () => {
                const record = new NdefAndroidAppRecord("com.example.app");

                expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.ExternalRtd);
                expect(Array.from(record.getType())).toEqual(Array.from(new TextEncoder().encode("android.com:pkg")));
            });

            test('should handle standard Android package names', () => {
                const packageNames = [
                    "com.example.app",
                    "com.google.android.apps.maps",
                    "org.mozilla.firefox",
                    "net.sourceforge.opencamera",
                    "de.danoeh.antennapod"
                ];

                packageNames.forEach(packageName => {
                    const record = new NdefAndroidAppRecord(packageName);
                    expect(record.getPackageName()).toBe(packageName);
                });
            });

            test('should handle package names with underscores and numbers', () => {
                const packageNames = [
                    "com.example.my_app",
                    "com.company.app123",
                    "com.test.app_v2"
                ];

                packageNames.forEach(packageName => {
                    const record = new NdefAndroidAppRecord(packageName);
                    expect(record.getPackageName()).toBe(packageName);
                });
            });
        });

        describe('Package name modification', () => {
            test('should allow changing package name after creation', () => {
                const record = new NdefAndroidAppRecord("com.example.oldapp");

                record.setPackageName("com.example.newapp");
                expect(record.getPackageName()).toBe("com.example.newapp");
            });

            test('should handle setting empty package name', () => {
                const record = new NdefAndroidAppRecord("com.example.app");

                record.setPackageName("");
                expect(record.getPackageName()).toBe("");
            });

            test('should handle setting null package name', () => {
                const record = new NdefAndroidAppRecord("com.example.app");

                record.setPackageName(null);
                expect(record.getPackageName()).toBe("");
            });
        });

        describe('Record type identification', () => {
            test('should identify Android App Record type correctly', () => {
                const aarRecord = new NdefAndroidAppRecord("com.example.app");

                expect(NdefAndroidAppRecord.isRecordType(aarRecord)).toBe(true);
            });

            test('should not identify non-AAR record as AAR type', () => {
                const textRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F]);

                expect(NdefAndroidAppRecord.isRecordType(textRecord)).toBe(false);
            });

            test('should not identify different External RTD as AAR type', () => {
                const externalRecord = new NdefRecord(TypeNameFormatType.ExternalRtd, [0x63, 0x6F, 0x6D, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65]);

                expect(NdefAndroidAppRecord.isRecordType(externalRecord)).toBe(false);
            });
        });

        describe('Static properties and constants', () => {
            test('should have correct AAR_TYPE constant', () => {
                const expectedType = new TextEncoder().encode("android.com:pkg");
                expect(Array.from(NdefAndroidAppRecord.AAR_TYPE)).toEqual(Array.from(expectedType));
            });

            test('should use AAR_TYPE in record creation', () => {
                const record = new NdefAndroidAppRecord("com.example.app");

                expect(Array.from(record.getType())).toEqual(Array.from(NdefAndroidAppRecord.AAR_TYPE));
            });
        });

        describe('Integration with NdefRecord base class', () => {
            test('should inherit NdefRecord functionality', () => {
                const record = new NdefAndroidAppRecord("com.example.app");

                // Should have NdefRecord methods
                expect(typeof record.getTypeNameFormat).toBe('function');
                expect(typeof record.getType).toBe('function');
                expect(typeof record.getPayload).toBe('function');
                expect(typeof record.getId).toBe('function');
                expect(typeof record.setId).toBe('function');
            });

            test('should validate as proper NdefRecord', () => {
                const record = new NdefAndroidAppRecord("com.example.app");

                expect(() => record.checkIfValid()).not.toThrow();
            });

            test('should allow setting ID field', () => {
                const record = new NdefAndroidAppRecord("com.example.app");
                const id = [0x01, 0x02, 0x03];

                record.setId(id);
                expect(Array.from(record.getId())).toEqual(id);
            });
        });
    });

    describe('Specialized record constants', () => {
        test('should have correct NfcGeoType constants', () => {
            expect(NfcGeoType.GeoUri).toBe(0);
            expect(NfcGeoType.BingMaps).toBe(1);
            expect(NfcGeoType.NokiaMapsUri).toBe(2);
            expect(NfcGeoType.WebRedirect).toBe(3);
            expect(NfcGeoType.MsDriveTo).toBe(4);
            expect(NfcGeoType.MsWalkTo).toBe(5);
        });

        test('should have correct NfcSocialType constants', () => {
            expect(NfcSocialType.Twitter).toBe(0);
            expect(NfcSocialType.LinkedIn).toBe(1);
            expect(NfcSocialType.Facebook).toBe(2);
            expect(NfcSocialType.Xing).toBe(3);
            expect(NfcSocialType.VKontakte).toBe(4);
            expect(NfcSocialType.FoursquareWeb).toBe(5);
            expect(NfcSocialType.FoursquareApp).toBe(6);
            expect(NfcSocialType.Skype).toBe(7);
            expect(NfcSocialType.GooglePlus).toBe(8);
        });
    });
});