﻿using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TradeWebApp.Controllers.Stripe
{
    public class CardsController : ApiController
    {
        public string apiKey = "Your API Key"; 
        // GET: api/Cards
        public IEnumerable<string> Get()
        {
            var api = new StripeClient(apiKey);
           
            return new string[] { "value1", "value2" };
        }

        // GET: api/Cards/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Cards
        public void Post([FromBody]string value)
        {
            
        }

        // PUT: api/Cards/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Cards/5
        public void Delete(int id)
        {
        }
    }
}
