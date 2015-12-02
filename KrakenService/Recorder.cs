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
using CsvHelper;
using HtmlAgilityPack;

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
        public List<TradingData> ListOftradingDatasFiltered { get; set; } 

        //Recent trade property
        public RecentTrades OHLCReceived { get; set; }
        public List<OHLCData> ListOfOHLCData { get; set; }
       
        
        //Bablance property
        public Balance CurrentBalance { get; set; }
        
        //orderbook property
        public OrdersBook ordersBook { get; set; }
        public List<CurrentOrder> ListOfCurrentOrder { get; set; }
        public List<List<CurrentOrder>> OrderBookPerT { get; set; }

        // My orders section
        public List<CurrentOrder> OpenedOrders { get; set; }
        

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
            OrderBookPerT = new List<List<CurrentOrder>>();
            ListOfOHLCData = new List<OHLCData>();
            Pair = i_pair;
            IntervalInSecond = Convert.ToDouble(ConfigurationManager.AppSettings["IntervalStoredInMemoryInSecond"]); // it is to keep the data in memory from X (inetrval) to now.
            OrderBookCount = Convert.ToInt16(ConfigurationManager.AppSettings["OrderBookCount"]);
            OpenedOrders = new List<CurrentOrder>();

            // Start recording 
            GetOpenOrders();
            RecordBalance();
            GetLastTradingDataRecorded();
            Task.Run(() => GetRecordsRegularly());
        }

        public void GetRecordsRegularly()
        {
            long? sinceOHLCdata = null;
            long? sinceTradeData = null;

            while(true)
            {
                sinceTradeData = RecordRecentTradeData(sinceTradeData);
                Thread.Sleep(1000);
                RecordOrderBook();
                Thread.Sleep(1000);
                sinceOHLCdata = RecordOHLCData(sinceOHLCdata);
                Thread.Sleep(1000);
            }
        }

        #region Deserialize method

        public ServerTime GetServerTime()
        {
            Newtonsoft.Json.Linq.JObject servertimeJson = JObject.Parse(client.GetServerTime().ToString());
            try
            {

                servertime = JsonConvert.DeserializeObject<ServerTime>(servertimeJson["result"].ToString());
                return servertime;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return servertime;
            }
        }

        public RecentTrades GetRecentTrades(long? since)
        {
            recenttrades = null;
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

        public RecentTrades GetOHLCDatas(long? since)
        {
            recenttrades = null;
            int PeriodOHLCData = Convert.ToInt16(ConfigurationManager.AppSettings["PeriodOHLCData"]);

            JObject jo = JObject.Parse(client.GetOHLCData(Pair,PeriodOHLCData,since).ToString());
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
            catch (Exception ex)
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
        //Public
        public long? RecordRecentTradeData(long? since)
        {
            TradingData lastdata = GetLastTradingRecord();
         
            // check last data recorded in the file and format it.
            if (lastdata != null && since == null)
            {
                String last = Convert.ToString(lastdata.UnixTime, NumberProvider);        
                String lastgood = last.Replace(".","");
                while(lastgood.Length < 19)
                {
                    lastgood += "0";
                }

                since = Convert.ToInt64(lastgood, NumberProvider);
            }

            if (ListOftradingDatas.Count == 0 && ListOftradingDatasFiltered != null && ListOftradingDatasFiltered.Count > 0)
            {
                ListOftradingDatas = ListOftradingDatasFiltered;
            }

            string filePath = CheckFileAndDirectoryTradingData();          
                
            // Sending rate increase the meter and check if can continue ootherwise stop 4sec;               
            SRM.RateAddition(2);
            HTMLUpdate("LastAction", "RecordRecentTradeData");

            recenttrades = this.GetRecentTrades(since);
            // null if error in parsing likely due to a error message from API
            if(recenttrades == null)
            {
                return null;
            }

            //Treatment of the datas and store it in list
            foreach (List<string> ls in recenttrades.Datas)
            {
                // Foreach line, register in file and in the lsit
                TradingData td = new TradingData();
                int i = 0;
                foreach (string s in ls)
                {
                    RecordTradingDataInList(i, s, td);
                    i++;
                }

                ListOftradingDatas.Add(td);
            }

            // Filtered also the ListOftradingDatas to get only 86400;
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(filePath)))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(ListOftradingDatas);
            }

            //record last filtered data in file
            RecordLastTradingData();
            since = recenttrades.Last;
            return since;
        }

        public long? RecordOHLCData(long? since)
        {
            string filePath = this.CheckFileAndDirectoryOHLCData();

            OHLCData lastdata = GetLastLineOHLCDataRecorded();
            if( lastdata != null && since == null)
            {
                since = (long)lastdata.time;
            }

           
            // Sending rate increase the meter and check if can continue ootherwise stop 4sec;              
            SRM.RateAddition(2);
            HTMLUpdate("LastAction", "RecordOHLCData");
            OHLCReceived = this.GetOHLCDatas(since);

            // null if error in parsing likely due to a error message from API
            if (OHLCReceived == null)
            {
                return null ;
            }

            foreach (List<string> ls in OHLCReceived.Datas)
            {
                // Foreach line, register in file and in the lsit
                OHLCData td = new OHLCData();
                int i = 0;
                foreach (string s in ls)
                {
                    RecordOHLCDataInList(i, s, td);
                    i++;
                }
                    
                ListOfOHLCData.Add(td);
            }

            using (StreamWriter writer = new StreamWriter(File.OpenWrite(filePath)))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(ListOfOHLCData);
            }

            since = OHLCReceived.Last;
            return since;
        }

        public void RecordOrderBook()
        {
            string filePath = CheckFileAndDirectoryOrdersBook();

            // Sending rate increase the meter and check if can continue ootherwise stop 4sec;
            SRM.RateAddition(2);
            HTMLUpdate("LastAction", "RecordOrderBook");

            var ordersbook = this.GetOrdersBook();

            // null if error in parsing likely due to a error message from API
            if (ordersbook == null)
            {
                return;
            }

            //empty list before filling it up
            ListOfCurrentOrder.Clear();

            foreach (List<string> ls in ordersBook.Asks)
            {
                // Foreach line, register in file and in the lsit
                CurrentOrder co = new CurrentOrder();
                co.OrderType = "ask";

                int i = 0;
                foreach (string s in ls)
                {
                    RecordOrdersBookInList(i, s, co);
                    i++;
                }
                    
                ListOfCurrentOrder.Add(co);
            }

            foreach(List<string> ls in ordersbook.Bids)
            {
                CurrentOrder co = new CurrentOrder();
                int i = 0;
                co.OrderType = "bid";

                foreach(string s in ls)
                {
                    RecordOrdersBookInList(i, s, co);
                    i++;
                }

                ListOfCurrentOrder.Add(co);
            }
            
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(filePath)))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(ListOfCurrentOrder);
            }

            OrderBookPerT.Add(ListOfCurrentOrder);

         }

        public void RecordLastTradingData()
        {
            string filePathLast = CheckFileAndDirectoryLastTradingData();

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            ListOftradingDatasFiltered = ListOftradingDatas.Where(a => a.UnixTime > (unixTimestamp - IntervalInSecond)).ToList();

            // EMpty the fiel before filling it 
            System.IO.File.WriteAllText(filePathLast, string.Empty);

            // Record last trade data - over written by new data each time
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(filePathLast)))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(ListOftradingDatasFiltered);
            }
        }

        //Private
        public void RecordBalance()
        {              
                SRM.RateAddition(2);
                HTMLUpdate("LastAction", "RecordBalance");
                JObject jo = JObject.Parse(client.GetBalance().ToString());
                try
                {
                    if(jo["error"] != null && jo["error"].ToString() != "[]" )
                    {
                        Console.WriteLine(jo["error"]);
                        return;
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

        public string GetOpenOrders()
        {
           
            //Sleep to avoid temporary lock out caused by GetOpenOrder() method call
            SRM.RateAddition(2);
            HTMLUpdate("LastAction", "GetOpenOrders");

            JObject obj = JObject.Parse(client.GetOpenOrders().ToString());
            OpenedOrders.Clear();
            try
            {
                JObject OpenedOrdersJson = (JObject)obj["result"]["open"];
                // if orderID is empty, it means that no orders are currently done or th application has been stopped and started 
                if (OpenedOrders != null && OpenedOrders.ToString() != "{}")
                {
                    string txid = OpenedOrdersJson.Properties().First().Name;
                    // Foreach orders store each orders 
                    foreach (JProperty jn in OpenedOrdersJson.Properties())
                    {
                        CurrentOrder openedorder = new CurrentOrder();
                        JObject order = (JObject)OpenedOrdersJson[jn.Name];
                        openedorder.OrderID = jn.Name;
                        openedorder.Type = order["descr"]["type"].ToString();
                        openedorder.OrderType = order["descr"]["ordertype"].ToString();
                        openedorder.Price = Convert.ToDouble(order["descr"]["price"], NumberProvider);
                        openedorder.Price2 = Convert.ToDouble(order["descr"]["price2"],NumberProvider);
                        openedorder.Volume = Convert.ToDouble(order["vol"], NumberProvider);

                        if (OpenedOrders.Count == 0 || OpenedOrders.Where(a => a.OrderID == openedorder.OrderID).Count() == 0)
                            OpenedOrders.Add(openedorder);
                    }

                    return txid;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Opened Order Error: " + obj["error"]);
                return null;
            }
        }

        #endregion

        #region Reader

        public OHLCData GetLastLineOHLCDataRecorded()
        {
            OHLCData OHLCLastData = new OHLCData();

            using (StreamReader reader = File.OpenText(CheckFileAndDirectoryOHLCData()))
            {
                var csv = new CsvReader(reader);
                var records = csv.GetRecords<OHLCData>();
                try
                {
                    OHLCLastData = records.OrderByDescending(a => a.time).FirstOrDefault();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return OHLCLastData;
        }

        public List<OHLCData> GetLastRowsOHLCDataRecorded(int Rows)
        {
            List<OHLCData> OHLCLastRows = new List<OHLCData>();

            using (StreamReader reader = File.OpenText(CheckFileAndDirectoryOHLCData()))
            {
                var csv = new CsvReader(reader);
                var records = csv.GetRecords<OHLCData>();
                try
                {
                    OHLCLastRows = records.OrderByDescending(a => a.time).Take(Rows).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return OHLCLastRows;
        }

        public TradingData GetLastTradingRecord()
        {
            List<TradingData> TradingDataList = new List<TradingData>();
            TradingData LastTradingData = new TradingData();

            using (StreamReader reader = File.OpenText(CheckFileAndDirectoryLastTradingData()))
            {
                try
                {
                    var csv = new CsvReader(reader);
                    while (csv.Read())
                    {
                        try
                        {
                            var record = csv.GetRecord<TradingData>();
                            TradingDataList.Add(record);
                        }
                        catch(Exception)
                        {

                        }
                    }

                    LastTradingData = TradingDataList.OrderByDescending(a => a.UnixTime).FirstOrDefault();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return LastTradingData;
        }

        public void GetLastTradingDataRecorded()
        {
            ListOftradingDatasFiltered = new List<TradingData>();
            using (StreamReader reader = File.OpenText(CheckFileAndDirectoryLastTradingData()))
            {
                try
                {
                    var csv = new CsvReader(reader);
                    while (csv.Read())
                    {
                        try
                        {
                            var record = csv.GetRecord<TradingData>();
                            ListOftradingDatasFiltered.Add(record);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                catch (Exception)
                {
                    
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
            if (!File.Exists(pathFile))
            {
                using (var myFile = File.Create(pathFile))
                {
                    // interact with myFile here, it will be disposed automatically
                }
            }

            return pathFile;
        }

        public string CheckFileAndDirectoryLastTradingData()
        {
            string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), "TradingData" + Pair);
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "LastTradingData");
            if (!File.Exists(pathFile))
            {
                using (var myFile = File.Create(pathFile))
                {
                    // interact with myFile here, it will be disposed automatically
                }
            }

            return pathFile;
        }

        public string CheckFileAndDirectoryOrdersBook()
        {
            string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), "OrdersBook" + Pair);
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "OrdersBook_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day);
            if (!File.Exists(pathFile))
            {
                using (var myFile = File.Create(pathFile))
                {
                    // interact with myFile here, it will be disposed automatically
                }
            }

            return pathFile;
        }

        public string CheckFileAndDirectoryOHLCData()
        {
            string OHLCInterval = ConfigurationManager.AppSettings["PeriodOHLCData"];
            string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), "OHLCData" + Pair);
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "OHLCData_" + OHLCInterval);
            if (!File.Exists(pathFile))
            {
                using (var myFile = File.Create(pathFile))
                {
                    // interact with myFile here, it will be disposed automatically
                }
            }

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

        public void RecordOHLCDataInList(int i, string value, OHLCData td)
        {
            // Create a NumberFormatInfo object and set some of its properties.

            switch (i)
            {
                case 0:
                    td.time = Convert.ToDouble(value, NumberProvider);
                    break;
                case 1:
                    td.open = Convert.ToDouble(value, NumberProvider);
                    break;
                case 2:
                    td.high = Convert.ToDouble(value, NumberProvider);
                    break;
                case 3:
                    td.low = Convert.ToDouble(value, NumberProvider);
                    break;
                case 4:
                    td.close = Convert.ToDouble(value, NumberProvider);
                    break;
                case 5:
                    td.vwap = value;
                    break;
                case 6:
                    td.volume = Convert.ToDouble(value, NumberProvider);
                    break;
                case 7:
                    td.count = Convert.ToInt32(value);
                    break;
            }
        }

        public void HTMLUpdate(string ElementId, string valueToUpdate)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(@"C:\Users\vlemaitre\Documents\GitHub\KrakenApplication\KrakenApp\ResultPage.html");
                HtmlNode lastprice = doc.GetElementbyId(ElementId);
                lastprice.InnerHtml = valueToUpdate;
                doc.Save(@"C:\Users\vlemaitre\Documents\GitHub\KrakenApplication\KrakenApp\ResultPage.html");
            }
            catch(Exception)
            {

            }
        }

        #endregion 

    }
}
