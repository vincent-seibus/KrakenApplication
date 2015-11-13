using KrakenClient;
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
        public List<string> OrderId { get; set; }

        // Context property
        public bool buying { get; set; }
        public bool selling { get; set; }
        public bool bought { get; set; }
        public bool sold { get; set; }
        public bool pending { get; set; }
        public KrakenOrder CurrentOrder { get; set; }
        private NumberFormatInfo NumberProvider { get; set; } 

        public Player(Analysier i_analysier, string i_pair)
        {
            NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ".";

            analysier = i_analysier;
            client = new KrakenClient.KrakenClient();
            OrderId = new List<string>();

            buying = false;
            selling = false;
            bought = false;
            sold = false;
            pending = true;
        }

        public string  Sell()
        {
            //  change this method if it is different
            analysier.SellAverageAndStandardDeviation();
            analysier.GetVolumeToSell();

            // create the order
            KrakenOrder order = new KrakenOrder();
            order.Pair = "XBTEUR";
            order.Type = "sell";
            order.OrderType = "stop-loss-profit";
            order.Price = Math.Round(Convert.ToDecimal(analysier.PriceToSellStopLoss,NumberProvider),3);
            order.Price2 = Math.Round(Convert.ToDecimal(analysier.PriceToSellProfit, NumberProvider), 3);
            order.Volume = Convert.ToDecimal(analysier.VolumeToSell,NumberProvider);

            Console.WriteLine("Sell !!! price:" + order.Price + " ; price2: " + order.Price2 + " ; volume:" + order.Volume);
            //Console.ReadKey();
            string response = client.AddOrder(order).ToString();

            selling = true;

            //Get order id from response
            GetOrderIdFromResponse(response);

            return response;
        }
        
        public string Buy()
        {
            //  change this method if it is different
            analysier.BuyAverageAndStandardDeviation();
            analysier.GetVolumeToBuy();

            // create the order
            KrakenOrder order = new KrakenOrder();
            order.Pair = "XBTEUR";
            order.Type = "buy";
            order.OrderType = "stop-loss-profit";
            order.Price = Math.Round(Convert.ToDecimal(analysier.PriceToBuyStopLoss,NumberProvider),3);
            order.Price2 = Math.Round(Convert.ToDecimal(analysier.PriceToBuyProfit,NumberProvider),3);
            order.Volume = Convert.ToDecimal(analysier.VolumeToBuy,NumberProvider);

            // Send request to buy
            Console.WriteLine("Buy !!! price:" + order.Price + " ; price2: " + order.Price2 + " ; volume:" + order.Volume);
            //Console.ReadKey();
            string response = client.AddOrder(order).ToString();
            // Change the status of the player
            buying = true;

            //Get order id from response
            GetOrderIdFromResponse(response);

            return response;
        }

        public void Play()
        {
            if(buying)
            {
               
                // If buying check if the order has passed
                JToken openedorders = GetOpenOrders();
                if (openedorders != null && openedorders.ToString() == "{}")
                {
                    // if the order is passed, start selling
                    buying = false;
                    bought = true;
                    Console.WriteLine("Bought !!");
                    Thread.Sleep(Convert.ToInt16(ConfigurationManager.AppSettings["WaitingTimeBetweenBuyAndSell"]));
                    Sell();
                    return;
                }

                return;
               
            }

            if(selling)
            {
                

                // If buying check if the order has passed
                JToken openedorders = GetOpenOrders();
                if (openedorders != null && openedorders.ToString() == "{}")
                {
                    // if the order is passed, start selling
                    sold = true;
                    selling = false;
                    Console.WriteLine("Sold !!");
                    Thread.Sleep(Convert.ToInt16(ConfigurationManager.AppSettings["WaitingTimeBetweenBuyAndSell"]));
                    Buy();
                    return;
                }

                return;
            }

            if(pending)
            {
                Buy();
            }

            pending = false;
        }

        #region helpers

        public List<string> GetOrderIdFromResponse(string response)
        {
            JObject resp = JObject.Parse(response);
            try
            {                
                JArray array = (JArray)resp["result"]["txid"];
                OrderId = array.ToObject<List<string>>();
                return OrderId;
            }
            catch(Exception ex)
            {
                Console.WriteLine(resp);
                return OrderId;
            }
        }

        public JToken  GetOpenOrders()
        {

            //Sleep to avoid temporary lock out caused by GetOpenOrder() method call
            Thread.Sleep(4500);

            JObject obj = JObject.Parse(client.GetOpenOrders().ToString());
           
            try
            {           
                var OpenedOrders = obj["result"]["open"];

                // if orderID is empty, it means that no orders are currently done
                if(OrderId.Count == 0)
                {
                    return null;
                }

                if(OpenedOrders != null)
                {
                    
                }
                return OpenedOrders;
            }
            catch(Exception ex)
            {
                Console.WriteLine(obj);
                return null;
            }

           
        }

        #endregion 

    }
}
