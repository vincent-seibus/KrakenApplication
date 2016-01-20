using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class Position
    {
        [Key]
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public double userref { get; set; }
        public double BTC { get; set; }
        public double TotalBTC { get; set; }
        public double TotalEUR { get; set; }
    }
}
