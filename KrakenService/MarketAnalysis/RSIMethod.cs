using CsvHelper;
using KrakenService.KrakenObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.MarketAnalysis
{
    public class RSIMethod : AbstractAnalysier, IAnalysier
    {
        #region property interface

        public double VolumeToBuy
        {
            get ; set;        
        }

        public double VolumeToSell
        {
            get;
            set;      
        }

        public double PriceToSellProfit
        {
            get;
            set;      
        }

        public double PriceToSellStopLoss
        {
            get;
            set;      
        }

        public double PriceToBuyProfit
        {
            get;
            set;      
        }

        #endregion 

        public double IndexRSI {get;set;}

        public RSIMethod(string Pair, Recorder rec, double PercentageOfFund)
            : base(Pair, rec, PercentageOfFund)
            {
                 
                 
            }
        
        #region Method interface

        public bool Buy()
        {
            throw new NotImplementedException();
        }

        public bool Sell()
        {
            throw new NotImplementedException();
        }

        public bool Buying()
        {
            throw new NotImplementedException();
        }

        public bool Selling()
        {
            throw new NotImplementedException();
        }

        public bool CancelSelling()
        {
            throw new NotImplementedException();
        }

        public bool CancelBuying()
        {
            throw new NotImplementedException();
        }

        public void CancelledSelling()
        {
            throw new NotImplementedException();
        }

        public void CancelledBuying()
        {
            throw new NotImplementedException();
        }

        public double GetVolumeToBuy()
        {
            throw new NotImplementedException();
        }

        public double GetVolumeToSell()
        {
            throw new NotImplementedException();
        }

        public double GetPriceToBuy()
        {
            throw new NotImplementedException();
        }

        public double GetPriceToSell()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region method specific RSI
        
        public void InitializeRSI()
        {
            CalculateIndexRSI(recorder.GetLastRowsOHLCDataRecorded(24,30));
        }

        public double CalculateIndexRSI(List<OHLCData> listdata)
        {
            double rsi = 0;
            double H = 0;
            double B = 0;
            double alpha = 0.1;
            H = listdata.OrderByDescending(a => a.time).Select(a => a.close - a.open).Where(a => a > 0).Aggregate((ema, nextQuote) => alpha * nextQuote + (1 - alpha) * ema);
            B = Math.Abs(listdata.OrderByDescending(a => a.time).Select(a => a.close - a.open).Where(a => a < 0).Aggregate((ema, nextQuote) => alpha * nextQuote + (1 - alpha) * ema));

            rsi = 100 - (100 / (1 + (H / B)));
            IndexRSI = rsi;
            return rsi;
        }

        public double GetLastMiddleQuote()
        {
            OrdersBook = recorder.ListOfCurrentOrder;
            try
            {
                LastLowerAsk = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "ask").OrderBy(a => a.Price).FirstOrDefault().Price);
                LastHigherAsk = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "ask").OrderByDescending(a => a.Price).FirstOrDefault().Price);
                LastHigherBid = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "bid").OrderByDescending(a => a.Price).FirstOrDefault().Price);
                LastLowerBid = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "bid").OrderBy(a => a.Price).FirstOrDefault().Price);
                double SumVolumeBid = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "bid").Sum(a => a.Volume));
                double SumVolumeAsk = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "ask").Sum(a => a.Volume));
                double BidDepth = LastHigherBid - LastLowerBid;
                double AskDepth = LastHigherAsk - LastLowerAsk;
                LastMiddleQuote = (LastHigherBid + LastLowerAsk) / 2;
                double BidDepthPercentage = BidDepth / LastMiddleQuote;
                double AskDepthPercentage = AskDepth / LastMiddleQuote;

                //record the Order book analysied data
                string filepath = CheckFileAndDirectoryOrderBookAnalysis();
                List<OrderBookAnalysedData> list = new List<OrderBookAnalysedData>();
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                orderBookAnalysedData = new OrderBookAnalysedData()
                {
                    UnixTimestamp = unixTimestamp,
                    Timestamp = DateTime.UtcNow,
                    LowerAsk = LastLowerAsk,
                    LowerBid = LastLowerBid,
                    HigherAsk = LastHigherAsk,
                    HigherBid = LastHigherBid,
                    AskDepth = AskDepth,
                    AskVolume = SumVolumeAsk,
                    BidDepth = BidDepth,
                    BidVolume = SumVolumeBid,
                    DepthRatio = BidDepth / AskDepth,
                    VolumeRatio = SumVolumeBid / SumVolumeAsk
                };
                list.Add(orderBookAnalysedData);

                using (StreamWriter writer = File.AppendText(filepath))
                {
                    var csv = new CsvWriter(writer);
                    foreach (var item in list)
                    {
                        csv.WriteRecord(item);
                    }
                }

                return LastMiddleQuote;
            }
            catch (Exception ex)
            {
                return LastMiddleQuote;
            }
        }

        #endregion

    }
}
