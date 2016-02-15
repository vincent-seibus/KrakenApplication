using KrakenService.Data;
using KrakenService.KrakenObjects;
using KrakenService.KrakenObjects.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KrakenService.MarketAnalysis
{
    public class MarketDataRecorder
    {
        public OrderBookAnalysisMethod orderbook {get;set;}
        public RSIMethod rsi30 {get;set;}
        public RSIMethod rsi60 {get;set;}
        public RSIMethod rsi1440 {get;set;}
        public MySqlIdentityDbContext db {get;set;}

        public bool IsRecording { get; set; }

        public MarketDataRecorder(OrderBookAnalysisMethod i_orderbook, RSIMethod i_rsi30,  RSIMethod i_rsi60, RSIMethod i_rsi1440)
        {
            db = new MySqlIdentityDbContext();
            orderbook = i_orderbook;
            rsi30 = i_rsi30;
            rsi60 = i_rsi60;
            rsi1440 = i_rsi1440;
            IsRecording = true;

            Task.Run(() => LoopRecord());
        }

        public void LoopRecord()
        {
            while(IsRecording)
            {
                try
                {
                    Record();
                    Thread.Sleep(10000);
                }
                catch(Exception ex)
                {

                }
            }
        }

        public void Record()
        {
            MarketData md = new MarketData();
            TradingData td = db.TradingDatas.OrderByDescending(a => a.UnixTime).FirstOrDefault();
            md.BuyOrSell = td.BuyOrSell;
            md.EMAVolumeWeightedRatio = orderbook.orderBookAnalysedData.EMA;
            md.EMAVolumeWeightedRatio100 = orderbook.orderBookAnalysedData100.EMA;
            md.MarketOrLimit = td.MarketOrLimit;
            md.MiddleQuote = orderbook.LastMiddleQuote;
            md.Price = td.Price;
            md.RSI1440 = rsi1440.IndexRSI;
            md.RSI30 = rsi30.IndexRSI;
            md.RSI60 = rsi60.IndexRSI;
            md.UnixTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            md.Volume = td.Volume;
            md.VolumeWeightedRatio = orderbook.orderBookAnalysedData.VolumeWeightedRatio;
            md.VolumeWeightedRatio100 = orderbook.orderBookAnalysedData100.VolumeWeightedRatio;
            md.HigherBid = orderbook.HigherBid;
            md.LowerAsk = orderbook.LowerAsk;
            md.Spread = orderbook.Spread;

            db.MarketDatas.Add(md);
            db.SaveChanges();
        }
    }
}
