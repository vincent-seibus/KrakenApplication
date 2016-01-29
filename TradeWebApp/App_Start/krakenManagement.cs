using KrakenService;
using KrakenService.KrakenObjects;
using KrakenService.MarketAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace TradeWebApp
{
    public static class krakenManagement
    {
        public static void Initialize()
        {
            // The pair we will work on
            string pair = "XXBTZEUR";

            SendingRateManager SRM = new SendingRateManager();
            KrakenService.Recorder rec1 = new KrakenService.Recorder(pair, SRM);

            //HighFrequencyMethod ana1 = new HighFrequencyMethod(pair,rec1,0.6);

            RSIMethod rsi30min48period = new RSIMethod(pair, rec1, 0);
            rsi30min48period.InitializeRSI(30, 48);
            RSIMethod rsi60min48period = new RSIMethod(pair, rec1, 0);
            rsi30min48period.InitializeRSI(60, 48);
            RSIMethod rsi1440min14period = new RSIMethod(pair, rec1, 0);
            rsi30min48period.InitializeRSI(1440, 14);

            OrderBookAnalysisMethod orderAna1 = new OrderBookAnalysisMethod(pair, rec1, 0.2);
            orderAna1.InitializeOrderBook();

            NewPlayer play1 = new NewPlayer(orderAna1, pair, SRM, LimitOrMarket.market);
            orderAna1.intialize();

            int i = 0;
            while (i < 40)
            {

                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Last Middle Quote:" + orderAna1.LastMiddleQuote);
                Console.WriteLine("Last Trade price:" + orderAna1.LastPrice);
                Console.WriteLine("BTC : " + rec1.CurrentBalance.BTC + "; EURO : " + rec1.CurrentBalance.EUR);
                Console.WriteLine("Player state :" + play1.playerState + " ; minimal earning required: " + orderAna1.MinimalPercentageOfEarning);
                i++;
            }
            Console.WriteLine("--------------------------------------");
            Console.ReadKey();

            while (true)
            {
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                play1.Play();
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Player Status :" + play1.playerState);
                Console.WriteLine("Last Middle Quote:" + orderAna1.LastMiddleQuote);
                Console.WriteLine("Last Trade price:" + orderAna1.LastPrice);
                Console.WriteLine("Opened Order:");
                Console.WriteLine("VolumeWeightedRatio : " + orderAna1.orderBookAnalysedData.VolumeWeightedRatio);
                Console.WriteLine("BTC : " + rec1.CurrentBalance.BTC + "; EURO : " + rec1.CurrentBalance.EUR);
                Console.WriteLine("minimal earning required: " + orderAna1.MinimalPercentageOfEarning);
                Thread.Sleep(2000);
            }       
        }
    }
}