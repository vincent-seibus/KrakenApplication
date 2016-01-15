using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KrakenService;
using KrakenService.MarketAnalysis;
using KrakenService.KrakenObjects;

namespace UnitTestKraken
{
    [TestClass]
    public class NewPlayerTest
    {
        [TestMethod]
        public void Test()
        {
            // The pair we will work on
            string pair = "XXBTZEUR";

            SendingRateManager SRM = new SendingRateManager();
            KrakenService.Recorder rec1 = new KrakenService.Recorder(pair, SRM);

            OrderBookAnalysisMethod orderAna1 = new OrderBookAnalysisMethod(pair, rec1, 0.0);
            orderAna1.InitializeOrderBook();

            orderAna1.VolumeWeightedRatioTresholdToBuy = 1.5;
            orderAna1.VolumeWeightedRatioTresholdToSell = 1.5;
            orderAna1.Buy();
            orderAna1.Sell();

        }

    }
}
