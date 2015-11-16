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
             KrakenService.Recorder rec1 = new KrakenService.Recorder(pair,SRM);
             KrakenService.Analysier ana1 = new KrakenService.Analysier(rec1);
             KrakenService.Player play1 = new Player(ana1, rec1.Pair,SRM);

            int i = 0;
            while(i < 40)
            {
                Thread.Sleep(1000);
                Console.WriteLine(i + ",");
                Console.WriteLine("Number :" + ana1.TradingDatasList.Count + " ; Higher:" + ana1.HigherPrice + " ; Lower: " + ana1.LowerPrice + " ; average : " + ana1.WeightedAverage + " ; Standard deviation : " + ana1.WeightedStandardDeviation);
                Console.WriteLine("BTC : " + rec1.CurrentBalance.BTC + "; EURO : " + rec1.CurrentBalance.EUR);
                i++;
            }

            Console.ReadKey();

            while(true)
            {
                Console.WriteLine(i + ",");
                Console.WriteLine("Number :" + ana1.TradingDatasList.Count + " ; Higher:" + ana1.HigherPrice + " ; Lower: " + ana1.LowerPrice + " ; average : " + ana1.WeightedAverage + " ; Standard deviation : " + ana1.WeightedStandardDeviation);
                Console.WriteLine("BTC : " + rec1.CurrentBalance.BTC + "; EURO : " + rec1.CurrentBalance.EUR);
                Console.WriteLine("------");
                play1.Play();
                Console.WriteLine("------");
                Thread.Sleep(2000);
            }       
           
        }

        
    }
}
