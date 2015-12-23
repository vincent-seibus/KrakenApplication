﻿using CsvHelper;
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

        #endregion

        public OrderBookAnalysisMethod(string Pair, Recorder rec, double PercentageOfFund)
            : base(Pair, rec, PercentageOfFund)
            {
                                  
            }

        #region Method Interface
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
           
        }

        public void CancelledBuying()
        {
            
        }

        public double GetVolumeToBuy()
        {
            // record balance and price
            recorder.RecordBalance();
            CurrentBalance = recorder.CurrentBalance;

            // Get total balance adjusted by price to buy
            CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / PriceToBuyProfit);
            CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * PriceToBuyProfit);

            //Calculate volume to buy

            //Check if percentage not null
            if (PercentageOfFund != null && PercentageOfFund != 0)
            {
                // calculate the percentage of the total balance to invest
                VolumeToBuy = CurrentBalance.TotalBTC * (double)PercentageOfFund;

                // Check if not superior to the curreny balance of euro
                if (VolumeToBuy > (CurrentBalance.EUR / PriceToBuyProfit))
                {
                    // return current euro balance if yes
                    VolumeToBuy = CurrentBalance.EUR / PriceToBuyProfit;
                }
            }
            else
            {
                VolumeToBuy = CurrentBalance.EUR / PriceToBuyProfit;
            }

            return VolumeToBuy;
        }

        public double GetVolumeToSell()
        {
            // record balance and price
            recorder.RecordBalance();
            CurrentBalance = recorder.CurrentBalance;

            // Get total balance adjusted by price to buy
            CurrentBalance.TotalBTC = CurrentBalance.BTC + (CurrentBalance.EUR / PriceToSellProfit);
            CurrentBalance.TotalEUR = CurrentBalance.EUR + (CurrentBalance.BTC * PriceToSellProfit);

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

        public double GetPriceToBuy()
        {
            PriceToBuyProfit = LastMiddleQuote;
            PriceToBuyStopLoss = 0;

            return PriceToBuyProfit;
        }

        public double GetPriceToSell()
        {
            PriceToSellProfit = LastMiddleQuote;
            PriceToSellStopLoss = 0;

            return PriceToSellProfit;
        }
        #endregion

        #region method specific OrderBookAnalysed

        public void InitializeOrderBook()
        {
              DbOrderBook = new MySqlIdentityDbContext();
              OrderBookIndexesPlay = true;
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
                    // ADD LOGGER
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
                    VolumeRatio = SumVolumeBid / SumVolumeAsk
                };
                list.Add(orderBookAnalysedData);
                
                //register in mysql database              
                DbOrderBook.OrderBookDatas.AddRange(list);
                DbOrderBook.SaveChanges();                

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
