using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingFramework
{
    public class Schedule
    {
        public string FlightNumber { get; set; }
        public string PeriodOfOperation { get; set; }
        public string DaysOfOperation { get; set; }
        public string DepartureTime { get; set; }
        public string OriginalStation { get; set; }
        public string DestinationStation { get; set; }
        public string Aircraft { get; set; }
    }
}
