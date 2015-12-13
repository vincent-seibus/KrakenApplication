using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.MarketAnalysis
{
    public class HighFrequencyMethod : AbstractAnalysier, IAnalysier
    {
        
        public double VolumeToBuy { get; set; }
        public double VolumeToSell { get; set; }
        public double PriceToSellProfit { get; set; }
        public double PriceToSellStopLoss { get; set; }
        public double PriceToBuyProfit { get; set; }
        public double PriceToBuyStopLoss { get; set; }
            
        public HighFrequencyMethod(string Pair, Recorder rec, double PercentageOfFund)
            : base(Pair, rec, PercentageOfFund)
            {
                 
                 
            }
        
        public bool Buy()
        {
            //Make sure that the last price is not too high compared to the average price
            double limit = WeightedAverage + 2 * WeightedStandardDeviation - LastPrice * MinimalPercentageOfEarning;

            if (LastPrice < limit)
            {
                GetPriceToBuy();
                GetVolumeToBuy();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Sell()
        {
            // Why you should sell 

            // Because you have BTC 

            // Because the BTC is higher than the average

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
                    if(MyOpenedOrders.First().Price < LastPrice)
                    {
                        if (!recorder.GetOpenOrders())
                            return true;

                        var OpenedOrders = recorder.OpenedOrders.Select(a => a.OrderID);

                        if (MyOpenedOrders.Select(a => a.OrderID).Intersect(OpenedOrders).Any())
                            return true;
                    }
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
                    if (MyOpenedOrders.First().Price > LastPrice)
                    {
                        if (!recorder.GetOpenOrders())
                            return true;

                        var OpenedOrders = recorder.OpenedOrders.Select(a => a.OrderID);

                        if (MyOpenedOrders.Select(a => a.OrderID).Intersect(OpenedOrders).Any())
                            return true;
                    }
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
            double limit = WeightedAverage - WeightedStandardDeviation - MinimalPercentageOfEarning * LastPrice;

            if (LastPrice < limit)
            {
                return true;
            }

            return false;
        }

        public bool CancelBuying()
        {
            double limit = WeightedAverage + WeightedStandardDeviation + MinimalPercentageOfEarning * LastPrice;

            if (LastPrice > limit)
            {
                return true;
            }

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
            if (PercentageOfFund != null && PercentageOfFund != 0 )
            {
                // calculate the percentage of the total balance to invest
                VolumeToSell = CurrentBalance.TotalBTC * (double)PercentageOfFund;

                // Check if not superior to the curreny balance of euro
                if (VolumeToSell  > CurrentBalance.BTC)
                {
                    // return current euro balance if yes
                    VolumeToBuy = CurrentBalance.BTC;
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
            PriceToBuyProfit = LastPrice - (MinimalPercentageOfEarning * LastPrice );
            PriceToBuyStopLoss = 0;

            return PriceToBuyProfit;
        }

        public double GetPriceToSell()
        {
            PriceToSellProfit = (MinimalPercentageOfEarning * LastPrice) + LastPrice;
            PriceToSellStopLoss = 0;

            return PriceToSellProfit;
        }
    }
}
