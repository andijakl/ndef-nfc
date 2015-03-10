/****************************************************************************
**
** Copyright (C) 2012 - 2014 Andreas Jakl.
** All rights reserved.
**
** Code example for the NDEF Library for Proximity APIs (NFC).
**
** Created by Andreas Jakl (2013).
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
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace NdefDemoWin
{
    public sealed partial class AboutFlyout : SettingsFlyout
    {
        private readonly ResourceLoader _loader = new ResourceLoader();

        public AboutFlyout()
        {
            InitializeComponent();
        }

        private void BtnNdefLibrary_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(_loader.GetString("AboutNdefLibraryUri"));
        }
        private void BtnNfcInteractor_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(_loader.GetString("AboutNfcInteractorUri"));
        }
        private void BtnTwitter_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(_loader.GetString("AboutTwitterUri"));
        }

        private static async void LaunchUri(string uri)
        {
            var webUri = new Uri(uri);
            await Windows.System.Launcher.LaunchUriAsync(webUri);
        }

    }
}
