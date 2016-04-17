using KrakenService.Data;
using KrakenService.Helpers;
using KrakenService.KrakenObjects.DataModels;
using KrakenService.MarketAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService
{
    public class PlayerV2
    {
        //Logger
        private static readonly log4net.ILog log =
           log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Contexte
        public PlayerStateV2 playerState { get; set; }
        public Position position { get; set; }
        //Database
        public MySqlIdentityDbContext db { get; set; }
        public MarketData marketData { get; set; }
        
        public PlayerV2(double EMA)
        {
            db = new MySqlIdentityDbContext();
            Start();
        }

        public void Start()
        {
            playerState = PlayerStateV2.Started;                        
            Task.Run(() => Running());                      
        }

        private void Running()
        {
            while (playerState != PlayerStateV2.Stopped)
            { 
                if (playerState == PlayerStateV2.Playing)
                {
                    try
                    {
                        WhatToDo();
                    }
                    catch(Exception ex)
                    {
                       log.Error(" PlayerV2.Running - " + ex.Message + " - " + ex.StackTrace);
                    }
                }
            }
        }

        public void Play()
        {
            playerState = PlayerStateV2.Playing;
        }

        public void Pause()
        {
            playerState = PlayerStateV2.Pausing;
        }

        public void Stop()
        {
            playerState = PlayerStateV2.Stopped;
        }

        public void WhatToDo()
        {
            marketData = db.MarketDatas.OrderByDescending(a => a.UnixTime).FirstOrDefault();
            // if EMA above 1.8 and position is closed
            if(marketData.EMAVolumeWeightedRatio > 1.8 && position == Position.Closed)
                Buy();

            // if EMA under 1.4 and position is opened
            if (marketData.EMAVolumeWeightedRatio > 1.4 && position == Position.Opened)
                Sell();

            // if EMA under 0.55 and position is closed
            if (marketData.EMAVolumeWeightedRatio > 0.55 && position == Position.Opened)
                Put();

            // if EMA under 0.7 and position is opened
            if (marketData.EMAVolumeWeightedRatio > 0.7 && position == Position.Opened)
                Call();
        }

        public void Buy()
        {
             SendEmail(" Buy ");
             position = Position.Opened;
        }

        public void Sell()
        {
            SendEmail(" Sell ");
            position = Position.Closed;
        }

        public void Put() // or sell to open
        {
            SendEmail(" Put ");
            position = Position.Opened;
        }

        public void Call() // or buy to close
        {
            SendEmail(" Call ");
            position = Position.Closed;
        }

        public void SendEmail(string body)
        {           
            if (marketData != null)
            {
                body = body + " </br> Last price:" + marketData.Price +"</br> EMA: " + marketData.EMAVolumeWeightedRatio;
                ServiceEmail.Send("vincent.lemaitre.01@gmail.com", "kraken mouvement", body, true);
                log.Info(" Email Sent :  " + body);
            }
        }
        
    }



    public enum Position
    {
        Opened,
        Closed,
    }

    public enum PlayerStateV2
    {
        Playing,
        Pausing,
        Stopped,
        Started,
    }
}
