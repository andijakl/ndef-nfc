// Copyright 2012 - 2016 Andreas Jakl. All rights reserved.
// NDEF Library for Proximity APIs / NFC
// http://andijakl.github.io/ndef-nfc/
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.

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
