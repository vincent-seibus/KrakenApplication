using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class CurrentOrder
    {
        public string OrderType { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public double Timestamp { get; set; }
    }
}
