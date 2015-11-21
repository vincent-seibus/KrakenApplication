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
