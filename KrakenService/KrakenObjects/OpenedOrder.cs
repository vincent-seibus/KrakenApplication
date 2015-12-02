using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrakenService.KrakenObjects
{
    public class OpenedOrder
    {
        public string OrderID { get; set; }
        public string UserRef { get; set; }
        public string Status { get; set; } // pending = order pending book entry ; open = open order; closed = closed order ; canceled = order canceled ; expired = order expired
        public string Opentm { get; set; } // unix timestamp of when order was placed
        public string Starttm { get; set; } // unix timestamp of order start time (or 0 if not set)
        public string Starttm { get; set; } // unix timestamp of order end time (or 0 if not set)
        public string Pair { get; set; }

        public string OrderType { get; set; } // ask or bid for the book / limit or market or other for my orders
        public string Type { get; set; } // buy or sell for my order
        public double Price { get; set; }
        public double? Price2 { get; set; } // profit price if stop profit limit order
        public double Volume { get; set; } 
        public double Leverage { get; set; } // amount of leverage 1, 1.5 ,2.0
        public string Descriptiion  { get; set; }
        public string Close  { get; set; } // close = conditional close order description (if conditional close set)
        public string Fee  { get; set; } //fee = total fee (quote currency)
    }
}
    /*/ OrderType
    market
    limit (price = limit price)
    stop-loss (price = stop loss price)
    take-profit (price = take profit price)
    stop-loss-profit (price = stop loss price, price2 = take profit price)
    stop-loss-profit-limit (price = stop loss price, price2 = take profit price)
    stop-loss-limit (price = stop loss trigger price, price2 = triggered limit price)
    take-profit-limit (price = take profit trigger price, price2 = triggered limit price)
    trailing-stop (price = trailing stop offset)
    trailing-stop-limit (price = trailing stop offset, price2 = triggered limit offset)
    stop-loss-and-limit (price = stop loss price, price2 = limit price)
    /*/// OrderType

/*/ 

   
vol_exec = volume executed (base currency unless viqc set in oflags)
cost = total cost (quote currency unless unless viqc set in oflags)

price = average price (quote currency unless viqc set in oflags)
stopprice = stop price (quote currency, for trailing stops)
limitprice = triggered limit price (quote currency, when limit based order type triggered)
misc = comma delimited list of miscellaneous info
    stopped = triggered by stop price
    touched = triggered by touch price
    liquidated = liquidation
    partial = partial fill
oflags = comma delimited list of order flags
    viqc = volume in quote currency
    fcib = prefer fee in base currency (default if selling)
    fciq = prefer fee in quote currency (default if buying)
    nompp = no market price protection
trades = array of trade ids related to order (if trades info requested and data available)