using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TradeWebApp.Models;
using System.Web;
using System.Web.Caching;
using System.Threading.Tasks;

namespace TradeWebApp.Controllers
{
    public class DashboardsController : ApiController
    {
        // GET: api/Dashboards
        [Route("api/Dashboards")]     
        public Dashboard Get()
        {
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/Start")]
        [AcceptVerbs("GET")]
        public Dashboard Start()
        {
            krakenManagement.Start();
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/Pause")]
        [AcceptVerbs("GET")]
        public Dashboard Pause()
        {
            krakenManagement.Pause();
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/Stop")]
        [AcceptVerbs("GET")]
        public Dashboard Stop()
        {
            krakenManagement.Stop();
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
        }


        // GET: api/Dashboards/5
        [Route("api/Dashboards/Init")]
        [AcceptVerbs("GET")]
        public Dashboard Init()
        {
            Task.Run(() => krakenManagement.Initialize());
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/PlayerState")]
        [AcceptVerbs("GET")]
        public Dashboard PlayerStateChange(int id)
        {
            krakenManagement.ChangePlayerState(id);
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
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
