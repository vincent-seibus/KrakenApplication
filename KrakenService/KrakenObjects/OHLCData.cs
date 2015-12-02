using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class OHLCData
    {
        public double time { get; set; }
        public double open {get;set;}
        public double high {get;set;}
        public double low {get;set;}
         public double close {get;set;}
         public string  vwap {get;set;}
         public double volume {get;set;}
         public int count {get;set;}
    }
}

/*/
pair = asset pair to get OHLC data for
interval = time frame interval in minutes (optional):
	1 (default), 5, 15, 30, 60, 240, 1440, 10080, 21600
since = return committed OHLC data since given id (optional.  exclusive)

<pair_name> = pair name
    array of array entries(<time>, <open>, <high>, <low>, <close>, <vwap>, <volume>, <count>)
last = id to be used as since when polling for new, committed OHLC data

/*/