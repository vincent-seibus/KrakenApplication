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
    }
}
