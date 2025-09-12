using System;

namespace NdefLibrary.Ndef
{
    public class NdefAppointment
    {
        public string Subject { get; set; }
        public string Details { get; set; }
        public NdefOrganizer Organizer { get; set; }
        public Uri Uri { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public bool AllDay { get; set; }
        public string Location { get; set; }
        public TimeSpan? Reminder { get; set; }
    }

    public class NdefOrganizer
    {
        public string DisplayName { get; set; }
        public string Address { get; set; }
    }
}
