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
        public RecentTrades recenttrades { get; set; }
        public List<TradingData> ListOftradingDatas { get; set; }
        public Balance CurrentBalance { get; set; }

        // Config property
        // inetrval in second is the last interval of data to keep 
        public double IntervalInSecond { get; set; }
        public double RequestSendingRate { get; set; }
        public SendingRateManager SRM { get; set; }
        private NumberFormatInfo NumberProvider { get; set; } 

        public Recorder(string i_pair, SendingRateManager srm)
        {
            NumberProvider = new NumberFormatInfo();
            NumberProvider.CurrencyDecimalSeparator = ".";

            SRM = srm;
            CurrentBalance = new Balance();
            client = new KrakenClient.KrakenClient();
            ListOftradingDatas = new List<TradingData>();
            Pair = i_pair;
            IntervalInSecond = Convert.ToDouble(ConfigurationManager.AppSettings["IntervalStoredInMemoryInSecond"]); // it is to keep the data in memory from X (inetrval) to now.

            // Start recording 
            Task.Run(() => RecordRecentTradeData());
            Task.Run(() => RecordBalance());
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
            try
            {
                JObject servertimeJson = JObject.Parse(client.GetRecentTrades(Pair, since).ToString());
                var result = JsonConvert.DeserializeObject<dynamic>(servertimeJson["result"].ToString());
                recenttrades = new RecentTrades();
                recenttrades.Pair = Pair;
                JArray jsondatas = (JArray)servertimeJson["result"][Pair];
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

        #endregion 

        #region Record region

        public void RecordRecentTradeData()
        {
            long? since = null;
            string filePath = CheckFileAndDirectory();

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

        }

        public void RecordBalance()
        {
            while (true)
            {
                SRM.RateAddition(2);       
                try
                {
                                
                    JObject jo = JObject.Parse(client.GetBalance().ToString());
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
        public string CheckFileAndDirectory()
        {
              string pathDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(),"TradingData" + Pair);
            if(!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            string pathFile = Path.Combine(pathDirectory, "TradingData_" + DateTime.Now.Year+DateTime.Now.Month+DateTime.Now.Day);  
            if(!File.Exists(pathFile))
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

        #endregion 


    }
}
