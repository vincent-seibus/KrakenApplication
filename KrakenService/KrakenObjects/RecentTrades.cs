using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class RecentTrades
    {
        public string Pair { get; set; }
        public List<List<string>> Datas {get;set;}
        public long Last { get; set; }
    }
}
