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
using System;

namespace NdefDemoWinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Enable frame rate counter for debugging
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}