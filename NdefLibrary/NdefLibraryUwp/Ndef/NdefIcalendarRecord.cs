/****************************************************************************
**
** Copyright (C) 2012-2017 Andreas Jakl - http://www.nfcinteractor.com/
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2014).
** More information: http://andijakl.github.io/ndef-nfc/
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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Appointments;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers;
using NdefLibrary.Ndef;

namespace NdefLibraryUwp.Ndef
{
    /// <summary>
    /// A platform-specific class for Windows 8.1 that is able to convert the 
    /// Windows Appointment class into a valid iCalendar structure and back again.
    /// 
    /// Warning: Alpha Status! Interfaces and functionality of this class might still
    /// change.
    /// </summary>
    /// <remarks>
    /// The iCalendar file can then be stored on an NFC tag or sent to another device
    /// in order to immediately create the appointment in the user's calendar.
    /// 
    /// As the Windows Appointment class and the iCalendar standards to not directly
    /// match in all properties, this class will do the conversion and transform
    /// from one type to the other, aiming to keep as much information as possible.
    /// 
    /// As such - especially for the intended usecase and as plain text iCalendar data
    /// gets quite large - the class only supports a single appointment and not an
    /// iCalendar file that contains complete calendar information formed from multiple
    /// individual events.
    /// 
    /// The NDEF record has a specific MIME type that identifies it as a calendar record,
    /// its payload is UTF8-encoded text that contains the iCalendar data (similar to
    /// an .ics file).
    /// 
    /// Note that the iCalendar standard is a cross-platform calendar standard and not
    /// to be confused with Apple's iCal.
    /// 
    /// <see cref="http://www.ietf.org/rfc/rfc2445.txt"/>
    /// </remarks>
    public class NdefIcalendarRecord : NdefIcalendarRecordBase
    {
        private const string ProductIdDefault = "-//nfcinteractor.com//NDEF Library for NFC//EN";
        private Appointment _appointmentData;

        public Appointment AppointmentData
        {
            get { return _appointmentData; }
            set
            {
                _appointmentData = value; 
                UpdatePayload();
            }
        }

        private string _productId;
        /// <summary>
        /// The product ID is mandatory for an ics file according to the specification.
        /// If you do not set this from your app, a default product ID will be used.
        /// See the official iCalendar specification for further details.
        /// <see cref="http://www.ietf.org/rfc/rfc2445.txt"/>
        /// </summary>
        public string ProductId
        {
            get
            {
                // The property MUST be specified once in an iCalendar object.
                return _productId ?? ProductIdDefault;
            }
            set
            {
                _productId = value;
                UpdatePayload();
            }
        }

        /// <summary>
        ///  Create a new iCalendar record based on the supplied appointment.
        /// The appointment will be automatically converted to a vCard for the
        /// payload of the record, using a matching algorithm to transform
        /// the data from the Windows Appointment class to the vCard standard.
        /// </summary>
        /// <param name="ap">Appointment to use for this iCalendar record.</param>
        public NdefIcalendarRecord(Appointment ap)
        {
            AppointmentData = ap;
        }

        /// <summary>
        /// Parses the supplied iCalendar compatible payload (UTF8 encoded string)
        /// and converts it into the Windows 8 Appointment class by matching the
        /// properties.
        /// Afterwards, the method sets the payload of the record by converting
        /// the Appointment class back to an iCalendar file, to maintain consistency
        /// between the stored AppointmentData and the ics data in the payload.
        /// </summary>
        /// <param name="iCalendarPayload"></param>
        public NdefIcalendarRecord(byte[] iCalendarPayload)
        {
            ConvertIcalendarToAppointment(iCalendarPayload);
        }

        public NdefIcalendarRecord(NdefRecord other)
            :base(other)
        {
            if (!IsRecordType(this))
                throw new NdefException(NdefExceptionMessagesUwp.ExInvalidCopy);

            ConvertIcalendarToAppointment(_payload);
        }


        private void UpdatePayload()
        {
            if (AppointmentData == null)
                return;

            var ics = ConvertAppointmentToIcalendar(AppointmentData);
            var icsString = new CalendarSerializer().SerializeToString(ics);
            Payload = Encoding.UTF8.GetBytes(icsString);
        }

        private Calendar ConvertAppointmentToIcalendar(Appointment ap)
        {
            var ics = new Calendar { ProductId = this.ProductId };

            var evt = ics.Create<CalendarEvent>();

            // Event description and main properties
            evt.Summary = ap.Subject;
            evt.Description = ap.Details;
            evt.Organizer = new Organizer(ap.Organizer.Address) { CommonName = ap.Organizer.DisplayName };
            evt.Url = ap.Uri;

            // Time
            evt.Start = new CalDateTime(ap.StartTime.UtcDateTime, TimeZoneInfo.Utc.Id);
            evt.Duration = ap.Duration;
            evt.IsAllDay = ap.AllDay;
            
            // Location
            evt.Location = ap.Location;
            
            // TODO: recurrence
            
            // Reminder
            if (ap.Reminder != null)
            {
                var reminderSource = (TimeSpan) ap.Reminder;
                evt.Alarms.Add(new Alarm
                {
                    //Duration = ap.Reminder,
                    Trigger = new Trigger(reminderSource),
                    //Action = AlarmAction.Audio,
                    //Description = "Reminder"
                });
            }

            return ics;
        }

        private void ConvertIcalendarToAppointment(byte[] iCalendarPayload)
        {
            var iCalendarStream = new MemoryStream(iCalendarPayload);
            var calendarCollection = Calendar.LoadFromStream(iCalendarStream);

            if (calendarCollection.Count >= 1)
            {
                if (calendarCollection.Count >= 2)
                {
                    Debug.WriteLine("Note: the library only parses the first calendar object.");
                }
                var curCalendar = calendarCollection.First();
                ProductId = curCalendar.ProductId;

                if (curCalendar.Events != null && curCalendar.Events.Count >= 1)
                {
                    if (curCalendar.Events.Count >= 2)
                    {
                        Debug.WriteLine("Note: the library only parses the first event object.");
                    }
                    var evt = curCalendar.Events.First();

                    // Internalize event data
                    var ap = new Appointment
                    {
                        // Event description and main properties
                        Subject = evt.Summary,
                        Details = evt.Description,

                        // Time
                        StartTime = evt.Start.AsUtc,
                        Duration = evt.Duration,
                        AllDay = evt.IsAllDay,

                        // Location
                        Location = evt.Location,

                        // TODO: recurrence
                    };
                    if (evt.Url != null)
                    {
                        ap.Uri = evt.Url;
                    }
                    if (evt.Organizer != null)
                    {
                        ap.Organizer = new AppointmentOrganizer
                        {
                            DisplayName = evt.Organizer.CommonName
                        };
                        if (evt.Organizer.Value != null) ap.Organizer.Address = evt.Organizer.Value.ToString();
                    }

                    // Reminder
                    if (evt.Alarms != null && evt.Alarms.Count > 0)
                    {
                        ap.Reminder =  evt.Alarms.First().Trigger.Duration;
                    }
                        
                    AppointmentData = ap;
                }
                else
                {
                    throw new NdefException(NdefExceptionMessagesUwp.ExIcalendarNoEventFound);
                }
            }
            else
            {
                throw new NdefException(NdefExceptionMessagesUwp.ExIcalendarNoCalendarFound);
            }
        }
    }
}
