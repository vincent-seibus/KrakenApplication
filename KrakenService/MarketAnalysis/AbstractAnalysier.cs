using CsvHelper;
using KrakenService.KrakenObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KrakenService.MarketAnalysis
{
    public abstract class AbstractAnalysier
    {
        // property to be set by external input 
        public string Pair { get; set; }
        public Recorder recorder { get; set; }

        // property set internally 
        public Balance CurrentBalance { get; set; }
        public List<OpenedOrder> MyOpenedOrders { get; set; }

        // Property to work
        public List<OrderOfBook> OrdersBook { get; set; }
        public List<TradingData> TradingDatasList { get; set; }
        public double WeightedStandardDeviation { get; set; }
        public double WeightedAverage { get; set; }
        public double LowerPrice { get; set; }
        public double HigherPrice { get; set; }
        public double LastPrice { get; set; }
        public double LastMiddleQuote { get; set; }
        public double LastLowerAsk { get; set; }
        public double LastHigherBid { get; set; }
        public double LastLowerBid { get; set; }
        public double LastHigherAsk { get; set; }
        public double StochasticKIndex { get; set; }
        public double StochasticDIndex { get; set; }
        public double RSIIndex { get; set; }
     
        //Property configuration   
        private NumberFormatInfo NumberProvider { get; set; }
        public double PercentageOfFund { get; set; }
        private double Multiplicateur { get; set; }
        public double MinimalPercentageOfEarning { get; set; } // it is in percent 
     
        public AbstractAnalysier(string i_Pair, Recorder rec, double percentageoffund)
        {
            NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ".";

            double Fee = Convert.ToDouble(ConfigurationManager.AppSettings["FeeInPercentage"], NumberProvider);
            double MarginOnFee = Convert.ToDouble(ConfigurationManager.AppSettings["MarginOnFeeInPercentage"], NumberProvider);
            MinimalPercentageOfEarning = (Fee + MarginOnFee) / 100;
            Multiplicateur = Convert.ToDouble(ConfigurationManager.AppSettings["StandardDeviationMultplicateurStopLoss"]);
            PercentageOfFund = percentageoffund;
            Pair = i_Pair;                      
            recorder = rec;

            MyOpenedOrders = new List<OpenedOrder>();           
            CurrentBalance = rec.CurrentBalance;           

            Task.Run(() => GetPropertyToWork()); // Looped every second
            Task.Run(() => GetCurrentTotalBalance());
        }

        //Loop
        public void GetPropertyToWork()
        {
            while (true)
            {
                TradingDatasList = recorder.ListOftradingDatas;
                try
                {
                    GetWeightedAverage();
                    GetWeightedStandardDeviation();
                    GetHigherPrice();
                    GetLowerPrice();
                    GetLastMiddleQuote();
                    GetLastPrice();
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void GetCurrentTotalBalance()
        {
            while (true)
            {
                CurrentBalance = recorder.CurrentBalance;
                CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / LastPrice);
                CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * LastPrice);
                Thread.Sleep(10000);
            }
        }

        #region get standard index

        public double GetWeightedStandardDeviation()
        {            
            try
            {
                double WeighedPriceToAverage = TradingDatasList.ToList().Select(a => Math.Pow((a.Price - WeightedAverage), 2)).Sum();
                double VolumeSum = TradingDatasList.ToList().Select(a => a.Volume).Sum();
                WeightedStandardDeviation = Math.Sqrt(WeighedPriceToAverage / VolumeSum);
                return WeightedStandardDeviation;
            }
            catch (Exception ex)
            {
                return WeightedStandardDeviation;
            }
        }

        public double GetWeightedAverage()
        {
            try
            {
                double WeightedPriceSum = TradingDatasList.ToList().Select(a => a.Price * a.Volume).Sum();
                double VolumeSum = TradingDatasList.ToList().Select(a => a.Volume).Sum();
                WeightedAverage = WeightedPriceSum / VolumeSum;
                return WeightedAverage;
            }
            catch (Exception ex)
            {
                return WeightedAverage;
            }
        }

        public double GetLowerPrice()
        {
            try
            {
                LowerPrice = TradingDatasList.ToList().Select(a => a.Price).DefaultIfEmpty().Min();
            }
            catch (Exception ex)
            {
                return LowerPrice;
            }

            return LowerPrice;
        }

        public double GetHigherPrice()
        {
            try
            {
                HigherPrice = TradingDatasList.Select(a => a.Price).DefaultIfEmpty().Max();
            }
            catch (Exception ex)
            {
                return HigherPrice;
            }

            return HigherPrice;
        }

        public double GetLastPrice()
        {            
            try
            {
                LastPrice = Convert.ToDouble(TradingDatasList.OrderByDescending(a => a.UnixTime).FirstOrDefault().Price, NumberProvider);
                return LastPrice;
            }
            catch (Exception ex)
            {
                return LastPrice;
            }
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
                list.Add(new OrderBookAnalysedData() 
                {   UnixTimestamp = unixTimestamp,
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
                    VolumeRatio = SumVolumeBid / SumVolumeAsk });
                
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
                
        #region helpers

        public string CheckFileAndDirectoryOrderBookAnalysis()
        {
            string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), "OrderBookDataAnalysed" + Pair);
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "OrderBookDataAnalysed_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day);
            if (!File.Exists(pathFile))
            {
                using (var myFile = File.Create(pathFile))
                {
                    // interact with myFile here, it will be disposed automatically
                }
            }

            return pathFile;
        }

        public void intialize()
        {
            recorder.GetOpenOrders();
            if (recorder.OpenedOrders.Count == 0)
            {
                MyOpenedOrders.Clear();
            }
            else
            {
                if (MyOpenedOrders.Count != 0)
                {
                    var OpenedOrders = recorder.OpenedOrders.Select(a => a.OrderID);
                    if (!MyOpenedOrders.Select(a => a.OrderID).Intersect(OpenedOrders).Any())
                    {
                        MyOpenedOrders.Clear();
                    }
                }
            }
        }

        #endregion 
    }
}
