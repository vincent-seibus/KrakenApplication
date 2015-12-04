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
        public Recorder recorder { get; set; }
        public List<TradingData> TradingDatasList { get; set; }
        public List<CurrentOrder> ordersBook { get; set; }
        public List<CurrentOrder> MyOpenedOrders { get; set; }
        public double WeightedStandardDeviation { get; set; }
        public double WeightedAverage { get; set; }
        public double LowerPrice { get; set; }
        public double HigherPrice { get; set; }
        public Balance CurrentBalance { get; set; }
        public double LastPrice { get; set; }
        public double LastMiddleQuote { get; set; }
        public double LastLowerAsk { get; set; }
        public double LastHigherBid { get; set; }
        public double StochasticKIndex { get; set; }
        public double StochasticDIndex { get; set; }
        public double RSIIndex { get; set; }

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

            recorder = rec;
            Multiplicateur = Convert.ToInt16(ConfigurationManager.AppSettings["StandardDeviationMultplicateurStopLoss"]);
            TradingDatasList = new List<TradingData>();
            TradingDatasList = rec.ListOftradingDatasFiltered;
            ordersBook = rec.ListOfCurrentOrder; 
            CurrentBalance = rec.CurrentBalance;
            MyOpenedOrders = rec.OpenedOrders;
            Pair = rec.Pair;
            Task.Run(() => Calculate());
             Task.Run(() => GetCurrentTotalBalance());
        }

        public void Calculate()
        {
            while (true)
            {
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
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
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

        public double GetLastPrice()
        {
            try
            {
                LastPrice = Convert.ToDouble(TradingDatasList.OrderByDescending(a => a.UnixTime).FirstOrDefault().Price, NumberProvider);
                return LastPrice;
            }
            catch(Exception ex)
            {
                return LastPrice;
            }
        }

        public double GetLastMiddleQuote()
        {
            try
            {
                LastLowerAsk = Convert.ToDouble(ordersBook.Where(a => a.OrderType == "ask").OrderBy(a => a.Price).FirstOrDefault().Price);
                LastHigherBid = Convert.ToDouble(ordersBook.Where(a => a.OrderType == "bid").OrderByDescending(a => a.Price).FirstOrDefault().Price);
                LastMiddleQuote = (LastHigherBid + LastLowerAsk) / 2;
                return LastMiddleQuote;
            }
            catch(Exception ex)
            {
                return LastMiddleQuote;
            }
        }

        public void  GetCurrentTotalBalance()
        {
            while (true)
            {
                CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / LastPrice);
                CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * LastPrice);
                Thread.Sleep(30000);
            }
        }
        
        #endregion 

        #region result for player

        /// <summary>
        /// Method of the standard deviation and mobile average
        /// </summary>
        /// <param name="BuyOrSell"> define the type of order by 'buy' or 'sell' </param>
        /// <param name="PercentagePosition"> define the percentage of th current balance to invest through this method </param>
        public void BollingerMethod(string BuyOrSell , double? PercentagePosition = null )
        {
            switch(BuyOrSell)
            {
                case "sell":
                    // set the price to sell
                      PriceToSellProfit = WeightedAverage + WeightedStandardDeviation;
                      PriceToSellStopLoss = 0;
                    // set the volume to sell
                      GetVolumeToSell(PercentagePosition);
                    break;
                case "buy":
                    // set the price to buy
                    PriceToBuyProfit = WeightedAverage - WeightedStandardDeviation;
                    PriceToBuyStopLoss = 0;
                    // set the volume to buy
                    GetVolumeToBuy(PercentagePosition);
                    break;
            }
        }

        /// <summary>
        /// Method to buy and sell the smallest spread , the constraint is the fees
        /// </summary>
        /// <param name="BuyOrSell">define the type of order by 'buy' or 'sell'</param>
        /// <param name="PercentagePosition">define the percentage of th current balance to invest through this method</param>
        public void HighFrequencyMethod(string BuyOrSell, double? PercentagePosition = null)
        {
            switch (BuyOrSell)
            {
                case "sell":
                    // set the price to sell
                    PriceToSellProfit = (MinimalPercentageOfEarning * LastPrice / 2) + LastPrice;
                    PriceToSellStopLoss = LastPrice - (MinimalPercentageOfEarning * LastPrice);
                    // set the volume to sell
                    GetVolumeToSell(PercentagePosition);
                    break;
                case "buy":
                    // set the price to buy
                    PriceToSellProfit = LastPrice - (MinimalPercentageOfEarning * LastPrice / 2);
                    PriceToSellStopLoss = LastPrice + (MinimalPercentageOfEarning * LastPrice);
                    // set the volume to buy
                    GetVolumeToBuy(PercentagePosition);
                    break;
            }
        }



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

        public void GetVolumeToBuy(double? PercentagePosition = null )
        {
            // record balance and price
            recorder.RecordBalance();
            BuyAverageAndStandardDeviation();

            // Get total balance adjusted by price to buy
            CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / PriceToBuyProfit);
            CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * PriceToBuyProfit);

            //Calculate volume to buy

            //Check if percentage not null
            if(PercentagePosition != null)
            {
                // calculate the percentage of the total balance to invest
                VolumeToBuy = CurrentBalance.TotalBTC * (double)PercentagePosition;

                // Check if not superior to the curreny balance of euro
                if(VolumeToBuy > (CurrentBalance.EUR / PriceToBuyProfit))
                {
                    // return current euro balanc if yes
                    VolumeToBuy = CurrentBalance.EUR / PriceToBuyProfit;
                }                
            }
            else
            {
                VolumeToBuy = CurrentBalance.EUR / PriceToBuyProfit;
            }                        
           
        }

        public void GetVolumeToSell(double? PercentagePosition = null)
        {
            recorder.RecordBalance();
            CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / PriceToSellProfit);
            CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * PriceToSellProfit);
            VolumeToSell =  CurrentBalance.BTC;
        }

        // Bool method used to Go-No go decision

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

        public bool OpenedOrdersExist()
        {
            if(MyOpenedOrders.Count == 0)
            {
                return false;
            }

            // check if 2 prices wih stop profit type
            double openPrice;
            if(MyOpenedOrders.First().Price2 == null || MyOpenedOrders.First().Price2 == 0)
            {
                openPrice = MyOpenedOrders.First().Price;
            }
            else
            {
                openPrice = (double)MyOpenedOrders.First().Price2;
            }

            // check if 2 prices with stop profit type
            
            if (MyOpenedOrders.First().Type == "sell" && openPrice <= LastPrice)
            {
                recorder.GetOpenOrders();
                return OpenedOrdersExist();
            }

            if (MyOpenedOrders.First().Type == "buy" && openPrice >= LastPrice)
            {
                recorder.GetOpenOrders();
                return OpenedOrdersExist();
            }

            return true;
        }

        public bool CancelOpenedOrder()
        {
            if(MyOpenedOrders.Count == 0)
            {
                return false;
            }

            if (MyOpenedOrders.First().Type == "sell" && (MyOpenedOrders.First().Price - 4 * WeightedStandardDeviation) > LastMiddleQuote)
            {
                return true;
            }

            if (MyOpenedOrders.First().Type == "buy" && (MyOpenedOrders.First().Price + 4 * WeightedStandardDeviation) < LastMiddleQuote)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Special Function 

        public void GetRSI()
        {
            double H = 0;
            double B = 0;
            RSIIndex = H * 100 / (H - B);
        }

        public void GetStochasticK()
        {
            StochasticKIndex = (LastPrice - LowerPrice) * 100 / (HigherPrice - LowerPrice);
        }

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

        public double MMA(IEnumerable<double> data, double alpha)
        {
            double mma = 0;
            int i = 0;
            foreach (double d in data)
            {
                mma = mma + alpha * Math.Pow(1 - alpha, i) * d;
                i++;
            }

            return mma;
        }

        #endregion
    }
}
