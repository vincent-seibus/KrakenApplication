using KrakenService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TradeWebApp.Models
{
    public class Dashboard
    {
        [Key]
        public int id { get; set; }
        public PlayerState PlayerState { get; set; }
        public double LastMiddleQuote { get; set; }
        public double LastPrice { get; set; }
        public double VolumeWeightedRatio { get; set; }
        public double BalanceEuro { get; set; }
        public double BalanceBtc { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsStopping { get; set; }
        
    }
}