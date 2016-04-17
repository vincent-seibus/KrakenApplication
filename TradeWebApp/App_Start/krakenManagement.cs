using KrakenService;
using KrakenService.Helpers;
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
        // Logger 

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // property config
        public static double FundPercentage { get; set; }

        // Property context
        public static int InitializeTime { get; set; }
        public static bool IsPlaying { get; set; }
        public static bool IsStopping { get; set; }
        public static bool IsStopped { get; set; }
        public static bool IsPaused { get; set; }
        public static bool IsStarted { get; set; }


        public static void Initialize()
        {

            log.Info("Kraken management initialize ....");

            // The pair we will work on
            string pair = "XXBTZEUR";
            IsPlaying = false;
            IsStopping = false;
            IsStopped = false;
            IsStarted = false;
            IsPaused = false;

            SendingRateManager SRM = new SendingRateManager();
            KrakenService.Recorder rec1 = new KrakenService.Recorder(pair, SRM);

            //HighFrequencyMethod ana1 = new HighFrequencyMethod(pair,rec1,0.6);

            RSIMethod rsi30min48period = new RSIMethod(pair, rec1, 0);
            rsi30min48period.InitializeRSI(30, 48);
            RSIMethod rsi60min48period = new RSIMethod(pair, rec1, 0);
            rsi60min48period.InitializeRSI(60, 48);
            RSIMethod rsi1440min14period = new RSIMethod(pair, rec1, 0);
            rsi1440min14period.InitializeRSI(1440, 14);

            if (FundPercentage == null || FundPercentage == 0)
                FundPercentage = 0.6;

            OrderBookAnalysisMethod orderAna1 = new OrderBookAnalysisMethod(pair, rec1, FundPercentage);
            orderAna1.InitializeOrderBook();

            NewPlayer play1 = new NewPlayer(orderAna1, pair, SRM, LimitOrMarket.market);
            orderAna1.intialize();

            recorder = rec1;
            orderbook = orderAna1;
            player = play1;

            MarketDataRecorder marketRecorder = new MarketDataRecorder(orderbook, rsi30min48period, rsi60min48period, rsi1440min14period);

            InitializeTime = 0;
            while (InitializeTime < 40)
            {
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Thread.Sleep(1000);
                InitializeTime++;
            }

            Task.Run(() => PlayerLoop(play1));
        }             

        public static void Start()
        {
            if(IsStopped)
            {
                IsStopped = false;
                IsStopping = false;
                Task.Run(() => PlayerLoop(player));
            }

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
                try
                {

                    if (IsPlaying)
                    {
                        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        player.Play();
                        IsStarted = true;
                        IsPaused = false;
                    }
                    else
                    {
                        IsStarted = false;
                        IsPaused = true;
                    }
                    
                    dashboard.LastMiddleQuote = orderbook.LastMiddleQuote;
                    dashboard.LastPrice = orderbook.LastPrice;
                    dashboard.VolumeWeightedRatio = orderbook.orderBookAnalysedData.VolumeWeightedRatio ?? 0.0;
                    dashboard.VolumeWeightedRatio100 = orderbook.orderBookAnalysedData100.VolumeWeightedRatio ?? 0.0;
                    dashboard.PlayerState = player.playerState;
                    dashboard.BalanceBtc = orderbook.CurrentBalance.BTC;
                    dashboard.BalanceEuro = orderbook.CurrentBalance.EUR;
                    dashboard.IsPlaying = IsPlaying;
                    dashboard.IsStopping = IsStopping;
                    dashboard.EMA = orderbook.orderBookAnalysedData.EMA;
                    dashboard.EMA100 = orderbook.orderBookAnalysedData100.EMA;
                    HttpRuntime.Cache.Add("Dashboard", dashboard, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 1, 0), CacheItemPriority.Normal, null);

                                   
                }
                catch(Exception ex)
                {
                    log.Error(" krakenManagement.PlayerLoop.While - " + ex.Message + " - " + ex.InnerException + " - " + ex.StackTrace );
                }

                Thread.Sleep(2000);
            }

            IsStopped = true;

        }

        public static object ChangePlayerState(int PlayerStateId)
        {
            try
            {
                PlayerState playerstate = (PlayerState)PlayerStateId;
                player.playerState = playerstate;
                return new { error = "" ,  player.playerState };
            }
            catch(Exception ex)
            {
                return new { error = ex.Message, PlayerState = player.playerState };
            }
            
        }

        public static void ChangeFundPercentage(double value)
        {
            FundPercentage = value;
            Stop();
            while(!IsStopped)
            {
                Stop();
                Thread.Sleep(100);
            }

            Task.Run(() => PlayerLoop(player));
        }

        #region Read variable

        public static KrakenService.Recorder recorder { get; set; }
        public static NewPlayer player { get; set; }
        public static OrderBookAnalysisMethod orderbook { get; set; }

        #endregion

    }
}