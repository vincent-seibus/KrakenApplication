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
        public string OrderType { get; set; } // ask or bid for the book / limit or market or other for my orders
        public string Type { get; set; } // buy or sell for my order
        public double Price { get; set; }
        public double? Price2 { get; set; } // profit price if stop profit limit order
        public double Volume { get; set; }
        public double Timestamp { get; set; }
    }
}
