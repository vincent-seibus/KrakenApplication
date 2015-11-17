using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KrakenService.KrakenObjects;
using System.Threading;
using System.IO;
using System.Configuration;
using KrakenClient;
using System.Globalization;

namespace KrakenService
{
    public class Recorder
    {
        public KrakenClient.KrakenClient client { get; set; }
        public string Pair { get; set; }
        public ServerTime servertime { get; set; }
        
        //Recent trade property
        public RecentTrades recenttrades { get; set; }
        public List<TradingData> ListOftradingDatas { get; set; }
        
        //Bablance property
        public Balance CurrentBalance { get; set; }
        
        //orderbook property
        public OrdersBook ordersBook { get; set; }
        public List<CurrentOrder> ListOfCurrentOrder { get; set; }

        // Config property
        // inetrval in second is the last interval of data to keep 
        public double IntervalInSecond { get; set; }
        public double RequestSendingRate { get; set; }
        public SendingRateManager SRM { get; set; }
        private NumberFormatInfo NumberProvider { get; set; }
        public int OrderBookCount { get; set; }

        public Recorder(string i_pair, SendingRateManager srm)
        {
            NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ".";           

            SRM = srm;
            CurrentBalance = new Balance();
            client = new KrakenClient.KrakenClient();
            ListOftradingDatas = new List<TradingData>();
            ListOfCurrentOrder = new List<CurrentOrder>();
            Pair = i_pair;
            IntervalInSecond = Convert.ToDouble(ConfigurationManager.AppSettings["IntervalStoredInMemoryInSecond"]); // it is to keep the data in memory from X (inetrval) to now.
            OrderBookCount = Convert.ToInt16(ConfigurationManager.AppSettings["OrderBookCount"]);

            // Start recording 
            Task.Run(() => RecordRecentTradeData());
            Task.Run(() => RecordBalance());
            Task.Run(() => RecordOrderBook());
        }

        #region Deserialize method

        public ServerTime GetServerTime()
        {
            Newtonsoft.Json.Linq.JObject servertimeJson = JObject.Parse(client.GetServerTime().ToString());
            servertime = JsonConvert.DeserializeObject<ServerTime>(servertimeJson["result"].ToString());
            return servertime;
        }

        public RecentTrades GetRecentTrades(long? since)
        {
            JObject jo = JObject.Parse(client.GetRecentTrades(Pair, since).ToString());
            try
            {
                // check if error
                if (jo["error"] != null && jo["error"].ToString() != "[]")
                {
                    Console.WriteLine(jo["error"]);
                    return null;
                }

                var result = JsonConvert.DeserializeObject<dynamic>(jo["result"].ToString());
                recenttrades = new RecentTrades();
                recenttrades.Pair = Pair;
                JArray jsondatas = (JArray)jo["result"][Pair];
                recenttrades.Datas = jsondatas.ToObject<List<List<string>>>();
                recenttrades.Last = result.last;
                return recenttrades;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public OrdersBook GetOrdersBook()
        {
            JObject jo = JObject.Parse(client.GetOrderBook(Pair, OrderBookCount).ToString());
            try
            {
                // check if error
                if (jo["error"] != null && jo["error"].ToString() != "[]")
                {
                    Console.WriteLine(jo["error"]);
                    return null;
                }
                               
                JArray jsonasks = (JArray)jo["result"][Pair]["asks"];
                JArray jsonbids = (JArray)jo["result"][Pair]["bids"];
                List<List<string>> listasks = jsonasks.ToObject<List<List<string>>>();
                List<List<string>> listbids = jsonbids.ToObject<List<List<string>>>();

                ordersBook = new OrdersBook();

                ordersBook.Asks = listasks;
                ordersBook.Bids = listbids;
                ordersBook.Pair = Pair;

                return ordersBook;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        #endregion 

        #region Record region

        public void RecordRecentTradeData()
        {
            long? since = null;
            string filePath = CheckFileAndDirectoryTradingData();

            while (true)
            {
                // Sending rate increase the meter and check if can continue ootherwise stop 4sec;
                SRM.RateAddition(2);

                recenttrades = this.GetRecentTrades(since);

                // null if error in parsing likely due to a error message from API
                if(recenttrades == null)
                {
                    //Thread.Sleep(4000);
                    continue;
                }

                foreach (List<string> ls in recenttrades.Datas)
                {
                    // Foreach line, register in file and in the lsit
                    TradingData td = new TradingData();
                    int i = 0;
                    foreach (string s in ls)
                    {
                        RecordTradingDataInList(i, s, td);
                        File.AppendAllText(filePath,s + ",");
                        i++;
                        //Console.Write(s);
                    }
                    //Console.WriteLine();
                    ListOftradingDatas.Add(td);
                    File.AppendAllText(filePath,Environment.NewLine);
                }

                since = recenttrades.Last;
                Double interval = GetServerTime().unixtime;
                ListOftradingDatas.RemoveAll(a => a.UnixTime < (interval - IntervalInSecond));

                // Sleep to avoid to reach the limit;
                //Thread.Sleep(4000);
            }
        }

        public void RecordOHLCData()
        {

        }

        public void RecordOrderBook()
        {
            string filePath = CheckFileAndDirectoryOrdersBook();

            while(true)
            {
                // Sending rate increase the meter and check if can continue ootherwise stop 4sec;
                SRM.RateAddition(2);

                var ordersbook = this.GetOrdersBook();

                // null if error in parsing likely due to a error message from API
                if (ordersbook == null)
                {
                    continue;
                }

                foreach (List<string> ls in ordersBook.Asks)
                {
                    // Foreach line, register in file and in the lsit
                    CurrentOrder co = new CurrentOrder();
                    co.OrderType = "ask";
                    int i = 0;
                    foreach (string s in ls)
                    {
                        if (i == 0)
                            File.AppendAllText(filePath, "ask,");

                        RecordOrdersBookInList(i, s, co);
                        File.AppendAllText(filePath, s + ",");
                        i++;
                    }
                    //Console.WriteLine();
                    ListOfCurrentOrder.Add(co);
                    File.AppendAllText(filePath, Environment.NewLine);
                }

                foreach(List<string> ls in ordersbook.Bids)
                {
                    CurrentOrder co = new CurrentOrder();
                    int i = 0;
                    co.OrderType = "bid";
                    foreach(string s in ls)
                    {
                        if (i == 0)
                            File.AppendAllText(filePath, "bid,");

                        RecordOrdersBookInList(i, s, co);
                        File.AppendAllText(filePath, s + ",");
                        i++;
                    }

                    ListOfCurrentOrder.Add(co);
                    File.AppendAllText(filePath, Environment.NewLine);
                }

            }
        }

        public void RecordBalance()
        {
            while (true)
            {
                SRM.RateAddition(2);
                JObject jo = JObject.Parse(client.GetBalance().ToString());
                try
                {
                    if(jo["error"] != null && jo["error"].ToString() != "[]" )
                    {
                        Console.WriteLine(jo["error"]);
                        continue;
                    }

                    CurrentBalance.BTC = Convert.ToDouble(jo["result"]["XXBT"], NumberProvider);
                    CurrentBalance.EUR = Convert.ToDouble(jo["result"]["ZEUR"], NumberProvider);
                    //Thread.Sleep(4500);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // Log error
                }
            }
        }

        #endregion

        #region helpers

        /// <summary>
        /// Return file path to fill up
        /// </summary>
        /// <returns></returns>
        public string CheckFileAndDirectoryTradingData()
        {
              string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(),"TradingData" + Pair);
            if(!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "TradingData_" + DateTime.Now.Year+DateTime.Now.Month+DateTime.Now.Day);  
            if(!File.Exists(pathFile))
            File.Create(pathFile);

            return pathFile;
        }

        public string CheckFileAndDirectoryOrdersBook()
        {
            string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), "OrdersBook" + Pair);
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "OrdersBook_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day);
            if (!File.Exists(pathFile))
                File.Create(pathFile);

            return pathFile;
        }

        public void RecordTradingDataInList(int i, string value, TradingData td)
        {
            // Create a NumberFormatInfo object and set some of its properties.
          
            switch(i)
            {
                case 0:
                    td.Price = Convert.ToDouble(value, NumberProvider);
                    break;
                case 1:
                    td.Volume = Convert.ToDouble(value, NumberProvider);
                    break;
                case 2:
                    td.UnixTime = Convert.ToDouble(value, NumberProvider);
                    break;
                case 3:
                td.BuyOrSell = value;
                    break;
                case 4:
                td.MarketOrLimit = value;
                    break;
                case 5:
                    td.Misc = value;
                    break;
            }
        }

        public void RecordOrdersBookInList(int i, string value, CurrentOrder order)
        {
            switch(i)
            {
                case 0:
                    order.Price = Convert.ToDouble(value,NumberProvider);
                 break;
                case 1:
                 order.Volume = Convert.ToDouble(value,NumberProvider);
                 break;
                case 2:
                 order.Timestamp = Convert.ToDouble(value, NumberProvider);
                 break;
                
            }
        }

        #endregion 


    }
}
