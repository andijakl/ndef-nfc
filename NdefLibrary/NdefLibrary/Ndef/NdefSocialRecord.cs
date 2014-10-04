/****************************************************************************
**
** Copyright (C) 2012-2013 Andreas Jakl, Mopius - http://www.mopius.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
** More information: http://ndef.codeplex.com/
**
** GNU Lesser General Public License Usage
** This file may be used under the terms of the GNU Lesser General Public
** License version 2.1 as published by the Free Software Foundation and
** appearing in the file LICENSE.LGPL included in the packaging of this
** file. Please review the following information to ensure the GNU Lesser
** General Public License version 2.1 requirements will be met:
** http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html.
**
** GNU General Public License Usage
** Alternatively, this file may be used under the terms of the GNU General
** Public License version 3.0 as published by the Free Software Foundation
** and appearing in the file LICENSE.GPL included in the packaging of this
** file. Please review the following information to ensure the GNU General
** Public License version 3.0 requirements will be met:
** http://www.gnu.org/copyleft/gpl.html.
**
****************************************************************************/

using System.Collections.Generic;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Link to one of the supported social networks by
    /// simply selecting the network and specifying the username.
    /// </summary>
    /// <remarks>
    /// Tapping a tag written with this record type will take
    /// the user to the social network web site, where he can then
    /// for example start following you on Twitter.
    /// 
    /// As this class is based on the Smart URI base class, the
    /// payload is formatted as a URI record initially. When first
    /// adding Smart Poster information (like a title), the payload
    /// instantly transforms into a Smart Poster.
    /// </remarks>
    public class NdefSocialRecord : NdefSmartUriRecord
    {
        /// <summary>
        /// List of social networks this class can be used to generate a link for.
        /// </summary>
        public enum NfcSocialType
        {
            Twitter = 0,
            LinkedIn,
            Facebook,
            Xing,
            VKontakte,
            FoursquareWeb,
            FoursquareApp,
            Skype,
            GooglePlus
        }

        /// <summary>
        /// Supported social network types and the respective format strings to create the URIs.
        /// </summary>
        private static readonly Dictionary<NfcSocialType, string> SocialTagTypeUris = new Dictionary<NfcSocialType, string>
                                                                                          {
                                                                                        { NfcSocialType.Twitter, "http://twitter.com/{0}" },
                                                                                        { NfcSocialType.LinkedIn, "http://linkedin.com/in/{0}" },
                                                                                        { NfcSocialType.Facebook, "http://facebook.com/{0}" },
                                                                                        { NfcSocialType.Xing, "http://xing.com/profile/{0}" },
                                                                                        { NfcSocialType.VKontakte, "http://vkontakte.ru/{0}" },
                                                                                        { NfcSocialType.FoursquareWeb, "http://m.foursquare.com/v/{0}" },
                                                                                        { NfcSocialType.FoursquareApp, "foursquare://venues/{0}" },
                                                                                        { NfcSocialType.Skype, "skype:{0}?call" },
                                                                                        { NfcSocialType.GooglePlus, "https://plus.google.com/{0}" },
                                                                                    };

        private string _socialUserName;
        /// <summary>
        /// Username / id of the social network.
        /// </summary>
        public string SocialUserName
        {
            get { return _socialUserName; }
            set
            {
                _socialUserName = value;
                UpdatePayload();
            }
        }

        private NfcSocialType _socialType;
        /// <summary>
        /// Format to use for encoding the social network URL.
        /// </summary>
        public NfcSocialType SocialType
        {
            get { return _socialType; }
            set
            {
                _socialType = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Format the URI of the SmartUri base class.
        /// </summary>
        private void UpdatePayload()
        {
            Uri = string.Format(SocialTagTypeUris[SocialType], SocialUserName);
        }

        /// <summary>
        /// Checks if the contents of the record are valid; throws an exception if
        /// a problem is found, containing a textual description of the issue.
        /// </summary>
        /// <exception cref="NdefException">Thrown if no valid NDEF record can be
        /// created based on the record's current contents. The exception message 
        /// contains further details about the issue.</exception>
        /// <returns>True if the record contents are valid, or throws an exception
        /// if an issue is found.</returns>
        public override bool CheckIfValid()
        {
            // First check the basics
            if (!base.CheckIfValid()) return false;

            // Check specific content of this record
            if (string.IsNullOrEmpty(SocialUserName))
            {
                throw new NdefException(NdefExceptionMessages.ExSocialNoUser);
            }
            return true;
        }
    }
}
