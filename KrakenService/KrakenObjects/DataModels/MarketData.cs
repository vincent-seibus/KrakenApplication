using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects.DataModels
{
    public class MarketData
    {
        [Key]
        public string Id { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public double UnixTime { get; set; }
        public string BuyOrSell { get; set; }
        public string MarketOrLimit { get; set; }
        public double HigherBid { get; set; }
        public double LowerAsk { get; set; }
        public double MiddleQuote { get; set; }
        public double Spread { get; set; }
        public double? VolumeWeightedRatio { get; set; }
        public double? VolumeWeightedRatio100 { get; set; }
        public double EMAVolumeWeightedRatio { get; set; }
        public double EMAVolumeWeightedRatio100 { get; set; }
        public double RSI30 { get; set; }
        public double RSI60 { get; set; }
        public double RSI1440 { get; set; }

        public MarketData()
        {
            this.Id = Guid.NewGuid().ToString();
        }

    }
}
