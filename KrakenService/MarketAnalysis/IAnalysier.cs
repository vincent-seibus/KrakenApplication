using KrakenService.KrakenObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.MarketAnalysis
{
    public interface IAnalysier
    {
         double VolumeToBuy { get; set; }
         double VolumeToSell { get; set; }
         double PriceToSellProfit { get; set; }
         double PriceToSellStopLoss { get; set; }
         double PriceToBuyProfit { get; set; }
               
         bool Buy();

         bool Sell();

         bool CancelSelling();

         bool CancelBuying();

         double GetVolumeToBuy();

         double GetVolumeToSell();

         double GetPriceToBuy();

         double GetPriceToSell();

    }
}
