using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Stripe;

namespace TradeWebApp.Controllers
{
    public class StripeController : ApiController
    {
        public string apiKey = "Your API Key"; 
        // GET: api/Stripe
        public IEnumerable<string> Get()
        {
            var api = new StripeClient(apiKey);
            
            return new string[] { api.ApiEndpoint, api.ApiVersion };
        }

        // GET: api/Stripe/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Stripe
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Stripe/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Stripe/5
        public void Delete(int id)
        {
        }
    }
}
