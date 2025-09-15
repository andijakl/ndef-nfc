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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace NdefDemoWinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Set window title
            this.Title = "NFC / NDEF Demo (WinUI 3)";
            
            // Set up navigation event handlers
            ContentFrame.NavigationFailed += OnNavigationFailed;
            
            // Navigate to the main page
            ContentFrame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            var pageTypeName = e.SourcePageType?.FullName ?? "Unknown";
            throw new InvalidOperationException($"Failed to load Page {pageTypeName}", e.Exception);
        }
    }
}