using KrakenClient;
using KrakenService.KrakenObjects;
using Newtonsoft.Json.Linq;
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
    public class Player
    {
        public Analysier analysier { get; set; }
        public KrakenClient.KrakenClient client {get; set;}
        public string Pair { get; set; }
        public List<CurrentOrder> MyOpenedOrders { get; set; }

        // Context property
        public PlayerState playerState { get; set; }
        public KrakenOrder CurrentOrder { get; set; }
        private NumberFormatInfo NumberProvider { get; set; }
        public SendingRateManager SRM { get; set; }
        
        public Player(Analysier i_analysier, string i_pair, SendingRateManager srm)
        {
            NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ".";
            SRM = srm;

            analysier = i_analysier;
            client = new KrakenClient.KrakenClient();
            MyOpenedOrders = analysier.MyOpenedOrders;
            playerState = PlayerState.Pending;

        }

        public string  Sell()
        {
            //Checking if the context is correct
            if(!playerState.HasFlag(PlayerState.ToSell))
            {
                return null;
            }

            //SendingRateCheck
            SRM.RateAddition(1);

            //  change this method if it is different
            analysier.SellAverageAndStandardDeviation();
            analysier.GetVolumeToSell();
                       

            // create the order
            KrakenOrder order = new KrakenOrder();
            order.Pair = "XBTEUR";
            order.Type = "sell";
            order.OrderType = "stop-loss-profit-limit";
            order.Price = Math.Round(Convert.ToDecimal(analysier.PriceToSellStopLoss,NumberProvider),3);
            order.Price2 = Math.Round(Convert.ToDecimal(analysier.PriceToSellProfit, NumberProvider), 3);
            order.Volume = Convert.ToDecimal(analysier.VolumeToSell,NumberProvider);

            Console.WriteLine("Sell !!! price:" + order.Price + " ; price2: " + order.Price2 + " ; volume:" + order.Volume);
            //Console.ReadKey();
            string response = client.AddOrder(order).ToString();

            //Get order id from response
            // Check response if no error and change status, don't change status otherwise
            if(GetOrderIdFromResponse(response) != null)
            {
                playerState = PlayerState.Selling;
                analysier.recorder.GetOpenOrders();
            }
            
            return response;
        }
        
        public string Buy()
        {
            //Checking if the context is correct
            if (!playerState.HasFlag(PlayerState.ToBuy))
            {
                return null;
            }

            //SendingRateCheck
            SRM.RateAddition(1);

            //  change this method if it is different
            analysier.BuyAverageAndStandardDeviation();
            analysier.GetVolumeToBuy();

            // create the order
            KrakenOrder order = new KrakenOrder();
            order.Pair = "XBTEUR";
            order.Type = "buy";
            order.OrderType = "stop-loss-profit-limit";
            order.Price = Math.Round(Convert.ToDecimal(analysier.PriceToBuyStopLoss,NumberProvider),3);
            order.Price2 = Math.Round(Convert.ToDecimal(analysier.PriceToBuyProfit,NumberProvider),3);
            order.Volume = Convert.ToDecimal(analysier.VolumeToBuy,NumberProvider);

            // Send request to buy
            Console.WriteLine("Buy !!! price:" + order.Price + " ; price2: " + order.Price2 + " ; volume:" + order.Volume);
            //Console.ReadKey();
            string response = client.AddOrder(order).ToString();
          

            //Get order id from response
            // Check response if no error and change status, don't change status otherwise
            if (GetOrderIdFromResponse(response) != null)
            {
                playerState = PlayerState.Buying;
                analysier.recorder.GetOpenOrders();
            }

            return response;
        }

        public void Play()
        {
            switch (playerState)
            {
                // BUYING ---------------------------
                case PlayerState.Buying:
                    // If buying check if the order has passed
                    if (!analysier.OpenedOrdersExist())
                    {                      
                        playerState = PlayerState.Bought;
                        Console.WriteLine("Bought !!");
                        break;
                    }
                    break;

                // SELLING --------------------------
                case PlayerState.Selling:
                    // If buying check if the order has passed
                    if (!analysier.OpenedOrdersExist())
                    {
                        playerState = PlayerState.Sold;
                        Console.WriteLine("Sold !!");
                        break;
                    }
                    break;

                // TO BUY ---------------------------
                case PlayerState.ToBuy:
                    Buy();
                    break;

                // TO SELL --------------------------
                case PlayerState.ToSell:
                    Sell();
                    break;

                // SOLD -----------------------------
                case PlayerState.Sold:
                    // Check if the analysier is ok to buy or sell with the current market data
                    if (!analysier.SellorBuy())
                    {
                        Console.WriteLine("DON'T BUY - Margin too low !!");
                        break;
                    }
                    playerState = PlayerState.ToBuy;
                    break;

                // BOUGHT -----------------------------
                case PlayerState.Bought:
                    playerState = PlayerState.ToSell;
                    return;

                // PENDING ----------------------------
                case PlayerState.Pending:
                    // Check if the analysier is ok to buy or sell with the current market data
                    
                    if(analysier.OpenedOrdersExist())
                    {
                        if (MyOpenedOrders.First().Type == "sell")
                            playerState = PlayerState.Selling;
                        if (MyOpenedOrders.First().Type == "buy")
                            playerState = PlayerState.Buying;

                        break;
                    }
                    
                    if(!analysier.SellorBuy())
                    {
                        Console.WriteLine("DON'T BUY - Margin too low !!");
                        break;
                    }
                    playerState = PlayerState.ToBuy;
                    break;
            }
           
        }

        #region helpers

        public string GetOrderIdFromResponse(string response)
        {
            JObject resp = JObject.Parse(response);
            try
            {                
                JArray array = (JArray)resp["result"]["txid"];
                //Orders = array.ToObject<List<string>>();
                return array.ToString();
            }
            catch(Exception ex)
            {
                Console.WriteLine(resp);
                return null;
            }
        }

        #endregion 

    }

    public enum PlayerState
    {
        ToBuy = 0,
        Buying =1,
        Bought = 2,
        ToSell = 3,
        Selling = 4,
        Sold = 5,
        Pending = 100,        
    }
}
