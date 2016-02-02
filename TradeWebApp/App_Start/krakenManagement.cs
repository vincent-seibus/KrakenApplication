using KrakenService;
using KrakenService.KrakenObjects;
using KrakenService.MarketAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using TradeWebApp.Models;

namespace TradeWebApp
{
    public static class krakenManagement
    {
        public static void Initialize()
        {
            // The pair we will work on
            string pair = "XXBTZEUR";
            IsPlaying = false;
            IsStopping = false;

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

            recorder = rec1;
            orderbook = orderAna1;
            player = play1;

            int i = 0;
            while (i < 40)
            {
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Thread.Sleep(1000);              
                i++;
            }

            Task.Run(() => PlayerLoop(play1));
        }

        public static bool IsPlaying { get; set; }
        public static bool IsStopping { get; set; }

        public static void Start()
        {
            IsPlaying = true;
        }

        public static void Pause()
        {
            IsPlaying = false;
        }

        public static void Stop()
        {
            IsStopping = true;
        }

        public static void PlayerLoop(NewPlayer player)
        {
            Dashboard dashboard = new Dashboard();           
             while (!IsStopping)
            {
                if (IsPlaying)
                {
                    Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    player.Play();
                }
                
                
                 dashboard.LastMiddleQuote = orderbook.LastMiddleQuote ;
                dashboard.LastPrice = orderbook.LastPrice ;
                dashboard.VolumeWeightedRatio = orderbook.orderBookAnalysedData.VolumeWeightedRatio ?? 0.0;
                dashboard.PlayerState = player.playerState;
                HttpRuntime.Cache.Add("Dashboard", dashboard, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 1, 0), CacheItemPriority.Normal, null);               
                Thread.Sleep(2000);
            }
        }

        #region Read variable

        public static KrakenService.Recorder recorder { get; set; }
        public static NewPlayer player { get; set; }
        public static OrderBookAnalysisMethod orderbook { get; set; }

        #endregion

    }
}