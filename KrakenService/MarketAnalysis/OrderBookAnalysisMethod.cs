using CsvHelper;
using KrakenService.Data;
using KrakenService.KrakenObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KrakenService.MarketAnalysis
{
    public class OrderBookAnalysisMethod : AbstractAnalysier, IAnalysier
    {
        #region Property Interface 
        public double VolumeToBuy { get; set; }
        public double VolumeToSell { get; set; }
        public double PriceToSellProfit { get; set; }
        public double PriceToSellStopLoss { get; set; }
        public double PriceToBuyProfit { get; set; }
        public double PriceToBuyStopLoss { get; set; }
        #endregion

        #region Property Specific
        public bool OrderBookIndexesPlay { get; set; }
        public OrderBookAnalysedData orderBookAnalysedData { get; set; }
        public double LastMiddleQuote { get; set; }
        public MySqlIdentityDbContext DbOrderBook { get; set; }
        public double VolumeWeightedRatioTresholdToBuy { get; set; }
        public double VolumeWeightedRatioTresholdToSell { get; set; }
        #endregion

        public OrderBookAnalysisMethod(string Pair, Recorder rec, double PercentageOfFund)
            : base(Pair, rec, PercentageOfFund)
            {
                                  
            }

        #region Method Interface
        public bool Buy()
        {
            try
            {
                DbOrderBook = new MySqlIdentityDbContext();
                orderBookAnalysedData = DbOrderBook.OrderBookDatas.OrderByDescending(a => a.UnixTimestamp).FirstOrDefault();
                VolumeWeightedRatioTresholdToBuy = 1.3;
                if (orderBookAnalysedData.VolumeWeightedRatio < VolumeWeightedRatioTresholdToBuy)
                {

                    return false;
                }

                GetPriceToBuy();
                GetVolumeToBuy();
                return true;
            }
            catch (Exception ex)
            {
                // Logger 
                Console.WriteLine("error buy() : " + ex.Message);
                return false;
            }
        }

        public bool Sell()
        {
            VolumeWeightedRatioTresholdToSell = 1.1;
            if (orderBookAnalysedData.VolumeWeightedRatio > VolumeWeightedRatioTresholdToSell)
            {
                return false;
            }

            
            GetPriceToSell();
            GetVolumeToSell();
            return true;
        }

        public bool Buying()
        {
            try
            {
                if (MyOpenedOrders.Count != 0)
                {
                        if (!recorder.GetOpenOrders())
                            return true;

                        var OpenedOrders = recorder.OpenedOrders.Select(a => a.OrderID);

                        if (MyOpenedOrders.Select(a => a.OrderID).Intersect(OpenedOrders).Any())
                            return true;

                        CurrentBalance = recorder.CurrentBalance;           

                        return false;             
                }

                return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(typeof(HighFrequencyMethod).ToString() + ".Buying : " + ex.Message);
                return true;
            }
        }

        public bool Selling()
        {
            try
            {
                if (MyOpenedOrders.Count != 0)
                {
                   
                        if (!recorder.GetOpenOrders())
                            return true;

                        var OpenedOrders = recorder.OpenedOrders.Select(a => a.OrderID);

                        if (MyOpenedOrders.Select(a => a.OrderID).Intersect(OpenedOrders).Any())
                            return true;

                        CurrentBalance = recorder.CurrentBalance;      

                        return false;                   
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(typeof(HighFrequencyMethod).ToString() + ".Selling : " + ex.Message);
                return true;
            }
        }

        public bool CancelSelling()
        {
            return false;
        }

        public bool CancelBuying()
        {
            return false;
        }

        public void CancelledSelling()
        {
            
        }

        public void CancelledBuying()
        {
            
        }
        /// <summary>
        /// Set the volume of bitcoin to buy in function of the current balance, the percentage of fund allocate dto this method and the specific rules taht qpply for this method
        /// </summary>
        /// <returns>Volume of bitcoin to buy</returns>
        public double GetVolumeToBuy()
        {
            // record balance and price
            recorder.RecordBalance();
            CurrentBalance = recorder.CurrentBalance;

            // Get total balance adjusted by price to buy
            CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / LastMiddleQuote);
            CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * LastMiddleQuote);

            //Calculate volume to buy

            //Check if percentage not null
            if (PercentageOfFund != null && PercentageOfFund != 0)
            {
                // calculate the percentage of the total balance to invest
                VolumeToBuy = CurrentBalance.TotalBTC * (double)PercentageOfFund;

                // Check if not superior to the curreny balance of euro
                if (VolumeToBuy > (CurrentBalance.EUR / LastMiddleQuote))
                {
                    // return current euro balance if yes
                    VolumeToBuy = CurrentBalance.EUR / LastMiddleQuote;
                }
            }
            else
            {
                VolumeToBuy = CurrentBalance.EUR / LastMiddleQuote;
            }
            


            return VolumeToBuy;
        }
        /// <summary>
        /// Set the volume of bitcoin to sell in function of the current balance, the percentage of fund allocate dto this method and the specific rules taht qpply for this method
        /// </summary>
        /// <returns>Volume of bitcoin to sell</returns>
        public double GetVolumeToSell()
        {
            // record balance and price
            recorder.RecordBalance();
            CurrentBalance = recorder.CurrentBalance;

            // Get total balance adjusted by price to buy
            CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / LastMiddleQuote);
            CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * LastMiddleQuote);

            //Calculate volume to sell

            //Check if percentage not null
            if (PercentageOfFund != null && PercentageOfFund != 0)
            {
                // calculate the percentage of the total balance to invest
                VolumeToSell = CurrentBalance.TotalBTC * (double)PercentageOfFund;

                // Check if not superior to the curreny balance of euro
                if (VolumeToSell > CurrentBalance.BTC)
                {
                    // return current euro balance if yes
                    VolumeToSell = CurrentBalance.BTC;
                }
            }
            else
            {
                VolumeToBuy = CurrentBalance.BTC;
            }

            return VolumeToBuy;
        }
        /// <summary>
        /// No price set as buy and sell at market , 
        /// </summary>
        /// <returns> Always 0</returns>
        public double GetPriceToBuy()
        {
            PriceToBuyProfit = 0;
            return PriceToBuyProfit;
        }
        /// <summary>
        ///  No price set as buy and sell at market , 
        /// </summary>
        /// <returns></returns>
        public double GetPriceToSell()
        {
            PriceToSellProfit = 0;
            return PriceToSellProfit;
        }
        #endregion

        #region method specific OrderBookAnalysed

        /// <summary>
        /// Initialisation of the order book analysis. As to be initialise before anything else
        /// </summary> 
        public void InitializeOrderBook()
        {
              DbOrderBook = new MySqlIdentityDbContext();
              OrderBookIndexesPlay = true;
              VolumeWeightedRatioTresholdToBuy = 1.5;
              VolumeWeightedRatioTresholdToSell = 1.4;
              Task.Run(() => GetOrderBookIndexesLoop());
        }

        public void GetOrderBookIndexesLoop()
        {
            while(OrderBookIndexesPlay)
            {
                try
                {
                    CalculateOrderBookIndexes();                   
                } 
                catch(Exception ex)
                {
                    Console.WriteLine("Error : GetOrderBookIndexesLoop method");
                }

                Thread.Sleep(5000);
            }
        }

        public double CalculateOrderBookIndexes()
        {
            OrdersBook = recorder.ListOfCurrentOrder;
            try
            {
                double LastLowerAsk = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "ask").OrderBy(a => a.Price).FirstOrDefault().Price);
                double LastHigherAsk = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "ask").OrderByDescending(a => a.Price).FirstOrDefault().Price);
                double LastHigherBid = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "bid").OrderByDescending(a => a.Price).FirstOrDefault().Price);
                double LastLowerBid = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "bid").OrderBy(a => a.Price).FirstOrDefault().Price);
                double SumVolumeBid = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "bid").Sum(a => a.Volume));
                double SumVolumeAsk = Convert.ToDouble(OrdersBook.Where(a => a.OrderType == "ask").Sum(a => a.Volume));
                double BidDepth = LastHigherBid - LastLowerBid;
                double AskDepth = LastHigherAsk - LastLowerAsk;
                LastMiddleQuote = (LastHigherBid + LastLowerAsk) / 2;
                double BidDepthPercentage = BidDepth / LastMiddleQuote;
                double AskDepthPercentage = AskDepth / LastMiddleQuote;
                double VolumeWeightedRatio = (SumVolumeBid / BidDepth) / (SumVolumeAsk / AskDepth);


                //record the Order book analysied data

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
                    VolumeRatio = SumVolumeBid / SumVolumeAsk,
                    VolumeWeightedRatio = VolumeWeightedRatio
                };
                list.Add(orderBookAnalysedData);
                
                //register in mysql database              
                DbOrderBook.OrderBookDatas.AddRange(list);
                DbOrderBook.SaveChanges();                

                return LastMiddleQuote;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : Calculate orderbookindexes method");
                return LastMiddleQuote;
            }
        }
        
        #endregion
    }
}
