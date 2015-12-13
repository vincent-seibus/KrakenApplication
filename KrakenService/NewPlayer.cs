using KrakenClient;
using KrakenService.KrakenObjects;
using KrakenService.MarketAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService
{
    public class NewPlayer
    {
        public IAnalysier analysier { get; set; }
        public KrakenClient.KrakenClient client { get; set; }
        public string Pair { get; set; }     

        // Context property
        public PlayerState playerState { get; set; }
        public KrakenOrder CurrentOrder { get; set; }       
        public SendingRateManager SRM { get; set; }

        // Config property
        private NumberFormatInfo NumberProvider { get; set; }

        public NewPlayer(IAnalysier i_analysier, string i_pair, SendingRateManager srm)
        {
            NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ConfigurationManager.AppSettings["CurrencyDecimalSeparator"];
            SRM = srm;

            analysier = i_analysier;
            client = new KrakenClient.KrakenClient();
            playerState = RetrieveContext();
        }

        public bool SellAtLimit()
        {
            if (!analysier.Sell())
            {
                return false;
            }

            //SendingRateCheck
            SRM.RateAddition(1);

            // create the order
            KrakenOrder order = new KrakenOrder();
            order.Pair = "XBTEUR";
            order.Type = "sell";
            order.OrderType = "limit";
            order.Price = Math.Round(Convert.ToDecimal(analysier.PriceToSellProfit, NumberProvider), 3);
            order.Volume = Convert.ToDecimal(analysier.VolumeToSell, NumberProvider);

            Console.WriteLine("Sell !!! price:" + order.Price + " ; volume:" + order.Volume);
            string response = client.AddOrder(order).ToString();

            //Get order id from response
            // Check response if no error and change status, don't change status otherwise
            OpenedOrder orderOpened = new OpenedOrder() { OrderType = "limit", Type = "sell", Price = (double)order.Price, Volume = (double)order.Volume };
            if (GetOrderIdFromResponse(response, orderOpened) != null)
            {
                return true;
            }

            return false;
        }

        public bool BuyAtLimit()
        {           

            if(!analysier.Buy())
            {
                return false;
            }

            //SendingRateCheck
            SRM.RateAddition(1);

            // create the order
            KrakenOrder order = new KrakenOrder();
            order.Pair = "XBTEUR";
            order.Type = "buy";
            order.OrderType = "limit";
            order.Price = Math.Round(Convert.ToDecimal(analysier.PriceToBuyProfit, NumberProvider), 3);
            order.Volume = Convert.ToDecimal(analysier.VolumeToBuy, NumberProvider);

            // Send request to buy
            Console.WriteLine("Buy !!! price:" + order.Price + " ; volume:" + order.Volume);
            //Console.ReadKey();
            string response = client.AddOrder(order).ToString();


            //Get order id from response
            // Check response if no error and change status, don't change status otherwise
            OpenedOrder orderOpened = new OpenedOrder() { OrderType = "limit", Type = "buy", Price = (double)order.Price, Volume = (double)order.Volume };
            if (GetOrderIdFromResponse(response, orderOpened) != null)
            {            
                    return true;                           
            }

            return false;
        }

        public void Buying()
        {
            if (!analysier.Buying())
            {
                playerState = PlayerState.Bought;
                analysier.MyOpenedOrders.Clear();
                Console.WriteLine("Bought !!");
                return;
            }

            if (analysier.CancelBuying())
            {
                playerState = PlayerState.ToCancelBuying;
                return;
            }
        }

        public void Selling()
        {
            // If buying check if the order has passed
            if (!analysier.Selling())
            {
                playerState = PlayerState.Sold;
                analysier.MyOpenedOrders.Clear();
                Console.WriteLine("Sold !!");
                return;
            }

            if (analysier.CancelSelling())
            {
                playerState = PlayerState.ToCancelSelling;
                return;
            }

        }

        public void Play()
        {
            switch (playerState)
            {
                // TO BUY ---------------------------
                case PlayerState.ToBuy:
                    if (BuyAtLimit())
                        playerState = PlayerState.Buying;
                    break;

                // BUYING ---------------------------
                case PlayerState.Buying:                  
                    Buying();
                    break;

                // BOUGHT -----------------------------
                case PlayerState.Bought:
                    if (analysier.Sell())
                        playerState = PlayerState.ToSell;
                    break;

                // TO SELL --------------------------
                case PlayerState.ToSell:
                    if (SellAtLimit())
                        playerState = PlayerState.Selling;
                    break;

                // SELLING --------------------------
                case PlayerState.Selling:
                    Selling();
                    break;

                // SOLD -----------------------------
                case PlayerState.Sold:
                    if(analysier.Buy())
                    playerState = PlayerState.ToBuy;
                    break;


                // CANCEL SELLING -----------------------------
                case PlayerState.ToCancelSelling:                   
                    Cancel(analysier.MyOpenedOrders.First().OrderID);
                    break;

                // CANCEL BUYING -----------------------------
                case PlayerState.ToCancelBuying:
                    Cancel(analysier.MyOpenedOrders.First().OrderID);
                    break;

                //CANCELLED BUYING -----------------------------
                case PlayerState.CancelledBuying:
                    if (analysier.Buy())
                        playerState = PlayerState.ToBuy;
                    break;

                //CANCELLED SELLING -----------------------------
                case PlayerState.CancelledSelling:
                    if (analysier.Sell())
                        playerState = PlayerState.ToSell;
                    break;

            }

            SaveContext();

        }

        public void Cancel(string OrderId)
        {
            if (!playerState.HasFlag(PlayerState.ToCancelBuying) && !playerState.HasFlag(PlayerState.ToCancelSelling))
            {
                return;
            }

            string response = client.CancelOrder(OrderId).ToString();

            JObject resp = JObject.Parse(response);

            try
            {
                if (resp["error"] == null || resp["error"].ToString() == "[]")
                {
                    if (playerState.HasFlag(PlayerState.ToCancelBuying))
                        playerState = PlayerState.CancelledBuying;
                    if (playerState.HasFlag(PlayerState.ToCancelSelling))
                        playerState = PlayerState.CancelledSelling;

                    analysier.MyOpenedOrders.Clear();
                    Console.WriteLine("Order Cancelled");
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "; response:" + resp);
            }

        }

        #region helpers

        public string GetOrderIdFromResponse(string response, OpenedOrder order)
        {
            JObject resp = JObject.Parse(response);
            try
            {
                JArray array = (JArray)resp["result"]["txid"];
                order.OrderID = array[0].ToString();
                analysier.MyOpenedOrders.Add(order);
                return array.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(resp);
                return null;
            }
        }

        private void SaveContext()
        {
            try
            {
                string pathfile = CheckFileAndDirectoryContext();
                dynamic Context = new JObject();
                Context.PlayerState = Enum.GetName(typeof(PlayerState), playerState);
                if (analysier.MyOpenedOrders.Count != 0)
                    Context.Order = JsonConvert.SerializeObject(analysier.MyOpenedOrders.FirstOrDefault());
                else
                    Context.Order = "";
                File.WriteAllText(pathfile, Context.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to save the context : " + ex.Message);
            }
        }

        private PlayerState RetrieveContext()
        {
            try
            {
                string pathfile = CheckFileAndDirectoryContext();
                string json = File.ReadAllText(pathfile);
                JObject Context = JObject.Parse(json);
                playerState = (PlayerState)Enum.Parse(typeof(PlayerState), Context["PlayerState"].ToString());

                if (!string.IsNullOrEmpty(Context["Order"].ToString()))
                {
                    OpenedOrder order = JsonConvert.DeserializeObject<OpenedOrder>(Context["Order"].ToString());
                    analysier.MyOpenedOrders.Add(order);
                }

                return playerState;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to retrieve the context : " + ex.Message);
                return PlayerState.ToBuy;
            }
        }

        private string CheckFileAndDirectoryContext()
        {
            string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), "ContextData" + Pair );
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "ContextData_" + analysier.GetType().ToString());
            if (!File.Exists(pathFile))
            {
                using (var myFile = File.Create(pathFile))
                {
                    // interact with myFile here, it will be disposed automatically
                }
            }

            return pathFile;
        }

        #endregion

    }

  
}
