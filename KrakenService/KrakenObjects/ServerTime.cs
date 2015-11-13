using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class ServerTime
    {
        public long unixtime { get; set; }
        public string rfc1123 {get;set;}
    }


}
