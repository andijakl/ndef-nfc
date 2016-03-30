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