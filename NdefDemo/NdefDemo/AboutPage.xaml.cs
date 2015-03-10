/****************************************************************************
**
** Copyright (C) 2012 Andreas Jakl.
** All rights reserved.
**
** Code example for the NDEF Library for Proximity APIs (NFC).
**
** Created by Andreas Jakl (2012).
** More information: http://andijakl.github.io/ndef-nfc/
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
using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using NdefDemo.Resources;

namespace NdefDemo
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void BtnNdefLibrary_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(AppResources.AboutNdefLibraryUri);
        }

        private void BtnNfcInteractor_Click(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(AppResources.AboutNfcInteractorUri));
        }

        private void BtnTwitter_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(AppResources.AboutTwitterUri);
        }

        private void LaunchUri(string uri)
        {
            var task = new WebBrowserTask { Uri = new Uri(uri, UriKind.Absolute) };
            task.Show();
        }
    }
}