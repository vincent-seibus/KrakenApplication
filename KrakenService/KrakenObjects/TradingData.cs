﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class TradingData
    {
        public double Price { get; set; }
        public double Volume { get; set; }
        public double UnixTime { get; set; }
        public string BuyOrSell { get; set; }
        public string MarketOrLimit { get; set; }
        public string Misc { get; set; } // usually empty
    }

}
