using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class CurrentOrder
    {
        public string OrderID { get; set; }
        public string OrderType { get; set; } // limit or market or other
        public string Type { get; set; } // buy or sell
        public double Price { get; set; }
        public double Volume { get; set; }
        public double Timestamp { get; set; }
    }
}
