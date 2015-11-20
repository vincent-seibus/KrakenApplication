using KrakenService.KrakenObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KrakenService
{
    public class Analysier
    {
        // Datas and middle result
        public List<TradingData> TradingDatasList { get; set; }
        public double WeightedStandardDeviation { get; set; }
        public double WeightedAverage { get; set; }
        public double LowerPrice { get; set; }
        public double HigherPrice { get; set; }
        public Balance CurrentBalance { get; set; }
        

        //Config property
        public string Pair { get; set; }
        public int Multiplicateur { get; set; }
        public double Fee { get; set; }
        private NumberFormatInfo NumberProvider { get; set; }
        public double MinimalPercentageOfEarning { get; set; } // it is in percent 
        public double MarginOnFee { get; set; } // it is in percent 

        // result to deliver to player
        public double PriceToSellProfit { get; set; }
        public double PriceToSellStopLoss { get; set; }
        public double PriceToBuyProfit { get; set; }
        public double PriceToBuyStopLoss { get; set; }
        public double VolumeToBuy { get; set; }
        public double VolumeToSell { get; set; }
        public double PotentialPercentageOfEarning { get; set; } // it is in percent 

        public Analysier(Recorder rec)
        {
             NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ".";

            Fee = Convert.ToDouble(ConfigurationManager.AppSettings["FeeInPercentage"], NumberProvider);
            MarginOnFee = Convert.ToDouble(ConfigurationManager.AppSettings["MarginOnFeeInPercentage"], NumberProvider);

            Multiplicateur = Convert.ToInt16(ConfigurationManager.AppSettings["StandardDeviationMultplicateurStopLoss"]);
            TradingDatasList = new List<TradingData>();
            TradingDatasList = rec.ListOftradingDatas;
            CurrentBalance = rec.CurrentBalance;
            Pair = rec.Pair;
            Task.Run(() => Calculate());
        }

        public void Calculate()
        {
            while (true)
            {
                GetWeightedAverage();
                GetWeightedStandardDeviation();
                GetHigherPrice();
                GetLowerPrice();                
                Thread.Sleep(1000);
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
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
            {
                return HigherPrice;
            }
           
            return HigherPrice;
        }

        
        #endregion 

        #region result for player

        public void SellAverageAndStandardDeviation()
        {
            PriceToSellProfit = WeightedAverage + WeightedStandardDeviation;

            PriceToSellStopLoss = WeightedAverage - Multiplicateur * WeightedStandardDeviation;
        }

        public void BuyAverageAndStandardDeviation()
        {
            PriceToBuyProfit = WeightedAverage - WeightedStandardDeviation;

            PriceToBuyStopLoss = WeightedAverage + Multiplicateur * WeightedStandardDeviation;
        }

        public void GetVolumeToBuy()
        {            
            VolumeToBuy = CurrentBalance.EUR / PriceToBuyStopLoss;
        }

        public void GetVolumeToSell()
        {
            VolumeToSell =  CurrentBalance.BTC;
        }

        public bool SellorBuy()
        {
            PotentialPercentageOfEarning = WeightedStandardDeviation * 2 * 100 / WeightedAverage; // it is in percent
            MinimalPercentageOfEarning = Fee + MarginOnFee;

            if (PotentialPercentageOfEarning > MinimalPercentageOfEarning)
            {
                return true;
            }

            return false;
        }

        #endregion

        public double GaussFunction(double price)
        {
            double result;
            result = 0.5 * Math.Pow(((price - WeightedAverage) / WeightedStandardDeviation),2);
            result = Math.Exp(-result) / (WeightedStandardDeviation*Math.Sqrt(2* Math.PI));
            return result;
        }

        /// <summary>
        /// This function allow to find the extreme of the function by resolving f'(x) = 0
        /// </summary>
        /// <param name="price"></param>
        /// <param name="priceTocompare"></param>
        /// <returns></returns>
        public double DerivativeOfGaussFunctionByPrice(double price, double priceTocompare)
        {
            return 0;
        }


    }
}
