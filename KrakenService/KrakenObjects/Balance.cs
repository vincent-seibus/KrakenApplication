using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class Balance
    {
        public double EUR { get; set; }
        public double BTC { get; set; }
        public double TotalBTC { get; set; }
        public double TotalEUR { get; set; }
    }
}
