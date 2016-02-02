using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TradeWebApp.Models;
using System.Web;
using System.Web.Caching;

namespace TradeWebApp.Controllers
{
    public class DashboardsController : ApiController
    {
        // GET: api/Dashboards
        [Route("api/Dashboards")]
        [AcceptVerbs("GET")]
        public Dashboard Get()
        {
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
        }

        // GET: api/Dashboards/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Dashboards
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Dashboards/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Dashboards/5
        public void Delete(int id)
        {
        }
    }
}
