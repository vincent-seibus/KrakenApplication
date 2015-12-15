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
         List<OpenedOrder> MyOpenedOrders { get; set; }
                       
        // Decision on the market
         bool Buy();

         bool Sell();

         bool Buying();

         bool Selling();

         bool CancelSelling();

         bool CancelBuying();

         void CancelledSelling();

         void CancelledBuying();

        // Intensity of the decision
         double GetVolumeToBuy();

         double GetVolumeToSell();

         double GetPriceToBuy();

         double GetPriceToSell();     

    }
}
