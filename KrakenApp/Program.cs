using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using KrakenClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using KrakenService;
using KrakenService.KrakenObjects;
using System.Threading;
using System.Globalization;
using HtmlAgilityPack;
using System.Configuration;
using KrakenService.MarketAnalysis;


namespace KrakenApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // The pair we will work on
            string pair = "XXBTZEUR";

             SendingRateManager SRM = new SendingRateManager();
             KrakenClient.KrakenClient client = new KrakenClient.KrakenClient();
              

             //string result = client.CancelOrder("O3V2NP-DTB7Z-EEKGIC").ToString();
             Console.WriteLine(client.GetBalance());
             //Console.ReadKey();

             KrakenService.Recorder rec1 = new KrakenService.Recorder(pair,SRM);
             //KrakenService.Analysier ana1 = new KrakenService.Analysier(rec1);
             //KrakenService.Player play1 = new Player(ana1, rec1.Pair,SRM);
             HighFrequencyMethod ana1 = new HighFrequencyMethod(pair,rec1,0.3);
             NewPlayer play1 = new NewPlayer(ana1, pair, SRM);
                 
            int i = 0;
            while(i < 40)
            {
                
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                HTMLUpdate("DateTime", unixTimestamp.ToString());
                HTMLUpdate("LastPrice", ana1.LastPrice.ToString());
                HTMLUpdate("LastMiddleQuote", ana1.LastMiddleQuote.ToString());
                HTMLUpdate("LastLowerAsk", ana1.LastLowerAsk.ToString());
                HTMLUpdate("LastHigherBid", ana1.LastHigherBid.ToString());

                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Last Middle Quote:" + ana1.LastMiddleQuote);
                Console.WriteLine("Last Trade price:" + ana1.LastPrice);                
                Console.WriteLine("Average : " + ana1.WeightedAverage);
                Console.WriteLine("Ecart Type : " + ana1.WeightedStandardDeviation);
                Console.WriteLine("BTC : " + rec1.CurrentBalance.BTC + "; EURO : " + rec1.CurrentBalance.EUR);
                Console.WriteLine("Player state :" + play1.playerState + " ; minimal earning required: " + ana1.MinimalPercentageOfEarning);
                i++;
            }
            Console.WriteLine("--------------------------------------");
            Console.ReadKey();

            while(true)
            {
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                HTMLUpdate("DateTime", unixTimestamp.ToString());
                HTMLUpdate("LastPrice", ana1.LastPrice.ToString());
                HTMLUpdate("LastMiddleQuote", ana1.LastMiddleQuote.ToString());
                HTMLUpdate("LastLowerAsk", ana1.LastLowerAsk.ToString());
                HTMLUpdate("LastHigherBid", ana1.LastHigherBid.ToString());

                play1.Play();              
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Player Status :" + play1.playerState);
                Console.WriteLine("Last Middle Quote:" + ana1.LastMiddleQuote);
                Console.WriteLine("Last Trade price:" + ana1.LastPrice);
                Console.WriteLine("Opened order exist : " + rec1.OpenedOrders.Select(a => a.OrderID).ToString());
                Console.WriteLine("Average : " + ana1.WeightedAverage);
                Console.WriteLine("Ecart Type : " + ana1.WeightedStandardDeviation);
                Console.WriteLine("BTC : " + rec1.CurrentBalance.BTC + "; EURO : " + rec1.CurrentBalance.EUR);
                Console.WriteLine("minimal earning required: " + ana1.MinimalPercentageOfEarning);
                Thread.Sleep(2000);
            }       
           
        }

        public static void HTMLUpdate(string ElementId, string valueToUpdate)
        {
            
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(ConfigurationManager.AppSettings["UrlHtmlFile"]);//@"C:\Users\vlemaitre\Documents\GitHub\KrakenApplication\KrakenApp\ResultPage.html");
                HtmlNode lastprice = doc.GetElementbyId(ElementId);
                lastprice.InnerHtml = valueToUpdate;
                doc.Save(ConfigurationManager.AppSettings["UrlHtmlFile"]);//@"C:\Users\vlemaitre\Documents\GitHub\KrakenApplication\KrakenApp\ResultPage.html");
            }
            catch (Exception)
            {

            }
        }
    }
}
