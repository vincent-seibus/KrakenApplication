using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class OrdersBook
    {
        public string Pair { get; set; }
        public List<List<string>> Asks { get; set; }
        public List<List<string>> Bids { get; set; }
    }
}
