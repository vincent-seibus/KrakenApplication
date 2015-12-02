using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{    
    public class OrderOfBook
    {
        public string OrderType { get; set; } // ask or bid for the book / limit or market or other for my orders     
        public double Price { get; set; }
        public double Volume { get; set; }
        public double Timestamp { get; set; }
        public DateTime TimeRecorded { get; set; }  // time when the order was recorded
        public string UniqueId { get; set; } // A concatenation of all the property above except the time recorded
    }
}
