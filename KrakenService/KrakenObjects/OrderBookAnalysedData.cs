using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    [Serializable]
    public class OrderBookAnalysedData
    {

        [Key]
        public string Id { get; set; }
        public long UnixTimestamp { get; set; }
        public DateTime Timestamp { get; set; }
        public double LowerBid { get; set; }
        public double HigherBid { get; set; }
        public double LowerAsk { get; set; }
        public double HigherAsk { get; set; }
        public double BidDepth { get; set; }
        public double AskDepth { get; set; }
        public double BidVolume { get; set; }
        public double AskVolume { get; set; }
        public double DepthRatio { get; set; }
        public double VolumeRatio { get; set; }
        
        public OrderBookAnalysedData()
        {
            this.Id = Guid.NewGuid().ToString();
        }


    }
}
