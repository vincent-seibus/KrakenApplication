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
using KrakenService.Data;


namespace KrakenApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // The pair we will work on
            string pair = "XXBTZEUR";

             SendingRateManager SRM = new SendingRateManager();
             KrakenService.Recorder rec1 = new KrakenService.Recorder(pair, SRM);

             HighFrequencyMethod ana1 = new HighFrequencyMethod(pair,rec1,0.6);

             OrderBookAnalysisMethod orderAna1 = new OrderBookAnalysisMethod(pair, rec1, 0.0);
             orderAna1.InitializeOrderBook();

             RSIMethod rsi1 = new RSIMethod(pair, rec1, 0.4);
             rsi1.InitializeRSI(30, 48);

             NewPlayer play1 = new NewPlayer(ana1, pair, SRM);
             ana1.intialize(); 

            int i = 0;
            while(i < 40)
            {
                
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                HTMLUpdate("DateTime", unixTimestamp.ToString());
                HTMLUpdate("LastPrice", ana1.LastPrice.ToString());
          

                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Last Middle Quote:" + orderAna1.LastMiddleQuote);
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
                HTMLUpdate("LastMiddleQuote", orderAna1.LastMiddleQuote.ToString());              

                play1.Play();              
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Player Status :" + play1.playerState);
                Console.WriteLine("Last Middle Quote:" + orderAna1.LastMiddleQuote);
                Console.WriteLine("Last Trade price:" + ana1.LastPrice);
                Console.WriteLine("Opened Order:");
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
