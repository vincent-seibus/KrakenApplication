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
        public double LastPrice { get; set; }      
      
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

        #endregion 
   
        #region helpers

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
