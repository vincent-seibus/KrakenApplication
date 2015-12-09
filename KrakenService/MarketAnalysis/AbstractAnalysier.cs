using KrakenService.KrakenObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
        public double StochasticKIndex { get; set; }
        public double StochasticDIndex { get; set; }
        public double RSIIndex { get; set; }
     
        //Property configuration   
        private NumberFormatInfo NumberProvider { get; set; }
        public double PercentageOfFund { get; set; }
        private double Multiplicateur { get; set; }
        public double MinimalPercentageOfEarning { get; set; } // it is in percent 
     
        public AbstractAnalysier(string Pair, Recorder rec, double percentageoffund)
        {
            NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ".";

            double Fee = Convert.ToDouble(ConfigurationManager.AppSettings["FeeInPercentage"], NumberProvider);
            double MarginOnFee = Convert.ToDouble(ConfigurationManager.AppSettings["MarginOnFeeInPercentage"], NumberProvider);
            Multiplicateur = Convert.ToDouble(ConfigurationManager.AppSettings["StandardDeviationMultplicateurStopLoss"]);
            MinimalPercentageOfEarning = (Fee + MarginOnFee) / 100;
            recorder = rec;
            PercentageOfFund = percentageoffund;
            Pair = rec.Pair;
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
                    Thread.Sleep(1000);
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
                LastHigherBid = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "bid").OrderByDescending(a => a.Price).FirstOrDefault().Price);
                LastMiddleQuote = (LastHigherBid + LastLowerAsk) / 2;
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
