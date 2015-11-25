using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class SendingRateManager
    {
        public int meter { get; set; }

        public SendingRateManager()
        {
            meter = 0;
            Task.Run(() => RateReduction());
        }

        public void RateReduction()
        {
            while (true)
            {
                if (meter > 0)
                {
                    meter = meter - 1; 
                }

                Thread.Sleep(2000);

                HTMLUpdate("MeterCount", meter.ToString());
            }
        }

        public void RateAddition(int rate)
        {
            while(meter + rate >= 20 )
            {
                Thread.Sleep(rate * 2000);
            }
                meter = meter + rate;
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
            catch (Exception)
            {

            }
        }
    }
}
