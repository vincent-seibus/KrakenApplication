using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.MarketAnalysis
{
    public class BollingerMethod : AbstractAnalysier, IAnalysier
    {
        public double VolumeToBuy { get; set; }
        public double VolumeToSell { get; set; }
        public double PriceToSellProfit { get; set; }
        public double PriceToSellStopLoss { get; set; }
        public double PriceToBuyProfit { get; set; }
        public double PriceToBuyStopLoss { get; set; }

        #region spécific

        public double WeightedAverage1440min { get; set; }
        public double WeightedStandardDeviation1440min { get; set; }
        public double WeightedAverage240min { get; set; }
        public double WeightedStandardDeviation240min { get; set; }
        public double WeightedAverage60min { get; set; }
        public double WeightedStandardDeviation60min { get; set; }

        #endregion 


        public BollingerMethod(string Pair, Recorder rec, double PercentageOfFund)
            : base(Pair, rec, PercentageOfFund)
            {
                 
                 
            }
        
        public bool Buy()
        {            
            if (LastPrice > (WeightedAverage - 2 * WeightedStandardDeviation))
            {
                return false;
            }

            GetPriceToBuy();
            GetVolumeToBuy();
            return true;        
        }

        public bool Sell()
        {
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
                    if (MyOpenedOrders.First().Price > LastPrice)
                    {
                        if (!recorder.GetOpenOrders())
                            return true;

                        var OpenedOrders = recorder.OpenedOrders.Select(a => a.OrderID);

                        if (MyOpenedOrders.Select(a => a.OrderID).Intersect(OpenedOrders).Any())
                            return true;

                        return false;
                    }

                    return true;
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
                    if (MyOpenedOrders.First().Price < LastPrice)
                    {
                        if (!recorder.GetOpenOrders())
                            return true;

                        var OpenedOrders = recorder.OpenedOrders.Select(a => a.OrderID);

                        if (MyOpenedOrders.Select(a => a.OrderID).Intersect(OpenedOrders).Any())
                            return true;

                        return false;
                    }

                    return true;
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
            PriceToBuyProfit = 0;
            PriceToBuyStopLoss = 0;

            return PriceToBuyProfit;
        }

        public double GetPriceToSell()
        {
            PriceToSellProfit = WeightedAverage + WeightedStandardDeviation;
            PriceToSellStopLoss = 0;

            return PriceToSellProfit;
        }

    }
}
