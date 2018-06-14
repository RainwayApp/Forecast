using System;
using System.Collections.Generic;
using System.Text;

namespace Forecast.Models
{
    public class AddressInformation
    {
        public string IpAddress { get; set; }

        public int TimeToLive { get; set; }
        public bool CouldPing { get; set; }
    }
}
