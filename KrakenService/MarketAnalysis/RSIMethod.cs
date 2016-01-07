using CsvHelper;
using KrakenService.KrakenObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        #region property config

        public int Period { get; set; }
        public int Nperiod { get; set; }
        public bool RSIPlay { get; set; } 

        #endregion

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
            throw new NotImplementedException();
        }

        public double GetPriceToSell()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region method specific RSI
        
        public void InitializeRSI(int period, int nperiod)
        {
            Nperiod = nperiod;
            Period = period;
            RSIPlay = true;
            Task.Run(() => GetRSILoop(period,nperiod));     
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

        public void GetRSILoop(int period, int nperiod)
        {
            while (RSIPlay)
            {
                try
                {
                    switch (period)
                    {
                        case 30:
                            var list30 = recorder.ListOfOHLCData30.OrderByDescending(a => a.time).Take(nperiod).ToList();
                            CalculateIndexRSI(list30);
                            break;
                        case 60:
                            var list60 = recorder.ListOfOHLCData60.OrderByDescending(a => a.time).Take(nperiod).ToList();
                            CalculateIndexRSI(list60);
                            break;
                        case 1440:
                            var list1440 = recorder.ListOfOHLCData1440.OrderByDescending(a => a.time).Take(nperiod).ToList();
                            CalculateIndexRSI(list1440);
                            break;
                    }
                }
                catch(Exception ex)
                {
                    // ADD LOGGER
                }

                Thread.Sleep(3000);
            }
        }
      
        #endregion

    }
}
